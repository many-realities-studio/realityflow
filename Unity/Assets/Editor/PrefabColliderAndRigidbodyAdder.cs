using UnityEditor;
using UnityEngine;
using System.IO;

public class PrefabColliderAndRigidbodyAdder : EditorWindow
{
    private string prefabsFolderPath = "Assets/Prefabs";

    [MenuItem("Tools/Prefab Collider and Rigidbody Adder")]
    public static void ShowWindow()
    {
        GetWindow<PrefabColliderAndRigidbodyAdder>("Prefab Collider and Rigidbody Adder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Collider and Rigidbody Adder", EditorStyles.boldLabel);
        prefabsFolderPath = EditorGUILayout.TextField("Prefabs Folder Path", prefabsFolderPath);

        if (GUILayout.Button("Add Colliders and Rigidbodies"))
        {
            AddCollidersAndRigidbodiesToPrefabs();
        }
    }

    private void AddCollidersAndRigidbodiesToPrefabs()
    {
        if (!Directory.Exists(prefabsFolderPath))
        {
            Debug.LogError("Prefabs folder path does not exist.");
            return;
        }

        string[] prefabFiles = Directory.GetFiles(prefabsFolderPath, "*.prefab", SearchOption.AllDirectories);

        foreach (string filePath in prefabFiles)
        {
            string assetPath = filePath.Substring(filePath.IndexOf("Assets"));
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                if (instance != null)
                {
                    // Add MeshColliders and Rigidbody
                    AddMeshColliderAndRigidbody(instance);

                    // Apply changes to the prefab
                    PrefabUtility.SaveAsPrefabAsset(instance, assetPath);
                    DestroyImmediate(instance);
                    Debug.Log($"Updated prefab: {assetPath}");
                }
            }
            else
            {
                Debug.LogError($"Failed to load prefab at path: {assetPath}");
            }
        }

        AssetDatabase.Refresh();
    }

    private void AddMeshColliderAndRigidbody(GameObject instance)
    {
        // Find all MeshFilters in the prefab
        MeshFilter[] meshFilters = instance.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            // Add MeshCollider to each GameObject with a MeshFilter
            MeshCollider meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = true; // Set the MeshCollider to convex if required
            }
        }

        // Add Rigidbody to the root GameObject of the prefab if not already present
        if (instance.GetComponent<Rigidbody>() == null)
        {
            instance.AddComponent<Rigidbody>();
        }
    }
}
