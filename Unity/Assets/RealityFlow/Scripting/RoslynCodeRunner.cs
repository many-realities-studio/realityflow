using DilmerGames.Core.Singletons;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
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

    public IEnumerator RunCodeCoroutine(string updatedCode = null)
    {
        Logger.Instance.LogInfo("Compiling and executing new script...");

        // Ensure previous diagnostics are cleared
        diagnostics.Clear();

        updatedCode = string.IsNullOrEmpty(updatedCode) ? code : updatedCode;
        bool compilationCompleted = false;
        Assembly asm = null;

        // Run the compilation on a separate thread
        new System.Threading.Thread(() =>
        {
            asm = RealityFlow.Scripting.ScriptUtilities.CompileToAssembly(updatedCode, diagnostics);
            compilationCompleted = true;
        }).Start();

        // Wait until the compilation is done
        while (!compilationCompleted)
        {
            yield return null;
        }

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
            yield break;
        }

        // Find a type that contains a static 'Execute' method
        Type type = asm.GetTypes().Where(t => t.GetMethod("Execute", BindingFlags.Static | BindingFlags.Public) != null).FirstOrDefault();
        if (type == null)
        {
            Debug.LogError("No static Execute method found.");
            yield break;
        }

        // Get the static 'Execute' method
        MethodInfo method = type.GetMethod("Execute", BindingFlags.Static | BindingFlags.Public);
        if (method == null)
        {
            Debug.LogError("Execute method not found.");
            yield break;
        }

        // Invoke the static 'Execute' method asynchronously on the main thread
        bool methodInvocationCompleted = false;
        UnityMainThreadDispatcher.RunOnMainThread(() =>
        {
            try
            {
                method.Invoke(null, null);
            }
            catch (Exception e)
            {
                Debug.LogError("Error during script execution: " + e.Message);
            }
            methodInvocationCompleted = true;
        });

        // Wait until the method invocation is done
        while (!methodInvocationCompleted)
        {
            yield return null;
        }
    }




}
