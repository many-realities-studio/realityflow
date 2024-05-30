using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Ubiq.Messaging;
using Ubiq.Spawning;
using GraphProcessor;
public class SpawnParameterAtRay : MonoBehaviour, INetworkSpawnable
{

    public GameObject runtimeGraph;
    private RealityFlowGraphView rfgv;
    private BaseGraph graph;

    public InputActionReference cancelButtonReference = null;

    private bool spawnToggle = false;

    private string currentParameter = null;

    public NetworkId NetworkId { get; set; }
    NetworkContext context;

    private int count;
    // RealityFlow Black Team (Spring 2023) addition:
    // Added the new keyword to appease error message.
    new private string name;

	// Start is called before the first frame update
	void Awake()
    {
        // cancelButtonReference.action.started += CancelSpawn;
        rfgv = runtimeGraph.GetComponent<RealityFlowGraphView>();
        graph = rfgv.graph;
        count = 0;
    }

    // Function to spawn an object at the position of the ray interactor
    public void Spawn(Vector2 spawnPoint)
    {
        // This action is technically always active, so unless the toggle is on for spawning objects just return from here
        if (!spawnToggle || currentParameter == null)
        {
            Debug.Log("No node set: " + currentParameter);
            return;
        }
        Debug.Log("Spawn Point" + spawnPoint);
        // rfgv.SetNewNodeLocation(spawnPoint);
        name = String.Format("Parameter{0}", count++);
        rfgv.AddParameterStep2(currentParameter, name);
    }
    // Function to cancel spawning of the current prefab
    public void CancelSpawn()
    {
        Debug.Log("Canceling Spawn");
        if (spawnToggle)
        {
            spawnToggle = false;
            currentParameter = null;
            Debug.Log("Node Spawn Cancelled");
        }
    }

    // Function called by the UI button in the default object library. Sets spawn toggle and prefab.
    public void RaySpawnToggle(string nodeType)
    {
        Debug.Log("Node Spawn Toggled On");
        spawnToggle = true;
        currentParameter = nodeType;
    }
}
