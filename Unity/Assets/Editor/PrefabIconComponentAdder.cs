using UnityEditor;
using UnityEngine;
using System.IO;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;

public class PrefabIconComponentAdder : EditorWindow
{
    private string prefabsFolderPath = "Assets/Prefabs";

    [MenuItem("Tools/Prefab Icon Component Adder")]
    public static void ShowWindow()
    {
        GetWindow<PrefabIconComponentAdder>("Prefab Icon Component Adder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Icon Component Adder", EditorStyles.boldLabel);
        prefabsFolderPath = EditorGUILayout.TextField("Prefabs Folder Path", prefabsFolderPath);

        if (GUILayout.Button("Add Icon Components"))
        {
            AddIconComponentsToPrefabs();
        }

        if (GUILayout.Button("Update SimpleRotation"))
        {
            updateSimpleRotation();
        }

        // Be VERY careful if using this functionality.
        /*if (GUILayout.Button("Reset"))
        {
            resetPrefabsToBase();
        }*/
    }


    private void AddIconComponentsToPrefabs()
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
                    AddIconComponents(instance);

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

    private void AddIconComponents(GameObject instance)
    {
        // Add All the expected components, check if a component already exists for each type
        if (instance.GetComponent<ConstraintManager>() == null)
        {
            instance.AddComponent<ConstraintManager>();
        }

        if (instance.GetComponent<UGUIInputAdapterDraggable>() == null)
        {
            instance.AddComponent<UGUIInputAdapterDraggable>();
        }

        if (instance.GetComponent<SimpleRotation>() == null)
        {
            instance.AddComponent<SimpleRotation>();
        }
    }

    private void updateSimpleRotation()
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
                    updateSRotation(instance);

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

    private void updateSRotation(GameObject instance)
    {
        if (instance.GetComponent<SimpleRotation>() != null)
        {
            SimpleRotation srComponent = instance.GetComponent<SimpleRotation>();
            srComponent.speed = 20f;
            srComponent.ForwardZ = true;
            srComponent.ForwardY = false;
        }
    }

    private void resetPrefabsToBase()
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
                    // removes everything but the tranform on the base prefab
                    removeAllButTransform(instance);

                    // removes the colliders in children of the prefab.
                    removeCollidersInChildren(instance);

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

    private void removeAllButTransform(GameObject instance)
    {
        Component[] components = instance.GetComponents<Component>();
 
        foreach (Component comp in components)
        {
            Debug.Log(comp.GetType());
            if(!(comp is Transform))
            {
                DestroyImmediate(comp);
            }
        }
    }

    private void removeCollidersInChildren(GameObject instance)
    {
        MeshCollider[] meshColliders = instance.GetComponentsInChildren<MeshCollider>();
 
        foreach (MeshCollider collider in meshColliders)
        {
            DestroyImmediate(collider);
        }
    }
}
