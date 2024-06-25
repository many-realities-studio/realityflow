using UnityEngine;
using UnityEngine.Events;

public class UndoRedoManager : MonoBehaviour
{
    // Define events that other scripts can subscribe to
    public UnityEvent OnUndoPerformed;
    public UnityEvent OnRedoPerformed;

    // This method can be called by UI button click events to perform an undo action
    public void PerformUndo()
    {
        Debug.Log("Performing undo action...");
        RealityFlowAPI.Instance.UndoLastAction();

        // Invoke the OnUndoPerformed event to notify subscribers
        OnUndoPerformed?.Invoke();
    }
}
