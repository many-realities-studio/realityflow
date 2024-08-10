using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Spawning;
using UnityEngine;

/// <summary>
/// This class is used for the axis translate functionality of the gizmo
/// </summary>
public class GizmoTranslateAxis : GizmoTransform
{
    public Vector3 originalClosestApproach;
    public bool validLineProj;
    public float originalCrossDirection;
    public float crossThreshold = .15f;
    public float maxTranslateJump = 10;

    private ComponentTranslation currentOperation;
    private Vector3 startPos;

    // Update is called once per frame
    void Update()
    {
        if (!awake) return;

        if (StartOfRaySelect())
        {
            InitRayGizmolData();
            InitLineData();
            EnableLineRenderer(transform.position, GetAxisDirection(), 100f); 

            lastUpdateRaySelect = true;
            //BeginMeshOperation();
        }

        else if (EndOfRaySelect())
        {
            DisableLineRenderer();

            lastUpdateRaySelect = false;
            //EndMeshOperation();
        }

        if (lastUpdateRaySelect)
        {
            Vector3 newClosestApproach = GetPositionFromTranslate(GetGizmoContainer().transform.position, GetAxisDirection());
            Vector3 newGizmoPosition = originalGizmoPosition;

            if (!IsValidPosition()) return;

            newGizmoPosition -= GetPointInGrid(originalClosestApproach - newClosestApproach);
            // newGizmoPosition = CheckForPlaneOffset(newGizmoPosition);

            if (!IsValidDistance(newGizmoPosition)) return;

            GetGizmoContainer().transform.position = newGizmoPosition;
            GetAttachedObject().transform.position = newGizmoPosition;

            RealityFlowAPI.Instance.UpdateObjectTransform(GetAttachedObject().name);

            Debug.Log("The translate axis attached object... = " + GetAttachedObject());
        }
    }

    

    /// <summary>
    /// Checks if the point used for the new position is valid
    /// </summary>
    /// <returns>True if the point is valid, otherwise, false</returns>
    public bool IsValidPosition()
    {
        if (originalCrossDirection != GetCrossDirection(GetAxisDirection(), GetRayForward()))
            return false;
        if (NearParallel(GetAxisDirection(), GetRayForward()))
            return false;

        return true;
    } 

    /// <summary>
    /// Checks if the point used for the new position is at a valid distance
    /// </summary>
    /// <param name="newGizmoPosition">The new position to be tested</param>
    /// <returns>True if it is a valid distance, otherwise, false</returns>
    public bool IsValidDistance(Vector3 newGizmoPosition)
    {
        return !(Vector3.Distance(newGizmoPosition, GetGizmoContainer().transform.position) > maxTranslateJump);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="componentPosition"></param>
    /// <param name="axisDirection"></param>
    /// <returns></returns>
    public Vector3 GetPositionFromTranslate(Vector3 componentPosition, Vector3 axisDirection)
    {    
        return GetLineProjection(componentPosition, axisDirection, GetRayOriginPosition(), GetRayForward());
    }

    /// <summary>
    /// Gets the position on a line closest to another line
    /// </summary>
    /// <param name="linePosition">A position on some line</param>
    /// <param name="lineDirection">The direction of some line</param>
    /// <param name="rayPosition">The direction of a controller ray</param>
    /// <param name="rayDirection">The direction of a controller ray</param>
    /// <returns></returns>
    Vector3 GetLineProjection(Vector3 linePosition, Vector3 lineDirection, Vector3 rayPosition, Vector3 rayDirection)
    {
        Vector3 pos_diff = linePosition - rayPosition;
        Vector3 cross_normal = Vector3.Normalize(Vector3.Cross(lineDirection, rayDirection));
        Vector3 rejection = pos_diff - Vector3.Project(pos_diff, rayDirection) - Vector3.Project(pos_diff, cross_normal);

        float div = Vector3.Dot(lineDirection, Vector3.Normalize(rejection));

        // don't divide by zero
        if (div == 0) return 
                new Vector3(float.NaN, float.NaN, float.NaN);

        float distance_to_line_pos = Vector3.Magnitude(rejection) / div; 
        Vector3 closest_approach = linePosition - lineDirection * distance_to_line_pos;

        return closest_approach;
    }

    /// <summary>
    /// Checks if two vectors are near parallel
    /// </summary>
    /// <param name="a">The first vector</param>
    /// <param name="b">The second vector</param>
    /// <returns>True if two vectors are near parallel, otherwise, false</returns>
    bool NearParallel(Vector3 a, Vector3 b)
    {
        a = Vector3.Normalize(a);
        b = Vector3.Normalize(b);
        return Mathf.Abs(Vector3.Cross(a, b).magnitude) < crossThreshold;
    }

    /// <summary>
    /// Gets the cross magnitude of a vector as 1 or -1
    /// </summary>
    /// <param name="a">The first vector</param>
    /// <param name="b">The second vector</param>
    /// <returns>The cross magnitude of a vector as 1 or -1</returns>
    float GetCrossDirection(Vector3 a, Vector3 b)
    {
        a = Vector3.Normalize(a);
        b = Vector3.Normalize(b);

        if (Vector3.Cross(a, b).magnitude > 0)  
            return 1;

        return -1;
    }

    /// <summary>
    /// Sets the intial line data for originalClosestApproach and originalCrossDirection
    /// </summary>
    public void InitLineData()
    {
        originalClosestApproach = GetPositionFromTranslate(GetGizmoContainer().transform.position, GetAxisDirection());
        originalCrossDirection = GetCrossDirection(GetAxisDirection(), GetRayForward());
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
            if(GetAttachedObject().GetComponent<NetworkedMesh>())
            {
                // RealityFlowAPI.Instance.UpdatePrimitive(GetAttachedObject());
            }

            //HandleSelectionManager.Instance.mesh.CacheOperation(currentOperation);
        }
        catch {  }
        currentOperation = null;
    }
}

