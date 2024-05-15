using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class Node
    {
        public NodeDefinition Definition;

        [SerializeField]
        List<object> FieldValues = new();
        [SerializeField]
        List<InputNodePort> Inputs = new();
        [SerializeField]
        List<OutputNodePort> Outputs = new();

        public Node(NodeDefinition definition)
        {
            Definition = definition;
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
    }
}
