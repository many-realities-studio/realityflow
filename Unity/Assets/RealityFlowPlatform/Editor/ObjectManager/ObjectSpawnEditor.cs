using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectSpawn))]
public class ObjectSpawnEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get reference to the target script
        ObjectSpawn objectSpawn = (ObjectSpawn)target;

        // Draw the default inspector
        DrawDefaultInspector();

        // Add a custom button to the inspector
        if (GUILayout.Button("Spawn Object"))
        {

        }
    }
}
