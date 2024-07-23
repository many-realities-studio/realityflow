using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Dagre;
using RealityFlow.Collections;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class Graph : ISerializationCallbackReceiver
    {
        [SerializeField]
        string id;
        public string Id => id;

        public void SetId(string id) => this.id = id;

        /// <summary>
        /// The name of this graph.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public string name = "Graph";
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

        public ImmutableDictionary<PortIndex, PortIndex> ReverseEdges 
            => reverseEdges.ToImmutableDictionary();

        public IEnumerable<KeyValuePair<PortIndex, PortIndex>> Edges =>
            reverseEdges.Select(kv => new KeyValuePair<PortIndex, PortIndex>(kv.Value, kv.Key));

        /// <summary>
        /// Forward execution edges (output -> input)
        /// </summary>
        [SerializeField]
        [HideInInspector]
        BiMultiValueDict<PortIndex, NodeIndex> executionEdges = new();

        public IEnumerable<KeyValuePair<PortIndex, ImmutableList<NodeIndex>>> ExecutionEdges
            => executionEdges.Select(kv => new KeyValuePair<PortIndex, ImmutableList<NodeIndex>>(kv.Key, kv.Value.ToImmutableList()));

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
        Dictionary<string, HashSet<NodeIndex>> nodeTypes = new();

        /// <summary>
        /// A mapping of node definition names to node indices. Useful for looking up all nodes
        /// of a given type.
        /// </summary>
        public ImmutableDictionary<string, HashSet<NodeIndex>> NodeTypes
            => nodeTypes.ToImmutableDictionary();

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
        SerializableDict<string, NodeValueType> variables = new();

        public ImmutableDictionary<string, NodeValueType> Variables
            => variables.ToImmutableDictionary();

        public int ChangeTicks { get; private set; }

        public void IncrementChangeTicks()
            => ChangeTicks += 1;

        public void AddVariable(string name, NodeValueType type)
        {
            IncrementChangeTicks();
            variables.Add(name, type);
        }

        public void RemoveVariable(string name)
        {
            IncrementChangeTicks();
            variables.Remove(name);
        }

        public bool TryGetVariableType(string name, out NodeValueType type)
        {
            return variables.TryGetValue(name, out type);
        }

        public Graph(string realityflowId)
        {
            id = realityflowId;
        }

        public NodeIndex AddNode(NodeDefinition definition)
        {
            Node node = new(definition);
            NodeIndex index = nodes.Add(node);
            MutableNodesOfType(definition.Name).Add(index);
            IncrementChangeTicks();
            return index;
        }

        /// <summary>
        /// Use the dagre layouting algorithm to layout the graph automatically.
        /// 
        /// Starts a task and returns it. The task will take a while to complete, so don't block on 
        /// it.
        /// 
        /// This change will not be automatically persisted to the database! Make sure to update the
        /// graph in the database yourself after this task finishes.
        /// </summary>
        public Task LayoutNodes()
        {
            Task task = new(() =>
            {
                DagreInputGraph inputGraph = new()
                {
                    VerticalLayout = false
                };

                Dictionary<NodeIndex, DagreInputNode> mapping = new();

                foreach ((NodeIndex index, Node node) in nodes)
                {
                    mapping.Add(index, inputGraph.AddNode(null, null, null));
                }

                foreach ((PortIndex from, PortIndex to) in Edges)
                {
                    try
                    {
                        inputGraph.AddEdge(mapping[from.Node], mapping[to.Node]);
                    }
                    catch (DagreException) { }
                }

                foreach ((PortIndex from, ImmutableList<NodeIndex> tos) in ExecutionEdges)
                    foreach (NodeIndex to in tos)
                    {
                        inputGraph.AddEdge(mapping[from.Node], mapping[to]);
                    }

                inputGraph.Layout();

                foreach ((NodeIndex index, Node node) in Nodes)
                {
                    DagreInputNode inpNode = mapping[index];
                    node.Position = new(inpNode.X, inpNode.Y);
                }

                // Create a bounding box to re-center the graph
                Rect boundingBox = new();
                foreach ((NodeIndex index, Node node) in Nodes)
                {
                    if (node.Position.x < boundingBox.xMin)
                        boundingBox.xMin = node.Position.x;
                    if (node.Position.x > boundingBox.xMax)
                        boundingBox.xMax = node.Position.x;
                    if (node.Position.y < boundingBox.yMin)
                        boundingBox.yMin = node.Position.y;
                    if (node.Position.y > boundingBox.yMax)
                        boundingBox.yMax = node.Position.y;
                }

                Vector2 offset = -boundingBox.center;

                foreach ((NodeIndex index, Node node) in Nodes)
                {
                    node.Position += offset;
                }
            });
            task.Start();
            return task;
        }

        /// <summary>
        /// Returns a token asserting that this node once existed in this graph at the given index.
        /// Can be used to re-add a node to a graph with confidence that it will be valid to do so.
        /// </summary>
        public NodeMemory GetMemory(NodeIndex index)
            => new(Id, index, GetNode(index));

        public record NodeMemory(string Graph, NodeIndex Index, Node Node);

        public bool RememberNode(NodeMemory node, out NodeIndex index)
        {
            if (node.Graph != Id)
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

            if (node.Definition != null)
            {
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

                IncrementChangeTicks();
            }

            if (nodes.Remove(index) == false)
                Debug.LogError("Failed to remove existing node");

            return true;
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

            if (node.Definition == null)
                return;

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
            IncrementChangeTicks();
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

            IncrementChangeTicks();

            return true;
        }

        public bool ExecEdgeExists(PortIndex from, NodeIndex to)
        {
            return executionEdges.Contains(from, to);
        }

        public void RemoveDataEdge(PortIndex from, PortIndex to)
        {
            reverseEdges.Remove(to);

            IncrementChangeTicks();
        }

        public void RemoveExecutionEdge(PortIndex from, NodeIndex to)
        {
            executionEdges.Remove(from, to);

            IncrementChangeTicks();
        }

        public bool TryGetOutputPortOf(PortIndex inputPort, out PortIndex outputPort)
            => reverseEdges.TryGetValue(inputPort, out outputPort);

        public ImmutableList<NodeIndex> GetExecutionInputPortsOf(PortIndex outputPort)
            => executionEdges[outputPort];

        public void ApplyJson(string json)
        {
            Graph fromJson = JsonUtility.FromJson<Graph>(json);

            id = fromJson.id;
            name = fromJson.name;
            nodes = fromJson.nodes;
            nodeTypes = fromJson.nodeTypes;
            reverseEdges = fromJson.reverseEdges;
            executionEdges = fromJson.executionEdges;
            inputPorts = fromJson.inputPorts;
            outputPorts = fromJson.outputPorts;
            reverseInputPortEdges = fromJson.reverseInputPortEdges;
            reverseOutputPortEdges = fromJson.reverseOutputPortEdges;
            executionInputs = fromJson.executionInputs;
            inputExecutionEdges = fromJson.inputExecutionEdges;
            variadicPassthrough = fromJson.variadicPassthrough;
            variadicOutput = fromJson.variadicOutput;
            variables = fromJson.variables;

            IncrementChangeTicks();
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            nodeTypes = new();
            variables ??= new();

            List<NodeIndex> invalidNodes = new();
            List<(PortIndex, PortIndex)> invalidDataEdges = new();
            List<(PortIndex, NodeIndex)> invalidExecEdges = new();

            foreach ((NodeIndex index, Node node) in nodes)
                if (node.Definition != null)
                    MutableNodesOfType(node.Definition.Name).Add(index);
                else
                    invalidNodes.Add(index);

            foreach (NodeIndex index in invalidNodes)
                RemoveNode(index);

            foreach ((PortIndex to, PortIndex from) in reverseEdges)
                if (!ContainsNode(to.Node) || !ContainsNode(from.Node))
                    invalidDataEdges.Add((from, to));

            foreach ((PortIndex from, PortIndex to) in invalidDataEdges)
                RemoveDataEdge(from, to);

            foreach ((PortIndex from, ImmutableList<NodeIndex> tos) in executionEdges)
                foreach (NodeIndex to in tos)
                    if (!ContainsNode(from.Node) || !ContainsNode(to))
                        invalidExecEdges.Add((from, to));

            foreach ((PortIndex from, NodeIndex to) in invalidExecEdges)
                RemoveExecutionEdge(from, to);
        }
    }
}