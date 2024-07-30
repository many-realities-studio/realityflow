using UnityEngine;
using System.Collections;
using Graph = RealityFlow.NodeGraph.Graph;
using RealityFlow.NodeGraph;

public class SpawnAndManipulate : MonoBehaviour
{
    public static void Execute()
    {
        // Spawn a Cube object with the specified position
        RealityFlowAPI.Instance.SpawnObject("Cube", new Vector3(-0.95f, -0.05f, 2.19f), Vector3.one, Quaternion.identity, RealityFlowAPI.SpawnScope.Room);

        // Create a node graph for the spawned object
        CreateComprehensiveGraphProcedure("Cube", 100.0f);
    }

    private static void CreateComprehensiveGraphProcedure(string objId, float spacing)
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
        NodeDefinition setPositionDef = RealityFlowAPI.Instance.NodeDefinitionDict["SetPosition"];
        NodeDefinition thisObjectDef = RealityFlowAPI.Instance.NodeDefinitionDict["ThisObject"];
        NodeDefinition vector3Def = RealityFlowAPI.Instance.NodeDefinitionDict["Vector3 Right"];
        
        // Define positions for the nodes using the defined spacing
        Vector2[] positions = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(spacing, 0),
            new Vector2(0, spacing),
            new Vector2(spacing, spacing),
            new Vector2(-spacing, 0),
            new Vector2(0, -spacing)
        };

        // Add new nodes to the graph and set their positions
        NodeIndex floatAddNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatAddDef);
        RealityFlowAPI.Instance.SetNodePosition(graph, floatAddNode, positions[0]);

        NodeIndex setPositionNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, setPositionDef);
        RealityFlowAPI.Instance.SetNodePosition(graph, setPositionNode, positions[1]);

        NodeIndex thisObjectNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, thisObjectDef);
        RealityFlowAPI.Instance.SetNodePosition(graph, thisObjectNode, positions[2]);

        NodeIndex vector3Node = RealityFlowAPI.Instance.AddNodeToGraph(graph, vector3Def);
        RealityFlowAPI.Instance.SetNodePosition(graph, vector3Node, positions[3]);

        // Set the input constant values for the new nodes
        RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatAddNode, 0, new FloatValue(2.0f));
        RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatAddNode, 1, new FloatValue(3.0f));

        // Create edges between the nodes
        PortIndex thisObjectOutput = new PortIndex(thisObjectNode, 0);
        PortIndex setPositionTarget = new PortIndex(setPositionNode, 0);
        PortIndex floatAddOutput = new PortIndex(floatAddNode, 0);

        RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, thisObjectOutput, setPositionTarget);
        RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, floatAddOutput, setPositionTarget);
        
        Debug.Log($"Added and linked nodes for procedure in the graph.");
    }
}