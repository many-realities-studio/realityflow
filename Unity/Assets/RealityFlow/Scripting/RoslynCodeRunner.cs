using DilmerGames.Core.Singletons;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class RoslynCodeRunner : Singleton<RoslynCodeRunner>
{
    [SerializeField]
    private string[] namespaces;

    [SerializeField]
    public string codeExecutionGameObjectName;

    [SerializeField]
    [TextArea(15, 35)]
    private string code;

    [SerializeField]
    private UnityEvent OnRunCodeCompleted;

    [SerializeField]
    private string[] resultVars;

    [SerializeField]
    [TextArea(5, 20)]
    private string resultInfo;

    private readonly List<Diagnostic> diagnostics = new List<Diagnostic>();

    public void RunCode(string updatedCode = null)
    {
        Logger.Instance.LogInfo("Executing RunCode...");

        updatedCode = string.IsNullOrEmpty(updatedCode) ? code : updatedCode;
        try
        {
            // Compile the provided code into an assembly
            Assembly asm = RealityFlow.Scripting.ScriptUtilities.CompileToAssembly(updatedCode, diagnostics);

            // Log any compilation diagnostics
            foreach (var diag in diagnostics)
            {
                if (diag.Severity == DiagnosticSeverity.Warning)
                    Debug.LogWarning(diag);
                else if (diag.Severity == DiagnosticSeverity.Error)
                    Debug.LogError(diag);
                else if (diag.Severity == DiagnosticSeverity.Info)
                    Debug.Log(diag);
            }

            // Clear diagnostics for next compilation
            diagnostics.Clear();

            // Check if the compilation resulted in a valid assembly
            if (asm == null)
            {
                Debug.LogError("Failed to compile code.");
                return;
            }

            // Find a type that contains a static 'Execute' method
            Type type = asm.GetTypes().FirstOrDefault(t => t.GetMethod("Execute", BindingFlags.Static | BindingFlags.Public) != null);
            if (type == null)
            {
                Debug.LogError("No static Execute method found.");
                return;
            }

            // Get the static 'Execute' method
            MethodInfo method = type.GetMethod("Execute", BindingFlags.Static | BindingFlags.Public);
            if (method == null)
            {
                Debug.LogError("Execute method not found.");
                return;
            }

            // Invoke the static 'Execute' method
            method.Invoke(null, null);
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during the process
            Logger.Instance.LogError(ex.Message);
        }
    }


}
