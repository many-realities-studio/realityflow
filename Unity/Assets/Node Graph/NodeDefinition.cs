using UnityEngine;
using System.Collections.Generic;

namespace RealityFlow.NodeGraph
{
    [CreateAssetMenu(
        fileName = "Node",
        menuName = "NodeGraph/NodeDefinition",
        order = 1
    )]
    public class NodeDefinition : ScriptableObject
    {
        public string Name => name;
        public List<NodePortDefinition> Inputs;
        public List<NodePortDefinition> Outputs;
        public List<NodeValueTypeConstraint> TypeVariableConstraints;
        public bool ExecutionInput;
        public List<string> ExecutionOutputs;
        public string EvaluationMethod;

        public bool IsRoot => !ExecutionInput && ExecutionOutputs.Count != 0;
        public bool IsPure => !ExecutionInput && ExecutionOutputs.Count == 0;

        public int? HighestTypeVariableIndex()
        {
            int highest = -1;

            for (int i = 0; i < Inputs.Count; i++)
            {
                int varIndex = (Inputs[i].Type.Data as NodeValueType.Kind.TypeVariable)?.Index ?? -1;
                if (highest < varIndex)
                    highest = varIndex;
            }

            for (int i = 0; i < Outputs.Count; i++)
            {
                int varIndex = (Outputs[i].Type.Data as NodeValueType.Kind.TypeVariable)?.Index ?? -1;
                if (highest < varIndex)
                    highest = varIndex;
            }

            return highest != -1 ? highest : null;
        }
    }
}