namespace RealityFlow.NodeGraph.Nodes.Functional
{
    public static class Integer
    {
        public static void Evaluate(Node node, EvalContext ctx)
        {
            ctx.SetOutputValue(node, 0, 12);
        }
    }
}
