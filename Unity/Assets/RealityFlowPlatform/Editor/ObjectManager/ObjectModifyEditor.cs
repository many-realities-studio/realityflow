using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectEdit))]
public class ObjectEditEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get reference to the target script
        ObjectEdit objectEdit = (ObjectEdit)target;

        // Draw the default inspector
        DrawDefaultInspector();

        // Add a custom button to the inspector
        if (GUILayout.Button("Select Object"))
        {


        }
    }
}
