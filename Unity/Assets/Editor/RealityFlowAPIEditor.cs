using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            realityFlowAPI.SpawnObject("Ladder", Vector3.zero, Vector3.one, Quaternion.identity, RealityFlowAPI.SpawnScope.Peer);
        }

        if (GUILayout.Button("Spawn Tree Stump"))
        {
            realityFlowAPI.SpawnObject("TreeStump", Vector3.zero, Vector3.zero, Quaternion.identity, RealityFlowAPI.SpawnScope.Peer);
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
            //Debug.Log("Undid last action.");
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
                floatAndSpinCoroutine = realityFlowAPI.StartCoroutine(FloatAndSpin(realityFlowAPI, "Ladder(Clone)", 1f, 45f, 5f));
                Debug.Log("Started floating and spinning Ladder.");
            }
        }

        // Button to stop floating and spinning behavior
        if (GUILayout.Button("Stop Floating and Spinning"))
        {
            if (floatAndSpinCoroutine != null)
            {
                //realityFlowAPI.StopAllCoroutines();
                realityFlowAPI.StopCoroutine(floatAndSpinCoroutine);
                floatAndSpinCoroutine = null;
                Debug.Log("Stopped floating and spinning Ladder.");
            }
        }
        if (GUILayout.Button("Scale and Move"))
        {
            realityFlowAPI.StartCoroutine(ScaleAndMove(realityFlowAPI, "Ladder(Clone)", new Vector3(2.0f, 2.0f, 2.0f), new Vector3(2.0f, 2.0f, 2.0f), 5.0f)); // Move to (2, 2, 2) and scale to (2, 2, 2) over 5 seconds
        }
        if (GUILayout.Button("Move characterSmall"))
        {
            MoveCharacterSmall(realityFlowAPI);
        }


    }

    private IEnumerator FloatAndSpin(RealityFlowAPI api, string objectName, float floatSpeed, float spinSpeed, float duration)
    {
        api.StartCompoundAction();

        Vector3 startPosition = api.FindSpawnedObject(objectName).transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float newY = startPosition.y + Mathf.Sin(elapsedTime * floatSpeed) * 0.5f;
            Quaternion newRotation = Quaternion.Euler(0, elapsedTime * spinSpeed, 0);
            api.UpdateObjectTransform(objectName, new Vector3(startPosition.x, newY, startPosition.z), newRotation, Vector3.one);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        api.EndCompoundAction();
    }

    private IEnumerator ScaleAndMove(RealityFlowAPI api, string objectName, Vector3 targetPosition, Vector3 targetScale, float duration)
    {
        api.StartCompoundAction();

        GameObject obj = api.FindSpawnedObject(objectName);
        if (obj == null)
        {
            Debug.LogError($"Object named {objectName} not found.");
            yield break;
        }

        Vector3 startPosition = obj.transform.position;
        Vector3 startScale = obj.transform.localScale;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, t);
            Vector3 newScale = Vector3.Lerp(startScale, targetScale, t);

            api.UpdateObjectTransform(objectName, newPosition, obj.transform.rotation, newScale);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position and scale are set
        api.UpdateObjectTransform(objectName, targetPosition, obj.transform.rotation, targetScale);

        api.EndCompoundAction();
    }
    private async Task BuildHouse(RealityFlowAPI api)
    {
        api.StartCompoundAction();

        // Create and place the walls
        GameObject wall1 = await api.SpawnObject("Climbing Wall", new Vector3(0, 0, 0), new Vector3(1, 1, 1), Quaternion.identity, RealityFlowAPI.SpawnScope.Peer);
        GameObject wall2 = await api.SpawnObject("Climbing Wall", new Vector3(0, 0, 2), new Vector3(1, 1, 1), Quaternion.Euler(0, 90, 0), RealityFlowAPI.SpawnScope.Peer);
        GameObject wall3 = await api.SpawnObject("Climbing Wall", new Vector3(2, 0, 2), new Vector3(1, 1, 1), Quaternion.Euler(0, 180, 0), RealityFlowAPI.SpawnScope.Peer);
        GameObject wall4 = await api.SpawnObject("Climbing Wall", new Vector3(2, 0, 0), new Vector3(1, 1, 1), Quaternion.Euler(0, -90, 0), RealityFlowAPI.SpawnScope.Peer);


        // Create and place the roof
        GameObject roof = await api.SpawnObject("Climbing Wall", new Vector3(1, 2, 1), new Vector3(1, 1, 1), Quaternion.Euler(90, 0, 0), RealityFlowAPI.SpawnScope.Peer);

        // Adjust the size of the walls to make them fit the structure
        await api.UpdateObjectTransform(wall1.name, new Vector3(0, 0, 0), Quaternion.identity, new Vector3(2, 2, 1));
        await api.UpdateObjectTransform(wall2.name, new Vector3(0, 0, 2), Quaternion.Euler(0, 90, 0), new Vector3(2, 2, 1));
        await api.UpdateObjectTransform(wall3.name, new Vector3(2, 0, 2), Quaternion.Euler(0, 180, 0), new Vector3(2, 2, 1));
        await api.UpdateObjectTransform(wall4.name, new Vector3(2, 0, 0), Quaternion.Euler(0, -90, 0), new Vector3(2, 2, 1));
        await api.UpdateObjectTransform(roof.name, new Vector3(1, 2, 1), Quaternion.Euler(90, 0, 0), new Vector3(2, 2, 2));

        api.EndCompoundAction();
    }
    private async void MoveCharacterSmall(RealityFlowAPI api)
    {
        GameObject characterSmall = await api.SpawnObject("characterSmall", Vector3.zero, Vector3.one, Quaternion.identity, RealityFlowAPI.SpawnScope.Peer);
        ;
        if (characterSmall != null)
        {
            Vector3 newPosition = new Vector3(5.0f, 2.0f, 3.0f); // Define the new position
            Quaternion newRotation = Quaternion.Euler(0, 90, 0); // Define the new rotation
            Vector3 newScale = new Vector3(1.5f, 1.5f, 1.5f); // Define the new scale

            api.StartCompoundAction();
            await api.UpdateObjectTransform("characterSmall", newPosition, newRotation, newScale);
            api.EndCompoundAction();

            Debug.Log("Moved characterSmall to new position.");
        }
        else
        {
            Debug.LogError("characterSmall not found in the scene.");
        }
    }

}


