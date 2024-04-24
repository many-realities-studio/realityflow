using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using Ubiq.Messaging;
using Ubiq.Spawning;
public class SpawnNodeAtRay : MonoBehaviour
{
    
    [Header("References to cursor")]
    [Tooltip("this is a cursor")]
    public TrackCursor cursor;
    public RealityFlowGraphView rfgv;
    private BaseGraph graph;
    private bool spawnToggle = false;
    private string currentNode = null;

    [Header("References for object spawning")]
    public string ObjectName;
    public GameObject spawnManager;

    public NetworkId NetworkId { get; set; }
    NetworkContext context;

	// Start is called before the first frame update
    void Start()
    {
        spawnManager = GameObject.Find("Spawn Manager");
    }
	void Awake()
    {
        graph = rfgv.graph;
    }

    // Function to spawn an object at the position of the ray interactor
    public void Spawn()
    {
        // This action is technically always active, so unless the toggle is on for spawning objects just return from here
        if (!spawnToggle || currentNode == null)
            return;

        // Don't spawn the node if the player is not pointing at a valid location on the board
        if (!cursor.isHovering)
        {
            Debug.Log("Failed to spawn node: cursor is not hovering whiteboard");
            return;
        }
        // Get last selected object's name
        Debug.Log(this.gameObject);
        if(spawnManager.GetComponent<GetObjectName>().aName != null)
        {
            ObjectName = spawnManager.GetComponent<GetObjectName>().aName;
        }
        // Retrieve the cursor position from the TrackCursor script attached to GraphContent on the whiteboard
        rfgv.newNodePosition = cursor.cursorPosition;

        // Add AddNodeCommandthe node to the whiteboard
        rfgv.AddNodeCommand(currentNode, ObjectName);
        Debug.Log("Spawned " + currentNode + " node");
    }

    // Function to cancel spawning of the current prefab
    public void CancelSpawn()
    {
        if (spawnToggle)
        {
            Debug.Log("Node spawn cancelled: no longer spawning " + currentNode);
            spawnToggle = false;
            currentNode = null;
        }
    }

    // Function called by the UI button in the default object library. Sets spawn toggle and prefab.
    public void RaySpawnToggle(string nodeType)
    {
        Debug.Log("Spawning toggled on for node: " + nodeType);
        spawnToggle = true;
        currentNode = nodeType;
    }
}
