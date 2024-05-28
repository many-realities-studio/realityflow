using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class GridTool grabs a reference to the active state of the grid tool and updates the snapping mode of the user.
/// </summary>
public class GridTool : MonoBehaviour
{
    public bool isActive;
    [Tooltip("On = 1\nOff = 0")]
    public float currentSnapModeValue;
    public float currentSnapUnitValue;

    void Start()
    {
        UpdateSnapMode.OnSnapModeChange += SwapSnapModeValues;
        UpdateSnapUnits.OnSnapUnitChange += SwapSnapUnitValues;
    }

    void OnDestroy()
    {
        UpdateSnapMode.OnSnapModeChange -= SwapSnapModeValues;
        UpdateSnapUnits.OnSnapUnitChange += SwapSnapUnitValues;
    }

    private void SwapSnapModeValues(float newSnapModeValue)
    {
        currentSnapModeValue = newSnapModeValue;
    }

    private void SwapSnapUnitValues(float newSnapUnitValue)
    {
        currentSnapUnitValue = newSnapUnitValue;
    }

    public void Activate(int tool, bool status)
    {
        if(tool == 6)
        {
            isActive = status;
        }
    }
}
