using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class CallDeleteCheck : MonoBehaviour
{
    // Define an event that other scripts can subscribe to
    public event UnityAction CallDeleteCheckAction;

    //public GameObject deleteMenu;

    // Method to toggle the visibility of the Near Menu Toolbox
    public void OpenDeleteMenu()
    {
        Debug.Log(" === Delete Menu ===");
        /*if(deleteMenu != null)
        {
            deleteMenu.SetActive(!deleteMenu.activeInHierarchy);
        }*/

        RealityFlowAPI.Instance.LogActionToServer("Deleted All Objects", new {});
        RealityFlowAPI.DeleteMenu.SetActive(!RealityFlowAPI.DeleteMenu.activeInHierarchy);
        //Debug.Log("Near Menu Toolbox " + (!isActive ? "enabled" : "disabled") + ".");

        // Invoke the event to notify subscribers
        CallDeleteCheckAction?.Invoke();
    }
}