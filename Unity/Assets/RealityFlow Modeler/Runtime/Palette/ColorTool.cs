using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;

/// <summary>
/// Class ColorTool grabs a reference to the active state of the color tool, updates the user's
/// current color, metallic, and smoothness values, and changes the material values of a mesh.
/// </summary>
public class ColorTool : MonoBehaviour
{
    public bool colorToolIsActive;
    public bool metallicToolIsActive;
    public bool smoothnessToolIsActive;
    [SerializeField] private Color currentColor;
    [SerializeField] private float currentMetallicValue;
    [SerializeField] private float currentSmoothnessValue;

    private GameObject leftHand;
    private GameObject rightHand;
    private XRRayInteractor rayInteractor;
    private RaycastHit currentHitResult;

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
        ColorPickerControl.OnColorChangeTool += SwapColors;
        ColorPickerControl.OnMetallicValueChange += SwapMetallicValues;
        ColorPickerControl.OnSmoothnessValueChange += SwapSmoothnessValues;
    }

    void OnDestroy()
    {
        PaletteHandManager.OnHandChange -= SwitchHands;
        ColorPickerControl.OnColorChangeTool -= SwapColors;
        ColorPickerControl.OnMetallicValueChange -= SwapMetallicValues;
        ColorPickerControl.OnSmoothnessValueChange -= SwapSmoothnessValues;
    }

    private void SwapColors(Color newColor)
    {
        // Debug.Log("Your new color is " + newColor);
        currentColor = newColor;
    }

    private void SwapMetallicValues(float newMetallicValue)
    {
        currentMetallicValue = newMetallicValue;
    }

    private void SwapSmoothnessValues(float newSmoothnessValue)
    {
        currentSmoothnessValue = newSmoothnessValue;
    }

    public void Activate(int tool, bool status)
    {
        if(tool == 2)
        {
            colorToolIsActive = status;
        }

        if(tool == 3)
        {
            metallicToolIsActive = status;
        }

        if(tool == 4)
        {
            smoothnessToolIsActive = status;
        }
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
                    UpdateMeshTexture();
                }
            }
        }
    }

    private void UpdateMeshTexture()
    {
        // Update the game object depending on the tool and if it is a user created mesh and not selected by anyone else
        if (currentHitResult.collider != null && currentHitResult.transform.gameObject.GetComponent<EditableMesh>()
            && currentHitResult.transform.gameObject.GetComponent<ObjectManipulator>().enabled)
        {
            if (colorToolIsActive)
            {
                currentHitResult.collider.gameObject.GetComponent<Renderer>().material.SetColor("_Color", currentColor);
            }
            if (metallicToolIsActive)
            {
                currentHitResult.collider.gameObject.GetComponent<Renderer>().material.SetFloat("_Metallic", currentMetallicValue);
            }
            if (smoothnessToolIsActive)
            {
                currentHitResult.collider.gameObject.GetComponent<Renderer>().material.SetFloat("_Glossiness", currentSmoothnessValue);
            }
        }
    }

    void Update()
    {
        if (colorToolIsActive || metallicToolIsActive || smoothnessToolIsActive)
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
