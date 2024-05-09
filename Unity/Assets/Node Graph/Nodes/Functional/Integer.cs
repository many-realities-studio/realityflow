using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityFlow.NodeGraph.Nodes.Functional
{
    public static class Integer
    {
        public static void Evaluate(Node node, EvalContext ctx)
        {
            ctx.valueCache[(node, 0)] = 12;
        }
    }
}
