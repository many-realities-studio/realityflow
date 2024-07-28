using UnityEngine;
using System.Collections;
using Graph = RealityFlow.NodeGraph.Graph;
using RealityFlow.NodeGraph;

public class SpawnAndSetupCube
{
    public static void Execute()
    {
        // Spawn a cube at the specified position
        Vector3 spawnPosition = new Vector3(-2.60f, -0.05f, 1.88f);
        GameObject cube = RealityFlowAPI.Instance.SpawnObject("Cube", spawnPosition, Vector3.one, Quaternion.identity, RealityFlowAPI.SpawnScope.Room);
        cube.tag = "Spawned";

        // Creating a node graph for the spawned cube
        CreateNodeGraphForCube(cube.name);
    }

    private static void CreateNodeGraphForCube(string objId)
    {
        // Create a new node graph
        Graph graph = RealityFlowAPI.Instance.CreateNodeGraphAsync();

        // Assign the graph to the cube
        RealityFlowAPI.Instance.AssignGraph(graph, GameObject.Find(objId));

        // Create node definitions
        NodeDefinition floatAddDef = RealityFlowAPI.Instance.NodeDefinitionDict["FloatAdd"];
        NodeDefinition setFloatVariableDef = RealityFlowAPI.Instance.NodeDefinitionDict["SetFloatVariable"];
        
        // Define spacing for nodes
        float spacing = 100.0f;
        Vector2[] positions = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(spacing, 0),
        };

        // Add nodes to the graph
        NodeIndex floatAddNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatAddDef);
        RealityFlowAPI.Instance.SetNodePosition(graph, floatAddNode, positions[0]);

        NodeIndex setFloatVariableNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, setFloatVariableDef);
        RealityFlowAPI.Instance.SetNodePosition(graph, setFloatVariableNode, positions[1]);

        // Set the input constant values for the FloatAdd node
        RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatAddNode, 0, new FloatValue(5.0f));
        RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatAddNode, 1, new FloatValue(5.0f));
        
        // Create connections (edges) between the nodes
        PortIndex addOutputPort = new PortIndex(floatAddNode, 0); // Output of FloatAdd node
        PortIndex setFloatInputPort = new PortIndex(setFloatVariableNode, 0); // Input of SetFloatVariable node
        RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, addOutputPort, setFloatInputPort);
    }
}