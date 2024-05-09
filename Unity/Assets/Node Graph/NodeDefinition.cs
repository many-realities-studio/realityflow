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
        public bool ExecutionInput;
        public List<string> ExecutionOutputs;
        public string EvaluationMethod;

        public bool IsRoot => !ExecutionInput && ExecutionOutputs.Count != 0;
        public bool IsPure => !ExecutionInput && ExecutionOutputs.Count == 0;
    }
}