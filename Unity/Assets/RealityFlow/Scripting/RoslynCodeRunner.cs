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
        Logger.Instance.LogInfo("Executing Runcode...");

        updatedCode = string.IsNullOrEmpty(updatedCode) ? null : updatedCode;
        try
        {
            code = $"{updatedCode ?? code}";

            Assembly asm = RealityFlow.Scripting.ScriptUtilities.CompileToAssembly(code, diagnostics);

            foreach (var diag in diagnostics)
            {
                if (diag.Severity is DiagnosticSeverity.Warning)
                    Debug.LogWarning(diag);
                else if (diag.Severity is DiagnosticSeverity.Error)
                    Debug.LogError(diag);
                else if (diag.Severity is DiagnosticSeverity.Info)
                    Debug.Log(diag);
            }

            diagnostics.Clear();

            if (asm is null)
            {
                Debug.LogError("Failed to compile chatgpt response");
                return;
            }

            Type[] comps =
                asm
                .GetTypes()
                .Where(ty => ty.IsSubclassOf(typeof(MonoBehaviour)))
                .ToArray();

            if (comps.Length < 1)
            {
                Debug.LogError("ChatGPT response didn't define a MonoBehaviour");
                return;
            }
            if (comps.Length > 1)
            {
                Debug.LogError("ChatGPT response defined more than one MonoBehaviour");
                return;
            }

            Type type = comps.Single();

            //gameObject.AddComponent(type);
        }
        catch (Exception mainCodeException)
        {
            Logger.Instance.LogError(mainCodeException.Message);
        }
    }
}
