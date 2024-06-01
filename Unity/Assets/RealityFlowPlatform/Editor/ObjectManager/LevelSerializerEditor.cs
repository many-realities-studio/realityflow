using UnityEditor;
using UnityEngine;
using RealityFlowPlatform;

[CustomEditor(typeof(LevelSerializer))]
public class LevelSerializerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector layout

        LevelSerializer levelSerializer = (LevelSerializer)target;

        if (GUILayout.Button("Send Objects to Database"))
        {
            string jsonData = levelSerializer.SerializeLevelToJson();
            Debug.Log(jsonData);
        }

        if (GUILayout.Button("Retrieve Objects from Database "))
        {
            string jsonData = levelSerializer.SerializeLevelToJson(); // For testing, using the same serialization data
            levelSerializer.DeserializeJsonToLevel(jsonData);
        }
    }
}
