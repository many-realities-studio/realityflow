using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RealityFlowAPI))]
public class RealityFlowAPIEditor : Editor
{

    GameObject objectToDespawn;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RealityFlowAPI realityFlowAPI = (RealityFlowAPI)target;

        if (GUILayout.Button("Spawn Cube"))
        {
            objectToDespawn = realityFlowAPI.SpawnObject("Cube", Vector3.zero, Vector3.one, Quaternion.identity, RealityFlowAPI.SpawnScope.Room);
        }

        if (GUILayout.Button("Despawn Cube"))
        {
            Debug.Log("Pressing Despawn button");
            Debug.Log("The objectToDespawn is " + objectToDespawn);
            if (objectToDespawn != null)
            {
                realityFlowAPI.DespawnObject(objectToDespawn);
            }
            else
            {
                Debug.LogError("Object to despawn not found.");
            }
        }
    }
}
