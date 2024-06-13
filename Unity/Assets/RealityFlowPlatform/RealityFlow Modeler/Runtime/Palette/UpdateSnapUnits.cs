using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;

/// <summary>
/// Class UpdateSnapUnits updates the size of the Snap Grid depending on which value the user has selected and
/// assigns the snap unit value across all menus to match what is currently selected.
/// </summary>
public class UpdateSnapUnits : MonoBehaviour
{
    private SnapGrid grid;
    private Slider snapUnitSlider;
    public float lastSnapUnitValue;
    private UpdateSnapUnits[] snapUnitsScripts;

    public static event Action<float> OnSnapUnitChange;

    void Start()
    {
        grid = FindObjectOfType<SnapGrid>();
        snapUnitSlider = gameObject.GetComponent<Slider>();

        // Get all references of any snap unit sliders on the owner's palette
        snapUnitsScripts = NetworkedPalette.reference.GetComponentsInChildren<UpdateSnapUnits>(true);
    }

    public void UpdateSnapUnitValue()
    {
        OnSnapUnitChange.Invoke(snapUnitSlider.SliderValue);
    }

    void Update()
    {
        // Update values only for the owner
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            if (lastSnapUnitValue != snapUnitSlider.SliderValue)
            {
                lastSnapUnitValue = snapUnitSlider.SliderValue;
                UpdateSnapUnitValue();
                UpdateSnapSize(snapUnitSlider.SliderValue);
            }
        }
    }

    private void UpdateSnapSize(float snapUnitValue)
    {
        switch(snapUnitValue)
        {
            case 0f:
                grid.size = 0.05f;
                grid.rotationSize = 5;
                break;
            case 0.25f:
                grid.size = 0.10f;
                grid.rotationSize = 15;
                break;
            case 0.5f:
                grid.size = 0.15f;
                grid.rotationSize = 30;
                break;
            case 0.75f:
                grid.size = 0.25f;
                grid.rotationSize = 45;
                break;
            case 1f:
                grid.size = 1f;
                grid.rotationSize = 90;
                break;
        }

        // Assign all other snap unit sliders the same value for the owner
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            for (int i = 0; i < snapUnitsScripts.Length; i++)
            {
                // Ensure that we do not assign this game object's slider values but rather only all other occurrences of UpdateSnapUnits
                if (snapUnitsScripts[i].transform.parent.parent.parent.name != gameObject.transform.parent.parent.parent.name)
                {
                    snapUnitsScripts[i].GetComponent<Slider>().SliderValue = snapUnitValue;
                    snapUnitsScripts[i].GetComponent<UpdateSnapUnits>().lastSnapUnitValue = snapUnitValue;
                }
            }
        }
    }
}
