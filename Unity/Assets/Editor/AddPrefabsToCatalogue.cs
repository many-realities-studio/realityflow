using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Ubiq.Spawning; // Ensure this matches your namespace

public class PrefabCatalogueEditor : EditorWindow
{
    private string folderPath = "Assets/Prefabs"; // Default folder path
    private PrefabCatalogue catalogue; // Reference to your existing catalogue class

    [MenuItem("Tools/Prefab Catalogue")]
    public static void ShowWindow()
    {
        GetWindow<PrefabCatalogueEditor>("Prefab Catalogue");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Catalogue Tool", EditorStyles.boldLabel);

        folderPath = EditorGUILayout.TextField("Prefabs Folder Path", folderPath);
        catalogue = (PrefabCatalogue)EditorGUILayout.ObjectField("Prefab Catalogue", catalogue, typeof(PrefabCatalogue), false);

        if (GUILayout.Button("Populate Catalogue"))
        {
            PopulateCatalogue();
        }

        if (GUILayout.Button("Remove Empty Entries"))
        {
            RemoveEmptyEntries();
        }
    }

    private void PopulateCatalogue()
    {
        if (catalogue == null)
        {
            Debug.LogError("Prefab Catalogue not set.");
            return;
        }

        string[] prefabPaths = Directory.GetFiles(folderPath, "*.prefab", SearchOption.AllDirectories);

        List<GameObject> newPrefabs = new List<GameObject>();

        foreach (string prefabPath in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null && !catalogue.prefabs.Contains(prefab))
            {
                newPrefabs.Add(prefab);
            }
        }

        catalogue.prefabs.AddRange(newPrefabs);

        EditorUtility.SetDirty(catalogue);
        AssetDatabase.SaveAssets();

        Debug.Log("Catalogue populated with " + newPrefabs.Count + " new prefabs. Total prefabs in catalogue: " + catalogue.prefabs.Count);
    }

    private void RemoveEmptyEntries()
    {
        if (catalogue == null)
        {
            Debug.LogError("Prefab Catalogue not set.");
            return;
        }

        int initialCount = catalogue.prefabs.Count;
        catalogue.prefabs.RemoveAll(prefab => prefab == null);

        EditorUtility.SetDirty(catalogue);
        AssetDatabase.SaveAssets();

        int removedCount = initialCount - catalogue.prefabs.Count;
        Debug.Log("Removed " + removedCount + " empty entries. Total prefabs in catalogue: " + catalogue.prefabs.Count);
    }
}
