using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using TransformTypes;
using UnityEngine;

public class ComponentSelectManipulator : MonoBehaviour
{
    private HandleSelectionManager selectionManager;

    private AttachGizmoState gizmoTool;
    private SnapGrid grid;
    private GridTool gridtool;

    private Vector3 lastPosition;
    private Vector3 lastScale;
    private Quaternion lastRotation;

    void Start()
    {
        selectionManager = HandleSelectionManager.Instance;    
        
        GameObject tools = GameObject.Find("RealityFlow Editor");
        gizmoTool = tools.GetComponentInChildren<AttachGizmoState>();

        gizmoTool.EnableLookForTarget(TransformType.All);
        gizmoTool.AttachGizmoToObject(gameObject);

        lastPosition = transform.localPosition;
        lastRotation = transform.localRotation;
        lastScale = transform.localScale;
    }

    private void OnDestroy()
    {
        gizmoTool.DisableLookForTarget();
    }

    public void RemoveGizmo()
    {
        gizmoTool.DisableLookForTarget();
        Destroy(gizmoTool.gizmoContainerInst);
        Destroy(gameObject);
    }

    public GameObject ReturnPairedObj()
    {
        return selectionManager.mesh.gameObject;
    }

    /// <summary>
    /// Moves the manipulator to a set position without moving any of the components
    /// </summary>
    public void SafeUpdatePosition(Vector3 newPos)
    {
        transform.localPosition = newPos;
        lastPosition = newPos;

        if(gizmoTool)
        {
            gizmoTool.gizmoContainerInst.transform.position = newPos;
        }
    }

    private void UpdateHandlePosition()
    {
        EditableMesh mesh = selectionManager.mesh;
        mesh.GetComponent<MeshVisulization>().UpdateHandlePositions();
    }

    private void TranslateSelection()
    {
        EditableMesh mesh = selectionManager.mesh;
        Vector3 offset = transform.localPosition - lastPosition;
        ComponentTransformations.TranslateVertices(mesh, offset);
        selectionManager.handleSelector.UpdateHandlePositions();
        //UpdateHandlePosition();

    }

    private void RotateSelection()
    {
        EditableMesh mesh = selectionManager.mesh;
        Quaternion rotation = transform.localRotation * Quaternion.Inverse(lastRotation);
        ComponentTransformations.RotateVertices(mesh, rotation);
        selectionManager.handleSelector.UpdateHandlePositions();
        //UpdateHandlePosition();
    }

    private void ScaleSelection()
    {
        EditableMesh mesh = selectionManager.mesh;
        Vector3 invertedLastScale = Vector3.zero;
        invertedLastScale.x = 1 / lastScale.x;
        invertedLastScale.y = 1 / lastScale.y;
        invertedLastScale.z = 1 / lastScale.z;
        Vector3 newScale = Vector3.Scale(transform.localScale, invertedLastScale);
        ComponentTransformations.ScaleVertices(mesh, newScale);
        selectionManager.handleSelector.UpdateHandlePositions();
        //UpdateHandlePosition();
    }

    private void HideBoundingBox()
    {
        SelectToolManager selectToolManager = selectionManager.mesh.GetComponent<SelectToolManager>();
        selectToolManager.boundsVisuals.SetActive(false);

    }

    void Update()
    {
        SelectToolManager selectToolManager = selectionManager.mesh.GetComponent<SelectToolManager>();
        if(selectToolManager)
        {
            selectToolManager.boundsVisuals.SetActive(true);
        }

        BoundsControl bc = selectionManager.mesh.GetComponent<BoundsControl>();

        if (transform.position != lastPosition)
        {
            //Debug.Log("Moving Selection!");
            HideBoundingBox();
            TranslateSelection();
            bc.RecomputeBounds();
            //UpdateHandlePosition();
        }

        if(lastRotation != transform.localRotation)
        {
            //Debug.Log("Rotating Selection!");
            HideBoundingBox();
            RotateSelection();
            bc.RecomputeBounds();
            //UpdateHandlePosition();
        }

        if(lastScale != transform.localScale)
        {
            //Debug.Log("Scaling Selection!");
            HideBoundingBox();
            ScaleSelection();
            bc.RecomputeBounds();
            //UpdateHandlePosition();
        }

        lastPosition = transform.localPosition;
        lastRotation = transform.localRotation;
        lastScale = transform.localScale;
    }
}
