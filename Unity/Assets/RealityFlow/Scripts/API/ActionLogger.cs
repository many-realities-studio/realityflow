using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RealityFlow.NodeGraph;
using Newtonsoft.Json;
using System;
using System.Reflection;

public class ActionLogger : MonoBehaviour
{
    public class LoggedAction
    {
        public string FunctionName { get; }
        public object[] Parameters { get; }

        public LoggedAction(string functionName, object[] parameters)
        {
            FunctionName = functionName;
            Parameters = parameters;
        }

        public virtual string GetDescription()
        {
            switch (FunctionName)
            {
                case "MoveObject":
                    return $"Moved object to position {Parameters[0]}";
                case "DeleteObject":
                    return $"Deleted object with ID {Parameters[0]}";
                case "CreateObject":
                    return $"Created object of type {Parameters[0]}";
                default:
                    return $"Executed action: {FunctionName}";
            }
        }
    }

    public class CompoundAction : LoggedAction
    {
        public List<LoggedAction> Actions { get; }

        public CompoundAction() : base("CompoundAction", null)
        {
            Actions = new List<LoggedAction>();
        }

        public void AddAction(LoggedAction action)
        {
            Actions.Add(action);
        }

        public override string GetDescription()
        {
            return $"Compound action with {Actions.Count} sub-actions";
        }
    }

    private bool isUndoing = false;
    private bool isRedoing = false;
    public Stack<LoggedAction> actionStack = new Stack<LoggedAction>();
    public Stack<LoggedAction> redoStack = new Stack<LoggedAction>();
    private CompoundAction currentCompoundAction;

    private List<string> codeQueue = new List<string>();
    private Queue<string> jsonActionQueue = new Queue<string>();

    // Dictionary to store method references
    private Dictionary<string, MethodInfo> apiMethods;

    private void Awake()
    {
        // Initialize the dictionary with method names and their corresponding MethodInfo objects, specifying parameter types
        apiMethods = new Dictionary<string, MethodInfo>
        {
            { "SpawnObject", typeof(RealityFlowAPI).GetMethod("SpawnObject", new Type[] { typeof(string), typeof(Vector3), typeof(Vector3), typeof(Quaternion), typeof(int) }) },
            { "DespawnObject", typeof(RealityFlowAPI).GetMethod("DespawnObject", new Type[] { typeof(string) }) },
            { "UpdateObjectTransform", typeof(RealityFlowAPI).GetMethod("UpdateObjectTransform", new Type[] { typeof(string), typeof(Vector3), typeof(Quaternion) }) },
            { "AddNodeToGraph", typeof(RealityFlowAPI).GetMethod("AddNodeToGraph", new Type[] { typeof(string), typeof(string), typeof(Vector3) }) },
            { "RemoveNodeFromGraph", typeof(RealityFlowAPI).GetMethod("RemoveNodeFromGraph", new Type[] { typeof(string) }) },
            { "AddDataEdgeToGraph", typeof(RealityFlowAPI).GetMethod("AddDataEdgeToGraph", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) }) },
            { "RemoveDataEdgeFromGraph", typeof(RealityFlowAPI).GetMethod("RemoveDataEdgeFromGraph", new Type[] { typeof(string) }) },
            { "AddExecEdgeToGraph", typeof(RealityFlowAPI).GetMethod("AddExecEdgeToGraph", new Type[] { typeof(string), typeof(string) }) },
            { "RemoveExecEdgeFromGraph", typeof(RealityFlowAPI).GetMethod("RemoveExecEdgeFromGraph", new Type[] { typeof(string) }) },
            { "SetNodePosition", typeof(RealityFlowAPI).GetMethod("SetNodePosition", new Type[] { typeof(string), typeof(Vector3) }) },
            { "SetNodeFieldValue", typeof(RealityFlowAPI).GetMethod("SetNodeFieldValue", new Type[] { typeof(string), typeof(string), typeof(string) }) },
            { "SetNodeInputConstantValue", typeof(RealityFlowAPI).GetMethod("SetNodeInputConstantValue", new Type[] { typeof(string), typeof(string), typeof(string) }) },
            { "AddVariableToGraph", typeof(RealityFlowAPI).GetMethod("AddVariableToGraph", new Type[] { typeof(string), typeof(string) }) },
            { "RemoveVariableFromGraph", typeof(RealityFlowAPI).GetMethod("RemoveVariableFromGraph", new Type[] { typeof(string) }) },
            { "SetTemplate", typeof(RealityFlowAPI).GetMethod("SetTemplate", new Type[] { typeof(string), typeof(bool) }) },
            { "SetStatic", typeof(RealityFlowAPI).GetMethod("SetStatic", new Type[] { typeof(string), typeof(bool) }) },
            { "SetRigidbodyFromStaticState", typeof(RealityFlowAPI).GetMethod("SetRigidbodyFromStaticState", new Type[] { typeof(string), typeof(bool) }) },
            { "SetCollidable", typeof(RealityFlowAPI).GetMethod("SetCollidable", new Type[] { typeof(string), typeof(bool) }) },
            { "SetGravity", typeof(RealityFlowAPI).GetMethod("SetGravity", new Type[] { typeof(string), typeof(bool) }) },
            { "SetUIText", typeof(RealityFlowAPI).GetMethod("SetUIText", new Type[] { typeof(string), typeof(string) }) },
            { "PlaySound", typeof(RealityFlowAPI).GetMethod("PlaySound", new Type[] { typeof(string) }) },
            { "AssignGraph", typeof(RealityFlowAPI).GetMethod("AssignGraph", new Type[] { typeof(string), typeof(Graph) }) },
            { "CreateNodeGraphAsync", typeof(RealityFlowAPI).GetMethod("CreateNodeGraphAsync", new Type[] { typeof(string) }) },
            { "UpdatePrefab", typeof(RealityFlowAPI).GetMethod("UpdatePrefab", new Type[] { typeof(string) }) },
            { "InstantiateNonPersisted", typeof(RealityFlowAPI).GetMethod("InstantiateNonPersisted", new Type[] { typeof(string), typeof(Vector3), typeof(Quaternion) }) },
            { "DestroyNonPersisted", typeof(RealityFlowAPI).GetMethod("DestroyNonPersisted", new Type[] { typeof(string) }) },
            { "LogActionToServer", typeof(RealityFlowAPI).GetMethod("LogActionToServer", new Type[] { typeof(string) }) },
            // Add other method mappings here as needed
        };
    }

    public void LogJsonAction(string jsonAction)
    {
        jsonActionQueue.Enqueue(jsonAction);
        Debug.Log($"Logged JSON action: {jsonAction}");
    }

    public IEnumerator ExecuteJsonActionsCoroutine()
    {
        while (jsonActionQueue.Count > 0)
        {
            string jsonAction = jsonActionQueue.Dequeue();
            Debug.Log($"Executing JSON action: {jsonAction}");


            var structuredAction = JsonConvert.DeserializeObject<StructuredAction>(jsonAction);
            ExecuteAction(structuredAction);

            // Debug.LogError($"Failed to execute JSON action: {ex.Message}");

            yield return null; // Wait for the next frame to continue execution
        }
    }

    private void ExecuteAction(StructuredAction action)
    {
        if (RealityFlowAPI.Instance == null)
        {
            Debug.LogError("RealityFlowAPI.Instance is not initialized.");
            return;
        }

        if (apiMethods.TryGetValue(action.Action, out MethodInfo method))
        {
            // Prepare the parameters array according to the method's expected signature
            var parameters = PrepareParameters(action.Parameters, method);
            if (parameters != null)
            {
                // Dynamically invoke the method
                method.Invoke(RealityFlowAPI.Instance, parameters);
            }
            else
            {
                Debug.LogError($"Parameters for action '{action.Action}' are invalid or missing.");
            }
        }
        else
        {
            Debug.LogError($"Unsupported action: {action.Action}");
        }
    }

    private object[] PrepareParameters(Dictionary<string, object> actionParams, MethodInfo method)
    {
        if (actionParams == null)
        {
            Debug.LogError("actionParams is null.");
            return null;
        }

        if (method == null)
        {
            Debug.LogError("method is null.");
            return null;
        }

        ParameterInfo[] methodParams = method.GetParameters();
        if (methodParams == null)
        {
            Debug.LogError("methodParams is null.");
            return null;
        }

        object[] parameters = new object[methodParams.Length];

        for (int i = 0; i < methodParams.Length; i++)
        {
            ParameterInfo param = methodParams[i];

            if (param == null)
            {
                Debug.LogError($"param at index {i} is null.");
                return null;
            }

            if (actionParams.TryGetValue(param.Name, out object value))
            {
                if (param.ParameterType == null)
                {
                    Debug.LogError($"param.ParameterType for '{param.Name}' is null.");
                    return null;
                }

                if (param.ParameterType == typeof(Vector3))
                {
                    parameters[i] = JsonConvert.DeserializeObject<Vector3>(value.ToString());
                }
                else if (param.ParameterType == typeof(Quaternion))
                {
                    parameters[i] = JsonConvert.DeserializeObject<Quaternion>(value.ToString());
                }
                else
                {
                    parameters[i] = Convert.ChangeType(value, param.ParameterType);
                }
            }
            else
            {
                Debug.LogError($"Missing parameter '{param.Name}' for action '{method.Name}'.");
                return null;
            }
        }

        return parameters;
    }



    public void LogAction(string functionName, params object[] parameters)
    {
        if (isUndoing || isRedoing) return;

        var action = new LoggedAction(functionName, parameters);
        if (currentCompoundAction != null)
        {
            currentCompoundAction.AddAction(action);
            Debug.Log($"Added action {functionName} to compound action.");
        }
        else
        {
            actionStack.Push(action);
            Debug.Log($"Logged action: {functionName}");
            // redoStack.Clear();
        }
    }

    public void LogGeneratedCode(string code)
    {
        codeQueue.Add(code);
        Debug.Log($"Logged generated code.");
    }

    public IEnumerator ExecuteLoggedCodeCoroutine()
    {
        foreach (var code in codeQueue)
        {
            yield return StartCoroutine(RoslynCodeRunner.Instance.RunCodeCoroutine(code));
        }
        codeQueue.Clear();
        Debug.Log("Executed all logged code sequentially.");
    }

    public void ClearCodeQueue()
    {
        codeQueue.Clear();
        Debug.Log("Cleared code queue.");
    }

    public LoggedAction GetLastAction()
    {
        var action = actionStack.Count > 0 ? actionStack.Pop() : null;
        if (action != null)
        {
            Debug.Log($"Popped action: {action.FunctionName}");
            if (isUndoing)
            {
                redoStack.Push(action);
                Debug.Log($"Saved action to redo stack: {action.FunctionName}");
            }
        }
        else
        {
            Debug.Log("No actions in stack to pop");
        }
        return action;
    }

    public LoggedAction GetLastRedoAction()
    {
        var action = redoStack.Count > 0 ? redoStack.Pop() : null;
        Debug.LogError("The current count of the redo stack is " + redoStack.Count);
        if (action != null)
        {
            Debug.Log($"Popped redo action: {action.FunctionName}");
            if (isRedoing)
            {
                actionStack.Push(action);
                Debug.Log($"Saved action back to action stack: {action.FunctionName}");
            }
        }
        else
        {
            Debug.Log("No actions in redo stack to pop");
        }
        return action;
    }

    public int GetActionStackCount()
    {
        return actionStack.Count;
    }

    public int GetRedoStackCount()
    {
        return redoStack.Count;
    }

    public void StartUndo()
    {
        isUndoing = true;
    }

    public void EndUndo()
    {
        isUndoing = false;
    }

    public void StartRedo()
    {
        isRedoing = true;
    }

    public void EndRedo()
    {
        isRedoing = false;
    }

    public void StartCompoundAction()
    {
        currentCompoundAction = new CompoundAction();
    }

    public void EndCompoundAction()
    {
        if (currentCompoundAction != null)
        {
            actionStack.Push(currentCompoundAction);
            currentCompoundAction = null;
        }
    }
}
