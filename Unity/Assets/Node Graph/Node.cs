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
        List<NodeValue> fieldValues = new();
        [SerializeField]
        List<InputNodePort> inputs = new();
        [SerializeField]
        List<OutputNodePort> outputs = new();
        [SerializeField]
        int variadicInputs;

        public int VariadicInputs => variadicInputs;

        public Node(NodeDefinition definition)
        {
            Definition = definition;
            for (int i = 0; i < definition.Fields.Count; i++)
                fieldValues.Add(definition.Fields[i].Default);
            for (int i = 0; i < definition.Inputs.Count; i++)
                inputs.Add(new());
            for (int i = 0; i < definition.Outputs.Count; i++)
                outputs.Add(new());
        }

        public bool TryGetField<T>(int index, out T field)
        {
            if (index < fieldValues.Count)
            {
                NodeValue nodeValue = fieldValues[index];
                if (nodeValue.TryGetValue(out field))
                    return true;
            }

            field = default;
            return false;
        }
        public InputNodePort GetInput(int index) => inputs[index];
        public OutputNodePort GetOutput(int index) => outputs[index];

        public bool TrySetField<T>(int index, T value)
        {
            if (index >= fieldValues.Count)
                return false;
            
            if (Definition.Fields[index].Default.GetValueType() != typeof(T))
                return false;

            fieldValues[index] = NodeValue.From(value);
            return true;
        }
        
        public void SetInputConstant(int index, NodeValue value)
        {
            inputs[index].ConstantValue = value;
        }
    }
}
