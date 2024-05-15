using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    public class EvalContext
    {
        public Graph graph;
        readonly Queue<NodeIndex> nodeQueue = new();
        readonly Stack<NodeIndex> nodeStack = new();
        readonly Dictionary<PortIndex, object> valueCache = new();

        public EvalContext(Graph graph)
        {
            this.graph = graph;
        }

        public T GetInput<T>(int port)
        {
            NodeIndex node = nodeStack.Peek();
            PortIndex input = new(node, port);
            object value;
            if (graph.TryGetOutputPortOf(input, out var outputPort))
            {
                if (valueCache.TryGetValue(outputPort, out value))
                { }
                else if (outputPort.GetNode(graph).Definition.IsPure)
                {
                    EvaluateNode(outputPort.Node);
                    value = valueCache[outputPort];
                }
                else
                    throw new InvalidDataFlowException();
            }
            else
                value = input.AsInput(graph).ConstantValue;

            if (value is T typedVal)
                return typedVal;
            else
                throw new GraphTypeMismatchException();
        }

        public void SetOutput<T>(int port, T value)
        {
            NodeIndex node = nodeStack.Peek();
            valueCache[new(node, port)] = value;
        }

        public void ExecuteTargetsOfPort(int port)
        {
            NodeIndex node = nodeStack.Peek();
            List<NodeIndex> nodes = graph.GetExecutionInputPortsOf(new(node, port));
            for (int i = 0; i < nodes.Count; i++)
                EvaluateNode(nodes[i]);
        }

        public void EvaluateNode(NodeIndex node)
        {
            Action<EvalContext> evaluate = graph.GetNode(node).Definition.GetEvaluation();
            if (evaluate is not null)
            {
                nodeStack.Push(node);
                evaluate(this);
                nodeStack.Pop();
            }
            else
                Debug.LogError("Failed to load evaluation method for node");
        }
    }
}