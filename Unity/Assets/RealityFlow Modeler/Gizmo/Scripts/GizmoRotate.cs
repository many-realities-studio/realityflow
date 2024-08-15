using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used for the rotate functionality of the gizmo
/// </summary>
public class GizmoRotate : GizmoTranslateAxis
{
    Vector3 hitPosition;
    Vector3 originalMeshRotate;
    float sign;
    // This effects the sensitivity of the rotation input
    float rotationMulti = 50;

    private ComponentRotation currentOperation;
    private Quaternion startRot;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (StartOfRaySelect())
        {
            InitRayGizmolData();

            originalMeshRotate = GetRotationInGrid(GetAttachedObject().transform.eulerAngles);
            hitPosition = GetRayCastHit();

            EnableLineRenderer(hitPosition, GetTorusTangent(), 50f, true);
            InitTanSphere();

            lastUpdateRaySelect = true;
            BeginMeshOperation();
        }

        else if (EndOfRaySelect())
        { 
            DisableLineRenderer();
            EndMeshOperation();
            Destroy(tanSphere);
            BakeRotation();

            if(lastUpdateRaySelect)
            {
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
                    RealityFlowAPI.Instance.UpdatePrimitive(manipulatedGO);
                }
            }

            lastUpdateRaySelect = false;
        }

        if (lastUpdateRaySelect)
        {
            GetAttachedObject().transform.eulerAngles = GetRotationInGrid(GetNewRotation(originalMeshRotate));
        }
    }

    /// <summary>
    /// Finds the new rotation of an object givin the controller input
    /// </summary>
    /// <param name="meshRotation">The inital mesh rotation</param>
    /// <returns>The new new mesh rotation</returns>
    Vector3 GetNewRotation(Vector3 meshRotation)
    {
        string name = GetParentName();
        Vector3 newTangentIntersect = GetTangetIntersect();

        float rotationOffset = Vector3.Distance(hitPosition, newTangentIntersect) * rotationMulti * sign;
        
        if (name == "X")
            meshRotation.x += rotationOffset; 
        else if (name == "Y")
            meshRotation.y += rotationOffset;
        else if (name == "Z")
            meshRotation.z += rotationOffset;

        return meshRotation;
    }

    /// <summary>
    /// Finds the tangent direction of the torus as at the selected point and axis direction
    /// </summary>
    /// <returns>The torus tangent direction</returns>
    Vector3 GetTorusTangent()
    {
        return Vector3.Normalize(Vector3.Cross(hitPosition - GetGizmoContainer().transform.position, GetAxisDirection()));
    }

    /// <summary>
    /// Gets the point along the torus tangent that is closest to the contoller ray  
    /// </summary>
    /// <returns>The point along the torus tangent that is closest to the contoller ray</returns>
    Vector3 GetTangetIntersect()
    {
        Vector3 gizmoOrigin = GetGizmoContainer().transform.position;
        Vector3 torusCenterPosition = Vector3.Project((hitPosition - gizmoOrigin), GetAxisDirection()) + gizmoOrigin;

        Vector3 torusTangent = Vector3.Normalize(Vector3.Cross(hitPosition - gizmoOrigin, GetAxisDirection()));
        Vector3 tangetIntersect = GetPositionFromTranslate(hitPosition, torusTangent);

        if (InvalidVector(tangetIntersect))
            return hitPosition;

        SetSign(torusCenterPosition, tangetIntersect);

        tanSphere.transform.position = tangetIntersect;

        return tangetIntersect; 
    }

    /// <summary>
    /// Finds the sign representation of the new hit position relative to the old tangent intersect
    /// </summary>
    /// <param name="torusCenterPosition">The torus centroid</param>
    /// <param name="tangetIntersect">The torus tangent</param>
    void SetSign(Vector3 torusCenterPosition, Vector3 tangetIntersect)
    {
        Vector3 relHit = Vector3.Normalize(hitPosition - torusCenterPosition);
        Vector3 relIntersect = Vector3.Normalize(tangetIntersect - torusCenterPosition);  
        Quaternion q = Quaternion.AngleAxis(90, torusCenterPosition);

        float angle = Vector3.SignedAngle(relHit, relIntersect, q * relHit) * Mathf.Deg2Rad;
        sign = Mathf.Sign(angle);
    }

    /// <summary>
    /// Checks if there is NaN in the vector
    /// </summary>
    /// <param name="vector">The vector to check</param>
    /// <returns>True if there is NaN in the vector, otherwise, false</returns>
    bool InvalidVector(Vector3 vector)
    {
        return float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);
    }

    void BeginMeshOperation()
    {
        if (currentOperation == null)
            currentOperation = new ComponentRotation(HandleSelectionManager.Instance.GetUniqueSelectedIndices());
        startRot = GetAttachedObject().transform.rotation;
    }

    void EndMeshOperation()
    {
        currentOperation.AddOffsetAmount(GetAttachedObject().transform.localRotation * Quaternion.Inverse(startRot));
        try
        {
            if(GetAttachedObject().GetComponent<NetworkedMesh>())
            {
                // RealityFlowAPI.Instance.UpdatePrimitive(GetAttachedObject());
                HandleSelectionManager.Instance.mesh.CacheOperation(currentOperation);
            }
            // prefabs are not meshes like primitve bases
            //HandleSelectionManager.Instance.mesh.CacheOperation(currentOperation);
        }
        catch { }
        currentOperation = null;
    }
}
