using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit;
using TransformTypes;

/// <summary>
/// This class is used as an interface to enable the gizmo functionality from the palette
/// </summary>
public class GizmoMenu : MonoBehaviour
{
    public XRRayInteractor interactor;
    private AttachGizmoState gizmoManager;
    private StatefulInteractable[] buttonToggleStates;

    private void Start()
    {
        // Only run this script if you are the owner of the palette
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            gizmoManager = GameObject.Find("Gizmo Manager").GetComponent<AttachGizmoState>();

            // Grab a reference to the game object that holds all of the transformation buttons
            foreach (Transform child in gameObject.transform)
            {
                if (child.gameObject.name.Contains("Grid"))
                {
                    buttonToggleStates = child.gameObject.GetComponentsInChildren<StatefulInteractable>();
                }
            }
        }
    }

    /// <summary>
    /// Enables the gizmo of a type to attach to selected objects
    /// </summary>
    void EnableLookForTarget(TransformType tType)
    {
        // Only run this script if you are the owner of the palette
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            gizmoManager.isActive = true;
            gizmoManager.EnableLookForTarget(tType);
        }
    }

    /// <summary>
    /// Enables the gizmo of all type to attach to selected objects
    /// </summary>
    public void EnableLookForAll()
    {
        EnableLookForTarget(TransformType.All);
    }

    /// <summary>
    /// Enables the gizmo of translate type to attach to selected objects
    /// </summary>
    public void EnableLookForTranslate()
    {
        EnableLookForTarget(TransformType.Translate);
    }

    /// <summary>
    /// Enables the gizmo of scale type to attach to selected objects
    /// </summary>
    public void EnableLookForScale()
    {
        EnableLookForTarget(TransformType.Scale);
    }
     
    /// <summary>
    /// Enables the gizmo of rotate type to attach to selected objects
    /// </summary>
    public void EnableLookForRotate()
    {
        EnableLookForTarget(TransformType.Rotate);
    }

    /// <summary>
    /// Disables the gizmo from attaching to selected objects
    /// </summary>
    public void DisableLookForTarget()
    {
        // Only run this script if you are the owner of the palette
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            //gizmoManager.isActive = false;
            gizmoManager.DisableLookForTarget();

            // Check if no buttons are toggled and if so then turn off the gizmo tool
            if (gizmoManager.isActive)
            {
                for (int i = 1; i < buttonToggleStates.Length; i++)
                {
                    if (buttonToggleStates[i].IsToggled)
                        return;
                }

                gizmoManager.isActive = false;
            }
        }
    }
}
