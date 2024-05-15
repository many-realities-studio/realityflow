using System;
using System.Collections.Generic;
using RealityFlow.Collections;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class Graph
    {
        /// <summary>
        /// The arena of nodes in this graph.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        Arena<Node> Nodes = new();

        // /// <summary>
        // /// Forward data (non-execution) edges (output -> input)
        // /// </summary>
        // [SerializeField]
        // SerializableDict<PortIndex, PortIndex> Edges = new();
        /// <summary>
        /// Backwards data (non-execution) edges (input -> output)
        /// </summary>
        [SerializeField]
        [HideInInspector]
        SerializableDict<PortIndex, PortIndex> ReverseEdges = new();

        /// <summary>
        /// Forward execution edges (output -> input)
        /// </summary>
        [SerializeField]
        [HideInInspector]
        MultiValueDictionary<PortIndex, NodeIndex> ExecutionEdges = new();

        /// <summary>
        /// Input ports, usually only present in subgraphs (such as within a for loop node)
        /// </summary>
        [SerializeField]
        List<NodeValueType> InputPorts = new();

        /// <summary>
        /// Output ports, usually only present in subgraphs (such as within a for loop node)
        /// </summary>
        [SerializeField]
        List<NodeValueType> OutputPorts = new();

        /// <summary>
        /// Backwards edges between a node and the graph's input ports (node -> graph input).
        /// </summary>
        [SerializeField]
        [HideInInspector]
        SerializableDict<PortIndex, int> ReverseInputPortEdges = new();

        /// <summary>
        /// Backwards dges between a node and the graph's output ports (graph output -> node).
        /// </summary>
        [SerializeField]
        [HideInInspector]
        SerializableDict<int, PortIndex> ReverseOutputPortEdges = new();

        [SerializeField]
        int ExecutionInputs;

        /// <summary>
        /// Execution edges from the inputs to the graph (ExecutionInputs) to nodes.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        MultiValueDictionary<int, NodeIndex> InputExecutionEdges = new();

        public void EvaluateFromRoot(NodeIndex root)
        {
            Node node = Nodes[root];
            if (!node.Definition.IsRoot)
                throw new ArgumentException("Tried to start evaluation from a non-root");

            EvalContext ctx = new(this);
            ctx.EvaluateNode(root);
        }

        public NodeIndex AddNode(NodeDefinition definition)
        {
            Node node = new(definition);
            NodeIndex index = Nodes.Add(node);
            return index;
        }

        public bool RemoveNode(NodeIndex index)
        {
            return Nodes.Remove(index);
        }

        public Node GetNode(NodeIndex index)
        {
            return Nodes[index];
        }

        public void AddEdge(NodeIndex from, int fromPort, NodeIndex to, int toPort)
        {
            // Edges.Add(new(from, fromPort), new(to, toPort));
            ReverseEdges.Add(new(to, toPort), new(from, fromPort));
        }

        public void AddExecutionEdge(NodeIndex from, int fromPort, NodeIndex to)
        {
            ExecutionEdges.Add(new(from, fromPort), to);
        }

        public bool TryGetOutputPortOf(PortIndex inputPort, out PortIndex outputPort)
            => ReverseEdges.TryGetValue(inputPort, out outputPort);

        // public bool TryGetInputPortOf(PortIndex outputPort, out PortIndex inputPort)
        //     => Edges.TryGetValue(outputPort, out inputPort);

        public List<NodeIndex> GetExecutionInputPortsOf(PortIndex outputPort)
            => ExecutionEdges[outputPort];
    }
}