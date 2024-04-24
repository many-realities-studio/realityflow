using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Spawning;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Ubiq.Avatars;

/// <summary>
/// Class EraserTool assigns the eraser tool to the user and allows the deletion of meshes through the Eraser button on the palette.
/// </summary>
public class EraserTool : MonoBehaviour
{
    public bool isActive;
    private GameObject leftHand;
    private GameObject rightHand;
    private XRRayInteractor rayInteractor;
    private StatefulInteractable eraserButton;

    private RaycastHit currentHitResult;
    private RaycastHit lastHitResult;

    void Start()
    {
        currentHitResult = new RaycastHit();
        leftHand = GameObject.Find("MRTK LeftHand Controller");
        rightHand = GameObject.Find("MRTK RightHand Controller");
        rayInteractor = rightHand.GetComponentInChildren<MRTKRayInteractor>();

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

    private void GetRayCollision()
    {
        rayInteractor.TryGetCurrent3DRaycastHit(out currentHitResult);

        if (currentHitResult.collider != null)
        {
            // Check if we're hitting a UI component
            if (currentHitResult.collider.gameObject.GetComponentInParent<CanvasRenderer>())
            {
                return;
            }

            // If the game object hit has an interactable
            if (currentHitResult.transform.gameObject.GetComponent<MRTKBaseInteractable>() != null)
            {
                if (currentHitResult.transform.gameObject.GetComponent<MRTKBaseInteractable>().IsRaySelected)
                {
                    DeleteMesh();
                }
            }
        }
    }

    public void Activate(int tool, bool status)
    {
        if(tool == 1)
        {
            isActive = status;
        }
    }
    
    public void DeleteMesh()
    {
        // Delete the game object if it is a user created mesh and not selected by anyone else
        if (currentHitResult.collider != null && currentHitResult.transform.gameObject.GetComponent<EditableMesh>()
            && currentHitResult.transform.gameObject.GetComponent<ObjectManipulator>().enabled)
        {
            // You should no longer be able to interact with this mesh
            currentHitResult.transform.gameObject.GetComponent<ObjectManipulator>().enabled = false;

            NetworkSpawnManager.Find(this).Despawn(currentHitResult.collider.gameObject);
        }
    }

    void Update()
    {
        if (isActive)
        {
            GetRayCollision();
        }
    }

    private void SwitchHands(bool isLeftHandDominant)
    {
        // Switch the interactor rays and triggers depending on the dominant hand
        if(isLeftHandDominant)
        {
            rayInteractor = leftHand.GetComponentInChildren<MRTKRayInteractor>();
        }
        else
        {
            rayInteractor = rightHand.GetComponentInChildren<MRTKRayInteractor>();
        }
    }
}
