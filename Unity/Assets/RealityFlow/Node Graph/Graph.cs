using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RealityFlow.Collections;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class Graph : ISerializationCallbackReceiver
    {
        /// <summary>
        /// The arena of nodes in this graph.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        Arena<Node> nodes = new();

        public IEnumerable<KeyValuePair<NodeIndex, Node>> Nodes
            => nodes.Select(kv => new KeyValuePair<NodeIndex, Node>(new NodeIndex(kv.Key), kv.Value));

        /// <summary>
        /// Backwards data (non-execution) edges (input -> output)
        /// </summary>
        [SerializeField]
        [HideInInspector]
        BiDict<PortIndex, PortIndex> reverseEdges = new();

        public IEnumerable<KeyValuePair<PortIndex, PortIndex>> Edges =>
            reverseEdges.Select(kv => new KeyValuePair<PortIndex, PortIndex>(kv.Value, kv.Key));

        /// <summary>
        /// Forward execution edges (output -> input)
        /// </summary>
        [SerializeField]
        [HideInInspector]
        BiMultiValueDict<PortIndex, NodeIndex> executionEdges = new();

        public IEnumerable<KeyValuePair<PortIndex, List<NodeIndex>>> ExecutionEdges => executionEdges;

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

        public IEnumerable<KeyValuePair<PortIndex, int>> InputEdges => reverseInputPortEdges;

        /// <summary>
        /// Backwards edges between a node and the graph's output ports (graph output -> node).
        /// </summary>
        [SerializeField]
        [HideInInspector]
        SerializableDict<int, PortIndex> reverseOutputPortEdges = new();

        public IEnumerable<KeyValuePair<int, PortIndex>> OutputEdges => reverseOutputPortEdges;

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

        public ImmutableList<NodeIndex> InputExecutionEdges(int index)
            => inputExecutionEdges[index];

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

        [NonSerialized]
        readonly Dictionary<string, HashSet<NodeIndex>> nodeTypes = new();
        /// <summary>
        /// A mapping of node definition names to node indices. Useful for looking up all nodes
        /// of a given type.
        /// </summary>
        public ImmutableDictionary<string, HashSet<NodeIndex>> NodeTypes
        {
            get => nodeTypes.ToImmutableDictionary();
        }

        HashSet<NodeIndex> MutableNodesOfType(string type)
        {
            if (nodeTypes.TryGetValue(type, out var list))
                return list;

            list = new();
            nodeTypes.Add(type, list);
            return list;
        }

        public ImmutableHashSet<NodeIndex> NodesOfType(string type) =>
            MutableNodesOfType(type).ToImmutableHashSet();

        [SerializeField]
        readonly Dictionary<string, NodeValueType> variables = new();

        public ImmutableDictionary<string, NodeValueType> Variables
            => variables.ToImmutableDictionary();

        public void AddVariable(string name, NodeValueType type)
        {
            variables.Add(name, type);
        }

        public void RemoveVariable(string name)
        {
            variables.Remove(name);
        }

        public bool TryGetVariableType(string name, out NodeValueType type)
        {
            return variables.TryGetValue(name, out type);
        }

        public NodeIndex AddNode(NodeDefinition definition)
        {
            Node node = new(definition);
            NodeIndex index = nodes.Add(node);
            MutableNodesOfType(definition.Name).Add(index);
            return index;
        }

        /// <summary>
        /// Returns a token asserting that this node once existed in this graph at the given index.
        /// Can be used to re-add a node to a graph with confidence that it will be valid to do so.
        /// </summary>
        public NodeMemory GetMemory(NodeIndex index)
            => new(this, index, GetNode(index));

        // TODO: Replace Graph with RealityFlowID of this graph
        public record NodeMemory(Graph Graph, NodeIndex Index, Node Node);

        public bool RememberNode(NodeMemory node, out NodeIndex index)
        {
            if (node.Graph != this)
            {
                index = default;
                return false;
            }

            nodes.Set(node.Index, node.Node);
            index = node.Index;
            MutableNodesOfType(node.Node.Definition.name).Add(index);
            return true;
        }

        public bool RemoveNode(NodeIndex index)
        {
            Node node = GetNode(index);

            for (int i = 0; i < node.Definition.Inputs.Count; i++)
            {
                PortIndex to = new(index, i);
                if (reverseEdges.ContainsKey(to))
                    reverseEdges.Remove(to);
            }

            for (int i = 0; i < node.Definition.Outputs.Count; i++)
            {
                PortIndex from = new(index, i);
                if (reverseEdges.TryGetKeys(from, out ImmutableList<PortIndex> targets))
                    foreach (PortIndex to in targets)
                        reverseEdges.Remove(to);
            }

            for (int i = 0; i < node.Definition.ExecutionOutputs.Count; i++)
            {
                PortIndex from = new(index, i);
                if (executionEdges.ContainsKey(from))
                    executionEdges.RemoveAll(from);
            }

            if (node.Definition.ExecutionInput)
                if (executionEdges.TryGetKeys(index, out ImmutableList<PortIndex> ports))
                    foreach (PortIndex from in ports)
                        executionEdges.Remove(from, index);

            MutableNodesOfType(node.Definition.name).Remove(index);
            return nodes.Remove(index);
        }

        public Node GetNode(NodeIndex index)
        {
            return nodes[index];
        }

        public bool ContainsNode(NodeIndex index)
            => nodes.Contains(index);

        public void EdgesOf(NodeIndex nodeIndex, List<(PortIndex, PortIndex)> data, List<(PortIndex, NodeIndex)> exec)
        {
            data.Clear();
            exec.Clear();

            Node node = GetNode(nodeIndex);

            for (int i = 0; i < node.Definition.Inputs.Count; i++)
            {
                PortIndex to = new(nodeIndex, i);
                if (reverseEdges.TryGetValue(to, out PortIndex from))
                    data.Add((from, to));
            }

            for (int i = 0; i < node.Definition.Outputs.Count; i++)
            {
                PortIndex from = new(nodeIndex, i);
                if (reverseEdges.TryGetKeys(from, out ImmutableList<PortIndex> targets))
                    foreach (PortIndex to in targets)
                        data.Add((from, to));
            }

            for (int i = 0; i < node.Definition.ExecutionOutputs.Count; i++)
            {
                PortIndex from = new(nodeIndex, i);
                if (executionEdges.TryGetValues(from, out ImmutableList<NodeIndex> tos))
                    foreach (NodeIndex to in tos)
                        exec.Add((from, to));
            }

            if (node.Definition.ExecutionInput)
                if (executionEdges.TryGetKeys(nodeIndex, out ImmutableList<PortIndex> ports))
                    foreach (PortIndex from in ports)
                        exec.Add((from, nodeIndex));
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
        /// <item>
        /// The edge already exists
        /// </item>
        /// </list>
        /// This method considers both data and execution edges for these conditions.
        /// </summary>
        public bool TryAddEdge(NodeIndex from, int fromPort, NodeIndex to, int toPort)
        {
            if (!CanAddEdge(from, fromPort, to, toPort))
                return false;

            reverseEdges.Add(new(to, toPort), new(from, fromPort));
            return true;
        }

        public bool CanAddEdge(NodeIndex from, int fromPort, NodeIndex to, int toPort)
        {
            PortIndex fromIdx = new(from, fromPort);
            PortIndex toIdx = new(to, toPort);

            if (from == to)
                return false;

            Node fromNode = GetNode(from);
            Node toNode = GetNode(to);

            if (fromPort >= fromNode.Definition.Outputs.Count)
                return false;
            if (toPort >= toNode.Definition.Inputs.Count)
                return false;

            if (!PortsCompatible(fromIdx, toIdx))
                return false;

            if (EdgeWouldFormCycle(from, to))
                return false;

            if (EdgeExists(fromIdx, toIdx))
                return false;

            return true;
        }

        public bool PortsCompatible(PortIndex from, PortIndex to)
        {
            return NodeValue.IsAssignableTo(
                GetNode(from.Node).Definition.Outputs[from.Port].Type,
                GetNode(to.Node).Definition.Inputs[to.Port].Type
            );
        }

        public bool EdgeWouldFormCycle(NodeIndex from, NodeIndex to)
        {
            // Note: This is probably not the most efficient way to do this check.
            // For maximum efficiency, a data structure similar to the following could be used:
            // https://www.sciencedirect.com/science/article/pii/S030439751000616X?ref=pdf_download&fr=RR-2&rr=885f16a41e862888
            // Each node would be associated to a disjoint set of the nodes reachable from it. 
            // When an edge is added, every node reachable from `to` is now reachable from `from`,
            // so `from` would unify its set with `to`'s. 
            // This idea would probably take too long to implement for the time being so it's being
            // put off, especially as graph edits should be relatively rare (relative to the 
            // number of frames where one does not occur).
            return DepthFirstSearch(to, from);
        }

        public bool EdgeExists(PortIndex from, PortIndex to)
        {
            return reverseEdges.Contains(new(to, from));
        }

        /// <summary>
        /// Returns true if `target` is reachable from `from` by data or execution edges.
        /// Also returns true if from == target.
        /// </summary>
        bool DepthFirstSearch(NodeIndex from, NodeIndex target)
        {
            if (from == target)
                return true;

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
        /// <item>
        /// The target does not have an input port
        /// </item> 
        /// <item>
        /// The edge already exists
        /// </item>
        /// </list>
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

            if (EdgeWouldFormCycle(from, to))
                return false;

            if (ExecEdgeExists(new(from, fromPort), to))
                return false;

            executionEdges.Add(new(from, fromPort), to);

            return true;
        }

        public bool ExecEdgeExists(PortIndex from, NodeIndex to)
        {
            return executionEdges.Contains(from, to);
        }

        public void RemoveDataEdge(PortIndex from, PortIndex to)
        {
            reverseEdges.Remove(to);
        }

        public void RemoveExecutionEdge(PortIndex from, NodeIndex to)
        {
            executionEdges.Remove(from, to);
        }

        public bool TryGetOutputPortOf(PortIndex inputPort, out PortIndex outputPort)
            => reverseEdges.TryGetValue(inputPort, out outputPort);

        public ImmutableList<NodeIndex> GetExecutionInputPortsOf(PortIndex outputPort)
            => executionEdges[outputPort];

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            foreach ((NodeIndex index, Node node) in nodes)
                MutableNodesOfType(node.Definition.Name).Add(index);
        }
    }
}