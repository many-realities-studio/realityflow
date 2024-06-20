using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Ubiq.Avatars;

/// <summary>
/// Class SelectToolManager keeps track of the current active state of the Select Tool and
/// allows the selection of the mesh this script is attached to.
/// </summary>
public class SelectToolManager : MonoBehaviour
{
    private SelectTool selectTool;
    private EraserTool eraserTool;
    private ColorTool colorTool;
    public AttachGizmoState gizmoTool;
    public ManipulationTool manipulationTool;
    private CopyTool copyTool;
    [HideInInspector] public GameObject boundsVisuals;
    private Material boundsMaterial;
    private bool lastHandlesActiveState;
    // These variables are used to keep track of the active states of all tools that should show bounds visuals
    private bool lastSelectToolActiveState, lastEraserToolActiveState, lastColorToolActiveState, lastMetallicToolActiveState, lastSmoothnessToolActiveState, lastManipulationsToolActiveState;
    private bool isSelected, deselectOnRelease;
    
    void Start()
    {
        // Find RealityFlow tools that need hover selection support
        GameObject tools = GameObject.Find("RealityFlow Editor");
        selectTool = tools.GetComponent<SelectTool>();
        eraserTool = tools.GetComponent<EraserTool>();
        colorTool = tools.GetComponent<ColorTool>();
        gizmoTool = tools.GetComponentInChildren<AttachGizmoState>();
        manipulationTool = tools.GetComponent<ManipulationTool>();
        copyTool = tools.GetComponent<CopyTool>();

        lastHandlesActiveState = gameObject.GetComponent<BoundsControl>().HandlesActive;

        // Find the child game object of this mesh that draws the bounds visuals
        foreach (Transform child in gameObject.transform)
        {
            if (child.gameObject.name.Contains("BoundingBox"))
            {
                boundsVisuals = child.gameObject;
            }

            // Find the immediate child game object of the bounding box that contains the instanced material
            foreach (Transform child2 in boundsVisuals.transform)
            {
                try
                {
                    boundsMaterial = child2.gameObject.GetComponent<MeshRenderer>().material;
                }
                catch(Exception e)
                {
                    Debug.LogError(e + " Does " + child2.gameObject.name + " have the ThickerSqueezableBox material?");
                }
                break;
            }
        }

        try
        {
            // By default, the bounds visuals should be off
            boundsVisuals.SetActive(false);
        }
        catch (NullReferenceException e)
        {
            Debug.LogError(e + " Bounds visuals for the mesh: " + gameObject.name + " was not found.");
        }

        // Ensure our meshes created through the palette is not flat
        gameObject.GetComponent<BoundsControl>().FlattenMode = FlattenMode.Never;
    }

    /// <summary>
    /// If the select, eraser, or color tool is active then disable manipulation for it and show the bounds visuals otherwise turn off bounds visuals
    /// </summary>
    public void PrepareSelectionForMesh()
    {
        if (selectTool.isActive || eraserTool.isActive || colorTool.colorToolIsActive || colorTool.metallicToolIsActive || colorTool.smoothnessToolIsActive || gizmoTool.isActive
            || manipulationTool.isActive || copyTool.isActive)
        {
            gameObject.GetComponent<ObjectManipulator>().AllowedManipulations = TransformFlags.None;
            boundsVisuals.SetActive(true);

        // If the user is using the select tool then this mesh should be able to be selected
        if (selectTool.isActive)
        {
            gameObject.GetComponent<BoundsControl>().ToggleHandlesOnClick = true;
        }

            // If any tool other than the select tool is active then we don't want to select a mesh but rather do its appropriate action
            if (colorTool.colorToolIsActive || colorTool.metallicToolIsActive || colorTool.smoothnessToolIsActive || eraserTool.isActive || gizmoTool.isActive || manipulationTool.isActive || copyTool.isActive)
            {
                gameObject.GetComponent<BoundsControl>().ToggleHandlesOnClick = false;
            }
        }
        else if (!selectTool.isActive || !eraserTool.isActive || !colorTool.colorToolIsActive || !colorTool.metallicToolIsActive || !colorTool.smoothnessToolIsActive)
        {
            if (!gameObject.GetComponent<NetworkedMesh>().isSelected)
            {
                boundsVisuals.SetActive(false);
            }
            else if (gameObject.GetComponent<NetworkedMesh>().isSelected && !gizmoTool.isActive && !manipulationTool.isActive && !copyTool.isActive)
            {
                //Debug.Log("Give all manipulations");
                gameObject.GetComponent<ObjectManipulator>().AllowedManipulations = TransformFlags.Move | TransformFlags.Rotate | TransformFlags.Scale;
            }
        }
        else
        {
            Debug.LogError("Somehow when you hovered over " + gameObject.name + " nothing happened since select tool is = " + selectTool.isActive);
        }
    }

    /// <summary>
    /// Enables all manipulations for the mesh when a mesh is hovered off when no tools are on.
    /// </summary>
    // May not be necessary since only the owner of a mesh should be able to interact with their selected meshes and is redundant 
    public void ReactivateManipulationsOnHoverOff()
    {
        if (!selectTool.isActive && !colorTool.colorToolIsActive && !colorTool.metallicToolIsActive && !colorTool.smoothnessToolIsActive && !eraserTool.isActive && !gizmoTool.isActive && !manipulationTool.isActive && !copyTool.isActive)
        {
            //Debug.Log("ReactivateManipulationsOnHoverOff()");
            gameObject.GetComponent<ObjectManipulator>().AllowedManipulations = TransformFlags.Move | TransformFlags.Rotate | TransformFlags.Scale;
        }
    }

    /// <summary>
    /// Enables all manipulations for the mesh when a tool is turned off.
    /// </summary>
    private void ReactivateManipulationsOnToolOff(bool toolState, ref bool lastToolState)
    {
        if (lastToolState != toolState)
        {
            lastToolState = toolState;

            if (!toolState)
            {
                //Debug.Log("ReactivateManipulationsOnToolOff()");
                gameObject.GetComponent<ObjectManipulator>().AllowedManipulations = TransformFlags.Move | TransformFlags.Rotate | TransformFlags.Scale;
            }
        }
    }

    public void SelectMesh()
    {
        if (!selectTool.isActive)
            return;

        if (!isSelected)
        {
            isSelected = true;
            deselectOnRelease = false;
            MeshSelectionManager.Instance.SelectMesh(gameObject);
        }
    }

    public void DeselectMesh()
    {
        if (!selectTool.isActive)
            return;

        if (deselectOnRelease)
        {
            isSelected = false;
            deselectOnRelease = false;
            MeshSelectionManager.Instance.DeselectMesh(gameObject);
        }
        else
        {
            deselectOnRelease = true;
        }
    }

    void Update()
    {
        // Only the owner should send over commands
        if (gameObject.GetComponent<NetworkedMesh>().owner)
        {
            if (selectTool.isActive)
            {
                // Sends networking data when the handles are enabled or disabled
                if (lastHandlesActiveState != gameObject.GetComponent<BoundsControl>().HandlesActive)
                {
                    lastHandlesActiveState = gameObject.GetComponent<BoundsControl>().HandlesActive;
                    gameObject.GetComponent<NetworkedMesh>().ControlSelection();

                    //SelectMesh();
                }
            }

            if (lastSelectToolActiveState != selectTool.isActive)
            {  
                ReactivateManipulationsOnToolOff(selectTool.isActive, ref lastSelectToolActiveState);
            }

            if (lastEraserToolActiveState != eraserTool.isActive)
            {  
                ReactivateManipulationsOnToolOff(eraserTool.isActive, ref lastEraserToolActiveState);  
            }

            if (lastColorToolActiveState != colorTool.colorToolIsActive)
            {  
                ReactivateManipulationsOnToolOff(colorTool.colorToolIsActive, ref lastColorToolActiveState);  
            } 

            if (lastMetallicToolActiveState != colorTool.metallicToolIsActive)
            {  
                ReactivateManipulationsOnToolOff(colorTool.metallicToolIsActive, ref lastMetallicToolActiveState);  
            }  

            if (lastSmoothnessToolActiveState != colorTool.smoothnessToolIsActive)
            {  
                ReactivateManipulationsOnToolOff(colorTool.smoothnessToolIsActive, ref lastSmoothnessToolActiveState);  
            }

            if (lastManipulationsToolActiveState != manipulationTool.isActive)
            {  
                ReactivateManipulationsOnToolOff(manipulationTool.isActive, ref lastManipulationsToolActiveState);  
            } 
        }
    }
}
