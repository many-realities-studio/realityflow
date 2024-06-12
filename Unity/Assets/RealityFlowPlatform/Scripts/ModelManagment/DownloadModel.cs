using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GLTFast;
using UnityEngine.XR.Interaction.Toolkit; // Add this namespace for XR Interaction Toolkit

public class DownloadModel : MonoBehaviour
{
    public void StartDownload(string modelUrl)
    {
        Debug.Log("Starting download with URL: " + modelUrl);
        StartCoroutine(DownloadModelCoroutine(modelUrl));
    }

    private IEnumerator DownloadModelCoroutine(string url)
    {
        Debug.Log("DownloadModelCoroutine URL: " + url);
        UnityWebRequest webRequest = UnityWebRequest.Get(url);

        // Wait on response
        yield return webRequest.SendWebRequest();

        // Check if response was bad
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download page. Result: "
                + webRequest.result + " Error: " + webRequest.error);
            yield break;
        }

        // Get the data
        byte[] data = webRequest.downloadHandler.data;
        Debug.Log("Succeeded. Received: " + data.Length + " bytes.");

        // Start parsing the data, leave coroutine till the parsing finishes
        var gltf = new GltfImport();
        var success_task = gltf.LoadGltfBinary(data, null);
        // gltf.Load
        yield return new WaitUntil(() => success_task.IsCompleted);

        // Check if the parsing had failed
        if (!success_task.Result)
        {
            Debug.LogError("Failed to parse the downloaded file");
            Debug.Log("Data: " + webRequest.downloadHandler.text);
            yield break;
        }
        Debug.Log("Successfully parsed the downloaded file");

        // Make an instance of the model
        var scene = gltf.InstantiateMainSceneAsync(transform.parent);
        yield return scene;

        // Debug components of the instantiated model
        if (scene.Result)
        {
            Debug.Log("Model instantiated successfully.");
            var instantiatedModel = transform.GetChild(transform.childCount - 1);
            var meshFilter = instantiatedModel.GetComponentInChildren<MeshFilter>();
            var meshRenderer = instantiatedModel.GetComponentInChildren<MeshRenderer>();

            // Ensure the model retains its original shape
            // instantiatedModel.localPosition = Vector3.zero;
            // instantiatedModel.localRotation = Quaternion.identity;
            // instantiatedModel.localScale = Vector3.one;

            if (meshFilter != null)
            {
                Debug.Log("MeshFilter found: " + meshFilter.name);
            }
            else
            {
                Debug.LogError("MeshFilter not found.");
            }

            if (meshRenderer != null)
            {
                Debug.Log("MeshRenderer found: " + meshRenderer.name);
            }
            else
            {
                Debug.LogError("MeshRenderer not found.");
            }

            // Apply a uniform scale if necessary
            float maxScale = Mathf.Max(instantiatedModel.localScale.x, Mathf.Max(instantiatedModel.localScale.y, instantiatedModel.localScale.z));
            instantiatedModel.localScale = new Vector3(maxScale, maxScale, maxScale);

            // Add Rigidbody component for physics
            Rigidbody rb = instantiatedModel.gameObject.AddComponent<Rigidbody>();

            // Add appropriate collider
            // Note: You might want to add a specific collider that matches the shape of your model
            Collider collider = instantiatedModel.gameObject.AddComponent<MeshCollider>();
            ((MeshCollider)collider).convex = true;

            // Optional: Configure Rigidbody properties
            rb.mass = 1.0f; // Example: Set mass to 1
            rb.useGravity = false; // Enable gravity
            rb.isKinematic = false; // Allow physics interactions

            Debug.Log("Rigidbody and Collider added to the model.");

            // Add XRGrabInteractable component for XR interaction
            XRGrabInteractable grabInteractable = instantiatedModel.gameObject.AddComponent<XRGrabInteractable>();

            // Optional: Configure XRGrabInteractable properties
            grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic; // Example: Set movement type to kinematic
            grabInteractable.throwOnDetach = true; // Allow throwing the object when released

            Debug.Log("XRGrabInteractable added to the model.");
        }
        else
        {
            Debug.LogError("Failed to instantiate the model.");
        }
    }
}