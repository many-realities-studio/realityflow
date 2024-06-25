using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using dotnow;
using dotnow.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using UnityEngine;

namespace RealityFlow.Scripting
{
    public static class ScriptUtilities
    {
        static CSharpCompilation compilation;

        readonly static dotnow.AppDomain Domain = new();

        public static readonly string[] CSharpKeywords = {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this",
            "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort",
            "using", "virtual", "void", "volatile", "while",
        };

        public static readonly HashSet<string> CSharpKeywordSet =
            CSharpKeywords.ToHashSet();

        static List<MetadataReference> references;
        static bool init;

        public static void Init()
        {
            if (init)
                return;

            // references = Resources.LoadAll<AssemblyReferenceAsset>("AssemblyReferences/")
            //     .Select(asm => asm.CompilerReference)
            //     .ToList();

            compilation = CSharpCompilation.Create(null)
                // .AddReferences(references)
                .AddReferences(
                    System.AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .Where(asm => !asm.IsDynamic)
                    .Select(asm => MetadataReference.CreateFromFile(asm.Location))
                )
                .WithOptions(new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    reportSuppressedDiagnostics: false
                ));

            init = true;
        }

        static Assembly Compile(CSharpCompilation csc, List<Diagnostic> diagnostics)
        {
            if (!init)
                throw new Exception("Must init script utils first");

            using var stream = new MemoryStream();
            EmitResult results = csc.Emit(stream);
            stream.Seek(0, SeekOrigin.Begin);

            if (diagnostics != null && results.Diagnostics.Length > 0)
                diagnostics.AddRange(results.Diagnostics);

            if (results.Success)
            {
                Assembly asm = Domain.LoadModuleStream(stream, true);
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
            var csc = compilation
                .AddSyntaxTrees(CSharpSyntaxTree.ParseText(file))
                .WithAssemblyName($"scripts_{Guid.NewGuid()}");
            return Compile(csc, diagnostics);
        }

        /// <summary>
        /// Compile a collection of C# files into an assembly.
        /// If diagnostics is not null, adds any warnings/errors to it.
        /// Returns null if not successful.
        /// </summary>
        public static Assembly CompileToAssembly(IEnumerable<string> files, List<Diagnostic> diagnostics)
        {
            var csc = compilation
                .AddSyntaxTrees(
                    files.Select(file => CSharpSyntaxTree.ParseText(file))
                )
                .WithAssemblyName($"scripts_{Guid.NewGuid()}");
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

            // Simply using `CreateDelegate` is tempting, but produces a broken function pointer
            // and SEGFAULT's unity, so don't do that.
            MethodInfo method = asm.GetType("Script").GetMethod("Eval");
            return () =>
            {
                method.Invoke(null, null);
            };
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

            MethodInfo method = asm.GetType("Script").GetMethod("Eval");
            return t1 =>
            {
                method.Invoke(null, new object[] { t1 });
            };
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
            MethodInfo method = asm.GetType("Script").GetMethod("Eval");
            return (t1, t2) =>
            {
                method.Invoke(null, new object[] { t1, t2 });
            };
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
            MethodInfo method = asm.GetType("Script").GetMethod("Eval");
            return () => (R)method.Invoke(null, null);
        }

        /// <summary>
        /// Produces a Func<T> from the body of a C# method. 
        /// This C# method body must return a value of type T.
        /// If diagnostics is not null, adds any warnings/errors to it.
        /// Returns null if not successful.
        /// </summary>
        public static Func<T1, R> GetFunc<T1, R>(string body, List<Diagnostic> diagnostics, string arg1Name)
        {
            Assembly asm = CompileToAssembly(
                GetScriptCode(body, typeof(R), new[] {
                    (typeof(T1), arg1Name)
                }),
                diagnostics
            );
            if (asm is null)
                return null;
            MethodInfo method = asm.GetType("Script").GetMethod("Eval");
            return t1 => (R)method.Invoke(null, new object[] { t1 });
        }

        /// <summary>
        /// Produces a Func<T> from the body of a C# method. 
        /// This C# method body must return a value of type T.
        /// If diagnostics is not null, adds any warnings/errors to it.
        /// Returns null if not successful.
        /// </summary>
        public static Func<T1, T2, R> GetFunc<T1, T2, R>(
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
            MethodInfo method = asm.GetType("Script").GetMethod("Eval");
            return (t1, t2) => (R)method.Invoke(null, new object[] { t1, t2 });
        }

        public static object CreateInstance(Type type)
            => Domain.CreateInstance(type);
    }
}