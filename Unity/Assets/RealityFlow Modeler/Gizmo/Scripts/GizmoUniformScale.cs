using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used for the uniform scale functionality of the gizmo
/// </summary>
public class GizmoUniformScale : GizmoTranslatePlane
{
    Vector3 planePoint;
    Vector3 originalMeshScale;
    Vector3 originalPlaneNormal;
    float carryOverScale;
    float prevScale;

    bool isPositive = false;


    // Update is called once per frame
    void Update()
    {
        if (!awake) return;

        if (StartOfRaySelect())
        {
            InitRayGizmolData();
            originalMeshScale = GetAttachedObject().transform.localScale;
            Debug.Log("The SCALE attached object... = " + GetAttachedObject());
            

            planePoint = GetRayCastHit();
            originalPlaneNormal = Vector3.Normalize(GetPlaneNormal());

            originalIntersection = PlaneIntersectionTest(
                planePoint, originalPlaneNormal
            );

            prevScale = 1;
            carryOverScale = 1; 

            lastUpdateRaySelect = true;
        }

        else if (EndOfRaySelect())
        {
            lastUpdateRaySelect = false;
            BakeRotation();
        }

        if (lastUpdateRaySelect)
        {
            Vector3 newIntersection = PlaneIntersectionTest(
                planePoint,
                Vector3.Normalize(GetPlaneNormal())
            );

            Vector3 newGizmoPosition = originalGizmoPosition;
            Vector3 prevScale = GetAttachedObject().transform.localScale;

            GetAttachedObject().transform.localScale = GetScaleInGrid(originalMeshScale * GetScaleFactor(newIntersection), prevScale);


            GameObject attachedGO = GetAttachedObject();
            GameObject manipulatedGO = null;

            if(attachedGO.GetComponent<ComponentSelectManipulator>() != null)
            {
                manipulatedGO = attachedGO.GetComponent<ComponentSelectManipulator>().ReturnPairedObj();
            }

            if(attachedGO.GetComponent<MyNetworkedObject>() != null)
            {
                RealityFlowAPI.Instance.UpdateObjectTransform(attachedGO.name);
            }

            if(attachedGO.GetComponent<EditableMesh>() != null)
            {
                RealityFlowAPI.Instance.UpdatePrimitive(attachedGO);
            }
                
            if (manipulatedGO != null && manipulatedGO.GetComponent<EditableMesh>() != null)
            {
                //manipulatedGO.GetComponent<EditableMesh>().smi = new SerializableMeshInfo(manipulatedGO);
                //manipulatedGO.GetComponent<EditableMesh>().RefreshMesh();
                RealityFlowAPI.Instance.UpdatePrimitive(manipulatedGO);
            }
        }
    }

    /// <summary>
    /// Gets a scale factor based on the position and ensures that it produces a smooth effect
    /// </summary>
    /// <param name="newIntersection"></param>
    /// <returns>The scale factor</returns>
    float GetScaleFactor(Vector3 newIntersection)
    {
        float scaleFactor = 1;
        bool prevIsPositive = isPositive;

        isPositive = IsPositiveSide(originalPlaneNormal, newIntersection - planePoint);

        if (prevIsPositive != isPositive)
        {
            planePoint = newIntersection;
            carryOverScale = prevScale;
        }

        float distance = Mathf.Pow((Vector3.Distance(planePoint, newIntersection) + 1), 1.25f);

        if (isPositive)
            scaleFactor *= distance;
        else
            scaleFactor /= distance;

        prevScale = scaleFactor * carryOverScale;

        return prevScale;
    }

    /// <summary>
    /// A method to determine if a point is on the "positive" side of a point
    /// </summary>
    /// <param name="planeNormal">A normal of a plane</param>
    /// <param name="newIntersection">An intersection on a plane</param>
    /// <returns>True if a point is on the "positive" side of a point, otherwise, false</returns>
    bool IsPositiveSide(Vector3 planeNormal, Vector3 newIntersection)
    {
        Vector3 sideVector = Vector3.Cross(planeNormal, new Vector3(0, -1, 0));
        return Vector3.Dot(sideVector, newIntersection) > 0f;
    }

    /// <summary>
    /// A method to wrap the GetRayForward method
    /// </summary>
    /// <returns>A vector to represent a normal of a plane</returns>
    Vector3 GetPlaneNormal()
    {
        return GetRayForward();
    }
}
