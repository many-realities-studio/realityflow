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


public class MeshManipulationModes : MonoBehaviour
{
    public static event Action<ManipulationMode> OnManipulationModeChange;

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
        // Only run this script if you are the owner of the palette
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            HandleSelectionManager handleSelectionManager = HandleSelectionManager.Instance;
            handleSelectionManager.ClearSelectedHandlesAndVertices();
            OnManipulationModeChange(mode);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
