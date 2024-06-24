using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnableNearMenuToolbox : MonoBehaviour
{
    // Reference to the Near Menu Toolbox GameObject
    public GameObject nearMenuToolbox;

    // Define an event that other scripts can subscribe to
    public event UnityAction EnableNearMenuToolboxAction;

    // Method to toggle the visibility of the Near Menu Toolbox
    public void ToggleNearMenuToolbox()
    {
        Debug.Log(" === TOGGLE NEAR MENU TOOLBOX ===");
        if (nearMenuToolbox != null)
        {
            bool isActive = nearMenuToolbox.activeSelf;
            nearMenuToolbox.SetActive(!isActive);
            Debug.Log("Near Menu Toolbox " + (!isActive ? "enabled" : "disabled") + ".");

            // Invoke the event to notify subscribers
            EnableNearMenuToolboxAction?.Invoke();
        }
        else
        {
            Debug.LogError("Near Menu Toolbox is not assigned.");
        }
    }
}