using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableNearMenuToolbox : MonoBehaviour
{
    // Reference to the Near Menu Toolbox GameObject
    public GameObject nearMenuToolbox;

    // Method to toggle the visibility of the Near Menu Toolbox
    public void ToggleNearMenuToolbox()
    {
        if (nearMenuToolbox != null)
        {
            bool isActive = nearMenuToolbox.activeSelf;
            nearMenuToolbox.SetActive(!isActive);
            Debug.Log("Near Menu Toolbox " + (!isActive ? "enabled" : "disabled") + ".");
        }
        else
        {
            Debug.LogError("Near Menu Toolbox is not assigned.");
        }
    }
}
