using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using RealityFlow.Collections;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    /// <summary>
    /// The primary class used to evaluate graphs/nodes.
    /// Stores relevant information during evaluation, such as intermediate results.
    /// </summary>
    [Serializable]
    public class EvalContext
    {
        GameObject target;
        readonly List<ReadonlyGraph> graphStack = new();
        readonly Queue<NodeIndex> nodeQueue = new();
        readonly List<NodeIndex> nodeStack = new();
        readonly Dictionary<PortIndex, object> nodeOutputCache = new();
        readonly Dictionary<(ReadonlyGraph, int), object> graphOutputCache = new();
        readonly Dictionary<string, NodeValue> startArguments = new();
        readonly Dictionary<(ReadonlyGraph, string), NodeValue> variableValues = new();

        public ImmutableDictionary<string, NodeValue> StartArguments => startArguments.ToImmutableDictionary();

        public GameObject GetThis() => target;

        public T GetField<T>(int index)
        {
            NodeIndex nodeIndex = nodeStack.Peek();
            ReadonlyGraph graph = graphStack.Peek();
            Node node = graph.GetNode(nodeIndex);
            if (node.TryGetField(index, out T field))
                return field;

            throw new InvalidCastException();
        }

        bool TryFindVariableGraph(string name, out ReadonlyGraph graph)
        {
            for (int i = graphStack.Count - 1; i >= 0; i--)
            {
                ReadonlyGraph currentGraph = graphStack[i];

                if (variableValues.TryGetValue((currentGraph, name), out _))
                {
                    graph = currentGraph;
                    return true;
                }

                if (currentGraph.TryGetVariableType(name, out NodeValueType type))
                {
                    variableValues[(currentGraph, name)] = NodeValue.DefaultFor(type);
                    graph = currentGraph;
                    return true;
                }
            }

            graph = default;
            return false;
        }

        NodeValue GetVariableOrDefault(string name)
        {
            if (TryFindVariableGraph(name, out ReadonlyGraph graph))
                return variableValues[(graph, name)];

            throw new ArgumentException($"{name} is not a variable in scope");
        }

        public T GetVariable<T>(string name)
        {
            return NodeValue.UnwrapValue<T>(GetVariableOrDefault(name));
        }

        public void SetVariable<T>(string name, T value)
        {
            if (TryFindVariableGraph(name, out ReadonlyGraph graph))
            {
                NodeValue oldValue = variableValues[(graph, name)];
                NodeValue nodeValue = NodeValue.From(value, oldValue.ValueType);
                variableValues[(graph, name)] = nodeValue;
            }
            else
                throw new ArgumentException($"{name} is not a variable in scope");
        }

        public void ClearVariables()
        {
            variableValues.Clear();
        }

        /// <summary>
        /// Returns number of dependencies enqueued.
        /// Throws InvalidDataFlowException if a non-pure unevaluated dependency is encountered.
        /// </summary>
        int EnqueueUnevaluatedPureNodeDependencies(NodeIndex index)
        {
            int count = 0;
            ReadonlyGraph graph = graphStack.Peek();
            Node node = graph.GetNode(index);
            for (int i = 0; i < node.Definition.Inputs.Count + node.VariadicInputs; i++)
                if (graph.TryGetOutputPortOf(new(index, i), out PortIndex from))
                    if (!nodeOutputCache.ContainsKey(from))
                    {
                        if (!graph.GetNode(from.Node).Definition.IsPure)
                            throw new InvalidDataFlowException();

                        nodeQueue.Enqueue(from.Node);
                        count += 1;
                    }

            return count;
        }

        /// <summary>
        /// Assumes that all dependencies were already evaluated.
        /// </summary>
        public T GetInput<T>(int port)
        {
            NodeIndex node = nodeStack[^1];
            PortIndex input = new(node, port);
            object value;
            ReadonlyGraph graph = graphStack.Peek();
            if (graph.TryGetOutputPortOf(input, out var outputPort))
                value = nodeOutputCache[outputPort];
            else
                value = input.AsInput(graph).ConstantValue.DynValue;

            if (value is T typedVal)
                return typedVal;
            else if
                (
                    typeof(T) == typeof(float)
                    && value is int intValue
                    && (float)intValue is T castValue
                )
                return castValue;
            else if (typeof(T) == typeof(string) && value.ToString() is T tString)
                return tString;
            else
                throw new GraphTypeMismatchException();
        }

        public void SetOutput<T>(int port, T value)
        {
            NodeIndex node = nodeStack[^1];
            nodeOutputCache[new(node, port)] = value;
        }

        public T GetGraphOutput<T>(ReadonlyGraph graph, int outputPort)
        {
            if (!graphOutputCache.TryGetValue((graph, outputPort), out object output))
            {
                Debug.LogError("failed to get output value of graph");
                return default;
            }

            if (output is not T)
            {
                Debug.LogError($"failed to get output value of graph as type {typeof(T).Name}");
                return default;
            }

            return (T)output;
        }

        public void ExecuteTargetsOfPort(int port)
        {
            NodeIndex node = nodeStack[^1];
            ReadonlyGraph graph = graphStack.Peek();
            ImmutableList<NodeIndex> nodes = graph.GetExecutionInputPortsOf(new(node, port));
            for (int i = 0; i < nodes.Count; i++)
                QueueNode(nodes[i]);
        }

        public void QueueNode(NodeIndex node)
        {
            nodeQueue.Enqueue(node);
        }

        void Evaluate(GameObject target)
        {
            // TODO: Use graph stack instead of recursion
            this.target = target;
            ReadonlyGraph graph = graphStack.Peek();
            while (nodeQueue.TryDequeue(out NodeIndex node))
            {
                try
                {
                    if (EnqueueUnevaluatedPureNodeDependencies(node) > 0)
                    {
                        nodeQueue.Enqueue(node);
                        continue;
                    }
                }
                catch (InvalidDataFlowException)
                {
                    Debug.LogError("Invalid graph configuration; node had unevaluated impure dependency");
                }

                Action<EvalContext> evaluate = graph.GetNode(node).Definition.GetEvaluation();
                if (evaluate is not null)
                {
                    nodeStack.Add(node);
                    evaluate(this);
                    nodeStack.Pop();
                }
                else
                {
                    Debug.LogError("Failed to load evaluation method for node; bailing");
                    break;
                }
            }

            for (int i = 0; i < graph.OutputPorts.Count; i++)
            {
                if (!graph.TryGetGraphOutputSource(i, out PortIndex output))
                {
                    Debug.LogError($"failed to get output source of index {i}");
                    continue;
                }

                if (!nodeOutputCache.TryGetValue(output, out object value))
                {
                    Debug.LogError($"failed to get output value");
                    continue;
                }

                graphOutputCache[(graph, i)] = value;
            }

            this.target = null;
            nodeQueue.Clear();
            nodeOutputCache.Clear();
            graphOutputCache.Clear();
        }

        public void EvaluateGraphFromRoot(GameObject target, ReadonlyGraph graph, NodeIndex root, params (string, NodeValue)[] startArguments)
        {
            foreach ((string name, NodeValue value) in startArguments)
                this.startArguments.Add(name, value);

            graphStack.Push(graph);

            try
            {
                Node node = graph.GetNode(root);
                if (!node.Definition.IsRoot)
                    Debug.LogError("Attempted to evaluate starting from non-root");

                QueueNode(root);
                Evaluate(target);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                graphStack.Pop();
                this.startArguments.Clear();
            }
        }

        public void EvaluateGraph(GameObject target, ReadonlyGraph graph, int executionInputPort)
        {
            graphStack.Push(graph);

            try
            {
                if (executionInputPort >= graph.ExecutionInputs)
                    throw new IndexOutOfRangeException();

                foreach (NodeIndex node in graph.InputExecutionEdges(executionInputPort))
                    QueueNode(node);

                Evaluate(target);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                graphStack.Pop();
            }
        }
    }
}