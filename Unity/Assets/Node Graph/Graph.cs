using System;
using System.Collections.Generic;
using RealityFlow.Collections;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class Graph
    {
        List<Node> Roots = new();
        /// <summary>
        /// Forward data (non-execution) edges (output -> input)
        /// </summary>
        Dictionary<(Node, int), (Node, int)> Edges = new();
        /// <summary>
        /// Backwards data (non-execution) edges (input -> output)
        /// </summary>
        Dictionary<(Node, int), (Node, int)> ReverseEdges = new();
        /// <summary>
        /// Forward execution edges (output -> input)
        /// </summary>
        MultiValueDictionary<(Node, int), Node> ExecutionEdges = new();

        public void EvaluateRoot(int root)
        {
            Queue<Node> nodeQueue = new();
            Dictionary<(Node, int), object> valueCache = new();
            EvalContext ctx = new(this, nodeQueue, valueCache);
            Roots[root].Evaluate(ctx);
        }

        public Node AddRoot(Node root)
        {
            Roots.Add(root);
            return root;
        }

        public void AddEdge(Node from, int fromPort, Node to, int toPort)
        {
            Edges.Add((from, fromPort), (to, toPort));
            ReverseEdges.Add((to, toPort), (from, fromPort));
        }

        public void AddExecutionEdge(Node from, int fromPort, Node to)
        {
            ExecutionEdges.Add((from, fromPort), to);
        }

        public (Node, int) GetOutputPortOf((Node, int) inputPort)
            => ReverseEdges[inputPort];

        public (Node, int) GetInputPortOf((Node, int) outputPort)
            => Edges[outputPort];

        public List<Node> GetExecutionInputPortsOf((Node, int) outputPort)
            => ExecutionEdges[outputPort];
    }
}