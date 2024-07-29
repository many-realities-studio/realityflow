using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Spawning;
using RealityFlow.NodeGraph;

public class NetworkedObjectSpawner : MonoBehaviour
{
    public GameObject parentObject;
    private const float scaleMultiplier = 57f; // Define the scale multiplier

    public void SpawnAllNetworkedObjects()
    {
        if (parentObject != null)
        {
            StartCoroutine(SpawnAllNetworkedObjectsCoroutine());
        }
        else
        {
            Debug.LogError("Parent Object is not set.");
        }
    }

    private IEnumerator SpawnAllNetworkedObjectsCoroutine()
    {
        List<MyNetworkedObject> networkedObjects = FindAllNetworkedObjects(parentObject);

        foreach (MyNetworkedObject networkedObject in networkedObjects)
        {
            if (networkedObject.enabled)
            {
                SpawnNetworkedObject(networkedObject);
                yield return null; // Wait for the next frame to continue
            }
        }

        Debug.Log("All enabled networked objects have been spawned.");
    }

    private List<MyNetworkedObject> FindAllNetworkedObjects(GameObject parent)
    {
        List<MyNetworkedObject> networkedObjects = new List<MyNetworkedObject>();
        SearchForNetworkedObjects(parent.transform, networkedObjects);
        return networkedObjects;
    }

    private void SearchForNetworkedObjects(Transform parent, List<MyNetworkedObject> networkedObjects)
    {
        foreach (Transform child in parent)
        {
            MyNetworkedObject networkedObject = child.GetComponent<MyNetworkedObject>();
            if (networkedObject != null)
            {
                networkedObjects.Add(networkedObject);
            }

            // Recursively search the child
            SearchForNetworkedObjects(child, networkedObjects);
        }
    }

    private async void SpawnNetworkedObject(MyNetworkedObject networkedObject)
    {
        if (RealityFlowAPI.Instance == null)
        {
            Debug.LogError("RealityFlowAPI instance not found.");
            return;
        }

        // Use the position, rotation, and original scale of the networkedObject to spawn it
        Vector3 position = networkedObject.transform.position;
        Quaternion rotation = networkedObject.transform.rotation;
        Vector3 originalScale = networkedObject.transform.localScale;

        GameObject spawnedObject = await RealityFlowAPI.Instance.SpawnObject(networkedObject.gameObject.name, position, originalScale, rotation);

        if (spawnedObject != null)
        {
            // Inherit the tag from the prefab
            spawnedObject.tag = networkedObject.gameObject.tag;

            // Scale up the object in the next frame using the API method
            StartCoroutine(ScaleUpObject(spawnedObject, position, rotation, originalScale * scaleMultiplier));

            // Ensure the collider is convex and update it
            MeshCollider meshCollider = spawnedObject.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.convex = true; // Set the convex property to true
                meshCollider.enabled = false;
                meshCollider.enabled = true; // Force recalculation
            }

            // Check if the object has the "Walls" tag and set it as static
            if (spawnedObject.CompareTag("Walls"))
            {
                VisualScript visualScript = spawnedObject.GetComponent<VisualScript>();
                if (visualScript != null)
                {
                    RealityFlowAPI.Instance.SetStatic(visualScript, true);
                }
            }

            // Ensure Rigidbody stays kinematic for "Teleport" tagged objects
            if (spawnedObject.CompareTag("Teleport"))
            {
                StartCoroutine(EnforceKinematic(spawnedObject));
            }
        }

        Debug.Log($"Spawned networked object: {networkedObject.gameObject.name} at position: {position}");
    }

    private IEnumerator ScaleUpObject(GameObject obj, Vector3 position, Quaternion rotation, Vector3 targetScale)
    {
        yield return new WaitForEndOfFrame(); // Wait for the end of the frame to ensure object is fully initialized

        // Update the object transform using the API
        RealityFlowAPI.Instance.UpdateObjectTransform(obj.name, position, rotation, targetScale);

        // Ensure the collider is updated after scaling
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
            collider.enabled = true;
        }
    }

    private IEnumerator EnforceKinematic(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("No Rigidbody found on the object.");
            yield break;
        }

        rb.isKinematic = true; // Ensure it's kinematic initially

        while (true)
        {
            if (!rb.isKinematic)
            {
                Debug.LogWarning("Rigidbody isKinematic property was modified. Resetting to kinematic.");
                rb.isKinematic = true;
            }
            yield return new WaitForEndOfFrame(); // Check every frame
        }
    }
}
