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


    public RoslynCodeRunner()
    {

    }

    readonly List<Diagnostic> diagnostics = new();
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
                Debug.LogError("failed to compile chatgpt response");
                return;
            }

            if (asm.DefinedTypes.Count() < 1)
            {
                Debug.LogError("ChatGPT response didn't define a type");
                return;
            }
            if (asm.DefinedTypes.Count() > 1)
            {
                Debug.LogError("ChatGPT response defined more than one type");
                return;
            }

            Type type = asm.DefinedTypes.Single();

            if (!type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                Debug.LogError("ChatGPT response returned non-monobehaviour");
                return;
            }

            gameObject.AddComponent(type);
        }
        catch (Exception mainCodeException)
        {
            Logger.Instance.LogError(mainCodeException.Message);
        }
    }
}