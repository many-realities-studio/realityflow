using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using UnityEngine;

namespace RealityFlow.Scripting
{
    public class Test : MonoBehaviour
    {
        List<Diagnostic> diagnostics = new();
        Action<string> printy;

        // Start is called before the first frame update
        void Start()
        {
            printy =
                    ScriptUtilities.GetAction<string>(
                        "Debug.Log(\"The script is working!\" + \" \" + whatToPrint);",
                        diagnostics,
                        "whatToPrint"
                    );

            if (diagnostics.Count > 0)
                diagnostics.ForEach(diag => Debug.Log(diag.ToString()));
        }

        // Update is called once per frame
        void Update()
        {
            printy("Test");
        }
    }
}