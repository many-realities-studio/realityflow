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

        if (GUILayout.Button("Spawn Cube"))
        {
            // Assuming that the prefab is at index 1
            var cubePrefab = manager.catalogue.prefabs[0];
            manager.SpawnWithPeerScope(cubePrefab);
        }

        if (GUILayout.Button("Spawn Tree"))
        {
            // Assuming that the prefab is at index 0
            var treePrefab = manager.catalogue.prefabs[1];
            manager.SpawnWithPeerScope(treePrefab);
        }


    }
}