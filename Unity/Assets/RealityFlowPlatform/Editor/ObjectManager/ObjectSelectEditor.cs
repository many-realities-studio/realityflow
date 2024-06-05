using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectSelect))]
public class ObjectManagerEditor : Editor
{
    SerializedProperty objectId;
    SerializedProperty outlineMaterial;

    void OnEnable()
    {
        // Find the serialized properties
        objectId = serializedObject.FindProperty("objectId");
        outlineMaterial = serializedObject.FindProperty("outlineMaterial");
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();

        // Draw the Object ID text field
        EditorGUILayout.PropertyField(objectId, new GUIContent("Object ID"));

        // Draw the Outline Material field
        EditorGUILayout.PropertyField(outlineMaterial, new GUIContent("Outline Material"));

        // Apply any modifications to the serialized properties
        serializedObject.ApplyModifiedProperties();

        // Get the ObjectSelect script
        ObjectSelect objectSelect = (ObjectSelect)target;

        // Create a button that calls the SelectAndOutlineObject method
        if (GUILayout.Button("Select Object"))
        {
            objectSelect.SelectAndOutlineObject(objectSelect.GetObjectId());
        }
        
        // Create a button that calls the deleteSelectedObject method
        if (GUILayout.Button("Delete Object"))
        {
            objectSelect.DeleteSelectedObject();
        }
    }
}