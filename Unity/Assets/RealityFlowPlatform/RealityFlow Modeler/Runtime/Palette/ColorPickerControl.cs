using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Microsoft.MixedReality.Toolkit;
using Slider = Microsoft.MixedReality.Toolkit.UX.Slider;

/// <summary>
/// Class ColorPickerControl handles creating and updating the colors for the HSV color panels
/// in the Color Menu on the palette.
/// </summary>
public class ColorPickerControl : MonoBehaviour
{
    public float currentHue, currentSat, currentVal;
    private float lastSat, lastVal;

    [SerializeField] public RawImage hueImage, satValImage, outputImage;

    [Header("Color controls")]
    [SerializeField] private StatefulInteractable colorButton;
    [SerializeField] private Slider hueSlider, metallicSlider, smoothnessSlider;

    private Color currentColor;
    private Color lastColorValue;
    private float lastHueSliderValue, lastMetallicSliderValue, lastSmoothnessSliderValue;

    private Texture2D hueTexture, svTexture, outputTexture;

    public static event Action<Color> OnColorChangeTool;
    public static event Action<float> OnMetallicValueChange;
    public static event Action<float> OnSmoothnessValueChange;

    private void Start()
    {
        CreateHueImage();
        CreateSVImage();
        CreateOutputImage();
        UpdateOutputImage();

        currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);
        lastColorValue = currentColor;
        lastSat = 0f;
        lastVal = 0f;
    }

    public void CreateHueImage()
    {
        //Debug.Log("CreateHueImage() in scene = " + gameObject.transform.parent.parent.parent.parent.name);
        hueTexture = new Texture2D(1, 16);
        hueTexture.wrapMode = TextureWrapMode.Clamp;
        hueTexture.name = "HueTexture";
        
        for(int i = 0; i < hueTexture.height; i++)
        {
            hueTexture.SetPixel(0, i, Color.HSVToRGB((float)i / hueTexture.height, 1, 1));
        }
        hueTexture.Apply();

        currentHue = 0;
        hueImage.texture = hueTexture;
    }

    public void CreateSVImage()
    {
        svTexture = new Texture2D(16, 16);
        svTexture.wrapMode = TextureWrapMode.Clamp;
        svTexture.name = "SatValTexture";

        for (int y = 0; y < svTexture.height; y++)
        {
            for(int x = 0; x < svTexture.width; x++)
            {
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));
            }
        }
        svTexture.Apply();

        // Debug.Log("CreateSVImage() was called for scene = " + gameObject.transform.parent.parent.parent.parent.name);
        currentSat = 0;
        currentVal = 0;

        satValImage.texture = svTexture;
    }

    private void CreateOutputImage()
    {
        outputTexture = new Texture2D(1, 16);
        outputTexture.wrapMode = TextureWrapMode.Clamp;
        outputTexture.name = "OutputTexture";
        // currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);

        for(int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColor);
        }

        outputTexture.Apply();

        outputImage.texture = outputTexture;
    }

    private void UpdateOutputImage()
    {
        // Debug.Log("UpdateOutputImage() in scene = " + gameObject.transform.parent.parent.parent.parent.name);
        if (outputTexture != null)
        {
            currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);
            for(int i = 0; i < outputTexture.height; i++)
            {
                outputTexture.SetPixel(0, i, currentColor);
            }

            outputTexture.Apply();
        }
        else
        {
            Debug.LogWarning("Output texture is null");
        }
    }

    public void SetSV(float S, float V)
    {
        // Debug.Log("SetSV was called in scene = " + gameObject.transform.parent.parent.parent.parent.name + " and Sat value = " + S + " and Val value = " + V);
        currentSat = S;
        currentVal = V;

        UpdateOutputImage();
    }

    public void UpdateSVImage()
    {
        //Debug.Log("UpdateSVImage() ran in scene = " + gameObject.transform.parent.parent.parent.parent.name);
        if (svTexture != null)
        {
            currentHue = hueSlider.SliderValue;

            for (int y = 0; y < svTexture.height; y++)
            {
                for(int x = 0; x < svTexture.width; x++)
                {
                    svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));
                }
            }

            svTexture.Apply();
            UpdateOutputImage();
        }
        else
        {
            Debug.LogWarning("svTexture texture is null");
        }
    }

    public void UpdateColorValue()
    {
        OnColorChangeTool.Invoke(currentColor);
    }

    public void UpdateMetallicValue()
    {
        OnMetallicValueChange.Invoke(metallicSlider.SliderValue);
    }

    public void UpdateSmoothnessValue()
    {
        OnSmoothnessValueChange.Invoke(smoothnessSlider.SliderValue);
    }

    void Update()
    {
        // Update values only for the owner
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            if (lastColorValue != currentColor)
            {
                lastColorValue = currentColor;
                UpdateColorValue();
            }

            if (lastMetallicSliderValue != metallicSlider.SliderValue)
            {
                lastMetallicSliderValue = metallicSlider.SliderValue;
                UpdateMetallicValue();
            }

            if (lastSmoothnessSliderValue != smoothnessSlider.SliderValue)
            {
                lastSmoothnessSliderValue = smoothnessSlider.SliderValue;
                UpdateSmoothnessValue();
            }
        }

        // When the hue slider is changed, we assume the user will want to be in color mode
        if (lastHueSliderValue != hueSlider.SliderValue)
        {
            lastHueSliderValue = hueSlider.SliderValue;
            colorButton.ForceSetToggled(true);
            UpdateSVImage();
        }

        if (lastSat != currentSat)
        {
            lastSat = currentSat;
            SetSV(currentSat, currentVal);
        }

        if (lastVal != currentVal)
        {
            lastVal = currentVal;
            SetSV(currentSat, currentVal);
        }
    }
}
