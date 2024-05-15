using System;
using UnityEngine;

namespace RealityFlow.NodeGraph.Testing
{
    public class TestCase01 : MonoBehaviour
    {
        public NodeDefinition OnInteract;
        public NodeDefinition Add;
        public NodeDefinition Integer;
        public NodeDefinition Print;

        NodeIndex start;

        public Graph ConstructGraph()
        {
            Graph graph = new();
            start = graph.AddNode(OnInteract);
            NodeIndex print = graph.AddNode(Print);
            graph.AddExecutionEdge(start, 0, print);
            NodeIndex twelve = graph.AddNode(Integer);
            graph.GetNode(twelve).TrySetField(0, 12);
            graph.AddEdge(twelve, 0, print, 0);

            return graph;
        }

        public void Start()
        {
            Graph graph = ConstructGraph();
            graph.EvaluateFromRoot(start);

            string json = JsonUtility.ToJson(graph, true);
            Debug.Log(json);
        }
    }
}