using UnityEngine;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Spawning;

public class RealityFlowAPI : MonoBehaviour
{
    [SerializeField] private List<GameObject> prefabCatalogue; // Populate in the Unity Editor

    private NetworkSpawnManager spawnManager;
    private NetworkContext networkContext;

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
            networkContext = NetworkScene.Register(this);
        }
    }

    public GameObject GetPrefabByName(string name)
    {
        foreach (var prefab in prefabCatalogue)
        {
            if (prefab.name == name)
                return prefab;
        }
        Debug.LogWarning($"Prefab named {name} not found.");
        return null;
    }

    public GameObject SpawnObject(string prefabName, Vector3 position, Quaternion rotation = default, SpawnScope scope = SpawnScope.Room)
{
    GameObject prefab = GetPrefabByName(prefabName);
    if (prefab == null)
    {
        Debug.LogError($"Prefab not found: {prefabName}");
        return null;
    }

    GameObject newObject = Instantiate(prefab, position, rotation);
    
    switch (scope)
    {
        case SpawnScope.Room:
            spawnManager.SpawnWithRoomScope(newObject);  // Make sure this method exists and is implemented correctly
            Debug.Log("Spawned with Room Scope");
            break;
        case SpawnScope.Peer:
            spawnManager.SpawnWithPeerScope(newObject);  // Make sure this method exists and is implemented correctly
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
            spawnManager.Despawn(objectToDespawn); // Assuming Despawn handles the network context
        }
        else
        {
            Debug.LogError("Object to despawn is null");
        }
    }

    // Implement INetworkComponent if there are specific network messages to handle
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Handle network messages here if necessary
    }
}
