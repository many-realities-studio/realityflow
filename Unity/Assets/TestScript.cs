using UnityEngine;
using System.Collections;
using Graph = RealityFlow.NodeGraph.Graph;
using RealityFlow.NodeGraph;

public class AddImpulseNode : MonoBehaviour 
{
    public static void Execute(string objId) 
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

        // Create a node definition for the Impulse node
        NodeDefinition impulseDef = RealityFlowAPI.Instance.NodeDefinitionDict["Impulse"];

        // Add the Impulse node to the graph
        NodeIndex impulseNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, impulseDef);
        
        // Set the position of the new Impulse node
        RealityFlowAPI.Instance.SetNodePosition(graph, impulseNode, new Vector2(0, 0));

        // Note: Adjust the position as needed.

        Debug.Log("Impulse node added to the graph.");
    }
}