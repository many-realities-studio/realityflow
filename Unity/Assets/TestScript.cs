using UnityEngine;
using System.Collections;
using Graph = RealityFlow.NodeGraph.Graph;
using RealityFlow.NodeGraph;

public class AddFloatAddNode
{
    public static void Execute()
    {
        string objId = "Bear";

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

        // Define spacing
        float spacing = 100.0f;

        // Position offset for spacing out the nodes in a rectangular grid pattern
        Vector2 position = new Vector2(0, 0);

        // Add new nodes to the graph and set their positions
        NodeIndex floatAddNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatAddDef);
        RealityFlowAPI.Instance.SetNodePosition(graph, floatAddNode, position);

        // Set the input constant values for the new nodes
        RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatAddNode, 0, new FloatValue(5.0f));
        RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatAddNode, 1, new FloatValue(5.0f));
    }
}