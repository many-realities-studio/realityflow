using System.Collections.Generic;
using RealityFlow.Collections;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    public class Graph
    {
        /// <summary>
        /// The arena of nodes in this graph.
        /// </summary>
        [SerializeField]
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
        SerializableDict<PortIndex, PortIndex> ReverseEdges = new();
        /// <summary>
        /// Forward execution edges (output -> input)
        /// </summary>
        [SerializeField]
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
        /// Edges between a node and the graph's input ports.
        /// </summary>
        [SerializeField]
        SerializableDict<PortIndex, int> ReverseInputPortEdges = new();
        /// <summary>
        /// Edges between a node and the graph's output ports.
        /// </summary>
        [SerializeField]
        SerializableDict<int, PortIndex> ReverseOutputPortEdges = new();

        /// <summary>
        /// Nodes that don't have an incoming execution port and have at least one outgoing
        /// execution port. Synced with Nodes automatically based on Node definitions.
        /// </summary>
        List<NodeIndex> Roots = new();

        public void EvaluateFromRoot(NodeIndex root)
        {
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