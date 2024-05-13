using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;

namespace RealityFlow.Scripting
{
    public static class ScriptUtilities
    {
        static CSharpCompilation compilation;

        static ScriptUtilities()
        {
            compilation = CSharpCompilation.Create("scripts")
                .AddReferences(
                    AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .Where(asm => !asm.IsDynamic)
                    .Select(asm => MetadataReference.CreateFromFile(asm.Location))
                )
                .WithOptions(new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    reportSuppressedDiagnostics: false
                ));

            CSharpScript.EvaluateAsync(ScriptOptions.Default.)
        }

        static Assembly Compile(CSharpCompilation csc, List<Diagnostic> diagnostics)
        {
            using var stream = new MemoryStream();
            EmitResult results = csc.Emit(stream);
            stream.Seek(0, SeekOrigin.Begin);

            if (diagnostics != null && results.Diagnostics.Length > 0)
                diagnostics.AddRange(results.Diagnostics);

            if (results.Success)
            {
                Assembly asm = Assembly.Load(stream.ToArray());
                return asm;
            }

            return null;
        }

        /// <summary>
        /// Compile a single C# file into an assembly.
        /// If diagnostics is not null, adds any warnings/errors to it.
        /// Returns null if not successful.
        /// </summary>
        public static Assembly CompileToAssembly(string file, List<Diagnostic> diagnostics)
        {
            var csc = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(file));
            return Compile(csc, diagnostics);
        }

        /// <summary>
        /// Compile a collection of C# files into an assembly.
        /// If diagnostics is not null, adds any warnings/errors to it.
        /// Returns null if not successful.
        /// </summary>
        public static Assembly CompileToAssembly(IEnumerable<string> files, List<Diagnostic> diagnostics)
        {
            var csc = compilation.AddSyntaxTrees(
                files.Select(file => CSharpSyntaxTree.ParseText(file))
            );
            return Compile(csc, diagnostics);
        }

        /// <summary>
        /// Produces an Action from the body of a C# method. 
        /// If diagnostics is not null, adds any warnings/errors to it.
        /// Returns null if not successful.
        /// </summary>
        public static Action GetAction(string body, List<Diagnostic> diagnostics)
        {
            Assembly asm = CompileToAssembly(
                $@"
                #pragma warning disable CS8019
                using System;
                using System.Collections.Generic;
                using UnityEngine;
                using Ubiq;
                
                static class Script {{ 
                    public static void Eval() 
                    {{ 
                        {body};
                    }} 
                }}",
                diagnostics
            );
            if (asm is null)
                return null;
            return (Action)Delegate.CreateDelegate(
                typeof(Action),
                asm.GetType("Script").GetMethod("Eval")
            );
        }

        /// <summary>
        /// Produces a Func<T> from the body of a C# method. 
        /// This C# method body must return a value of type T.
        /// If diagnostics is not null, adds any warnings/errors to it.
        /// Returns null if not successful.
        /// </summary>
        public static Func<T> GetFunc<T>(string body, List<Diagnostic> diagnostics)
        {
            Assembly asm = CompileToAssembly(
                $@"
                using System;
                using System.Collections.Generic;
                using UnityEngine;
                using Ubiq;
                
                static class Script {{ 
                    public static {typeof(T).FullName} Eval() 
                    {{
                        {body};
                    }} 
                }}",
                diagnostics
            );
            if (asm is null)
                return null;
            return (Func<T>)Delegate.CreateDelegate(
                typeof(Func<T>),
                asm.GetType("Script").GetMethod("Eval")
            );
        }
    }
}