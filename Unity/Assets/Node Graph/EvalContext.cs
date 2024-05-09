using System.Collections.Generic;

namespace RealityFlow.NodeGraph
{
    public class EvalContext
    {
        public Graph graph;
        public Queue<Node> nodeQueue;
        public Dictionary<(Node, int), object> valueCache;

        public EvalContext(
            Graph graph,
            Queue<Node> nodeQueue,
            Dictionary<(Node, int), object> valueCache
        )
        {
            this.graph = graph;
            this.nodeQueue = nodeQueue;
            this.valueCache = valueCache;
        }

        public object GetValueForInputPort((Node, int) input)
        {
            (Node, int) outputPort = graph.GetOutputPortOf(input);
            if (valueCache.TryGetValue(outputPort, out object value))
                return value;
            else if (outputPort.Item1.Definition.IsPure)
                outputPort.Item1.Evaluate(this);
            else
                throw new InvalidDataFlowException();

            return valueCache[outputPort];
        }

        public void ExecuteTargetsOfPort(Node node, int port)
        {
            List<Node> nodes = graph.GetExecutionInputPortsOf((node, port));
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].Evaluate(this);
        }
    }
}