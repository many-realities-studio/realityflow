using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using RealityFlow.Scripting;
using Microsoft.CodeAnalysis;

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
        [Multiline]
        public string EvaluationCode;
        public bool ExecutionInput;
        public List<string> ExecutionOutputs;

        public bool IsRoot => !ExecutionInput && ExecutionOutputs.Count != 0;
        public bool IsPure => !ExecutionInput && ExecutionOutputs.Count == 0;

        Action<EvalContext> eval;
        List<Diagnostic> diagnostics = new();
        public Action<EvalContext> GetEvaluation()
        {
            if (eval is null)
            {
                switch ((string.IsNullOrEmpty(EvaluationMethod), string.IsNullOrEmpty(EvaluationCode)))
                {
                    case (true, true):
                        Debug.LogError("One of EvaluationMethod and EvaluationCode required");
                        return null;
                    case (false, true):
                        string[] parts = EvaluationMethod.Split(';');
                        Type type = typeof(Node).Assembly.GetType(parts[0]);
                        MethodInfo method = type.GetMethod(parts[1]);
                        eval =
                            (Action<EvalContext>)Delegate.CreateDelegate(
                                typeof(Action<EvalContext>),
                                method
                            );
                        break;
                    case (true, false):
                        eval =
                            ScriptUtilities.GetAction<EvalContext>(EvaluationCode, diagnostics, "ctx");
                        foreach (var diag in diagnostics)
                            switch (diag.Severity)
                            {
                                case DiagnosticSeverity.Error:
                                    Debug.LogError(diag);
                                    break;
                                case DiagnosticSeverity.Warning:
                                    Debug.LogWarning(diag);
                                    break;
                                case DiagnosticSeverity.Info:
                                    Debug.Log(diag);
                                    break;
                            }
                        diagnostics.Clear();
                        break;
                    case (false, false):
                        Debug.LogError("Only one of EvaluationMethod and EvaluationCode allowed at once");
                        return null;
                }
            }

            return eval;
        }
    }
}