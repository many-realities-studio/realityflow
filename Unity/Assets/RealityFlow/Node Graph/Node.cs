using System;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Modes;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class Node : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public NodeDefinition Definition;
        public string DefinitionName;

        [SerializeField]
        List<NodeValueWrapper> fieldValues = new();
        [SerializeField]
        List<InputNodePort> inputs = new();
        [SerializeField]
        List<OutputNodePort> outputs = new();
        [SerializeField]
        int variadicInputs;

        public int VariadicInputs => variadicInputs;

        public Vector2 Position;

        public Node(NodeDefinition definition)
        {
            Definition = definition;
            for (int i = 0; i < definition.Fields.Count; i++)
                fieldValues.Add(new(definition.Fields[i].DefaultValue));
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
                value = fieldValues[index].Value;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetField<T>(int index, out T field)
        {
            if (index < fieldValues.Count)
            {
                NodeValue nodeValue = fieldValues[index].Value;
                if (NodeValue.TryGetValue(nodeValue, out field))
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

            if (NodeValue.IsNotAssignableTo(value.ValueType, Definition.Inputs[index].Type))
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

            if (NodeValue.IsNotAssignableTo(value.ValueType, Definition.Fields[index].DefaultType))
                return false;

            fieldValues[index] = new(value);
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

            fieldValues[index] = new(NodeValue.From(value, Definition.Fields[index].DefaultType));
            return true;
        }

        public void SetInputConstant(int index, NodeValue value)
        {
            inputs[index].ConstantValue = value;
        }

        public void OnBeforeSerialize()
        {
            DefinitionName = Definition.Name;
        }

        public void OnAfterDeserialize()
        {
            if (DefinitionName == null)
            {
                Debug.LogWarning("Node definition name null in serialized node from DB");
                return;
            }
            
            RealityFlowAPI.Instance.NodeDefinitionDict.TryGetValue(DefinitionName, out Definition);

            if (Definition == null)
                Debug.LogWarning("Node definition with unknown definition name");
        }
    }
}
