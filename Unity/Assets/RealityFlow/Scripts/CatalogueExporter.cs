using System.IO;
using UnityEngine;

public class CatalogueExporter : MonoBehaviour
{
    public GameObject[] catalogue; // Array of all objects in your catalogue

    void Start()
    {
        ExportCatalogueToText("CatalogueData.txt");
    }

    void ExportCatalogueToText(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (GameObject obj in catalogue)
            {
                writer.WriteLine("Object: " + obj.name);
                Component[] components = obj.GetComponents<Component>();
                foreach (Component component in components)
                {
                    writer.WriteLine("  Component: " + component.GetType().Name);
                    // Add specific component data extraction here if needed
                    // For example, if the component is a Transform, get position, rotation, and scale
                    if (component is Transform transform)
                    {
                        writer.WriteLine("    Position: " + transform.position);
                        writer.WriteLine("    Rotation: " + transform.rotation);
                        writer.WriteLine("    Scale: " + transform.localScale);
                    }
                }
                writer.WriteLine(); // Add a blank line for better readability
            }
        }

        Debug.Log("Catalogue exported to " + filePath);
    }
}
