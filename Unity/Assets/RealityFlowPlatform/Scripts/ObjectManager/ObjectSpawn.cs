using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Ubiq.Spawning;

public class ObjectSpawn : MonoBehaviour
{
    public GameObject prefabToSpawn;
    //private NetworkSpawnManager networkSpawnManager;

    private void Start()
    {
        /*// Find the NetworkSpawnManager in the scene
        networkSpawnManager = NetworkSpawnManager.Find(this);
        
        // Check if NetworkSpawnManager is found
        if (networkSpawnManager == null)
        {
            Debug.LogError("NetworkSpawnManager not found in the scene.");
            return;
        }*/

        // Example of how to spawn an object with room scope
        if (prefabToSpawn != null)
        {
            SpawnObjectWithRoomScope(prefabToSpawn);
        }
        else
        {
            Debug.LogError("Prefab to spawn is not assigned.");
        }
    }

    public void SpawnObjectWithRoomScope(GameObject prefab)
    {
        /*if (networkSpawnManager != null)
        {
            networkSpawnManager.SpawnWithRoomScope(prefab);
            Debug.Log($"Spawned {prefab.name} with room scope.");
        }
        else
        {
            Debug.LogError("NetworkSpawnManager is not initialized.");
        }*/
    }
}
