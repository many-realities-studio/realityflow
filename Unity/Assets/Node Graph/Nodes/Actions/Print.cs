using UnityEngine;

namespace RealityFlow.NodeGraph.Nodes.Actions
{
    public static class Print
    {
        public static void Evaluate(Node node, EvalContext ctx)
        {
            object lhs = ctx.GetValueForInputPort<object>(new(node, 0));
            Debug.Log(lhs);
        }
    }
}