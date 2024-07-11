using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using Unity.XR.CoreUtils;

/// <summary>
/// Class HandleSelector handles selecting handles and supporting different types of Handle Selection modes
/// </summary>
public class HandleSelector : MonoBehaviour
{
    public static HandleSelector Instance { get; private set; }

    private GameObject leftHand;
    private GameObject rightHand;
    public XRRayInteractor rayInteractor { get; private set; }
    private RaycastHit currentHitResult;

    public bool xrayActive { get; private set; }
    public LayerMask handleLayer;

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        PaletteHandManager.OnHandChange += SwitchHands;

        currentHitResult = new RaycastHit();
        var rig = Object.FindFirstObjectByType<XROrigin>().gameObject;
        leftHand = rig.transform.Find("Camera Offset/MRTK LeftHand Controller").gameObject;
        rightHand = rig.transform.Find("Camera Offset/MRTK RightHand Controller").gameObject;
        rayInteractor = rightHand.GetComponentInChildren<XRRayInteractor>();
    }

    void OnDestroy()
    {
        PaletteHandManager.OnHandChange -= SwitchHands;
    }

    private void GetFirstRayCollision()
    {
        rayInteractor.TryGetCurrent3DRaycastHit(out currentHitResult);

        if (currentHitResult.collider == null)
            return;

        Handle selectedHandle = currentHitResult.transform.gameObject.GetComponent<Handle>();
        if (selectedHandle != null)
        {
            MRTKBaseInteractable interactable = currentHitResult.transform.gameObject.GetComponent<MRTKBaseInteractable>();
            if (interactable != null && interactable.isSelected)
            {

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (HandleSpawner.Instance.mode == ManipulationMode.mObject)
            return;

    //GetFirstRayCollision();
#if !ENABLE_INPUT_SYSTEM
        if (Input.GetKeyDown(KeyCode.L))
        {
            HandleSpawner.Instance.ReverseFaceHandlesDirection();
            xrayActive = !xrayActive;
        }
#endif
    }

    private void SwitchHands(bool isLeftHandDominant)
    {
        // Switch the interactor rays and triggers depending on the dominant hand
        if (isLeftHandDominant)
        {
            rayInteractor = leftHand.GetComponentInChildren<XRRayInteractor>();
        }
        else
        {
            rayInteractor = rightHand.GetComponentInChildren<XRRayInteractor>();
        }
    }
}
