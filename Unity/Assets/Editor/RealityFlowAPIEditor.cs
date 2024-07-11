using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using RealityFlow.NodeGraph;

[CustomEditor(typeof(RealityFlowAPI))]
public class RealityFlowAPIEditor : Editor
{
    private string objectId = "668ef98ca81147bb77ec5b51";
    private RealityFlow.Collections.Arena<RealityFlow.NodeGraph.Node>.Index testNodeIndex;

    GameObject objectToDespawn;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RealityFlowAPI realityFlowAPI = RealityFlowAPI.Instance;

        if (GUILayout.Button("Spawn Bear"))
        {
            objectToDespawn = realityFlowAPI.SpawnObject("Bear", Vector3.zero, Vector3.one, Quaternion.identity, RealityFlowAPI.SpawnScope.Room);
        }

        if (GUILayout.Button("Despawn Bear"))
        {
            Debug.Log("Pressing Despawn button");
            Debug.Log("The objectToDespawn is " + objectToDespawn);
            realityFlowAPI.FindSpawnedObject("Bear");
            //Debug.Log("The objectToDespawn is " + realityFlowAPI.FindSpawnedObjectByName("Bear"));

            if (objectToDespawn != null)
            {
                realityFlowAPI.DespawnObject(objectToDespawn);
            }
            else
            {
                Debug.LogError("Object to despawn not found.");
            }
        }

        if (GUILayout.Button("Despawn Everything In The Room"))
        {
            Debug.Log("Pressing Despawn Everything button");
            DespawnAllObjects(realityFlowAPI);
        }

        // Adding the Undo button
        if (GUILayout.Button("Undo Last Action"))
        {
            Debug.Log("Pressing Undo Last Action button");
            // Assuming RealityFlowAPI has a method to handle the undo last action
            realityFlowAPI.UndoLastAction();
        }
        objectId = EditorGUILayout.TextField("Object ID", objectId);
        // Adding the Modify Node Graph button
        if (GUILayout.Button("Add Test Node"))
        {
            AddTestNode(objectId);
        }

        if (GUILayout.Button("Create and Link Nodes"))
        {
            CreateAndLinkNodes(objectId);
        }

        if (GUILayout.Button("Remove Test Node"))
        {
            RemoveTestNode(objectId);
        }

        if (GUILayout.Button("Update Node Position"))
        {
            UpdateNodePosition(objectId);
        }

        if (GUILayout.Button("Remove Edge"))
        {
            RemoveEdge(objectId);
        }

        if (GUILayout.Button("Test Node Field Values"))
        {
            TestNodeFieldValues(objectId);
        }
        if (GUILayout.Button("Graph Variable Management"))
        {
            GraphVariableManagement(objectId);
        }

        if (GUILayout.Button("Edge Cycle Detection"))
        {
            EdgeCycleDetection(objectId);
        }
        if (GUILayout.Button("Create and Link Logical Procedure"))
        {
            CreateAndLinkLogicalProcedure(objectId);
        }
        if (GUILayout.Button("Create and Link Conditional Procedure"))
        {
            CreateAndLinkConditionalProcedure(objectId);
        }
        if (GUILayout.Button("Create and Link Looping Procedure"))
        {
            CreateAndLinkLoopingProcedure(objectId);
        }



    }

    private void DespawnAllObjects(RealityFlowAPI realityFlowAPI)
    {
        List<GameObject> objectsToDespawn = new List<GameObject>(realityFlowAPI.SpawnedObjects.Keys);

        // Include peer-scoped objects
        foreach (GameObject obj in objectsToDespawn)
        {
            if (obj != null)
            {
                realityFlowAPI.DespawnObject(obj);
            }
        }
    }

    private void AddTestNode(string objId)
    {
        // Find the object
        GameObject obj = GameObject.Find(objId);

        if (obj == null)
        {
            Debug.LogError($"Object with ID {objId} not found.");
            return;
        }

        // Ensure the object has a VisualScript component
        var visualScript = obj.GetComponent<VisualScript>();
        if (visualScript == null)
        {
            Debug.LogError("VisualScript component not found on the object.");
            return;
        }

        // Get the current graph
        Graph graph = visualScript.graph;
        if (graph == null)
        {
            Debug.LogError("Graph not found on the VisualScript component.");
            return;
        }

        // Create a new node definition for FloatAdd
        NodeDefinition floatAddDef = RealityFlowAPI.Instance.NodeDefinitionDict["FloatAdd"];

        // Add a new FloatAdd node to the graph
        NodeIndex newFloatAddNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatAddDef);
        testNodeIndex = newFloatAddNode;
        // Set the field values for the new node
        RealityFlowAPI.Instance.SetNodeFieldValue(graph, newFloatAddNode, 0, new FloatValue(3.14f));
        RealityFlowAPI.Instance.SetNodeFieldValue(graph, newFloatAddNode, 1, new FloatValue(1.59f));

        Debug.Log($"Added new FloatAdd node to the graph: {newFloatAddNode}");
    }
    private void CreateAndLinkNodes(string objId)
    {
        // Find the object
        GameObject obj = GameObject.Find(objId);

        if (obj == null)
        {
            Debug.LogError($"Object with ID {objId} not found.");
            return;
        }

        // Ensure the object has a VisualScript component
        var visualScript = obj.GetComponent<VisualScript>();
        if (visualScript == null)
        {
            Debug.LogError("VisualScript component not found on the object.");
            return;
        }

        // Get the current graph
        Graph graph = visualScript.graph;
        if (graph == null)
        {
            Debug.LogError("Graph not found on the VisualScript component.");
            return;
        }

        // Create new node definitions
        NodeDefinition floatAddDef = RealityFlowAPI.Instance.NodeDefinitionDict["FloatAdd"];
        NodeDefinition floatSubtractDef = RealityFlowAPI.Instance.NodeDefinitionDict["FloatSubtract"];

        // Add new nodes to the graph
        NodeIndex floatAddNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatAddDef);
        NodeIndex floatSubtractNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatSubtractDef);

        // Set the field values for the new nodes
        RealityFlowAPI.Instance.SetNodeFieldValue(graph, floatAddNode, 0, new FloatValue(3.14f));
        RealityFlowAPI.Instance.SetNodeFieldValue(graph, floatAddNode, 1, new FloatValue(1.59f));

        RealityFlowAPI.Instance.SetNodeFieldValue(graph, floatSubtractNode, 0, new FloatValue(2.71f));
        RealityFlowAPI.Instance.SetNodeFieldValue(graph, floatSubtractNode, 1, new FloatValue(1.41f));

        // Create connections (edges) between the nodes
        PortIndex fromPort1 = new PortIndex(floatAddNode, 0); // Output of FloatAdd node
        PortIndex toPort1 = new PortIndex(floatSubtractNode, 0); // Input of FloatSubtract node

        RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, fromPort1, toPort1);

        Debug.Log($"Added new FloatAdd node to the graph: {floatAddNode}");
        Debug.Log($"Added new FloatSubtract node to the graph: {floatSubtractNode}");
        Debug.Log($"Linked FloatAdd node to FloatSubtract node.");
    }
    private void RemoveTestNode(string objId)
    {
        // Find the object
        GameObject obj = GameObject.Find(objId);

        if (obj == null)
        {
            Debug.LogError($"Object with ID {objId} not found.");
            return;
        }

        // Ensure the object has a VisualScript component
        var visualScript = obj.GetComponent<VisualScript>();
        if (visualScript == null)
        {
            Debug.LogError("VisualScript component not found on the object.");
            return;
        }

        // Get the current graph
        Graph graph = visualScript.graph;
        if (graph == null)
        {
            Debug.LogError("Graph not found on the VisualScript component.");
            return;
        }

        // Ensure the node to remove exists in the graph
        if (!graph.Nodes.Any(n => n.Key.Equals(testNodeIndex)))
        {
            Debug.LogError($"Node {testNodeIndex} not found in the graph.");
            return;
        }

        // Remove the node from the graph
        RealityFlowAPI.Instance.RemoveNodeFromGraph(graph, testNodeIndex);

        Debug.Log($"Removed node {testNodeIndex} from the graph.");
    }

    private void UpdateNodePosition(string objId)
    {
        // Find the object
        GameObject obj = GameObject.Find(objId);

        if (obj == null)
        {
            Debug.LogError($"Object with ID {objId} not found.");
            return;
        }

        // Ensure the object has a VisualScript component
        var visualScript = obj.GetComponent<VisualScript>();
        if (visualScript == null)
        {
            Debug.LogError("VisualScript component not found on the object.");
            return;
        }

        // Get the current graph
        Graph graph = visualScript.graph;
        if (graph == null)
        {
            Debug.LogError("Graph not found on the VisualScript component.");
            return;
        }

        // Assume we want to update the position of the node with index 1 for testing
        RealityFlow.Collections.Arena<RealityFlow.NodeGraph.Node>.Index nodeToUpdate = new RealityFlow.Collections.Arena<RealityFlow.NodeGraph.Node>.Index(1);
        Vector2 newPosition = new Vector2(2.0f, 2.0f);

        // Update the position of the node
        RealityFlowAPI.Instance.SetNodePosition(graph, testNodeIndex, newPosition);

        Debug.Log($"Updated position of node {testNodeIndex} to {newPosition}.");
    }
    private void RemoveEdge(string objId)
    {
        // Find the object
        GameObject obj = GameObject.Find(objId);

        if (obj == null)
        {
            Debug.LogError($"Object with ID {objId} not found.");
            return;
        }

        // Ensure the object has a VisualScript component
        var visualScript = obj.GetComponent<VisualScript>();
        if (visualScript == null)
        {
            Debug.LogError("VisualScript component not found on the object.");
            return;
        }

        // Get the current graph
        Graph graph = visualScript.graph;
        if (graph == null)
        {
            Debug.LogError("Graph not found on the VisualScript component.");
            return;
        }

        // Assume we want to remove an edge between node 1 output port 0 and node 2 input port 0 for testing
        RealityFlow.Collections.Arena<RealityFlow.NodeGraph.Node>.Index node1 = new RealityFlow.Collections.Arena<RealityFlow.NodeGraph.Node>.Index(1);
        RealityFlow.Collections.Arena<RealityFlow.NodeGraph.Node>.Index node2 = new RealityFlow.Collections.Arena<RealityFlow.NodeGraph.Node>.Index(2);

        PortIndex fromPort = new PortIndex(node1, 0);
        PortIndex toPort = new PortIndex(node2, 0);

        // Remove the edge
        RealityFlowAPI.Instance.RemoveDataEdgeFromGraph(graph, fromPort, toPort);

        Debug.Log($"Removed edge from node 1 port 0 to node 2 port 0.");
    }
    private void TestNodeFieldValues(string objId)
    {
        // Find the object
        GameObject obj = GameObject.Find(objId);

        if (obj == null)
        {
            Debug.LogError($"Object with ID {objId} not found.");
            return;
        }

        // Ensure the object has a VisualScript component
        var visualScript = obj.GetComponent<VisualScript>();
        if (visualScript == null)
        {
            Debug.LogError("VisualScript component not found on the object.");
            return;
        }

        // Get the current graph
        Graph graph = visualScript.graph;
        if (graph == null)
        {
            Debug.LogError("Graph not found on the VisualScript component.");
            return;
        }

        // Create a new node definition for FloatAdd
        NodeDefinition floatAddDef = RealityFlowAPI.Instance.NodeDefinitionDict["FloatAdd"];

        // Add a new FloatAdd node to the graph
        NodeIndex newFloatAddNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatAddDef);

        // Test setting different types of field values
        SetNodeFieldValueSafe(graph, testNodeIndex, 0, new FloatValue(3.14f));
        SetNodeFieldValueSafe(graph, testNodeIndex, 1, new FloatValue(1.59f));
        SetNodeFieldValueSafe(graph, testNodeIndex, 2, new IntValue(42));
        SetNodeFieldValueSafe(graph, testNodeIndex, 3, new BoolValue(true));

        Debug.Log($"Set various field values for node {newFloatAddNode}.");
    }

    private void SetNodeFieldValueSafe(Graph graph, NodeIndex nodeIndex, int fieldIndex, NodeValue value)
    {
        try
        {
            RealityFlowAPI.Instance.SetNodeFieldValue(graph, nodeIndex, fieldIndex, value);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to set node field value: {e.Message}");
        }
    }
    private void GraphVariableManagement(string objId)
    {
        // Find the object
        GameObject obj = GameObject.Find(objId);

        if (obj == null)
        {
            Debug.LogError($"Object with ID {objId} not found.");
            return;
        }

        // Ensure the object has a VisualScript component
        var visualScript = obj.GetComponent<VisualScript>();
        if (visualScript == null)
        {
            Debug.LogError("VisualScript component not found on the object.");
            return;
        }

        // Get the current graph
        Graph graph = visualScript.graph;
        if (graph == null)
        {
            Debug.LogError("Graph not found on the VisualScript component.");
            return;
        }

        // Add, update, and remove variables in the graph
        RealityFlowAPI.Instance.AddVariableToGraph(graph, "testVariable", NodeValueType.Float);
        RealityFlowAPI.Instance.SetNodeFieldValue(graph, new RealityFlow.Collections.Arena<RealityFlow.NodeGraph.Node>.Index(1), 0, new FloatValue(3.14f));
        RealityFlowAPI.Instance.RemoveVariableFromGraph(graph, "testVariable");

        Debug.Log($"Managed variables in the graph.");
    }
    private void EdgeCycleDetection(string objId)
    {
        // Find the object
        GameObject obj = GameObject.Find(objId);

        if (obj == null)
        {
            Debug.LogError($"Object with ID {objId} not found.");
            return;
        }

        // Ensure the object has a VisualScript component
        var visualScript = obj.GetComponent<VisualScript>();
        if (visualScript == null)
        {
            Debug.LogError("VisualScript component not found on the object.");
            return;
        }

        // Get the current graph
        Graph graph = visualScript.graph;
        if (graph == null)
        {
            Debug.LogError("Graph not found on the VisualScript component.");
            return;
        }

        // Add nodes and try to create a cycle
        NodeDefinition floatAddDef = RealityFlowAPI.Instance.NodeDefinitionDict["FloatAdd"];
        NodeIndex node1 = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatAddDef);
        NodeIndex node2 = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatAddDef);

        // Link the two nodes together to form a cycle
        PortIndex fromPort1 = new PortIndex(node1, 0);
        PortIndex toPort1 = new PortIndex(node2, 0);
        RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, fromPort1, toPort1);

        PortIndex fromPort2 = new PortIndex(node2, 0);
        PortIndex toPort2 = new PortIndex(node1, 0);
        RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, fromPort2, toPort2);

        Debug.Log("Created a cycle in the graph for testing edge cycle detection.");
    }
    private void CreateAndLinkLogicalProcedure(string objId)
    {
        // Find the object
        GameObject obj = GameObject.Find(objId);

        if (obj == null)
        {
            Debug.LogError($"Object with ID {objId} not found.");
            return;
        }

        // Ensure the object has a VisualScript component
        var visualScript = obj.GetComponent<VisualScript>();
        if (visualScript == null)
        {
            Debug.LogError("VisualScript component not found on the object.");
            return;
        }

        // Get the current graph
        Graph graph = visualScript.graph;
        if (graph == null)
        {
            Debug.LogError("Graph not found on the VisualScript component.");
            return;
        }

        // Create new node definitions
        NodeDefinition floatAddDef = RealityFlowAPI.Instance.NodeDefinitionDict["FloatAdd"];
        NodeDefinition floatMultiplyDef = RealityFlowAPI.Instance.NodeDefinitionDict["FloatMultiply"];

        // Add new nodes to the graph
        NodeIndex floatAddNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatAddDef);
        NodeIndex floatMultiplyNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatMultiplyDef);

        // Set the field values for the new nodes
        RealityFlowAPI.Instance.SetNodeFieldValue(graph, floatAddNode, 0, new FloatValue(2.0f));
        RealityFlowAPI.Instance.SetNodeFieldValue(graph, floatAddNode, 1, new FloatValue(3.0f));
        RealityFlowAPI.Instance.SetNodeFieldValue(graph, floatMultiplyNode, 1, new FloatValue(4.0f));

        // Create connections (edges) between the nodes
        PortIndex addOutputPort = new PortIndex(floatAddNode, 0); // Output of FloatAdd node
        PortIndex multiplyInputPort = new PortIndex(floatMultiplyNode, 0); // First input of FloatMultiply node

        RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, addOutputPort, multiplyInputPort);

        Debug.Log($"Added and linked nodes for logical procedure in the graph.");
    }

    private void CreateAndLinkConditionalProcedure(string objId)
    {
        // Find the object
        GameObject obj = GameObject.Find(objId);

        if (obj == null)
        {
            Debug.LogError($"Object with ID {objId} not found.");
            return;
        }

        // Ensure the object has a VisualScript component
        var visualScript = obj.GetComponent<VisualScript>();
        if (visualScript == null)
        {
            Debug.LogError("VisualScript component not found on the object.");
            return;
        }

        // Get the current graph
        Graph graph = visualScript.graph;
        if (graph == null)
        {
            Debug.LogError("Graph not found on the VisualScript component.");
            return;
        }

        // Create new node definitions
        NodeDefinition floatGreaterOrEqualDef = RealityFlowAPI.Instance.NodeDefinitionDict["IntGreaterOrEqual"];
        NodeDefinition setPositionDef = RealityFlowAPI.Instance.NodeDefinitionDict["SetPosition"];
        NodeDefinition thisObjectDef = RealityFlowAPI.Instance.NodeDefinitionDict["ThisObject"];
        NodeDefinition vector3Def = RealityFlowAPI.Instance.NodeDefinitionDict["Vector3 Right"];

        // Add new nodes to the graph
        NodeIndex greaterOrEqualNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatGreaterOrEqualDef);
        NodeIndex setPositionNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, setPositionDef);
        NodeIndex thisObjectNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, thisObjectDef);
        NodeIndex vector3Node = RealityFlowAPI.Instance.AddNodeToGraph(graph, vector3Def);

        // Set the field values for the new nodes
        //RealityFlowAPI.Instance.SetNodeFieldValue(graph, greaterOrEqualNode, 0, new IntValue(5));
        //RealityFlowAPI.Instance.SetNodeFieldValue(graph, greaterOrEqualNode, 1, new IntValue(10));

        // Create connections (edges) between the nodes
        PortIndex thisObjectOutput = new PortIndex(thisObjectNode, 0);
        PortIndex vector3Output = new PortIndex(vector3Node, 0);
        PortIndex conditionOutput = new PortIndex(greaterOrEqualNode, 0);

        PortIndex setPositionTarget = new PortIndex(setPositionNode, 0);
        PortIndex setPositionValue = new PortIndex(setPositionNode, 1);
        PortIndex conditionInput = new PortIndex(setPositionNode, 0);

        RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, thisObjectOutput, setPositionTarget);
        RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, vector3Output, setPositionValue);
        RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, conditionOutput, conditionInput);

        Debug.Log($"Added and linked nodes for conditional procedure in the graph.");
    }

    private void CreateAndLinkLoopingProcedure(string objId)
    {
        // Find the object
        GameObject obj = GameObject.Find(objId);

        if (obj == null)
        {
            Debug.LogError($"Object with ID {objId} not found.");
            return;
        }

        // Ensure the object has a VisualScript component
        var visualScript = obj.GetComponent<VisualScript>();
        if (visualScript == null)
        {
            Debug.LogError("VisualScript component not found on the object.");
            return;
        }

        // Get the current graph
        Graph graph = visualScript.graph;
        if (graph == null)
        {
            Debug.LogError("Graph not found on the VisualScript component.");
            return;
        }

        // Create new node definitions
        NodeDefinition intAddDef = RealityFlowAPI.Instance.NodeDefinitionDict["IntAdd"];
        NodeDefinition thisObjectDef = RealityFlowAPI.Instance.NodeDefinitionDict["ThisObject"];
        NodeDefinition setPositionDef = RealityFlowAPI.Instance.NodeDefinitionDict["SetPosition"];
        NodeDefinition vector3Def = RealityFlowAPI.Instance.NodeDefinitionDict["Vector3 Right"];

        // Add new nodes to the graph
        NodeIndex intAddNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, intAddDef);
        NodeIndex thisObjectNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, thisObjectDef);
        NodeIndex setPositionNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, setPositionDef);
        NodeIndex vector3Node = RealityFlowAPI.Instance.AddNodeToGraph(graph, vector3Def);

        // Set the field values for the new nodes
        //RealityFlowAPI.Instance.SetNodeFieldValue(graph, intAddNode, 0, new IntValue(0));
        //RealityFlowAPI.Instance.SetNodeFieldValue(graph, intAddNode, 1, new IntValue(1));

        // Create connections (edges) between the nodes
        PortIndex thisObjectOutput = new PortIndex(thisObjectNode, 0);
        PortIndex vector3Output = new PortIndex(vector3Node, 0);
        PortIndex intAddOutput = new PortIndex(intAddNode, 0);

        PortIndex setPositionTarget = new PortIndex(setPositionNode, 0);
        PortIndex setPositionValue = new PortIndex(setPositionNode, 1);

        RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, thisObjectOutput, setPositionTarget);
        RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, vector3Output, setPositionValue);
        RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, intAddOutput, setPositionValue);

        Debug.Log($"Added and linked nodes for looping procedure in the graph.");
    }





}
