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

    public void UndoLastAction()
    {
        Debug.Log("Attempting to undo last action.");
        Debug.Log($"Action stack count before undo: {GetActionStackCount()}");

        StartUndo();
        var lastAction = GetLastAction();
        EndUndo();

        if (lastAction == null)
        {
            Debug.Log("No actions to undo.");
            return;
        }

        if (lastAction is CompoundAction compoundAction)
        {
            foreach (var action in compoundAction.Actions)
            {
                UndoSingleAction(action);
            }
        }
        else
        {
            UndoSingleAction(lastAction);
        }

        Debug.Log($"Action stack after undo: {GetActionStackCount()}");
    }

    public void RedoLastAction()
    {
        Debug.Log("Attempting to redo last action.");
        Debug.Log($"Redo stack count before redo: {GetRedoStackCount()}");

        if (redoStack.Count == 0)
        {
            Debug.Log("No actions to redo.");
            return;
        }

        StartRedo();
        var lastRedoAction = GetLastRedoAction();
        EndRedo();

        if (lastRedoAction is CompoundAction compoundAction)
        {
            foreach (var action in compoundAction.Actions)
            {
                ExecuteSingleAction(action);
            }
        }
        else
        {
            ExecuteSingleAction(lastRedoAction);
        }

        Debug.Log($"Redo stack after redo: {GetRedoStackCount()}");
    }

    private void UndoSingleAction(LoggedAction action)
    {
        switch (action.FunctionName)
        {
            case nameof(RealityFlowAPI.SpawnObject):
                string prefabName = (string)action.Parameters[0] + "(Clone)";
                Debug.Log("The spawned object's name is " + prefabName);
                GameObject spawnedObject = RealityFlowAPI.Instance.FindSpawnedObject(prefabName);
                if (spawnedObject != null)
                {
                    RealityFlowAPI.Instance.DespawnObject(spawnedObject);
                }
                break;

            case nameof(RealityFlowAPI.DespawnObject):
                string objectToDespawnName = ((string)action.Parameters[0]).Replace("(Clone)", "").Trim();
                Debug.Log("Undoing the despawn of object named " + objectToDespawnName);
                Vector3 positionToDespawn = (Vector3)action.Parameters[1];
                Quaternion rotationToDespawn = (Quaternion)action.Parameters[2];
                Vector3 scaleToDespawn = (Vector3)action.Parameters[3];
                GameObject respawnedObject = RealityFlowAPI.Instance.SpawnObject(objectToDespawnName, positionToDespawn, scaleToDespawn, rotationToDespawn, RealityFlowAPI.SpawnScope.Peer);
                if (respawnedObject != null)
                {
                    respawnedObject.transform.localScale = scaleToDespawn;
                }
                break;

            case nameof(RealityFlowAPI.UpdateObjectTransform):
                string objectNameToUpdate = (string)action.Parameters[0];
                Vector3 oldPositionToUpdate = (Vector3)action.Parameters[1];
                Quaternion oldRotationToUpdate = (Quaternion)action.Parameters[2];
                Vector3 oldScaleToUpdate = (Vector3)action.Parameters[3];
                Debug.Log("Undoing the transform of object named " + objectNameToUpdate);
                GameObject objToUpdate = RealityFlowAPI.Instance.FindSpawnedObject(objectNameToUpdate);
                if (objToUpdate != null)
                {
                    RealityFlowAPI.Instance.UpdateObjectTransform(objectNameToUpdate, oldPositionToUpdate, oldRotationToUpdate, oldScaleToUpdate);
                }
                else
                {
                    Debug.LogError($"Object named {objectNameToUpdate} not found during undo transform.");
                }
                break;

            case nameof(RealityFlowAPI.AddNodeToGraph):
                Graph graphToUndo = (Graph)action.Parameters[0];
                NodeIndex indexToUndo = (NodeIndex)action.Parameters[2];
                graphToUndo.RemoveNode(indexToUndo);
                break;

                // Add cases for other functions...
        }
    }

    private void ExecuteSingleAction(LoggedAction action)
    {
        switch (action.FunctionName)
        {
            case nameof(RealityFlowAPI.SpawnObject):
                string prefabNameToExecute = (string)action.Parameters[0];
                Vector3 positionToExecute = (Vector3)action.Parameters[1];
                Vector3 scaleToExecute = (Vector3)action.Parameters[2];
                Quaternion rotationToExecute = (Quaternion)action.Parameters[3];
                RealityFlowAPI.SpawnScope scopeToExecute = (RealityFlowAPI.SpawnScope)action.Parameters[4];
                RealityFlowAPI.Instance.SpawnObject(prefabNameToExecute, positionToExecute, scaleToExecute, rotationToExecute, scopeToExecute);
                break;

            case nameof(RealityFlowAPI.DespawnObject):
                string objNameToExecute = (string)action.Parameters[0] + "(Clone)";
                GameObject spawnedObjectToExecute = RealityFlowAPI.Instance.FindSpawnedObject(objNameToExecute);
                if (spawnedObjectToExecute != null)
                {
                    RealityFlowAPI.Instance.DespawnObject(spawnedObjectToExecute);
                }
                break;

            case nameof(RealityFlowAPI.UpdateObjectTransform):
                string objectNameToExecute = (string)action.Parameters[0];
                Vector3 positionToExecuteUpdate = (Vector3)action.Parameters[1];
                Quaternion rotationToExecuteUpdate = (Quaternion)action.Parameters[2];
                Vector3 scaleToExecuteUpdate = (Vector3)action.Parameters[3];
                RealityFlowAPI.Instance.UpdateObjectTransform(objectNameToExecute, positionToExecuteUpdate, rotationToExecuteUpdate, scaleToExecuteUpdate);
                break;

            case nameof(RealityFlowAPI.AddNodeToGraph):
                Graph graphToExecute = (Graph)action.Parameters[0];
                NodeDefinition defToExecute = (NodeDefinition)action.Parameters[1];
                graphToExecute.AddNode(defToExecute);
                break;

                // Add cases for other functions...
        }
    }
}
