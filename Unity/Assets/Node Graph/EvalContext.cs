using System.Collections.Generic;

namespace RealityFlow.NodeGraph
{
    public class EvalContext
    {
        public Graph graph;
        public Queue<Node> nodeQueue;
        Dictionary<PortIndex, object> valueCache;

        public EvalContext(
            Graph graph,
            Queue<Node> nodeQueue,
            Dictionary<PortIndex, object> valueCache
        )
        {
            this.graph = graph;
            this.nodeQueue = nodeQueue;
            this.valueCache = valueCache;
        }

        public T GetValueForInputPort<T>(PortIndex input)
        {
            object value;
            if (graph.TryGetOutputPortOf(input, out var outputPort))
            {
                if (valueCache.TryGetValue(outputPort, out value))
                { }
                else if (outputPort.Node.Definition.IsPure)
                {
                    outputPort.Node.Evaluate(this);
                    value = valueCache[outputPort];
                }
                else
                    throw new InvalidDataFlowException();
            }
            else
                value = input.AsInput.ConstantValue;

            if (value is T typedVal)
                return typedVal;
            else
                throw new GraphTypeMismatchException();
        }

        public void SetOutputValue<T>(Node node, int port, T value)
        {
            valueCache[new(node, port)] = value;
        }

        public void ExecuteTargetsOfPort(Node node, int port)
        {
            List<Node> nodes = graph.GetExecutionInputPortsOf(new(node, port));
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].Evaluate(this);
        }
    }
}