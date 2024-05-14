using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RealityFlowAPI))]
public class RealityFlowAPIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RealityFlowAPI realityFlowAPI = (RealityFlowAPI)target;

        if (GUILayout.Button("Spawn Ladder"))
        {
            realityFlowAPI.SpawnObject("Ladder", Vector3.zero, Quaternion.identity, RealityFlowAPI.SpawnScope.Peer);
        }

        if (GUILayout.Button("Spawn Tree Stump"))
        {
            realityFlowAPI.SpawnObject("TreeStump", Vector3.zero, Quaternion.identity, RealityFlowAPI.SpawnScope.Peer);
        }

        if (GUILayout.Button("Despawn Ladder"))
        {
            GameObject objectToDespawn = GameObject.Find("Ladder");
            if (objectToDespawn != null)
            {
                realityFlowAPI.DespawnObject(objectToDespawn);
            }
        }

        // Add more buttons as needed
    }
}
