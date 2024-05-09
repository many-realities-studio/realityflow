using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityFlow.NodeGraph.Nodes.Actions
{
    public static class Print
    {
        public static void Evaluate(Node node, EvalContext ctx)
        {
            object lhs = ctx.GetValueForInputPort((node, 0));
            Debug.Log(lhs);
        }
    }
}