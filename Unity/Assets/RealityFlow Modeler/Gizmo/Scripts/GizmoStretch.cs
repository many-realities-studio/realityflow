using UnityEngine;

/// <summary>
/// This class is used for the stretch functionality of the gizmo
/// </summary>
public class GizmoStretch : GizmoTranslateAxis
{
    private Vector3 originalMeshScale;
    private float originalGizmoDistance;

    private ComponentScaling currentOperation;
    private Vector3 startScale;

    // Start is called before the first frame update
    void Start()
    {

        

    }

    // Update is called once per frame
    void Update()
    {
        if (!awake) return;

        if (StartOfRaySelect())
        {
            InitRayGizmolData();
            InitLineData();
            EnableLineRenderer(this.transform.position, GetAxisDirection(), 100f);
            InitTanSphere();

            originalMeshScale = GetStretchInGrid(GetAttachedObject().transform.localScale);
            originalGizmoDistance = GetGizmoOriginDistance();
            

            lastUpdateRaySelect = true;
            BeginMeshOperation();
        }

        else if (EndOfRaySelect())
        {
            DisableLineRenderer();
            EndMeshOperation();
            Destroy(tanSphere);
            BakeRotation();

            lastUpdateRaySelect = false;
        }

        if (lastUpdateRaySelect)
        {
            if (!IsValidPosition()) return;

            Vector3 newMeshScale = GetStretchInGrid(GetNewMeshScale(originalMeshScale));

            GetAttachedObject().transform.localScale = newMeshScale;

            if(GetAttachedObject().GetComponent<MyNetworkedObject>() != null)
            {
                RealityFlowAPI.Instance.UpdateObjectTransform(GetAttachedObject().name);
            }
        }

        
    }
    /// <summary>
    /// Gets the new mesh scale from the current mesh scale
    /// </summary>
    /// <param name="meshScale">The current mesh scale</param>
    /// <returns>The new mesh scale</returns>
    Vector3 GetNewMeshScale(Vector3 meshScale)
    {
        string name = GetParentName();

        if (name == "X")
            meshScale.x *= GetScaleFactor();
        else if (name == "Y")
            meshScale.y *= GetScaleFactor();
        else if (name == "Z")
            meshScale.z *= GetScaleFactor();

        return meshScale;
    }

    /// <summary>
    /// Gets the new scale factor from the new and original gizmo distance
    /// </summary>
    /// <returns>The new scale factor</returns>
    float GetScaleFactor()
    {
        float newGizmoDistance = GetGizmoOriginDistance();
        return newGizmoDistance / originalGizmoDistance;
    }

    /// <summary>
    /// Gets from the original distance of the gizmo and the new position
    /// </summary>
    /// <returns>The distance</returns>
    float GetGizmoOriginDistance()
    {
        Vector3 position = GetPositionFromTranslate(transform.position, GetAxisDirection());
        tanSphere.transform.position = position;
        return Vector3.Distance(GetGizmoContainer().transform.position, position);
    }

    void BeginMeshOperation()
    {
        if (currentOperation == null)
            currentOperation = new ComponentScaling(HandleSelectionManager.Instance.GetUniqueSelectedIndices());
        startScale = GetAttachedObject().transform.localScale;
    }

    void EndMeshOperation()
    {
        Vector3 invertedLastScale = Vector3.zero;
        invertedLastScale.x = 1 / startScale.x;
        invertedLastScale.y = 1 / startScale.y;
        invertedLastScale.z = 1 / startScale.z;
        Vector3 newScale = Vector3.Scale(GetAttachedObject().transform.localScale, invertedLastScale);
        currentOperation.AddOffsetAmount(newScale);
        try
        {
            if(GetAttachedObject().GetComponent<NetworkedMesh>())
            {
                // RealityFlowAPI.Instance.UpdatePrimitive(GetAttachedObject());
                HandleSelectionManager.Instance.mesh.CacheOperation(currentOperation);
            }
            //HandleSelectionManager.Instance.mesh.CacheOperation(currentOperation);
        }
        catch { }
        currentOperation = null;
    }
}
