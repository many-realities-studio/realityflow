using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionLogger
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
    }
    private bool isUndoing = false;

    private Stack<LoggedAction> actionStack = new Stack<LoggedAction>();
    private CompoundAction currentCompoundAction;

    public void LogAction(string functionName, params object[] parameters)
    {

        if (isUndoing) return;

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
        }
    }

    public LoggedAction GetLastAction()
    {
        var action = actionStack.Count > 0 ? actionStack.Pop() : null;
        if (action != null)
        {
            Debug.Log($"Popped action: {action.FunctionName}");
        }
        else
            Debug.Log("No actions in stack to pop");
        return action;
    }
    public int GetActionStackCount()
    {
        return actionStack.Count;
    }
    public void StartUndo()
    {
        isUndoing = true;
    }

    public void EndUndo()
    {
        isUndoing = false;
    }
    public void ClearActionStack()
    {
        actionStack.Clear();
        Debug.Log("Cleared action stack.");
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
