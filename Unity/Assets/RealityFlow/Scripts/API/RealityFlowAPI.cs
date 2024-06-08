using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Spawning;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RealityFlowAPI : MonoBehaviour, INetworkSpawnable
{
    private NetworkSpawnManager spawnManager;
    private readonly ActionLogger actionLogger = new();
    private NetworkContext networkContext;

    public NetworkId NetworkId { get; set; }

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
                    RealityFlowAPI api = FindObjectOfType<RealityFlowAPI>();
                    if (api == null)
                        api = new GameObject("RealityFlowAPI").AddComponent<RealityFlowAPI>();
                    _instance = api;
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
            spawnManager.roomClient.OnRoomUpdated.AddListener(OnRoomUpdated);
        }
    }

    public void ProcessTransformUpdate(string propertyKey, string jsonMessage)
    {
        var transformMessage = JsonUtility.FromJson<TransformMessage>(jsonMessage);
        Debug.Log($"Received transform update: {transformMessage.ObjectName}, Pos: {transformMessage.Position}, Rot: {transformMessage.Rotation}, Scale: {transformMessage.Scale}");
        GameObject obj = FindSpawnedObject(transformMessage.ObjectName);
        if (obj != null)
        {
            obj.transform.position = transformMessage.Position;
            obj.transform.rotation = transformMessage.Rotation;
            obj.transform.localScale = transformMessage.Scale;
        }
        else
        {
            Debug.LogError($"Object named {transformMessage.ObjectName} not found in ProcessTransformUpdate.");
        }
    }

    private void OnRoomUpdated(IRoom room)
    {
        foreach (var property in room)
        {
            if (property.Key.StartsWith("transform."))
            {
                ProcessTransformUpdate(property.Key, property.Value);
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

    public GameObject SpawnObject(string prefabName, Vector3 position, bool log, SpawnScope scope = SpawnScope.Room)
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

        if (newObject != null)
        {
            newObject.transform.position = position;
        }

        if (log)
            actionLogger.LogAction(new SpawnObject() { spawned = newObject });
        return newObject;
    }

    public void DespawnObject(GameObject objectToDespawn, bool log)
    {
        if (objectToDespawn != null)
        {
            // if (log)
                // actionLogger.LogAction(nameof(DespawnObject), objectToDespawn.name, objectToDespawn.transform.position, objectToDespawn.transform.rotation, objectToDespawn.transform.localScale);
            spawnManager.Despawn(objectToDespawn);
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
    public void UpdateObjectTransform(string objectName, bool log, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject obj = FindSpawnedObject(objectName);
        if (obj == null)
        {
            obj = GetPrefabByName(objectName);
        }
        if (obj != null)
        {
            // Log the current transform before making changes
            // if (log)
            //     actionLogger.LogAction(nameof(UpdateObjectTransform), objectName, obj.transform.position, obj.transform.rotation, obj.transform.localScale);
            Debug.Log("The object's current location is: position: " + obj.transform.position + " Object rotation: " + obj.transform.rotation + " Object scale: " + obj.transform.localScale);
            Debug.Log("The object's desired location is: position: " + position + " Object rotation: " + rotation + " Object scale: " + scale);

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.localScale = scale;

            // Serialize and send the transform update
            var message = new TransformMessage(objectName, position, rotation, scale);
            Debug.Log($"Sending transform update: {message.ObjectName}, Pos: {message.Position}, Rot: {message.Rotation}, Scale: {message.Scale}");
            var jsonMessage = JsonUtility.ToJson(message);
            var propertyKey = $"transform.{objectName}";
            spawnManager.roomClient.Room[propertyKey] = jsonMessage;
        }
        else
        {
            Debug.LogError($"Object named {objectName} not found.");
        }
    }

#if UNITY_EDITOR
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
        EditorUtility.SetDirty(spawnManager.catalogue);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Catalogue saved.");
    }
#endif

    public void UndoLastAction()
    {
        Debug.Log("Attempting to undo last action.");
        Debug.Log($"Action stack count before undo: {actionLogger.GetActionStackCount()}");

        var lastAction = actionLogger.GetLastAction();

        if (lastAction == null)
        {
            Debug.Log("No actions to undo.");
            return;
        }

        lastAction.Undo();

        Debug.Log($"Action stack after undo: {actionLogger.GetActionStackCount()}");
    }

    private void UndoSingleAction(ActionLogger.ILogAction action)
    {
        action.Undo();
    }

    public List<string> GetPrefabNames()
    {
        if (spawnManager != null && spawnManager.catalogue != null)
        {
            return spawnManager.catalogue.prefabs.Select(prefab => prefab.name).ToList();
        }
        return new List<string>();
    }

    public void StartCompoundAction()
    {
        actionLogger.StartCompoundAction();
    }

    public void EndCompoundAction()
    {
        actionLogger.EndCompoundAction();
    }
}

[Serializable]
public class TransformMessage
{
    public string ObjectName;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public TransformMessage(string objectName, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        ObjectName = objectName;
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
}
