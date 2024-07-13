using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RealityFlow.NodeGraph;

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
            redoStack.Clear();
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

    public void ClearActionStack()
    {
        actionStack.Clear();
        Debug.Log("Cleared action stack.");
    }

    public void ClearRedoStack()
    {
        redoStack.Clear();
        Debug.Log("Cleared redo stack.");
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
