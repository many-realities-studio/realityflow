using UnityEngine;
using System.Collections.Generic;

public class ObjectSelect : MonoBehaviour
{
    [SerializeField]
    private string objectId;
    private RfObjectManager rfObjectManager;
    private ObjectDelete objectDelete; // Add reference to ObjectDelete
    private GameObject selectedObject;
    public Material outlineMaterial;
    private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();
    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private Vector3 previousScale;

    void Start()
    {
        // Initialize RfObjectManager
        rfObjectManager = FindObjectOfType<RfObjectManager>();
        if (rfObjectManager == null)
        {
            Debug.LogError("RfObjectManager not found in the scene.");
            return;
        }

        // Initialize ObjectDelete
        objectDelete = FindObjectOfType<ObjectDelete>();
        if (objectDelete == null)
        {
            Debug.LogError("ObjectDelete script is not found in the scene.");
            return;
        }
    }

    async void LateUpdate()
    {
        // Check if the selected object's transform has changed
        if (selectedObject != null)
        {
            if (selectedObject.transform.position != previousPosition ||
                selectedObject.transform.rotation != previousRotation ||
                selectedObject.transform.localScale != previousScale)
            {
                // Update the previous transform values
                previousPosition = selectedObject.transform.position;
                previousRotation = selectedObject.transform.rotation;
                previousScale = selectedObject.transform.localScale;

                // Send updated transform to the database
                TransformData transformData = new TransformData
                {
                    position = selectedObject.transform.position,
                    rotation = selectedObject.transform.rotation,
                    scale = selectedObject.transform.localScale
                };

                // Await SaveObjectTransformToDatabase
                await rfObjectManager.SaveObjectTransformToDatabase(objectId, transformData);
            }
        }
    }

    public void SelectAndOutlineObject(string id)
    {
        // Find the object with the given ID
        GameObject objectToSelect = GameObject.Find(id);

        if (objectToSelect != null)
        {
            // Apply outline effect to the object
            OutlineEffect(objectToSelect);

            // If there is already a selected object, remove the outline
            if (selectedObject != null && selectedObject != objectToSelect)
            {
                RemoveOutlineEffect(selectedObject);
            }

            // Update the reference to the currently selected object
            selectedObject = objectToSelect;

            // Initialize previous transform values
            previousPosition = selectedObject.transform.position;
            previousRotation = selectedObject.transform.rotation;
            previousScale = selectedObject.transform.localScale;

            // Set the objectId for database updates
            objectId = id;
        }
        else
        {
            Debug.LogWarning("Object with ID " + id + " not found.");
        }
    }

    public void DeleteSelectedObject()
    {
        if (objectDelete != null && !string.IsNullOrEmpty(objectId))
        {
            objectDelete.DeleteObject(objectId);
        }
        else
        {
            Debug.LogError("ObjectDelete script is not found in the scene or objectId is null/empty.");
        }
    }

    void OutlineEffect(GameObject obj)
    {
        // Apply the outline effect (using material or component)
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                // Store the original material if not already stored
                if (!originalMaterials.ContainsKey(renderer.gameObject))
                {
                    originalMaterials[renderer.gameObject] = renderer.material;
                }
                // Apply the outline material
                renderer.material = outlineMaterial;
            }
        }
    }

    void RemoveOutlineEffect(GameObject obj)
    {
        // Remove the outline effect (restore original material)
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && originalMaterials.ContainsKey(renderer.gameObject))
            {
                renderer.material = originalMaterials[renderer.gameObject];
                originalMaterials.Remove(renderer.gameObject); // Optionally remove the entry if it's no longer needed
            }
        }
    }

    // Public getter and setter for objectId
    public string GetObjectId()
    {
        return objectId;
    }

    public void SetObjectId(string value)
    {
        objectId = value;
    }
}
