using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Spawning;

public class RealityFlowAPI : MonoBehaviour
{
    private NetworkSpawnManager spawnManager;

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
        //Debug.Log($"Attempting to spawn {prefabName}");
        GameObject prefab = GetPrefabByName(prefabName);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found: {prefabName}");
            return null;
        }

        GameObject newObject = Instantiate(prefab, position, rotation);
        newObject.name = prefabName;

        
        Debug.Log($"Spawned object: {newObject.name}, Instance ID: {newObject.GetInstanceID()}");

        switch (scope)
        {
            case SpawnScope.Room:
                spawnManager.SpawnWithRoomScope(newObject);  // Ensure this method is correctly implemented in NetworkSpawnManager
                Debug.Log("Spawned with Room Scope");
                break;
            case SpawnScope.Peer:
                spawnManager.SpawnWithPeerScope(newObject);  // Ensure this method is correctly implemented in NetworkSpawnManager
                Debug.Log("Spawned with Peer Scope");
                break;
            default:
                Debug.LogError("Unknown spawn scope");
                break;
        }

        return newObject;
    }

    public void DespawnObject(GameObject objectToDespawn)
    {
        if (objectToDespawn != null)
        {
            spawnManager.Despawn(objectToDespawn);
            Debug.Log("Despawned: " + objectToDespawn.name);
        }
        else
        {
            Debug.LogError("Object to despawn is null");
        }
    }
}