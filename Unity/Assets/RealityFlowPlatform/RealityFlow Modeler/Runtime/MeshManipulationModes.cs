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

    /*private ManipulationTool manipulationTool;

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
        // Only run this script if you are the owner of the palette
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            //Debug.Log(NetworkedPalette.reference.owner + NetworkedPalette.reference.ownerName);
            HandleSelectionManager handleSelectionManager = HandleSelectionManager.Instance;
            handleSelectionManager.ClearSelectedHandlesAndVertices();
            OnManipulationModeChange(mode);
            //manipulationTool.SetManipulationMode(mode);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
