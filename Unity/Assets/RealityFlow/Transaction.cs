using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transaction : MonoBehaviour
{
    public List<ActionLogger.LoggedAction> Actions { get; private set; }

    public Transaction()
    {
        Actions = new List<ActionLogger.LoggedAction>();
    }

    public void AddAction(ActionLogger.LoggedAction action)
    {
        Actions.Add(action);
    }
}
