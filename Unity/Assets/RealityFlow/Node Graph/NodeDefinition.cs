using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using RealityFlow.Scripting;
using Microsoft.CodeAnalysis;
using NaughtyAttributes;
using System.Linq;
using System.Text;
using UnityEditor;

namespace RealityFlow.NodeGraph
{
    /// <summary>
    /// A definition for a node type. Each instance of a node is created based on a definition.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Node",
        menuName = "NodeGraph/NodeDefinition",
        order = 1
    )]
    public class NodeDefinition : ScriptableObject
    {
        public string Name => name;
        /// <summary>
        /// Stores a name and default value for each field. Allows e.g. initializing graphs.
        /// </summary>
        public List<NodeFieldDefinition> Fields;
        public List<NodePortDefinition> Inputs;
        public List<NodePortDefinition> Outputs;
        /// <summary>
        /// Indicates that a node has a variable number of dynamic (untyped) input ports after
        /// its normal input ports.
        /// </summary>
        public bool IsVariadic;
        /// <summary>
        /// Indicates that a node which has variadic input ports should also have an equal number
        /// of output ports with the same types.
        /// </summary>
        public bool IsVariadicOutput;

        public EvalMethod EvaluationMethod;
        [ShowIf("EvaluationMethod", EvalMethod.MethodLookup)]
        public string EvalLookup;
        [ShowIf("EvaluationMethod", EvalMethod.MethodCode)]
        [TextArea(8, 16)]
        public string EvalCode;

        public bool ExecutionInput;
        public List<string> ExecutionOutputs;

        /// <summary>
        /// True if the node has no execution input and at least one execution output.
        /// </summary>
        public bool IsRoot => !ExecutionInput && ExecutionOutputs.Count != 0;
        /// <summary>
        /// True if the node has no execution inputs or outputs.
        /// </summary>
        public bool IsPure => !ExecutionInput && ExecutionOutputs.Count == 0;

        object scriptObject;
        Action<EvalContext> eval;
        List<Diagnostic> diagnostics = new();
        public Action<EvalContext> GetEvaluation()
        {
            if (eval is null)
            {
                switch (EvaluationMethod)
                {
                    case EvalMethod.MethodLookup:
                        GetEvalWithMethodLookup();
                        break;
                    case EvalMethod.MethodCode:
                        GetEvalWithMethodCode();
                        break;
                }
            }

            return eval;
        }

        private void GetEvalWithMethodLookup()
        {
            string[] parts = EvalLookup.Split(';');
            Type type = typeof(Node).Assembly.GetType(parts[0]);
            MethodInfo method = type.GetMethod(parts[1]);
            eval =
                (Action<EvalContext>)Delegate.CreateDelegate(
                    typeof(Action<EvalContext>),
                    method
                );
        }

        private void GetEvalWithMethodCode()
        {
            StringBuilder scriptFields = new();

            foreach (var (def, i) in Fields
                .Select((def, i) => (def, i))
                .Where(tup => !string.IsNullOrEmpty(tup.def.Name))
            )
            {
                if (ScriptUtilities.CSharpKeywordSet.Contains(def.Name))
                {
                    Debug.LogError($"Name `{def.Name}` is a reserved keyword; please change it");
                    return;
                }

                scriptFields.AppendFormat(
                    "{0} {1} => ctx.GetField<{0}>({2});",
                    NodeValue.GetEvalTimeType(def.DefaultType).FullName,
                    def.Name,
                    i
                );
            }

            foreach (var (def, i) in Inputs
                .Select((def, i) => (def, i))
                .Where(tup => !string.IsNullOrEmpty(tup.def.Name))
            )
            {
                if (ScriptUtilities.CSharpKeywordSet.Contains(def.Name))
                {
                    Debug.LogError($"Name `{def.Name}` is a reserved keyword; please change it");
                    return;
                }

                scriptFields.AppendFormat(
                    "{0} {1} => ctx.GetInput<{0}>({2});",
                    NodeValue.GetEvalTimeType(def.Type),
                    def.Name,
                    i
                );
            }

            foreach (var (def, i) in Outputs
                .Select((def, i) => (def, i))
                .Where(tup => !string.IsNullOrEmpty(tup.def.Name))
            )
            {
                if (ScriptUtilities.CSharpKeywordSet.Contains(def.Name))
                {
                    Debug.LogError($"Name `{def.Name}` is a reserved keyword; please change it");
                    return;
                }

                scriptFields.AppendFormat(
                    @"
                    {0} {1} 
                    {{ 
                        set 
                        {{ 
                            ctx.SetOutput({2}, value);
                        }}
                    }}",
                    NodeValue.GetEvalTimeType(def.Type),
                    def.Name,
                    i
                );
            }

            foreach (var (def, i) in ExecutionOutputs
                .Select((def, i) => (def, i))
                .Where(tup => !string.IsNullOrEmpty(tup.def))
            )
            {
                if (ScriptUtilities.CSharpKeywordSet.Contains(def))
                {
                    Debug.LogError($"Name `{def}` is a reserved keyword; please change it");
                    return;
                }

                scriptFields.AppendFormat(
                    @"
                    void {0}()
                    {{
                        ctx.ExecuteTargetsOfPort({1});
                    }}",
                    def,
                    i
                );
            }

            string scriptName = $"Script_{Name.Replace(" ", "")}";

            string definition = @$"
                            #pragma warning disable CS8019
                            using System;
                            using System.Collections.Generic;
                            using UnityEngine;
                            using Ubiq;
                            using RealityFlow.NodeGraph;

                            public class {scriptName}
                            {{
                                {scriptFields}
                                
                                EvalContext ctx;

                                public void Eval(EvalContext __ctx)
                                {{
                                    ctx = __ctx;
                                    {EvalCode}
                                }}
                            }}
                        ";

            Assembly asm = ScriptUtilities.CompileToAssembly(definition, diagnostics);
            Type scriptType = asm?.GetType(scriptName);

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

            if (scriptType is null)
            {
                Debug.LogError("Failed to load script type");
                return;
            }

            object script = ScriptUtilities.CreateInstance(scriptType);
            MethodInfo scriptMethod = scriptType.GetMethod("Eval");
            eval = ctx =>
            {
                scriptMethod.Invoke(script, new[] { ctx });
            };
        }

        [Serializable]
        public enum EvalMethod
        {
            MethodLookup,
            MethodCode,
        }

        public string GetDescriptor()
        {
            string fields =
                Fields.Count > 0
                ? Fields
                .Select(field => field.GetDescriptor())
                .Aggregate((acc, next) => $"{acc},\n{next}")
                : string.Empty;
            string inputs =
                Inputs.Count > 0
                ? Inputs
                .Select(input => input.GetDescriptor())
                .Aggregate((acc, next) => $"{acc},\n{next}")
                : string.Empty;
            string outputs = 
                Outputs.Count > 0
                ? Outputs
                .Select(output => output.GetDescriptor())
                .Aggregate((acc, next) => $"{acc},\n{next}")
                : string.Empty;

            string source =
                EvaluationMethod == EvalMethod.MethodCode
                ? $@"
                csharpSourceCode: ""
                    {EvalCode}
                "",
                "
                : string.Empty;

            string execOutputs = 
                ExecutionOutputs.Count > 0
                ? ExecutionOutputs
                .Aggregate((acc, next) => $"{acc},\n{next}")
                : string.Empty;

            return $@"{{
                name: ""{Name}"",
                fields: [
                    {fields}
                ],
                inputs: [
                    {inputs}
                ],
                outputs: [
                    {outputs}
                ],
                {source}
                hasExecutionPathInput: {ExecutionInput},
                executionPathOutputs: [
                    {execOutputs}
                ],
            }}";
        }
    }
}