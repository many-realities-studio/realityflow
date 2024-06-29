using UnityEditor;
using UnityEngine;
using System.IO;
using Ubiq.Messaging;
using Microsoft.MixedReality.Toolkit.UX;
using Microsoft.MixedReality.Toolkit.Examples.Demos;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using RealityFlow.NodeUI;

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

        if (GUILayout.Button("Update Component Settings"))
        {
            UpdateComponentSettingsOnPrefabs();
        }

        if (GUILayout.Button("Update Constraint Manager and Object Manipulator Components"))
        {
            ConstraintAndObjectManipulatorsOnPrefabs();
        }

        if (GUILayout.Button("Add Remaining Components"))
        {
            AddRemainingComponentsOnPrefabs();
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
        /*MeshFilter[] meshFilters = instance.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            // Add MeshCollider to each GameObject with a MeshFilter
            MeshCollider meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = true; // Set the MeshCollider to convex if required
            }
        }*/

        // Add Rigidbody to the root GameObject of the prefab if not already present
        if (instance.GetComponent<Rigidbody>() == null)
        {
            instance.AddComponent<Rigidbody>();
            instance.GetComponent<Rigidbody>().useGravity = false;
            instance.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void UpdateComponentSettingsOnPrefabs()
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
                    UpdateComponentSettings(instance);

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

    private void UpdateComponentSettings(GameObject instance)
    {
        // Set RigidBody Settings
        if (instance.GetComponent<Rigidbody>() != null)
        {
            instance.GetComponent<Rigidbody>().useGravity = false;
            instance.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void ConstraintAndObjectManipulatorsOnPrefabs()
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
                    ConstraintAndObjectManipulators(instance);

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

    private void ConstraintAndObjectManipulators(GameObject instance)
    {
        if (instance.GetComponent<ConstraintManager>() == null)
        {
            instance.AddComponent<ConstraintManager>();
            instance.GetComponent<ConstraintManager>().AutoConstraintSelection = true;
        }

        if (instance.GetComponent<ObjectManipulator>() == null)
        {
            ObjectManipulator objectManipulator = instance.AddComponent<ObjectManipulator>();
            objectManipulator.HostTransform = instance.transform;
            //objectManipulator.AllowedManipulations = TransformFlags.Move | TransformFlags.Rotate | TransformFlags.Scale;
            objectManipulator.AllowedInteractionTypes = InteractionFlags.Near | InteractionFlags.Ray | InteractionFlags.Gaze | InteractionFlags.Generic;
            //objectManipulator.selectMode = InteractableSelectMode.Multiple;
            objectManipulator.UseForcesForNearManipulation = false;
            objectManipulator.RotationAnchorNear = ObjectManipulator.RotateAnchorType.RotateAboutGrabPoint;
            objectManipulator.RotationAnchorFar = ObjectManipulator.RotateAnchorType.RotateAboutGrabPoint;
            //objectManipulator.ReleaseBehavior = ObjectManipulator.ReleaseBehaviorType.KeepVelocity | ObjectManipulator.ReleaseBehaviorType.KeepAngularVelocity;
            objectManipulator.ReleaseBehavior = 0;
            objectManipulator.SmoothingFar = true;
            objectManipulator.SmoothingNear = true;

            objectManipulator.MoveLerpTime = 0.001f;
            objectManipulator.RotateLerpTime = 0.001f;
            objectManipulator.ScaleLerpTime = 0.001f;
            objectManipulator.EnableConstraints = true;
            objectManipulator.ConstraintsManager = instance.GetComponent<ConstraintManager>() ?? instance.AddComponent<ConstraintManager>();
        }
    }

    private void AddRemainingComponentsOnPrefabs()
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

                    AddRemainingComponents(instance);

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

    private void AddRemainingComponents(GameObject instance)
    {
        // Add All the expected components, check if a component already exists for each type
        // Add MyNetworkedObject script
        if (instance.GetComponent<MyNetworkedObject>() == null)
        {
            instance.AddComponent<MyNetworkedObject>();
            // Assign events
            var myNetworkedObject = instance.GetComponent<MyNetworkedObject>();

            ObjectManipulator objManip = instance.GetComponent<ObjectManipulator>();

            if (objManip != null)
            {
                objManip.firstSelectEntered.AddListener((args) => myNetworkedObject.StartHold());
                objManip.lastSelectExited.AddListener((args) => myNetworkedObject.EndHold());
            }
        }

        // Add CacheObjectData script
        if (instance.GetComponent<CacheObjectData>() == null)
        {
            instance.AddComponent<CacheObjectData>();
        }
                        
        // Add whiteboard attatch
        if(instance.GetComponent<AttachedWhiteboard>() == null)
        {
            instance.AddComponent<AttachedWhiteboard>();
        }
        
    }
}
