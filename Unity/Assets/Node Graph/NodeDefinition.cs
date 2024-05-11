using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

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
        public string EvaluationMethod;
        public bool ExecutionInput;
        public List<string> ExecutionOutputs;

        public bool IsRoot => !ExecutionInput && ExecutionOutputs.Count != 0;
        public bool IsPure => !ExecutionInput && ExecutionOutputs.Count == 0;

        Action<Node, EvalContext> eval;
        public Action<Node, EvalContext> GetEvaluation()
        {
            if (eval is null)
            {
                string[] parts = EvaluationMethod.Split(';');
                Type type = typeof(Node).Assembly.GetType(parts[0]);
                MethodInfo method = type.GetMethod(parts[1]);
                eval = 
                    (Action<Node, EvalContext>)Delegate.CreateDelegate(
                        typeof(Action<Node, EvalContext>), 
                        method
                    );
            }

            return eval;
        }
    }
}