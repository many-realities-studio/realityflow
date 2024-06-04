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

        public IEnumerable<Node> Nodes => nodes;

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

        public List<NodeValueType> OutputPorts => outputPorts;

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

        public bool TryGetGraphOutputSource(int outputIndex, out PortIndex port)
            => reverseOutputPortEdges.TryGetValue(outputIndex, out port);

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

        /// <summary>
        /// Attempts to add an edge between two node ports. Fails under the following conditions:
        /// <list type="bullet">
        /// <item>
        /// Both ports are the same
        /// </item>
        /// <item>
        /// Either port is out of bounds for its node
        /// </item>
        /// <item>
        /// The type of the output port is not assignable to the type of the input port 
        /// </item>
        /// <item>
        /// The edge would form a cycle (`from` is reachable from `to`)
        /// </item>
        /// </list>
        /// This method considers both data and execution edges for these conditions.
        /// </summary>
        public bool TryAddEdge(NodeIndex from, int fromPort, NodeIndex to, int toPort)
        {
            if (from == to)
                return false;

            Node fromNode = GetNode(from);
            Node toNode = GetNode(to);

            if (fromPort >= fromNode.Definition.Outputs.Count)
                return false;
            if (toPort >= toNode.Definition.Inputs.Count)
                return false;

            if (NodeValue.IsNotAssignableTo(
                fromNode.Definition.Outputs[fromPort].Type,
                toNode.Definition.Inputs[toPort].Type)
            )
                return false;

            // Note: This is probably not the most efficient way to do this check.
            // For maximum efficiency, a data structure similar to the following could be used:
            // https://www.sciencedirect.com/science/article/pii/S030439751000616X?ref=pdf_download&fr=RR-2&rr=885f16a41e862888
            // Each node would be associated to a disjoint set of the nodes reachable from it. 
            // When an edge is added, every node reachable from `to` is now reachable from `from`,
            // so `from` would unify its set with `to`'s. 
            // This idea would probably take too long to implement for the time being so it's being
            // put off, especially as graph edits should be relatively rare (relative to the 
            // number of frames where one does not occur).
            if (DepthFirstSearch(to, from))
                return false;

            reverseEdges.Add(new(to, toPort), new(from, fromPort));
            return true;
        }

        /// <summary>
        /// Returns true if `target` is reachable from `from` by data or execution edges.
        /// </summary>
        bool DepthFirstSearch(NodeIndex from, NodeIndex target)
        {
            // because execution edges and data edges go in opposite directions, two DFS's are required.
            HashSet<NodeIndex> visited = new();
            Stack<NodeIndex> stack = new();

            stack.Push(target);

            while (stack.TryPop(out NodeIndex current))
            {
                Node currentNode = GetNode(current);

                for (int i = 0; i < currentNode.Definition.Inputs.Count; i++)
                    if (TryGetOutputPortOf(new(current, i), out PortIndex output))
                    {
                        if (output.Node == from)
                            return true;
                        if (!visited.Contains(output.Node))
                        {
                            stack.Push(output.Node);
                            visited.Add(output.Node);
                        }
                    }
            }

            visited.Clear();
            stack.Clear();

            stack.Push(from);

            while (stack.TryPop(out NodeIndex current))
            {
                Node currentNode = GetNode(current);

                for (int i = 0; i < currentNode.Definition.ExecutionOutputs.Count; i++)
                    foreach (var next in GetExecutionInputPortsOf(new(current, i)))
                    {
                        if (next == target)
                            return true;
                        if (!visited.Contains(next))
                        {
                            stack.Push(next);
                            visited.Add(next);
                        }
                    }
            }

            return false;
        }

        /// <summary>
        /// Attempts to add an edge between two execution node ports. Fails under the following conditions:
        /// <list type="bullet">
        /// <item>
        /// Both ports are the same
        /// </item>
        /// <item>
        /// The from port is out of bounds
        /// </item>
        /// <item>
        /// The edge would form a cycle (`from` is reachable from `to`)
        /// </item>
        /// </list>
        /// <item>
        /// The target does not have an input port
        /// </item> 
        /// This method considers both data and execution edges for these conditions.
        /// </summary>
        public bool TryAddExecutionEdge(NodeIndex from, int fromPort, NodeIndex to)
        {
            if (from == to)
                return false;

            if (fromPort >= GetNode(from).Definition.ExecutionOutputs.Count)
                return false;

            if (!GetNode(to).Definition.ExecutionInput)
                return false;

            if (DepthFirstSearch(to, from))
                return false;

            executionEdges.Add(new(from, fromPort), to);

            return true;
        }

        public bool TryGetOutputPortOf(PortIndex inputPort, out PortIndex outputPort)
            => reverseEdges.TryGetValue(inputPort, out outputPort);

        public List<NodeIndex> GetExecutionInputPortsOf(PortIndex outputPort)
            => executionEdges[outputPort];
    }
}