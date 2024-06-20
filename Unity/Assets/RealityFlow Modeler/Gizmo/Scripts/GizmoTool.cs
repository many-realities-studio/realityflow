using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TransformTypes;

public class GizmoTool : MonoBehaviour
{
    public bool isActive;
    public GameObject gizmoManager;

    public void SetGizmoMode(int newMode)
    {
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            TransformType mode;

            if (newMode == 1)
                mode = TransformType.Translate;
            else if (newMode == 2)
                mode = TransformType.Rotate;
            else if (newMode == 3)
                mode = TransformType.Scale;
            else
                mode = TransformType.All;

            gizmoManager.GetComponent<AttachGizmoState>().EnableLookForTarget(mode);
            // not sure if this still needs to be set
            // gizmoManager.GetComponent<AttachGizmoState>().isActive = isActive;
        }
    }

    public void Activate(int tool, bool status)
    {
        if (tool == 10)
        {
            if (isActive && !status)
            {
                gizmoManager.GetComponent<AttachGizmoState>().DisableLookForTarget();
                gizmoManager.GetComponent<AttachGizmoState>().DisableMeshRaySelection();
            }

            isActive = status;
            // not sure if this still needs to be set
            gizmoManager.GetComponent<AttachGizmoState>().isActive = status;
            
        }
    }
}