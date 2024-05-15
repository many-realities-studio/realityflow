using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class Node
    {
        public NodeDefinition Definition;

        [SerializeField]
        List<NodeValue> FieldValues = new();
        [SerializeField]
        List<InputNodePort> Inputs = new();
        [SerializeField]
        List<OutputNodePort> Outputs = new();

        public Node(NodeDefinition definition)
        {
            Definition = definition;
            for (int i = 0; i < definition.Fields.Count; i++)
                FieldValues.Add(new());
            for (int i = 0; i < definition.Inputs.Count; i++)
                Inputs.Add(new());
            for (int i = 0; i < definition.Outputs.Count; i++)
                Outputs.Add(new());
        }

        public bool TryGetField<T>(int index, out T field)
        {
            if (index < FieldValues.Count)
            {
                NodeValue nodeValue = FieldValues[index];
                if (nodeValue.TryGetValue(out field))
                    return true;
            }

            field = default;
            return false;
        }
        public InputNodePort GetInput(int index) => Inputs[index];
        public OutputNodePort GetOutput(int index) => Outputs[index];

        public bool TrySetField<T>(int index, T value)
        {
            if (index >= FieldValues.Count)
                return false;
            
            if (Definition.Fields[index].Default.GetValueType() != typeof(T))
                return false;

            FieldValues[index] = NodeValue.From(value);
            return true;
        }
        // TODO: Migrate to NodeValue
        public void SetInputConstant(int index, object value)
        {
            Inputs[index].ConstantValue = value;
        }
    }
}
