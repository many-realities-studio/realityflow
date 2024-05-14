
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealityFlowAPITest : MonoBehaviour
{
    void Start()
    {
        // Spawn a test object
        GameObject spawnedObject = RealityFlowAPI.Instance.SpawnObject("Ladder", Vector3.zero, Quaternion.identity,RealityFlowAPI.SpawnScope.Room);
        if (spawnedObject != null)
        {
            Debug.Log("Successfully spawned: " + spawnedObject.name);
        }

    // Optional: schedule despawning
       if (spawnedObject != null)
        {
            Invoke(nameof(DespawnTestObject), 5f);  // Despawn after 5 seconds
        }
    }

    private void DespawnTestObject()
    {
        GameObject objectToDespawn = GameObject.Find("Ladder");  // Assuming this is the instantiated name
        if (objectToDespawn != null)
        {
            RealityFlowAPI.Instance.DespawnObject(objectToDespawn);
            Debug.Log("Successfully despawned: " + objectToDespawn.name);
        }
    }
}
