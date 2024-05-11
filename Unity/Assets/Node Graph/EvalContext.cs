using System.Collections.Generic;

namespace RealityFlow.NodeGraph
{
    public class EvalContext
    {
        public Graph graph;
        public Queue<Node> nodeQueue;
        public Dictionary<PortIndex, object> valueCache;

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

        public object GetValueForInputPort(PortIndex input)
        {
            if (graph.TryGetOutputPortOf(input, out var outputPort))
            {
                if (valueCache.TryGetValue(outputPort, out object value))
                    return value;
                else if (outputPort.Node.Definition.IsPure)
                    outputPort.Node.Evaluate(this);
                else
                    throw new InvalidDataFlowException();

                return valueCache[outputPort];
            }
            else
            {
                InputNodePort inputPort = input.AsInput;
                return inputPort.ConstantValue;
            }
        }

        public void ExecuteTargetsOfPort(Node node, int port)
        {
            List<Node> nodes = graph.GetExecutionInputPortsOf(new(node, port));
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].Evaluate(this);
        }
    }
}