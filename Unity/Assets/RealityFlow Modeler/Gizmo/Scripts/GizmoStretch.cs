using UnityEngine;

/// <summary>
/// This class is used for the stretch functionality of the gizmo
/// </summary>
public class GizmoStretch : GizmoTranslateAxis
{
    private Vector3 originalMeshScale;
    private float originalGizmoDistance;

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
        }

        else if (EndOfRaySelect())
        {
            DisableLineRenderer();
            Destroy(tanSphere);

            lastUpdateRaySelect = false;
        }

        if (lastUpdateRaySelect)
        {
            Vector3 newMeshScale = originalMeshScale;

            if (!IsValidPosition()) return;

            newMeshScale = GetStretchInGrid(GetNewMeshScale(originalMeshScale));

            GetAttachedObject().transform.localScale = newMeshScale;
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
}
