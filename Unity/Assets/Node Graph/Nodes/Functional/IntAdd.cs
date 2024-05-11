namespace RealityFlow.NodeGraph.Nodes.Functional
{
    public static class IntAdd
    {
        public static void Evaluate(Node node, EvalContext ctx)
        {
            int lhs = ctx.GetValueForInputPort<int>(new(node, 0));
            int rhs = ctx.GetValueForInputPort<int>(new(node, 1));
            ctx.SetOutputValue(node, 0, lhs + rhs);
        }
    }
}