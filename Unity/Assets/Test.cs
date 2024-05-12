using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using RealityFlow.Utilities;
using UnityEngine;

public class Test : MonoBehaviour
{
    List<Diagnostic> diagnostics = new();
    Action printy;
    
    // Start is called before the first frame update
    void Start()
    {
        printy =
            ScriptUtilities.CreateDelegate<Action>(
                "public static void Print() {  }",
                diagnostics
            );

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Debug.Log(asm.FullName);
        }

        if (diagnostics.Count > 0)
            diagnostics.ForEach(diag => Debug.Log(diag.ToString()));
    }

    // Update is called once per frame
    void Update()
    {
        printy?.Invoke();
    }

    void OnDestroy()
    {
        ScriptUtilities.FreeScripts();
    }
}
