using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class Node
    {
        public readonly NodeDefinition Definition;

        [SerializeField]
        List<NodeValueType> typeSubstitutions = new();
        [SerializeField]
        List<InputNodePort> Inputs = new();
        [SerializeField]
        List<OutputNodePort> Outputs = new();

        public Node(NodeDefinition definition)
        {
            Definition = definition;
            for (int i = 0; i < definition.HighestTypeVariableIndex(); i++)
                typeSubstitutions.Add(null);
            for (int i = 0; i < definition.Inputs.Count; i++)
                Inputs.Add(new());
            for (int i = 0; i < definition.Outputs.Count; i++)
                Outputs.Add(new());
        }

        public InputNodePort GetInput(int index) => Inputs[index];
        public OutputNodePort GetOutput(int index) => Outputs[index];

        public void SetInputConstant(int index, object value)
        {
            Inputs[index].ConstantValue = value;
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
