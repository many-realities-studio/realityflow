using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    /// <summary>
    /// The primary class used to evaluate graphs/nodes.
    /// Stores relevant information during evaluation, such as intermediate results.
    /// </summary>
    public class EvalContext
    {
        readonly Stack<GraphView> graphStack = new();
        readonly Queue<NodeIndex> nodeQueue = new();
        readonly List<NodeIndex> nodeStack = new();
        readonly Dictionary<PortIndex, object> nodeOutputCache = new();
        readonly Dictionary<(GraphView, int), object> graphOutputCache = new();

        void PopNode() => nodeStack.RemoveAt(nodeStack.Count - 1);

        public T GetField<T>(int index)
        {
            NodeIndex nodeIndex = nodeStack[^1];
            GraphView graph = graphStack.Peek();
            Node node = graph.GetNode(nodeIndex);
            if (node.TryGetField(index, out T field))
                return field;

            throw new InvalidCastException();
        }

        /// <summary>
        /// Returns number of dependencies enqueued.
        /// Throws InvalidDataFlowException if a non-pure unevaluated dependency is encountered.
        /// </summary>
        int EnqueueUnevaluatedPureNodeDependencies(NodeIndex index)
        {
            int count = 0;
            GraphView graph = graphStack.Peek();
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
            GraphView graph = graphStack.Peek();
            if (graph.TryGetOutputPortOf(input, out var outputPort))
                value = nodeOutputCache[outputPort];
            else
                value = input.AsInput(graph).ConstantValue;

            if (value is T typedVal)
                return typedVal;
            else if 
                (
                    typeof(T) == typeof(float) 
                    && value is int intValue
                    && (float)intValue is T castValue
                )
                return castValue;
            else
                throw new GraphTypeMismatchException();
        }

        public void SetOutput<T>(int port, T value)
        {
            NodeIndex node = nodeStack[^1];
            nodeOutputCache[new(node, port)] = value;
        }

        public T GetGraphOutput<T>(GraphView graph, int outputPort)
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
            GraphView graph = graphStack.Peek();
            List<NodeIndex> nodes = graph.GetExecutionInputPortsOf(new(node, port));
            for (int i = 0; i < nodes.Count; i++)
                QueueNode(nodes[i]);
        }

        public void QueueNode(NodeIndex node)
        {
            nodeQueue.Enqueue(node);
        }

        void Evaluate()
        {
            GraphView graph = graphStack.Peek();
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
                    PopNode();
                }
                else
                    Debug.LogError("Failed to load evaluation method for node");
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
        }

        public void EvaluateGraphFromRoot(GraphView graph, NodeIndex root)
        {
            graphStack.Push(graph);

            Node node = graph.GetNode(root);
            if (!node.Definition.IsRoot)
                Debug.LogError("Attempted to evaluate starting from non-root");

            QueueNode(root);
            Evaluate();

            graphStack.Pop();
        }

        public void EvaluateGraph(GraphView graph, int executionInputPort)
        {
            graphStack.Push(graph);

            if (executionInputPort >= graph.ExecutionInputs)
                throw new IndexOutOfRangeException();

            foreach (NodeIndex node in graph.InputExecutionEdges(executionInputPort))
                QueueNode(node);

            Evaluate();

            graphStack.Pop();
        }
    }
}