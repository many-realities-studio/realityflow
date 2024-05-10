using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit;

/// <summary>
/// Class SVImageControl updates the color for the SVPanel in the Color Menu on the palette.
/// </summary>
public class SVImageControl : MonoBehaviour
{
    private MRTKBaseInteractable panelInteractable;
    
    [SerializeField]
    private Image pickerImage;

    private RawImage SVimage;

    private ColorPickerControl CC;
    private Vector3 pos;
    private float xNorm;
    private float yNorm;

    private RectTransform rectTransform, pickerTransform;
    private GameObject leftHand;
    private GameObject rightHand;
    [Tooltip("Used only for the initial call when the Color Menu is entered to determine which hand is dominant.")]
    [SerializeField] private StatefulInteractable dominantHandButton;
    private XRRayInteractor rayInteractor;
    private RaycastHit currentHitResult;

    private void Awake()
    {
        panelInteractable = GetComponent<MRTKBaseInteractable>();
        SVimage = GetComponent<RawImage>();
        CC = FindObjectOfType<ColorPickerControl>();
        rectTransform = GetComponent<RectTransform>();

        pickerTransform = pickerImage.GetComponent<RectTransform>();
        pickerTransform.localPosition = new Vector2(-59.92683f, -57.8448f);

        leftHand = GameObject.Find("Left Controller");
        rightHand = GameObject.Find("Right Controller");

        if (dominantHandButton.IsToggled)
        {
            rayInteractor = leftHand.GetComponentInChildren<XRRayInteractor>();
        }
        else
        {
            rayInteractor = rightHand.GetComponentInChildren<XRRayInteractor>();
        }

        if (rayInteractor == null)
        {
            Debug.LogWarning("No ray interactor found!");
        }

        PaletteHandManager.OnHandChange += SwitchHands;
    }

    void OnDestroy()
    {
        PaletteHandManager.OnHandChange -= SwitchHands;
    }

    private void GetRayCollision()
    {
        rayInteractor.TryGetCurrent3DRaycastHit(out currentHitResult);

        if (currentHitResult.collider != null && currentHitResult.collider.name == "SVPanel")
        {
            UpdateColor();
        }
    }

    void UpdateColor()
    {
        pos = rectTransform.InverseTransformPoint(currentHitResult.point);

        float deltaX = rectTransform.sizeDelta.x * 0.5f;
        float deltaY = rectTransform.sizeDelta.y * 0.5f;

        if(pos.x < -deltaX)
        {
            pos.x = -deltaX;
        }
        else if (pos.x > deltaX)
        {
            pos.x = deltaX;
        }

        if (pos.y < -deltaY)
        {
            pos.y = -deltaY;
        }
        else if(pos.y > deltaY)
        {
            pos.y = deltaY;
        }


        float x = pos.x + deltaX;
        float y = pos.y + deltaY;

        xNorm = x / rectTransform.sizeDelta.x;
        yNorm = y / rectTransform.sizeDelta.y;
    }

    public void SetColor()
    {
        pickerTransform.localPosition = pos;

        // Dynamically change the color of the picker to contrast that of the current color it's on
        pickerImage.color = Color.HSVToRGB(0, 0, 1 - yNorm);

        // Debug.Log("xNorm = " + xNorm + " and yNorm = " + yNorm);
        CC.SetSV(xNorm, yNorm);
    }

    void Update()
    {
        if (panelInteractable.IsRayHovered)
        {
            GetRayCollision();
        }   
    }

    private void SwitchHands(bool isLeftHandDominant)
    {
        // Switch the interactor rays and triggers depending on the dominant hand
        if(isLeftHandDominant)
        {
            rayInteractor = leftHand.GetComponentInChildren<XRRayInteractor>();
        }
        else
        {
            rayInteractor = rightHand.GetComponentInChildren<XRRayInteractor>();
        }
    }
}
