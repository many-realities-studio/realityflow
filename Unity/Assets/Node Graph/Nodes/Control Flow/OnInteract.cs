using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityFlow.NodeGraph.Nodes.ControlFlow
{
    public static class OnInteract
    {
        public static void Evaluate(Node node, EvalContext ctx)
        {
            ctx.ExecuteTargetsOfPort(node, 0);
        }
    }
}