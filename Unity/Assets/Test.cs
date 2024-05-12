using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using RealityFlow.Utilities;
using RealityFlow.Utilities.Proxy;
using UnityEngine;

public class Test : MonoBehaviour
{
    List<Diagnostic> diagnostics = new();
    ScriptDelegate<Action> printy;

    // Start is called before the first frame update
    void Start()
    {
        printy =
            ScriptUtilities.CreateDelegate<Action>(
                "public static void Print() { UnityEngine.Debug.Log(\"The script is working!\"); }",
                diagnostics
            );

        if (diagnostics.Count > 0)
            diagnostics.ForEach(diag => Debug.Log(diag.ToString()));
    }

    // Update is called once per frame
    void Update()
    {
        printy.Invoke();
    }

    void OnDestroy()
    {
        ScriptUtilities.FreeScripts();
    }
}
