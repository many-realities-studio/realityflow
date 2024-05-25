using DilmerGames.Core.Singletons;
using RoslynCSharp;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class RoslynCodeRunner : DilmerGames.Core.Singletons.Singleton<RoslynCodeRunner>
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
  private string activeCSharpSource = null;
  private ScriptProxy activeCrawlerScript = null;
  private ScriptDomain domain = null;
  public AssemblyReferenceAsset[] assemblyReferences;


  public RoslynCodeRunner()
  {

  }
  
  public void RunCode(string updatedCode = null)
  {
    Logger.Instance.LogInfo("Executing Runcode...");

    domain = ScriptDomain.CreateDomain("MazeCrawlerCode", true);

    // Add assembly references
    foreach (AssemblyReferenceAsset reference in assemblyReferences)
      domain.RoslynCompilerService.ReferenceAssemblies.Add(reference);

    updatedCode = string.IsNullOrEmpty(updatedCode) ? null : updatedCode;
    try
    {
      code = $"{(updatedCode ?? code)}";

      ScriptType type = domain.CompileAndLoadMainSource(code, ScriptSecurityMode.UseSettings, assemblyReferences);

      // Check for null
      if (type == null)
      {
        if (domain.RoslynCompilerService.LastCompileResult.Success == false)
          throw new Exception("Maze crawler code contained errors. Please fix and try again");
        else if (domain.SecurityResult.IsSecurityVerified == false)
          throw new Exception("Maze crawler code failed code security verification");
        else
          throw new Exception("Maze crawler code does not define a class. You must include one class definition of any name that inherits from 'RoslynCSharp.Example.MazeCrawler'");
      }

      // Check for base class
      if (type.IsSubTypeOf<MonoBehaviour>() == false)
        throw new Exception("Maze crawler code must define a single type that inherits from 'RoslynCSharp.Example.MazeCrawler'");




      // Create an instance
      // activeCrawlerScript = type.CreateInstance(mazeMouse);
      activeCSharpSource = code;
      type.CreateInstance(gameObject);


      // SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
      // var root = (CompilationUnitSyntax)tree.GetRoot();
      // var firstMember = root.Members[0];
      // var classInfo = (ClassDeclarationSyntax)firstMember;

      // var addedCodeForGOExecution = $"GameObject.Find(\"{codeExecutionGameObjectName}\").AddComponent<{classInfo.Identifier.Value}>();";
      // var codeToExecute = $"{code} {addedCodeForGOExecution}";
      // ScriptState<object> result = CSharpScript.RunAsync(codeToExecute, SetDefaultImports()).Result;

      // foreach (string var in resultVars)
      // {
      //     resultInfo += $"{result.GetVariable(var).Name}: {result.GetVariable(var).Value}\n";
      // }

      // OnRunCodeCompleted?.Invoke();
    }
    catch (Exception mainCodeException)
    {
      Logger.Instance.LogError(mainCodeException.Message);
    }
  }

  // private ScriptOptions SetDefaultImports()
  // {
  //     return ScriptOptions.Default
  //         .WithImports(namespaces.Select(n => n.Replace("using", string.Empty)
  //         .Trim()))
  //         // TODO - make these configurable instead of having to add each reference manually
  //         .AddReferences(
  //             typeof(MonoBehaviour).Assembly,
  //             typeof(Debug).Assembly,
  //             typeof(TextMeshPro).Assembly,
  //             typeof(IEnumerator).Assembly,
  //             typeof(StarterAssetsInputs).Assembly
  //         );
  // }
}
