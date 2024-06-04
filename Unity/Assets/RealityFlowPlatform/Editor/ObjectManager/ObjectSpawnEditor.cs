using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectSpawn))]
public class ObjectSpawnEditor : Editor
{
    private string prefabName; // Field to input the prefab name
    private string modelName; // Field to input the model name

    public override void OnInspectorGUI()
    {
        // Get reference to the target script
        ObjectSpawn objectSpawn = (ObjectSpawn)target;

        // Draw the default inspector
        DrawDefaultInspector();

        // Add a text field for the prefab name
        prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);

        // Add a custom button to the inspector
        if (GUILayout.Button("Spawn Prefab"))
        {
            if (!string.IsNullOrEmpty(prefabName))
            {
                objectSpawn.SpawnObjectWithRoomScope(prefabName);
            }
            else
            {
                Debug.LogWarning("Prefab name is empty. Please enter a valid prefab name.");
            }
        }

        // Add a text field for the model name
        modelName = EditorGUILayout.TextField("Model Name", modelName);

        // Add a custom button to the inspector
        if (GUILayout.Button("Spawn Model"))
        {
            if (!string.IsNullOrEmpty(modelName))
            {
                objectSpawn.SpawnObjectWithRoomScope(modelName);
            }
            else
            {
                Debug.LogWarning("Model name is empty. Please enter a valid prefab name.");
            }
        }
    }
}
