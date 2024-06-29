using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Microsoft.MixedReality.Toolkit.Input;
using Ubiq.Spawning;
using UnityEngine.XR.Interaction.Toolkit;

public class SpawnObjectAtRay : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    private GameObject currentPrefab = null;
    private bool spawnToggle = false;

    // Function to spawn an object at the position of the ray interactor
    public void Spawn()
    {
        // This action is technically always active, so unless the toggle is on for spawning objects just return from here
        if (!spawnToggle || !currentPrefab)
            return;

        // Get the spawn point of the object from the ray transform and Instantiate
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // Construct the hit position of the raycast
            Vector3 hitPos = new Vector3(hit.point.x, hit.point.y + 0.5f, hit.point.z);

            // Instantiate the object in the scene and over the network, then set its position to the hit position
            GameObject currentObject = NetworkSpawnManager.Find(this).SpawnWithPeerScope(currentPrefab);
            currentObject.transform.position = hitPos;  // This currently only works locally, location needs to be sent over network
            Debug.Log("Spawned " + currentPrefab.name);
        }
        else
            Debug.Log("Failed to spawn " + currentPrefab.name + ": invalid location");
    }

    // Function to cancel spawning of the current prefab
    public void CancelSpawn()
    {
        if (spawnToggle)
        {
            Debug.Log("Object spawn cancelled: no longer spawning " + currentPrefab.name);
            spawnToggle = false;
            currentPrefab = null;
        }
    }

    // Function called by the UI button in the default object library. Sets spawn toggle and prefab.
    public void RaySpawnToggle(GameObject objectPrefab)
    {
        Debug.Log("Spawning toggled on for object: " + objectPrefab.name);
        spawnToggle = true;
        currentPrefab = objectPrefab;
    }
}
