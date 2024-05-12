using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using RealityFlow.Utilities.Proxy;
using UnityEngine;

namespace RealityFlow.Utilities
{
    public static class ScriptUtilities
    {
        static AppDomain scriptDomain;
        static ScriptProxy proxy;

        static CSharpCompilation compilation;

        static ScriptUtilities()
        {
            Assembly dotnetStandardAsm = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .First(asm => asm.GetName().Name == "netstandard");

            compilation = CSharpCompilation.Create("scripts")
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(dotnetStandardAsm.Location),
                    MetadataReference.CreateFromFile(typeof(UnityEngine.Object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Debug).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ScriptUtilities).Assembly.Location)
                )
                .WithOptions(new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary
                ));

            var appInfo = new AppDomainSetup()
            {
                ApplicationName = "RealityFlowScripts",
                ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                DynamicBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            };
            var domain = AppDomain.CreateDomain("realityflow-scripts", new Evidence(), appInfo);
            scriptDomain = domain;

            proxy = (ScriptProxy)scriptDomain.CreateInstanceAndUnwrap(
                typeof(ScriptProxy).Assembly.FullName,
                typeof(ScriptProxy).FullName
            );

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                if (!asm.IsDynamic)
                    proxy.LoadStaticAssembly(asm.Location);
        }

        /// <summary>
        /// Create a delegate of type T from a snippet of C# code describing a static method. 
        /// Will be loaded in the global script domain, which must be manually cleaned up later 
        /// when it is convenient.
        /// Pass in a type like <c>Action</c> or <c>Func</c> for F.
        /// 
        /// Returns null if compilation fails, and fills the provided list with warnings and errors
        /// if it is not null.
        /// </summary>
        public static ScriptDelegate<F> CreateDelegate<F>(string code, List<Diagnostic> diagnostics)
        where
            F : Delegate
        {
            var tree = CSharpSyntaxTree.ParseText($"public static class Script {{ {code} }}");

            var csc = compilation.AddSyntaxTrees(tree);

            using var stream = new MemoryStream();
            EmitResult results = csc.Emit(stream);
            stream.Seek(0, SeekOrigin.Begin);

            if (diagnostics != null)
            {
                diagnostics.Clear();
                if (results.Diagnostics.Length > 0)
                    diagnostics.AddRange(results.Diagnostics);
            }

            if (results.Success)
                return proxy.LoadDelegateFromAppDomain<F>(stream);

            return null;
        }

        /// <summary>
        /// <b>WILL ABORT ALL CURRENTLY RUNNING SCRIPTS ON OTHER THREADS</b>
        /// 
        /// <para>
        /// Will free the AppDomain containing the scripts created by other methods in this class.
        /// Will abort all scripts! Do not call this if any scripts are still possibly running.
        /// In the absence of multithreading this will hopefully not be a problem, since all unity
        /// gameobjects run their updates on a single thread.
        /// </para>
        /// </summary>
        public static void FreeScripts()
        {
            try
            {
                AppDomain.Unload(scriptDomain);
                Debug.Log("Freed App Domain");
            }
            catch (CannotUnloadAppDomainException)
            {
                Debug.LogError("Failed to unload script AppDomain");
            }
        }
    }
}