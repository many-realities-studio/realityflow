namespace RealityFlow.NodeGraph.Nodes.Functional
{
    public static class Add
    {
        public static void Evaluate(Node node, EvalContext ctx)
        {
            object lhs = ctx.GetValueForInputPort((node, 0));
            object rhs = ctx.GetValueForInputPort((node, 1));
            ctx.valueCache[(node, 0)] = (int)lhs + (int)rhs;
        }
    }
}