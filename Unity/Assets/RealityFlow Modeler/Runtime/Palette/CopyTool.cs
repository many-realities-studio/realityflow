using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Spawning;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using TransformTypes;
using Unity.XR.CoreUtils;

/// <summary>
/// Class CopyTool assigns the copy tool to the user and allows the duplication of meshes through the Copy button on the palette.
/// </summary>
public class CopyTool : MonoBehaviour
{
    [SerializeField] private GameObject primitive;
    private GameObject copiedObject;
    // For use in running the copy tool once when a mesh is selected
    private bool isSelected;

    public bool isActive;
    private bool lastActiveState;
    private bool copyControllerButtonPressed;
    private bool haveGizmoSelected;
    private GameObject selectedObject;
    private GameObject selectedGizmo;
    private GameObject leftHand;
    private GameObject rightHand;
    private XRRayInteractor rayInteractor;

    private RaycastHit currentHitResult;
    private RaycastHit lastHitResult;

    private AttachGizmoState gizmoManager;

    void Start()
    {
        gizmoManager = gameObject.GetComponentInChildren<AttachGizmoState>();

        currentHitResult = new RaycastHit();
        var rig = Object.FindFirstObjectByType<XROrigin>().gameObject.gameObject;
        // Debug.Log(rig);
        leftHand = rig.transform.Find("Camera Offset/MRTK LeftHand Controller").gameObject;
        rightHand = rig.transform.Find("Camera Offset/MRTK RightHand Controller").gameObject;
        rayInteractor = rightHand.GetComponentInChildren<XRRayInteractor>();

        if (rayInteractor == null)
        {
            Debug.LogWarning("No ray interactor found!");
        }

        PaletteHandManager.OnHandChange += SwitchHands;
    }

    public void OnDestroy()
    {
        PaletteHandManager.OnHandChange -= SwitchHands;
    }

    public void Activate(int tool, bool status)
    {
        if(tool == 8)
        {
            isActive = status;
        }
    }

    private void GetRayCollision()
    {
        rayInteractor.TryGetCurrent3DRaycastHit(out currentHitResult);

        if (currentHitResult.collider != null)
        {
            // Check if we're hitting a transformation gizmo for the first time
            if (currentHitResult.transform.gameObject.GetComponent<MRTKBaseInteractable>() != null && currentHitResult.transform.gameObject.GetComponent<MRTKBaseInteractable>().IsRaySelected)
            {
                SaveObjectReferences(currentHitResult.collider.gameObject);
            }

            // We may be hitting a transformation gizmo where the collider is in the child of the object with the base interactable (i.e. rotation or plane gizmo)
            else if (currentHitResult.transform.parent.gameObject.GetComponent<MRTKBaseInteractable>() != null && currentHitResult.transform.parent.gameObject.GetComponent<MRTKBaseInteractable>().IsRaySelected)
            {
                SaveObjectReferences(currentHitResult.collider.transform.parent.gameObject);
            }

            // Check if we're hitting a UI component
            if (currentHitResult.collider.gameObject.GetComponentInParent<CanvasRenderer>())
            {
                return;
            }

            // If the game object hit has an interactable
            if (currentHitResult.transform.gameObject.GetComponent<MRTKBaseInteractable>() != null)
            {
                if (currentHitResult.transform.gameObject.GetComponent<MRTKBaseInteractable>().IsRaySelected && !isSelected)
                {
                    isSelected = true;
                    //Debug.Log("Copy the mesh");
                    
                    // Copy the game object if it is a user created mesh
                    if (currentHitResult.collider != null && currentHitResult.collider.gameObject != null && currentHitResult.transform.gameObject.GetComponent<EditableMesh>()
                        && currentHitResult.transform.gameObject.GetComponent<ObjectManipulator>().enabled)
                    {
                        CopyMesh(currentHitResult.collider.gameObject, currentHitResult.collider.gameObject.name);
                    }

                }
                else if (!currentHitResult.transform.gameObject.GetComponent<MRTKBaseInteractable>().IsRaySelected && isSelected)
                {
                    isSelected = false;
                }
            }
        }
    }

    private void SaveObjectReferences(GameObject hitResult)
    {
        if (hitResult.layer == LayerMask.NameToLayer("Gizmo") && !haveGizmoSelected)
        {
            // Debug.Log("Interacted with gizmo: " + hitResult.name);
            haveGizmoSelected = true;

            // Save references for the selected object
            selectedGizmo = hitResult;
            selectedObject = hitResult.GetComponent<GizmoTransform>().GetAttachedObject();
        }
    }

    private void CopyMesh(GameObject selectedMesh, string objectName)
    {
        copiedObject = NetworkSpawnManager.Find(this).SpawnWithRoomScopeWithReturn(primitive);
        // Network that this copied mesh is free to manipulate for everyone
        // copiedObject.GetComponent<NetworkedMesh>().EndHold();

        EditableMesh em = copiedObject.GetComponent<EditableMesh>();
        em.CreateMesh(selectedMesh.GetComponent<EditableMesh>());

        NetworkedMesh copiedObjectNetworkedMesh = copiedObject.transform.gameObject.GetComponent<NetworkedMesh>();
        copiedObjectNetworkedMesh.SetDuplicate(selectedMesh.name);

        // copiedObject.name = objectName + " Copy";

        Material mat = copiedObject.GetComponent<MeshRenderer>().material;
        Transform transform = copiedObject.GetComponent<Transform>();

        CopyMaterial(mat, selectedMesh.GetComponent<MeshRenderer>().material);
        CopyTransform(transform, selectedMesh.GetComponent<Transform>());
        //SelectCopiedMeshes();

        if (!gizmoManager.isActive)
        {
            //Debug.Log("Enable looking for gizmo");
            gizmoManager.isActive = true;
            gizmoManager.EnableLookForTarget(TransformType.All);
        }
    }
    
    private void CopyMaterial(Material myMat, Material otherMat)
    {
        myMat.SetColor("_Color", otherMat.GetColor("_Color"));
        myMat.SetFloat("_Metallic", otherMat.GetFloat("_Metallic"));
        myMat.SetFloat("_Glossiness", otherMat.GetFloat("_Glossiness"));
    }

    private void CopyTransform(Transform myTransform, Transform otherTransform)
    {
        myTransform.localPosition = otherTransform.localPosition;
        myTransform.localRotation = otherTransform.localRotation;
        myTransform.localScale = otherTransform.localScale;
    }

    private void SelectCopiedMeshes()
    {
        copiedObject.GetComponent<SelectToolManager>().SelectMesh();
    }

    /// <summary>
    /// Disables the gizmo from object when Copy Tool is toggled off
    /// </summary>
    private void DisableGizmo()
    {
        // Debug.Log("DisableGizmo() called");
        gizmoManager.isActive = false;
        gizmoManager.DisableLookForTarget();
    }

    void Update()
    {
        if (isActive)
        {
            GetRayCollision();

            if (haveGizmoSelected)
            {
                if (!selectedGizmo.GetComponent<MRTKBaseInteractable>().IsRaySelected)
                {
                    // Debug.Log("Gizmo de-selected");
                    haveGizmoSelected = false;
                }

                if (copyControllerButtonPressed)
                {
                    // Debug.Log("Copy mesh off A button");
                    copyControllerButtonPressed = false;
                    CopyMesh(selectedObject, selectedObject.name);
                }
            }
        }

        if (lastActiveState != isActive)
        {
            lastActiveState = isActive;

            if (!isActive)
            {
                DisableGizmo();
            }
        }
    }

    private void SwitchHands(bool isLeftHandDominant)
    {
        // Switch the interactor rays and triggers depending on the dominant hand
        if (isLeftHandDominant)
        {
            rayInteractor = leftHand.GetComponentInChildren<XRRayInteractor>();

            // Switch copy primary buttons to match dominant hand controller
            gameObject.GetComponents<OnButtonPress>()[0].enabled = false;
            gameObject.GetComponents<OnButtonPress>()[1].enabled = true;
        }
        else
        {
            rayInteractor = rightHand.GetComponentInChildren<XRRayInteractor>();

            // Switch copy primary buttons to match dominant hand controller
            gameObject.GetComponents<OnButtonPress>()[0].enabled = true;
            gameObject.GetComponents<OnButtonPress>()[1].enabled = false;
        }
    }

    /// <summary>
    /// Keeps track of when the button assigned for copying an object is pressed.
    /// </summary>
    public void EnableCopyOffGizmo()
    {
        copyControllerButtonPressed = true;
    }
}
