using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.Examples.Demos;
using RealityFlow.NodeUI;
using System.Linq;
using System.Reflection;

public class AddComponentsTool : EditorWindow
{
    private GameObject singlePrefab;
    private GameObject parentObject;
    private bool updateAllChildren;

    [MenuItem("Tools/Add Components Tool")]
    public static void ShowWindow()
    {
        GetWindow<AddComponentsTool>("Add Components Tool");
    }

    void OnGUI()
    {
        GUILayout.Label("Add Components to Prefabs", EditorStyles.boldLabel);

        singlePrefab = (GameObject)EditorGUILayout.ObjectField("Single Prefab", singlePrefab, typeof(GameObject), false);
        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), false);
        updateAllChildren = EditorGUILayout.Toggle("Update All Children", updateAllChildren);

        if (GUILayout.Button("Add Components"))
        {
            if (updateAllChildren && parentObject != null)
            {
                UpdateComponentsInChildren(parentObject);
            }
            else if (singlePrefab != null)
            {
                AddComponentsToPrefab(singlePrefab);
            }
            else
            {
                Debug.LogWarning("No prefab or parent object selected.");
            }
        }
    }

    void AddComponentsToPrefab(GameObject prefab)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (instance != null)
        {
            Debug.Log("Adding components to prefab: " + prefab.name);
            AddRequiredComponents(instance);
            PrefabUtility.SaveAsPrefabAsset(instance, AssetDatabase.GetAssetPath(prefab));
            DestroyImmediate(instance);
            Debug.Log("Components added and prefab saved: " + prefab.name);
        }
        else
        {
            Debug.LogWarning("Failed to instantiate prefab: " + prefab.name);
        }
    }

    void UpdateComponentsInChildren(GameObject parent)
    {
        Debug.Log("Updating components in children of: " + parent.name);
        Transform interior = parent.transform.Find("Interior");
        if (interior != null)
        {
            Debug.Log("Found Interior object: " + interior.name);
            AddComponentsToLeafNodes(interior.gameObject);
        }
        else
        {
            Debug.LogWarning("Interior object not found in: " + parent.name);
        }
    }

    void AddComponentsToLeafNodes(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.childCount == 0)
            {
                Debug.Log("Adding components to leaf node: " + child.name);
                AddRequiredComponents(child.gameObject);
            }
            else
            {
                Debug.Log("Recursively checking children of: " + child.name);
                AddComponentsToLeafNodes(child.gameObject);
            }
        }
    }

    void AddRequiredComponents(GameObject obj)
    {
        if (obj.GetComponent<ConstraintManager>() == null)
        {
            obj.AddComponent<ConstraintManager>();
            Debug.Log("Added ConstraintManager to: " + obj.name);
        }
        if (obj.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            Debug.Log("Added Rigidbody (isKinematic) to: " + obj.name);
        }
        if (obj.GetComponent<MyNetworkedObject>() == null)
        {
            obj.AddComponent<MyNetworkedObject>();
            Debug.Log("Added MyNetworkedObject to: " + obj.name);
        }
        if (obj.GetComponent<CacheObjectData>() == null)
        {
            obj.AddComponent<CacheObjectData>();
            Debug.Log("Added CacheObjectData to: " + obj.name);
        }
        if (obj.GetComponent<AttachedWhiteboard>() == null)
        {
            obj.AddComponent<AttachedWhiteboard>();
            Debug.Log("Added AttachedWhiteboard to: " + obj.name);
        }

        // Use reflection to add the TetheredPlacement component
        if (obj.GetComponent("TetheredPlacement") == null)
        {
            var type = GetType("TetheredPlacement");
            if (type != null)
            {
                obj.AddComponent(type);
                Debug.Log("Added TetheredPlacement to: " + obj.name);
            }
            else
            {
                Debug.LogWarning("TetheredPlacement type not found.");
            }
        }

        if (obj.GetComponent<CustomObjectManipulator>() == null)
        {
            obj.AddComponent<CustomObjectManipulator>();
            Debug.Log("Added CustomObjectManipulator to: " + obj.name);
        }
    }

    System.Type GetType(string typeName)
    {
        var type = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .FirstOrDefault(t => t.Name == typeName);
        return type;
    }
}
