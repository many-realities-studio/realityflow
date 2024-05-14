using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;

/// <summary>
/// This class is the parent class for all gizmo transformation classes
/// </summary>
public class GizmoTransform : MonoBehaviour
{
    public XRRayInteractor interactor;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject sphere;
    public GameObject tanSphere;

    public bool lastUpdateRaySelect = false;
    public bool awake = false;

    public Vector3 originalRayForward;
    public Vector3 originalRayPosition;
    public Vector3 originalGizmoPosition;
    public Vector3 originalGizmoScale;

    public GameObject gizmoContainer;
    public GameObject gizmoManager;

    private SnapGrid grid;
    private GridTool gridTool;


    public void Awake()
    {
        rightHand = GameObject.Find("MRTK XR Rig/Camera Offset/Right Controller");
        leftHand = GameObject.Find("MRTK XR Rig/Camera Offset/Left Controller");
        gizmoManager = GameObject.Find("Gizmo Manager");
        SetActiveInteractor();
        awake = true;
    }

    /// <summary>
    /// Returns true for the first time the controller ray is selecting the gizmo
    /// </summary>
    /// <returns>True for the first time the controller ray is selecting the gizmo</returns>
    public bool StartOfRaySelect()
    {
        return !lastUpdateRaySelect && this.GetComponent<MRTKBaseInteractable>().IsRaySelected;
    }

    /// <summary>
    /// Returns true for the first time the controller ray is no longer selecting the gizmo
    /// </summary>
    /// <returns>True for the first time the controller ray is no longer selecting the gizmo</returns>
    public bool EndOfRaySelect()
    {
        return lastUpdateRaySelect && !this.GetComponent<MRTKBaseInteractable>().IsRaySelected;
    }

    /// <summary>
    /// Gets the forward of the ray origin transform
    /// </summary>
    /// <returns>The ray origin transform forward</returns>
    public Vector3 GetRayForward()
    {
        //return interactor.transform.forward;
        return interactor.rayOriginTransform.forward;
    }

    /// <summary>
    /// Gets the position of the ray origin transform
    /// </summary>
    /// <returns>The ray origin transform position</returns>
    public Vector3 GetRayOriginPosition()
    {
        return interactor.rayOriginTransform.position;
    }

    /// <summary>
    /// Gets the name of the parent of the current object
    /// </summary>
    /// <returns>The name of the parent object</returns>
    public String GetParentName()
    {
        return transform.parent.name;
    }

    /// <summary>
    /// Sets the interactor from the current active contoller
    /// </summary>
    /// <returns>True if an interactor on an active contoller was found</returns>
    public bool SetActiveInteractor()
    {
        interactor = rightHand.GetComponentInChildren<XRRayInteractor>();

        if (interactor == null)
            interactor = leftHand.GetComponentInChildren<XRRayInteractor>();

        else if (interactor == null)
            return false;

        return true;
    }

    /// <summary>
    /// Used for testing
    /// </summary>
    /// <param name="position"></param>
    /// <param name="forward"></param>
    /// <param name="offset"></param>
    public void ExampleSphere(Vector3 position, Vector3 forward, float offset)
    {
        sphere.transform.position = position + (forward * offset);
    }

    /// <summary>
    /// Gets the GameObject object the gizmo is manipulating
    /// </summary>
    /// <returns> The GameObject the gizmo is manipulating</returns>
    public GameObject GetAttachedObject()
    {
       return gizmoManager.GetComponent<AttachGizmoState>().attachedGameObject;
    }
    /// <summary>
    /// Sets the active interactor and the orginal data for the ray forward, ray position, and gizmo position
    /// </summary>
    public void InitRayGizmolData()
    {
        SetActiveInteractor();
        originalRayForward = GetRayForward();
        originalRayPosition = GetRayOriginPosition();
        originalGizmoPosition = GetPointInGrid(GetGizmoContainer().transform.position);
    }

    /// <summary>
    /// Gets the GameObject for the gizmo container
    /// </summary>
    /// <returns>The GameObject for the gizmo container</returns>
    public GameObject GetGizmoContainer()
    {
        return gizmoManager.GetComponent<AttachGizmoState>().gizmoContainerInst;
    }

    /// <summary>
    /// Gets the axis associated with the parent of the current object if applicable
    /// </summary>
    /// <returns>An axis unit vector if applicable, otherwise, returns the zero vector</returns>
    public Vector3 GetAxisDirection()
    {
        String name = GetParentName();
        Vector3 direction = new Vector3(0, 0, 0);

        if (name == "X")
            direction = new Vector3(1, 0, 0);
        else if (name == "Y")
            direction = new Vector3(0, 1, 0);
        else if (name == "Z")
            direction = new Vector3(0, 0, 1);

        return direction;
    }

    /// <summary>
    /// Gets the current ray cast hits
    /// </summary>
    /// <returns>The current ray cast hits</returns>
    public Vector3 GetRayCastHit()
    {
        RaycastHit rayCastHit = new RaycastHit();
        interactor.TryGetCurrent3DRaycastHit(out rayCastHit);

        return rayCastHit.point;
    }

    /// <summary>
    /// Enables and sets the line renderer information
    /// </summary>
    /// <param name="rendererOrigin">The origin of the line</param>
    /// <param name="direction">The direction of the line</param>
    /// <param name="halfLength">Half of the length of the line</param>
    public void EnableLineRenderer(Vector3 rendererOrigin, Vector3 direction, float halfLength, bool ignoreSnap = false) 
    {
        LineRenderer lineRenderer = GetGizmoContainer().GetComponent<LineRenderer>();
        direction = Vector3.Normalize(direction);
         
        if (!ignoreSnap)
            rendererOrigin = GetPointInGrid(rendererOrigin);

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, direction * -halfLength + rendererOrigin);
        lineRenderer.SetPosition(1, direction * halfLength + rendererOrigin);
    }

    /// <summary>
    /// Disables the line renderer
    /// </summary>
    public void DisableLineRenderer()
    {
        GetGizmoContainer().GetComponent<LineRenderer>().enabled = false;
    }

    public void InitTanSphere()
    {
        tanSphere = Instantiate(sphere);
        tanSphere.SetActive(true);
        tanSphere.layer = LayerMask.NameToLayer("Gizmo");
    }

    /// <summary>
    /// Grabs a reference to the snapping mode for use in determining which snapping mode the owner is in.
    /// </summary>
    private void FindSnappingModeReference()
    {
        grid = FindObjectOfType<SnapGrid>();
        gridTool = FindObjectOfType<GridTool>();
    }

    // TODO: Add XML comments and move methods to proper files
    public Vector3 GetPointInGrid(Vector3 point)
    {
        if (gridTool == null || grid == null)
            FindSnappingModeReference();

        if (gridTool.isActive)
            return grid.GetNearestPointOnGrid(point);

        return point;
    }

    public Vector3 GetStretchInGrid(Vector3 inScale)
    {
        if (gridTool == null || grid == null)
            FindSnappingModeReference();

        if (!gridTool.isActive)
            return inScale;

        string name = GetParentName();

        if (name == "X")
            inScale.x = grid.GetNearestStretch(inScale.x);
        else if (name == "Y")
            inScale.y = grid.GetNearestStretch(inScale.y);
        else if (name == "Z")
            inScale.z = grid.GetNearestStretch(inScale.z);

        return inScale;
    }

    public Vector3 GetScaleInGrid(Vector3 inScale, Vector3 originalScale)
    {
        if (gridTool == null || grid == null)
            FindSnappingModeReference();

        if (!gridTool.isActive)
            return inScale;

        return grid.GetNearestScale(inScale, originalScale);
    }

    public Vector3 GetRotationInGrid(Vector3 inRotation)
    {
        if (gridTool == null || grid == null)
            FindSnappingModeReference();

        if (!gridTool.isActive)
            return inRotation;

        string name = GetParentName();

        if (name == "X")
            inRotation.x = grid.GetNearestRotation(inRotation.x);
        else if (name == "Y")
            inRotation.y = grid.GetNearestRotation(inRotation.y);
        else if (name == "Z")
            inRotation.z = grid.GetNearestRotation(inRotation.z);

        return inRotation;
    }

    public Vector3 CheckForPlaneOffset(Vector3 position)
    {
        if (!gridTool.isActive)
            return position;
            
        if (GetAttachedObject().GetComponent<EditableMesh>().baseShape != ShapeType.Plane)
            return position;

        Debug.Log("Pass");

        if (Mathf.Abs(position.y) == 0f)
            position.y += 0.05f;

        Debug.Log(position.y);

        return position;
    }
}
