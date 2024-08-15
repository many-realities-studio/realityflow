using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ManipulationMode
{ 
    mObject,
    vertex,
    edge,
    face
}

/// <summary>
/// This file provides the bridge between the palette and mesh manipulations through the use of an action. 
/// The action currently is only used in MeshVisualization.cs to display the correct handles.
/// </summary>
public class MeshManipulationModes : MonoBehaviour
{
    public static event Action<ManipulationMode> OnManipulationModeChange;

   /* private ManipulationTool manipulationTool;

    public void Awake()
    {
        GameObject tools = GameObject.Find("RealityFlow Editor");
        manipulationTool = tools.GetComponent<ManipulationTool>();
    }*/

    public void EnterVertexMode()
    {
        ChangeMode(ManipulationMode.vertex);
    }

    public void EnterEdgeMode()
    {
        ChangeMode(ManipulationMode.edge);
    }

    public void EnterFaceMode()
    {
        ChangeMode(ManipulationMode.face);
    }

    public void ExitMode()
    {
        ChangeMode(ManipulationMode.mObject);
    }

    private void ChangeMode(ManipulationMode mode)
    {
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            // consider changing to HandleSelectionManager.Instance.ClearSelectedHandlesAndVertices();
            HandleSelectionManager handleSelectionManager = HandleSelectionManager.Instance;
            handleSelectionManager.ClearSelectedHandlesAndVertices();
            
            if(mode != ManipulationMode.mObject)
            {
                handleSelectionManager.gizmoTool.isActive = true;
            } else
            {
                handleSelectionManager.gizmoTool.isActive = false;
            }

            OnManipulationModeChange(mode);
        }
    }
}
