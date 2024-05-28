using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;

/// <summary>
/// Class UpdateSnapMode gives a reference of the slider for the current snapping mode for use in the GridTool.
/// </summary>
public class UpdateSnapMode : MonoBehaviour
{
    private float lastSnappingSliderValue;
    private Slider snappingSlider;

    public static event Action<float> OnSnapModeChange;

    void Start()
    {
        snappingSlider = gameObject.GetComponent<Slider>();
    }

    public void UpdateSnapModeValue()
    {
        OnSnapModeChange.Invoke(snappingSlider.SliderValue);
    }

    void Update()
    {
        // Update values only for the owner
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            if (lastSnappingSliderValue != snappingSlider.SliderValue)
            {
                lastSnappingSliderValue = snappingSlider.SliderValue;
                UpdateSnapModeValue();
            }
        }
    }
}
