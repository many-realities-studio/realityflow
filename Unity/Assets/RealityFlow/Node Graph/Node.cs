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

        public Vector2 Position { get; set; }

        public Node(NodeDefinition definition)
        {
            Definition = definition;
            for (int i = 0; i < definition.Fields.Count; i++)
                fieldValues.Add(definition.Fields[i].Default);
            for (int i = 0; i < definition.Inputs.Count; i++)
                if (NodeValue.TryGetDefaultFor(definition.Inputs[i].Type, out NodeValue value))
                    inputs.Add(new() { ConstantValue = value });
                else
                    inputs.Add(new());
            for (int i = 0; i < definition.Outputs.Count; i++)
                outputs.Add(new());
        }

        public bool TryGetField(int index, out NodeValue value)
        {
            if (index < fieldValues.Count)
            {
                value = fieldValues[index];
                return true;
            }

            value = NodeValue.Null;
            return false;
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

        public bool TryGetInputValue(int index, out NodeValue value)
        {
            if (index < inputs.Count)
            {
                value = inputs[index].ConstantValue;
                return true;
            }

            value = default;
            return false;
        }

        public bool TrySetInputValue(int index, NodeValue value)
        {
            if (index >= inputs.Count)
                return false;

            if (NodeValue.IsNotAssignableTo(value.Type, Definition.Inputs[index].Type))
                return false;

            inputs[index].ConstantValue = value;
            return true;
        }

        /// <summary>
        /// Attempts to set the value of the given field.
        /// Fails if:
        /// <list type="bullet">
        /// <item>
        /// index is out of bounds
        /// </item>
        /// </list>
        /// </summary>
        public bool TrySetFieldValue(int index, NodeValue value)
        {
            if (index >= fieldValues.Count)
                return false;

            if (NodeValue.IsNotAssignableTo(value.Type, Definition.Fields[index].Default.Type))
                return false;

            fieldValues[index] = value;
            return true;
        }

        /// <summary>
        /// Attempts to set the value of the given field.
        /// Fails if:
        /// <list type="bullet">
        /// <item>
        /// index is out of bounds
        /// </item>
        /// <item>
        /// the given value does not match the type of the field
        /// </item>
        /// </list>
        /// </summary>
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
