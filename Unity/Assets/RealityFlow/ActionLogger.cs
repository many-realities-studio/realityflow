using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Collections.Generic;

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

    private Stack<LoggedAction> actionStack = new Stack<LoggedAction>();

    public void LogAction(string functionName, params object[] parameters)
    {
        actionStack.Push(new LoggedAction(functionName, parameters));
    }

    public LoggedAction GetLastAction()
    {
        return actionStack.Count > 0 ? actionStack.Pop() : null;
    }
}