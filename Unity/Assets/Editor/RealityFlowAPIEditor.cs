using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[CustomEditor(typeof(RealityFlowAPI))]
public class RealityFlowAPIEditor : Editor
{
    private Coroutine floatAndSpinCoroutine;

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
            Debug.Log("Pressing Despawn button");
            GameObject objectToDespawn = realityFlowAPI.FindSpawnedObject("Ladder" + "(Clone)");
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
        if (GUILayout.Button("Update Ladder Transform"))
        {
            //Move the ladder to a new position, rotate it, and change its scale
            Vector3 newPosition = new Vector3(1, 1, 1);
            Quaternion newRotation = Quaternion.Euler(45, 45, 45);
            Vector3 newScale = new Vector3(1.5f, 1.5f, 1.5f);

            realityFlowAPI.UpdateObjectTransform("Ladder(Clone)", newPosition, newRotation, newScale);
            Debug.Log("Updated Ladder Transform");
        }


        // Button to test adding characterSmall prefab to the catalogue
        if (GUILayout.Button("Add prefab to Catalogue"))
        {
            string path = "Assets/Prefabs/TestObject.prefab"; // Ensure this is the correct path to your prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            //GameObject prefab = GameObject.Find("characterSmall");
            if (prefab != null)
            {
                realityFlowAPI.AddPrefabToCatalogue(prefab);
                Debug.Log("prefab added to catalogue.");
            }
            else
            {
                Debug.LogError("prefab not found at " + path);
            }
        }
        if (GUILayout.Button("Undo Last Action"))
        {
            realityFlowAPI.UndoLastAction();
            Debug.Log("Undid last action.");
        }

        // Button to test adding an in-scene game object to the catalogue
        if (GUILayout.Button("Add In-Scene GameObject to Catalogue"))
        {
            GameObject inSceneObject = GameObject.Find("characterSmall");
            if (inSceneObject != null)
            {
                realityFlowAPI.AddGameObjectToCatalogue(inSceneObject);
                Debug.Log("In-scene game object added to catalogue.");
            }
            else
            {
                Debug.LogError("In-scene game object not found.");
            }
        }
        // Button to start floating and spinning behavior
        if (GUILayout.Button("Start Floating and Spinning"))
        {
            if (floatAndSpinCoroutine == null)
            {
                floatAndSpinCoroutine = realityFlowAPI.StartCoroutine(FloatAndSpin(realityFlowAPI, "Ladder(Clone)", 1f, 45f));
                Debug.Log("Started floating and spinning Ladder.");
            }
        }

        // Button to stop floating and spinning behavior
        if (GUILayout.Button("Stop Floating and Spinning"))
        {
            if (floatAndSpinCoroutine != null)
            {
                realityFlowAPI.StopCoroutine(floatAndSpinCoroutine);
                floatAndSpinCoroutine = null;
                Debug.Log("Stopped floating and spinning Ladder.");
            }
        }
    }

    private IEnumerator FloatAndSpin(RealityFlowAPI api, string objectName, float floatSpeed, float spinSpeed)
    {
        Vector3 startPosition = api.FindSpawnedObject(objectName).transform.position;
        float elapsedTime = 0;

        while (true)
        {
            float newY = startPosition.y + Mathf.Sin(elapsedTime * floatSpeed) * 0.5f;
            Quaternion newRotation = Quaternion.Euler(0, elapsedTime * spinSpeed, 0);
            api.UpdateObjectTransform(objectName, new Vector3(startPosition.x, newY, startPosition.z), newRotation, Vector3.one);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}


