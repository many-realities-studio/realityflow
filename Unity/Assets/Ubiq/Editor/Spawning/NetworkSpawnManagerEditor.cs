using UnityEditor;
using UnityEngine;
using Ubiq.Spawning;

[CustomEditor(typeof(NetworkSpawnManager))]
public class NetworkSpawnManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NetworkSpawnManager manager = (NetworkSpawnManager)target;
    }
}