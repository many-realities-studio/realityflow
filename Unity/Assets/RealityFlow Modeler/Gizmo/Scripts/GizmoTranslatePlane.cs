using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class is used for the plane translate functionality of the gizmo
/// </summary>
public class GizmoTranslatePlane : GizmoTransform
{
    public Vector3 originalIntersection;

    void Update()
    {
        if (!awake) return;

        if (StartOfRaySelect())
        {
            InitRayGizmolData();
            originalIntersection = PlaneIntersectionTest(
                GetGizmoContainer().transform.position,
                Vector3.Normalize(GetPlaneNormal())
            );

            lastUpdateRaySelect = true;
        }

        else if (EndOfRaySelect())
        {
            lastUpdateRaySelect = false;
        }

        if (lastUpdateRaySelect)
        {
            Vector3 newIntersection = PlaneIntersectionTest(
                GetGizmoContainer().transform.position, 
                Vector3.Normalize(GetPlaneNormal())
            );

            Vector3 newGizmoPosition = originalGizmoPosition;

            newGizmoPosition -= GetPointInGrid(originalIntersection - newIntersection);
            newGizmoPosition = CheckForPlaneOffset(newGizmoPosition);

            GetGizmoContainer().transform.position = newGizmoPosition;
            GetAttachedObject().transform.position = newGizmoPosition;

            Debug.Log("The translate plane attached object... = " + GetAttachedObject());
        }
    }

    /// <summary>
    /// Finds the intersection between a plane and contoller ray
    /// </summary>
    /// <param name="planePoint">A point on the plane</param>
    /// <param name="planeNormal">The normal of the plane</param>
    /// <returns>The intersection between a plane and contoller ray</returns>
    public Vector3 PlaneIntersectionTest(Vector3 planePoint, Vector3 planeNormal)
    {
        Vector3 rayOrigin = GetRayOriginPosition();
        Vector3 rayDirection = GetRayForward();
        Vector3 intersectionPoint = new Vector3();

        float numerator = 0;
        float denominator = 0;

        for (int i = 0; i < 3; i++)
        {
            numerator += (planeNormal[i] * planePoint[i]) - (planeNormal[i] * rayOrigin[i]);
            denominator += planeNormal[i] * rayDirection[i];
        }

        float t = numerator / denominator;

        for (int i = 0; i < 3; i++)
        {
            intersectionPoint[i] =  rayOrigin[i] + rayDirection[i] * t;
        }

        return intersectionPoint;
    }

    /// <summary>
    /// Finds the plane normal based on the gizmo plane selected
    /// </summary>
    /// <returns>The plane normal based on the gizmo plane selected</returns>
    Vector3 GetPlaneNormal()
    {
        String name = GetParentName();
        if (name == "XY")
            return Vector3.Cross(new Vector3(1, 0, 0), new Vector3(0, 1, 0));

        else if (name == "XZ")
            return Vector3.Cross(new Vector3(1, 0, 0), new Vector3(0, 0, 1));

        else if (name == "YZ")
            return Vector3.Cross(new Vector3(0, 1, 0), new Vector3(0, 0, 1));

        else
            return new Vector3(0, 0, 0);
    }
}


/*
/// <summary>
/// This class is used for the plane translate functionality of the gizmo
/// </summary>
public class GizmoTranslatePlane : GizmoTransform
{
    public Vector3 originalIntersection;

    private ComponentTranslation currentOperation;
    private Vector3 startPos;

    void Update()
    {
        if (!awake) return;

        if (StartOfRaySelect())
        {
            GetAttachedObject().GetComponent<MeshFilter>().mesh.RecalculateBounds();
            InitRayGizmolData();
            originalIntersection = PlaneIntersectionTest(
                GetGizmoContainer().transform.position,
                Vector3.Normalize(GetPlaneNormal())
            );

            lastUpdateRaySelect = true;
            BeginMeshOperation();
        }

        else if (EndOfRaySelect())
        {
            lastUpdateRaySelect = false;
            EndMeshOperation();
        }

        if (lastUpdateRaySelect)
        {
            Vector3 newIntersection = PlaneIntersectionTest(
                GetGizmoContainer().transform.position, 
                Vector3.Normalize(GetPlaneNormal())
            );

            Vector3 newGizmoPosition = originalGizmoPosition;

            newGizmoPosition -= GetPointInGrid(originalIntersection - newIntersection);
            newGizmoPosition = CheckForPlaneOffset(newGizmoPosition);

            GetGizmoContainer().transform.position = newGizmoPosition;
            GetAttachedObject().transform.position = newGizmoPosition;
        }
    }

    /// <summary>
    /// Finds the intersection between a plane and contoller ray
    /// </summary>
    /// <param name="planePoint">A point on the plane</param>
    /// <param name="planeNormal">The normal of the plane</param>
    /// <returns>The intersection between a plane and contoller ray</returns>
    public Vector3 PlaneIntersectionTest(Vector3 planePoint, Vector3 planeNormal)
    {
        Vector3 rayOrigin = GetRayOriginPosition();
        Vector3 rayDirection = GetRayForward();
        Vector3 intersectionPoint = new Vector3();

        float numerator = 0;
        float denominator = 0;

        for (int i = 0; i < 3; i++)
        {
            numerator += (planeNormal[i] * planePoint[i]) - (planeNormal[i] * rayOrigin[i]);
            denominator += planeNormal[i] * rayDirection[i];
        }

        float t = numerator / denominator;

        for (int i = 0; i < 3; i++)
        {
            intersectionPoint[i] =  rayOrigin[i] + rayDirection[i] * t;
        }

        return intersectionPoint;
    }

    /// <summary>
    /// Finds the plane normal based on the gizmo plane selected
    /// </summary>
    /// <returns>The plane normal based on the gizmo plane selected</returns>
    Vector3 GetPlaneNormal()
    {
        String name = GetParentName();
        if (name == "XY")
            return Vector3.Cross(new Vector3(1, 0, 0), new Vector3(0, 1, 0));

        else if (name == "XZ")
            return Vector3.Cross(new Vector3(1, 0, 0), new Vector3(0, 0, 1));

        else if (name == "YZ")
            return Vector3.Cross(new Vector3(0, 1, 0), new Vector3(0, 0, 1));

        else
            return new Vector3(0, 0, 0);
    }

    void BeginMeshOperation()
    {
        if (currentOperation == null)
            currentOperation = new ComponentTranslation(HandleSelectionManager.Instance.GetUniqueSelectedIndices());
        startPos = GetAttachedObject().transform.position;
    }

    void EndMeshOperation()
    {
        currentOperation.AddOffsetAmount(GetAttachedObject().transform.position - startPos);
        try
        {
            HandleSelectionManager.Instance.mesh.CacheOperation(currentOperation);
        }
        catch { }
        currentOperation = null;
    }
}
*/