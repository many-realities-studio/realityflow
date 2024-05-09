using System;
using System.Collections.Generic;
using System.Reflection;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class Node
    {
        public readonly NodeDefinition Definition;

        List<NodePort> Inputs;
        List<NodePort> Outputs;

        public Node(NodeDefinition definition)
        {
            Definition = definition;
            Inputs = new();
            for (int i = 0; i < definition.Inputs.Count; i++)
                Inputs.Add(new());
            Outputs = new();
            for (int i = 0; i < definition.Outputs.Count; i++)
                Outputs.Add(new());
        }

        public void Evaluate(EvalContext ctx)
        {
            string[] parts = Definition.EvaluationMethod.Split(';');
            Type type = Assembly.GetExecutingAssembly().GetType(parts[0]);
            MethodInfo method = type.GetMethod(parts[1]);
            method.Invoke(null, new object[] { this, ctx });
        }
    }
}
