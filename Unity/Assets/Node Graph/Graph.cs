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
        Arena<Node> nodes = new();

        /// <summary>
        /// Backwards data (non-execution) edges (input -> output)
        /// </summary>
        [SerializeField]
        [HideInInspector]
        SerializableDict<PortIndex, PortIndex> reverseEdges = new();

        /// <summary>
        /// Forward execution edges (output -> input)
        /// </summary>
        [SerializeField]
        [HideInInspector]
        MultiValueDictionary<PortIndex, NodeIndex> executionEdges = new();

        /// <summary>
        /// Input ports, usually only present in subgraphs (such as within a for loop node)
        /// </summary>
        [SerializeField]
        List<NodeValueType> inputPorts = new();

        /// <summary>
        /// Output ports, usually only present in subgraphs (such as within a for loop node)
        /// </summary>
        [SerializeField]
        List<NodeValueType> outputPorts = new();

        /// <summary>
        /// Backwards edges between a node and the graph's input ports (node -> graph input).
        /// </summary>
        [SerializeField]
        [HideInInspector]
        SerializableDict<PortIndex, int> reverseInputPortEdges = new();

        /// <summary>
        /// Backwards edges between a node and the graph's output ports (graph output -> node).
        /// </summary>
        [SerializeField]
        [HideInInspector]
        SerializableDict<int, PortIndex> reverseOutputPortEdges = new();

        [SerializeField]
        int executionInputs;

        public int ExecutionInputs => executionInputs;

        /// <summary>
        /// Execution edges from the inputs to the graph (ExecutionInputs) to nodes.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        MultiValueDictionary<int, NodeIndex> inputExecutionEdges = new();

        /// <summary>
        /// True if this graph, when a subgraph of a node, should have an input port for each
        /// variadic input to its containing node.
        /// </summary>
        [SerializeField]
        bool variadicPassthrough;

        /// <summary>
        /// True if this graph, when a subgraph of a node, should have an output port for each
        /// variadic input to its containing node.
        /// </summary>
        [SerializeField]
        bool variadicOutput;

        public List<NodeIndex> InputExecutionEdges(int index)
            => inputExecutionEdges[index];

        public NodeIndex AddNode(NodeDefinition definition)
        {
            Node node = new(definition);
            NodeIndex index = nodes.Add(node);
            return index;
        }

        public bool RemoveNode(NodeIndex index)
        {
            return nodes.Remove(index);
        }

        public Node GetNode(NodeIndex index)
        {
            return nodes[index];
        }

        public void AddEdge(NodeIndex from, int fromPort, NodeIndex to, int toPort)
        {
            reverseEdges.Add(new(to, toPort), new(from, fromPort));
        }

        public void AddExecutionEdge(NodeIndex from, int fromPort, NodeIndex to)
        {
            executionEdges.Add(new(from, fromPort), to);
        }

        public bool TryGetOutputPortOf(PortIndex inputPort, out PortIndex outputPort)
            => reverseEdges.TryGetValue(inputPort, out outputPort);

        public List<NodeIndex> GetExecutionInputPortsOf(PortIndex outputPort)
            => executionEdges[outputPort];
    }
}