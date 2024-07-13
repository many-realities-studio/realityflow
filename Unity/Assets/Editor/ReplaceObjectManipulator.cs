using UnityEngine;
using UnityEditor;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using System.IO;

public class ReplaceObjectManipulator : EditorWindow
{
    [MenuItem("Tools/Replace ObjectManipulator")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceObjectManipulator>("Replace ObjectManipulator");
    }

    private string folderPath = "Assets";

    void OnGUI()
    {
        GUILayout.Label("Replace ObjectManipulator with CustomObjectManipulator", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        if (GUILayout.Button("Replace Components"))
        {
            ReplaceComponentsInFolder(folderPath);
        }
    }

    private void ReplaceComponentsInFolder(string folder)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null)
            {
                ReplaceComponentsInPrefab(prefab);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void ReplaceComponentsInPrefab(GameObject prefab)
    {
        bool modified = false;
        foreach (Transform childTransform in prefab.GetComponentsInChildren<Transform>(true))
        {
            ObjectManipulator original = childTransform.GetComponent<ObjectManipulator>();
            if (original != null)
            {
                DestroyImmediate(original, true);
                childTransform.gameObject.AddComponent<CustomObjectManipulator>();
                modified = true;
            }
        }

        if (modified)
        {
            Debug.Log("Replaced ObjectManipulator in prefab: " + prefab.name);
        }
    }
}
