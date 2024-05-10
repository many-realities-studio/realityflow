using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using TransformTypes;

/// <summary>
/// This class manages which object is the gizmo should attach to
/// </summary>
public class AttachGizmoState : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject gizmoContainerPrefab;
    public GameObject gizmoContainerInst;
    public GameObject attachedGameObject;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject sphere;
    public TransformType transformType;

    public XRRayInteractor interactor;

    public bool lookForTarget;
    public bool checkMeshRaySelection;
    bool lastUpdateRaySelect;

    // Primarily used in SelectToolManager.cs to see the current active state of gizmo tools
    public bool isActive;

    List<GameObject> disabledComponents;
    Dictionary<TransformType, string> TransfromTypeStringDict
       = new Dictionary<TransformType, string>
   {
        { TransformType.All, "All" },
        { TransformType.Rotate, "Rotate" },
        { TransformType.Scale, "Scale"},
        { TransformType.Translate, "Translate" }
   };

    void Start()
    {
        checkMeshRaySelection = false;
        attachedGameObject = null;
        lookForTarget = false;
        rightHand = GameObject.Find("MRTK RightHand Controller");
        leftHand = GameObject.Find("MRTK LeftHand Controller");
        disabledComponents = new List<GameObject>();
        SetActiveInteractor();
    }

    // Update is called once per frame
    void Update()
    {
        if (!lookForTarget)
        {
            if (attachedGameObject != null)
                DetachGizmoFromObject();
            return;
        }

        // Checks for the active contoller every .8 seconds, should be a better way to do this
        Invoke(nameof(SetActiveInteractor), .8f);

        // if (attachedGameObject != null)
        //     DeactivateFreeTransform(attachedGameObject);

        GameObject target = GetRayCollision();

        if (target == null)
            return; 

        if (StartOfRaySelect(target))
        {
            if (DoSwitchObjectSelect(target))
                DetachGizmoFromObject();

            AttachGizmoToObject(target);

            lastUpdateRaySelect = true;
        }

        else if (EndOfRaySelect(target))
        {
            lastUpdateRaySelect = false;
        }
    }

    /// <summary>
    /// Checks if the target object is an object the gizmo should attach to
    /// </summary>
    /// <param name="target">The new object the gizmo will check if it should attach to</param>
    /// <returns></returns>
    bool DoSwitchObjectSelect(GameObject target)
    {
        if (attachedGameObject == null) return false;
        return target.GetInstanceID() != attachedGameObject.GetInstanceID();
    }

    /// <summary>
    /// Attaches the gizmo to the target object
    /// </summary>
    /// <param name="target">The target object the gizmo should attach to</param>
    public void AttachGizmoToObject(GameObject target)
    {
        //Debug.Log("Attach gizmo");
        attachedGameObject = target;

        // If the game object has already been selected with the Select Tool then retain its selectedness
        // if (attachedGameObject.GetComponent<NetworkedMesh>().isSelected)
        // {
        //     attachedGameObject.GetComponent<BoundsControl>().ToggleHandlesOnClick = false;
        // }

        try
        {
            attachedGameObject.GetComponent<MeshCollider>().enabled = false;
            attachedGameObject.GetComponent<BoundsControl>().HandlesActive = true;
            attachedGameObject.GetComponent<NetworkedMesh>().ControlSelection();
            //attachedGameObject.GetComponent<NetworkedMesh>().isSelected = true;
            DeactivateFreeTransform(attachedGameObject);
        }
        catch
        {
            Debug.Log("Target object is missing a component!");
        }

        gizmoContainerInst = Instantiate(gizmoContainerPrefab);
        gizmoContainerInst.transform.position = target.transform.position;
        gizmoContainerInst.SetActive(false);         

        DisableNonTypeMatching();

        // We add a delay to allow the gizmo to resize before it is shown to the user
        Invoke(nameof(ActivateGizmo), .02f);
    }

    /// <summary> 
    /// Detaches the gizmo from the attached game object
    /// </summary> 
    void DetachGizmoFromObject()
    {
        //Debug.Log("Detach gizmo");
        try
        {
            attachedGameObject.GetComponent<MeshCollider>().enabled = true;
            //attachedGameObject.GetComponent<NetworkedMesh>().isSelected = false;
            // Turn off the handles if all the gizmo tools are off
            attachedGameObject.GetComponent<BoundsControl>().HandlesActive = false;
            attachedGameObject.GetComponent<NetworkedMesh>().ControlSelection();
            ActivateFreeTransform(attachedGameObject);
        }
        catch { Debug.Log("missing components"); }

        attachedGameObject = null;

        Destroy(gizmoContainerInst);       
    }

    /// <summary>
    /// Sets the gizmo to active. This method was created to be used by Invoke().
    /// </summary>
    void ActivateGizmo()
    {
        gizmoContainerInst.SetActive(true);
    }

    /// <summary>
    /// Gets the game object from a ray collision if it is an EditableMesh and has an MRTKBaseInteractable component
    /// </summary>
    /// <returns>The game object if it meets the condition, otherwise, null</returns>
    private GameObject GetRayCollision()
    {
        RaycastHit currentHitResult = new RaycastHit();
        interactor.TryGetCurrent3DRaycastHit(out currentHitResult);

        if (currentHitResult.transform == null)
            return null;
        if (currentHitResult.transform.gameObject.GetComponent<MRTKBaseInteractable>() == null)
            return null;
        if (currentHitResult.transform.gameObject.GetComponent<EditableMesh>() == null)
            return null;

        return currentHitResult.transform.gameObject;
    }

    /// <summary>
    /// Sets the interactor from the active contoller
    /// </summary>
    /// <returns>True if an interactor was found, otherwise, false</returns>
    bool SetActiveInteractor()
    {
        interactor = rightHand.GetComponentInChildren<MRTKRayInteractor>();

        if (interactor == null)
            interactor = leftHand.GetComponentInChildren<MRTKRayInteractor>();

        else if (interactor == null)
            return false;

        return true;
    }

    /// <summary>
    /// Enables if the gizmo should look for a new object to attach to
    /// </summary> 
    public void EnableLookForTarget(TransformType tType)
    {
        lookForTarget = true;
        transformType = tType;
         
        if (gizmoContainerInst != null) DisableNonTypeMatching();
    }

    /// <summary>
    /// Disables if the gizmo should look for a new object to attach to
    /// </summary>
    public void DisableLookForTarget()
    {
        lookForTarget = false;
        transformType = TransformType.None;

        ReEnableComponents();
    }

    /// <summary>
    /// Returns if this is the start of the selection of a game object
    /// </summary>
    /// <param name="target">The target game object under question</param>
    /// <returns>True if this is the start of selection, otherwise, false</returns>
    bool StartOfRaySelect(GameObject target)
    {
        return !lastUpdateRaySelect && target.GetComponent<MRTKBaseInteractable>().IsRaySelected;
    }

    /// <summary>
    /// Returns if this is the end of selection of a game object
    /// </summary>
    /// <param name="target">The target game object under question</param>
    /// <returns>True if this is the end of selection, otherwise, false</returns>
    bool EndOfRaySelect(GameObject target)
    {
        return lastUpdateRaySelect && !target.GetComponent<MRTKBaseInteractable>().IsRaySelected;
    }

    /// <summary>
    /// Sets the ObjectManipulator AllowedManipulations to None
    /// </summary>
    /// <param name="target">The game object to set the AllowedManipulations</param>
    void DeactivateFreeTransform(GameObject target)
    {
        attachedGameObject.GetComponent<ObjectManipulator>().AllowedManipulations
            = TransformFlags.None;
    }

    /// <summary>
    /// Sets the ObjectManipulator AllowedManipulations to Move, Rotate, and Scale
    /// </summary>
    /// <param name="target">The game object to set the AllowedManipulations</param>
    void ActivateFreeTransform(GameObject target)
    {
        target.GetComponent<ObjectManipulator>().AllowedManipulations 
            = TransformFlags.Move | TransformFlags.Rotate | TransformFlags.Scale;
    }

    /// <summary>
    /// Checks if the name of the game object is of the same type as the current transform type
    /// </summary>
    /// <param name="subComponent">The game object to check</param>
    /// <returns>True if the name of the game object is of the same type as the current transform type, otherwise, false</returns>
    bool IsTypeMatching(GameObject subComponent)
    {
        if (transformType == TransformType.All)
            return true;

        if (transformType == TransformType.Rotate)
            return subComponent.name == TransfromTypeStringDict[transformType];

        if (transformType == TransformType.Translate)
            return subComponent.name == "Translate" || subComponent.name == "Plane";

        if (transformType == TransformType.Scale)
            return subComponent.name == "Scale" || subComponent.name == "Stretch";

        return false;
    }

    /// <summary>
    /// Disables gizmo components if they are not of the same type as the current transformation type
    /// </summary>
    void DisableNonTypeMatching()
    {
        GameObject gizmo = gizmoContainerInst.transform.GetChild(0).gameObject;

        for (int i = 0; i < gizmo.transform.childCount; i++)
        {
            for (int j = 0; j < gizmo.transform.GetChild(i).childCount; j++)
            {
                GameObject subComponent = gizmo.transform.GetChild(i).transform.GetChild(j).gameObject;

                if (IsTypeMatching(subComponent)) continue;

                subComponent.SetActive(false);
                disabledComponents.Add(subComponent);
            }
        }
    }

    /// <summary>
    /// Enables the components in disabledComponents and clears the list
    /// </summary>
    void ReEnableComponents()
    {
        foreach (GameObject go in disabledComponents)
        {
            if (go == null) continue;
            go.SetActive(true);
        }

        disabledComponents.Clear();
    }

    GameObject GetActiveContollerFarRay()
    {
        GameObject activeContoller = rightHand;

        if (rightHand.GetComponentInChildren<MRTKRayInteractor>() == null)
            activeContoller = leftHand;            

        return activeContoller.transform.Find("Far Ray").gameObject;
    }
    public void EnableMeshRaySelection()
    {
        if (!lookForTarget) return;

        GameObject farRay = GetActiveContollerFarRay();
        LayerMask gizmo = 1 << LayerMask.NameToLayer("Gizmo");
        LayerMask ui = 1 << LayerMask.NameToLayer("UI");

        farRay.GetComponent<MRTKRayInteractor>().raycastMask = gizmo | ui;       
    }

    public void DisableMeshRaySelection()
    {
        if (!lookForTarget) return;

        GameObject farRay = GetActiveContollerFarRay();
        LayerMask everything = ~0;

        farRay.GetComponent<MRTKRayInteractor>().raycastMask = everything;
    }
}
