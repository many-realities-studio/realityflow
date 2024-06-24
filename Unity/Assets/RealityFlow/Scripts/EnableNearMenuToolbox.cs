using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
// Where is the button? 
public class EnableNearMenuToolbox : MonoBehaviour
{
    // Define an event that other scripts can subscribe to
    public event UnityAction EnableNearMenuToolboxAction;

    // Method to toggle the visibility of the Near Menu Toolbox
    public void ToggleNearMenuToolbox()
    {
        Debug.Log(" === TOGGLE NEAR MENU TOOLBOX ===");
// Mabye. One Moment.
        RealityFlowAPI.NearMenuToolbox.SetActive(!RealityFlowAPI.NearMenuToolbox.activeInHierarchy);
        // Debug.Log("Near Menu Toolbox " + (!isActive ? "enabled" : "disabled") + ".");

        // Invoke the event to notify subscribers
        EnableNearMenuToolboxAction?.Invoke();
    }
}