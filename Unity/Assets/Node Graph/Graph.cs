using System.Collections.Generic;
using RealityFlow.Collections;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    public class Graph 
    {
        [SerializeField]
        List<SerRef<Node>> Roots = new();
        /// <summary>
        /// Forward data (non-execution) edges (output -> input)
        /// </summary>
        [SerializeField]
        SerializableDict<PortIndex, PortIndex> Edges = new();
        /// <summary>
        /// Backwards data (non-execution) edges (input -> output)
        /// </summary>
        [SerializeField]
        SerializableDict<PortIndex, PortIndex> ReverseEdges = new();
        /// <summary>
        /// Forward execution edges (output -> input)
        /// </summary>
        [SerializeField]
        MultiValueDictionary<PortIndex, Node> ExecutionEdges = new();

        public void EvaluateRoot(int root)
        {
            Queue<Node> nodeQueue = new();
            Dictionary<PortIndex, object> valueCache = new();
            EvalContext ctx = new(this, nodeQueue, valueCache);
            Roots[root].Value.Evaluate(ctx);
        }

        public Node AddRoot(Node root)
        {
            Roots.Add(root);
            return root;
        }

        public void AddEdge(Node from, int fromPort, Node to, int toPort)
        {
            Edges.Add(new(from, fromPort), new(to, toPort));
            ReverseEdges.Add(new(to, toPort), new(from, fromPort));
        }

        public void AddExecutionEdge(Node from, int fromPort, Node to)
        {
            ExecutionEdges.Add(new(from, fromPort), to);
        }

        public bool TryGetOutputPortOf(PortIndex inputPort, out PortIndex outputPort)
            => ReverseEdges.TryGetValue(inputPort, out outputPort);

        public bool GetInputPortOf(PortIndex outputPort, out PortIndex inputPort)
            => Edges.TryGetValue(outputPort, out inputPort);

        public List<Node> GetExecutionInputPortsOf(PortIndex outputPort)
            => ExecutionEdges[outputPort];
    }
}