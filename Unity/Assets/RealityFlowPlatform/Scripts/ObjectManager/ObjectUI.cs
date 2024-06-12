using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // Assuming you use TextMeshPro for text fields

public class ObjectUI : MonoBehaviour
{
    public TextMeshProUGUI idText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public Image objectImage; // Assuming you have an image for visual representation

    public void SetData(RfObject rfObject)
    {
        string shortenedID = rfObject.id.Length > 4 ? rfObject.id.Substring(rfObject.id.Length - 4) : rfObject.id;
        idText.text = "ID: " + shortenedID;
        nameText.text = "Name: " + rfObject.name;
        typeText.text = "Type: " + rfObject.type;
        // Update the image if needed, e.g., using a sprite based on the object type
    }

    public static void PopulateUI(Transform contentContainer, GameObject objectPrefab, List<RfObject> objects)
    {
        if (contentContainer == null)
        {
            Debug.LogError("Content Container is not assigned.");
            return;
        }

        if (objectPrefab == null)
        {
            Debug.LogError("Object Prefab is not assigned.");
            return;
        }

        // Clear existing UI elements
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // Instantiate and populate UI elements for each object
        foreach (var obj in objects)
        {
            // Instantiate the prefab
            GameObject newObjectUI = Instantiate(objectPrefab, contentContainer);

            // Set the text fields using ObjectUI component
            ObjectUI objectUI = newObjectUI.GetComponent<ObjectUI>();
            if (objectUI != null)
            {
                objectUI.SetData(obj);
            }
            else
            {
                Debug.LogError("ObjectUI component not found on instantiated prefab.");
            }

            // Debug.Log($"Populated UI with object: {obj.name}");
        }
    }
}