using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

        static string GetScriptCode(string body, Type returnType, (Type type, string name)[] arguments)
        {
            string argumentList = arguments
                .Select(arg => $"{arg.type.FullName} {arg.name}")
                .Aggregate((accum, next) => $"{accum}, {next}")
                .ToString();
            return $@"
                #pragma warning disable CS8019
                using System;
                using System.Collections.Generic;
                using UnityEngine;
                using Ubiq;
                
                static class Script {{ 
                    public static {returnType?.FullName ?? "void"} Eval({argumentList}) 
                    {{ 
                        {body};
                    }} 
                }}";
        }

        /// <summary>
        /// Produces an Action from the body of a C# method. 
        /// If diagnostics is not null, adds any warnings/errors to it.
        /// Returns null if not successful.
        /// </summary>
        public static Action GetAction(string body, List<Diagnostic> diagnostics)
        {
            Assembly asm = CompileToAssembly(
                GetScriptCode(body, returnType: null, Array.Empty<(Type, string)>()),
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
        /// Produces an Action from the body of a C# method. 
        /// If diagnostics is not null, adds any warnings/errors to it.
        /// Returns null if not successful.
        /// </summary>
        public static Action<T1> GetAction<T1>(string body, List<Diagnostic> diagnostics, string arg1Name)
        {
            Assembly asm = CompileToAssembly(
                GetScriptCode(body, returnType: null, new[] {
                    (typeof(T1), arg1Name)
                }),
                diagnostics
            );
            if (asm is null)
                return null;
            return (Action<T1>)Delegate.CreateDelegate(
                typeof(Action<T1>),
                asm.GetType("Script").GetMethod("Eval")
            );
        }

        /// <summary>
        /// Produces an Action from the body of a C# method. 
        /// If diagnostics is not null, adds any warnings/errors to it.
        /// Returns null if not successful.
        /// </summary>
        public static Action<T1, T2> GetAction<T1, T2>(
            string body,
            List<Diagnostic> diagnostics,
            string arg1Name,
            string arg2Name
        )
        {
            Assembly asm = CompileToAssembly(
                GetScriptCode(body, returnType: null, new[] {
                    (typeof(T1), arg1Name),
                    (typeof(T2), arg2Name)
                }),
                diagnostics
            );
            if (asm is null)
                return null;
            return (Action<T1, T2>)Delegate.CreateDelegate(
                typeof(Action<T1, T2>),
                asm.GetType("Script").GetMethod("Eval")
            );
        }

        /// <summary>
        /// Produces a Func<T> from the body of a C# method. 
        /// This C# method body must return a value of type T.
        /// If diagnostics is not null, adds any warnings/errors to it.
        /// Returns null if not successful.
        /// </summary>
        public static Func<R> GetFunc<R>(string body, List<Diagnostic> diagnostics)
        {
            Assembly asm = CompileToAssembly(
                GetScriptCode(body, typeof(R), Array.Empty<(Type, string)>()),
                diagnostics
            );
            if (asm is null)
                return null;
            return (Func<R>)Delegate.CreateDelegate(
                typeof(Func<R>),
                asm.GetType("Script").GetMethod("Eval")
            );
        }

        /// <summary>
        /// Produces a Func<T> from the body of a C# method. 
        /// This C# method body must return a value of type T.
        /// If diagnostics is not null, adds any warnings/errors to it.
        /// Returns null if not successful.
        /// </summary>
        public static Func<R, T1> GetFunc<R, T1>(string body, List<Diagnostic> diagnostics, string arg1Name)
        {
            Assembly asm = CompileToAssembly(
                GetScriptCode(body, typeof(R), new[] {
                    (typeof(T1), arg1Name)
                }),
                diagnostics
            );
            if (asm is null)
                return null;
            return (Func<R, T1>)Delegate.CreateDelegate(
                typeof(Func<R, T1>),
                asm.GetType("Script").GetMethod("Eval")
            );
        }

        /// <summary>
        /// Produces a Func<T> from the body of a C# method. 
        /// This C# method body must return a value of type T.
        /// If diagnostics is not null, adds any warnings/errors to it.
        /// Returns null if not successful.
        /// </summary>
        public static Func<R, T1, T2> GetFunc<R, T1, T2>(
            string body,
            List<Diagnostic> diagnostics,
            string arg1Name,
            string arg2Name
        )
        {
            Assembly asm = CompileToAssembly(
                GetScriptCode(body, typeof(R), new[] {
                    (typeof(T1), arg1Name),
                    (typeof(T1), arg2Name)
                }),
                diagnostics
            );
            if (asm is null)
                return null;
            return (Func<R, T1, T2>)Delegate.CreateDelegate(
                typeof(Func<R, T1, T2>),
                asm.GetType("Script").GetMethod("Eval")
            );
        }
    }
}