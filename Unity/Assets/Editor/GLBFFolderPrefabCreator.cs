using UnityEditor;
using UnityEngine;
using System.IO;

public class GLBToPrefabUnpacker : EditorWindow
{
    private string inputFolderPath = "Assets/GLBFiles";
    private string outputFolderPath = "Assets/Prefabs";

    [MenuItem("Tools/GLB to Prefab Unpacker")]
    public static void ShowWindow()
    {
        GetWindow<GLBToPrefabUnpacker>("GLB to Prefab Unpacker");
    }

    private void OnGUI()
    {
        GUILayout.Label("GLB to Prefab Unpacker", EditorStyles.boldLabel);
        inputFolderPath = EditorGUILayout.TextField("Input Folder Path", inputFolderPath);
        outputFolderPath = EditorGUILayout.TextField("Output Folder Path", outputFolderPath);

        if (GUILayout.Button("Unpack"))
        {
            UnpackGLBFilesToPrefabs();
        }
    }

    private void UnpackGLBFilesToPrefabs()
    {
        if (!Directory.Exists(inputFolderPath))
        {
            Debug.LogError("Input folder path does not exist.");
            return;
        }

        if (!Directory.Exists(outputFolderPath))
        {
            Directory.CreateDirectory(outputFolderPath);
        }

        string[] glbFiles = Directory.GetFiles(inputFolderPath, "*.glb");

        foreach (string filePath in glbFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string assetPath = Path.Combine(inputFolderPath, fileName + ".glb");
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (model != null)
            {
                GameObject instance = Instantiate(model);

                // Unpack the prefab completely if it's a prefab instance
                if (PrefabUtility.IsPartOfPrefabInstance(instance))
                {
                    PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }

                // Add MeshCollider and Rigidbody to the prefab
                AddMeshColliderAndRigidbody(instance);

                // Save the unpacked instance as a new prefab
                string prefabPath = Path.Combine(outputFolderPath, fileName + ".prefab");
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                DestroyImmediate(instance);
                Debug.Log($"Prefab unpacked and created: {prefabPath}");
            }
            else
            {
                Debug.LogError($"Failed to load model at path: {assetPath}");
            }
        }

        AssetDatabase.Refresh();
    }

    private void AddMeshColliderAndRigidbody(GameObject instance)
    {
        // Find all MeshRenderers in the prefab
        MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            // Add MeshCollider to each MeshRenderer's GameObject
            MeshCollider meshCollider = meshRenderer.gameObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = meshRenderer.gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = true; // Set the MeshCollider to convex
            }
        }

        // Add Rigidbody to the root GameObject of the prefab if not already present
        if (instance.GetComponent<Rigidbody>() == null)
        {
            instance.AddComponent<Rigidbody>();
        }
    }
}
