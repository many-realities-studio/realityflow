using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionLogger
{
    public interface ILogAction
    {
        string Name();

        void Undo();
    }

    public class CompoundAction : ILogAction
    {
        public List<ILogAction> Actions { get; } = new();

        public string Name() => "CompoundAction";

        public void Undo()
        {
            foreach (var action in Actions)
                action.Undo();
        }

        public void AddAction(ILogAction action)
        {
            Actions.Add(action);
        }
    }

    private readonly Stack<ILogAction> actionStack = new();
    private CompoundAction currentCompoundAction;

    public void LogAction<A>(A action)
    where
        A : ILogAction
    {
        if (currentCompoundAction != null)
        {
            currentCompoundAction.AddAction(action);
            Debug.Log($"Added action {action.Name()} to compound action.");
        }
        else
        {
            actionStack.Push(action);
            Debug.Log($"Logged action: {action.Name()}");
        }
    }

    public ILogAction GetLastAction()
    {
        var action = actionStack.Count > 0 ? actionStack.Pop() : null;
        if (action != null)
        {
            Debug.Log($"Popped action: {action.Name()}");
        }
        else
            Debug.Log("No actions in stack to pop");
        return action;
    }

    public int GetActionStackCount()
    {
        return actionStack.Count;
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
