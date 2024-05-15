using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Spawning;
using System;
using UnityEditor;

public class RealityFlowAPI : MonoBehaviour
{
    private NetworkSpawnManager spawnManager;
    private ActionLogger actionLogger = new ActionLogger();

    // Singleton instance
    private static RealityFlowAPI _instance;
    private static readonly object _lock = new object();

    public enum SpawnScope
    {
        Room,
        Peer
    }

    public static RealityFlowAPI Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<RealityFlowAPI>() ?? new GameObject("RealityFlowAPI").AddComponent<RealityFlowAPI>();
                }
                return _instance;
            }
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Makes the object persistent across scenes
            spawnManager = NetworkSpawnManager.Find(this);
            if (spawnManager == null)
            {
                Debug.LogError("NetworkSpawnManager not found on the network scene!");
            }
        }
    }

    public GameObject GetPrefabByName(string name)
    {
        if (spawnManager != null && spawnManager.catalogue != null)
        {
            foreach (var prefab in spawnManager.catalogue.prefabs)
            {
                if (prefab.name == name)
                    return prefab;
            }
        }
        Debug.LogWarning($"Prefab named {name} not found in function GetPrefabByName.");
        return null;
    }

    public GameObject SpawnObject(string prefabName, Vector3 position, Quaternion rotation = default, SpawnScope scope = SpawnScope.Room)
    {
        GameObject newObject = spawnManager.catalogue.prefabs.Find(prefab => prefab.name.Equals(prefabName, StringComparison.OrdinalIgnoreCase));
        if (newObject == null)
        {
            Debug.LogError($"Prefab not found: {prefabName}");
            return null;
        }

        switch (scope)
        {
            case SpawnScope.Room:
                spawnManager.SpawnWithRoomScope(newObject);
                Debug.Log("Spawned with Room Scope");
                break;
            case SpawnScope.Peer:
                spawnManager.SpawnWithPeerScope(newObject);
                Debug.Log("Spawned with Peer Scope");
                break;
            default:
                Debug.LogError("Unknown spawn scope");
                break;
        }

        actionLogger.LogAction(nameof(SpawnObject), prefabName, position, rotation, scope);
        return newObject;
    }

    public void DespawnObject(GameObject objectToDespawn)
    {
        if (objectToDespawn != null)
        {
            spawnManager.Despawn(objectToDespawn);
            actionLogger.LogAction(nameof(DespawnObject), objectToDespawn);
            Debug.Log("Despawned: " + objectToDespawn.name);
        }
        else
        {
            Debug.LogError("Object to despawn is null");
        }
    }

    public GameObject FindSpawnedObject(string objectName)
    {
        if (spawnManager == null)
        {
            Debug.LogError("SpawnManager is not initialized.");
            return null;
        }

        // Search in the spawnedForRoom dictionary
        foreach (var kvp in spawnManager.GetSpawnedForRoom())
        {
            if (kvp.Value.name.Equals(objectName, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        // Search in the spawnedForPeers dictionary
        foreach (var peerDict in spawnManager.GetSpawnedForPeers())
        {
            foreach (var kvp in peerDict.Value)
            {
                if (kvp.Value.name.Equals(objectName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }
        }

        Debug.LogWarning($"Object named {objectName} not found in the spawned objects.");
        return null;
    }

    // Method to update the transform of a networked object
    public void UpdateObjectTransform(string objectName, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject obj = FindSpawnedObject(objectName);
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.localScale = scale;
            spawnManager.roomClient.OnRoomUpdated.Invoke(spawnManager.roomClient.Room);
            //spawnManager.roomClient.OnPeerUpdated.Invoke(spawnManager.roomClient.Room);
            actionLogger.LogAction(nameof(UpdateObjectTransform), objectName, position, rotation, scale);
        }
        else
        {
            Debug.LogError($"Object named {objectName} not found.");
        }
    }

    public void AddPrefabToCatalogue(GameObject prefab)
    {
        if (spawnManager != null && spawnManager.catalogue != null)
        {
            if (!spawnManager.catalogue.prefabs.Contains(prefab))
            {
                spawnManager.catalogue.prefabs.Add(prefab);
                Debug.Log($"Prefab {prefab.name} added to catalogue.");
                SaveCatalogue();
            }
            else
            {
                Debug.LogWarning($"Prefab {prefab.name} is already in the catalogue.");
            }
        }
        else
        {
            Debug.LogError("NetworkSpawnManager or its catalogue is null.");
        }
    }

    public void AddGameObjectToCatalogue(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Debug.LogError("Cannot add null GameObject to catalogue.");
            return;
        }

        string localPath = "Assets/Prefabs/" + gameObject.name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, localPath);
        if (prefab != null)
        {
            AddPrefabToCatalogue(prefab);
        }
        else
        {
            Debug.LogError("Failed to create prefab from GameObject.");
        }
    }

    private void SaveCatalogue()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(spawnManager.catalogue);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Catalogue saved.");
#endif
    }

    public void UndoLastAction()
    {
        var lastAction = actionLogger.GetLastAction();
        if (lastAction == null) return;

        switch (lastAction.FunctionName)
        {
            case nameof(SpawnObject):
                string prefabName = (string)lastAction.Parameters[0];
                GameObject spawnedObject = FindSpawnedObject(prefabName);
                DespawnObject(spawnedObject);
                break;

            case nameof(DespawnObject):
                GameObject obj = (GameObject)lastAction.Parameters[0];
                SpawnObject(obj.name, obj.transform.position, obj.transform.rotation, SpawnScope.Peer);
                break;

            case nameof(UpdateObjectTransform):
                string objName = (string)lastAction.Parameters[0];
                Vector3 oldPosition = (Vector3)lastAction.Parameters[1];
                Quaternion oldRotation = (Quaternion)lastAction.Parameters[2];
                Vector3 oldScale = (Vector3)lastAction.Parameters[3];
                UpdateObjectTransform(objName, oldPosition, oldRotation, oldScale);
                break;

                // Add cases for other functions...
        }
    }
}
