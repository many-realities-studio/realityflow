using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class CallDeleteCheck : MonoBehaviour
{
    public event UnityAction CallDeleteCheckAction;
    
    public void OpenDeleteMenu()
    {
        Debug.Log(" === Delete Menu ===");
        /*if(deleteMenu != null)
        {
            deleteMenu.SetActive(!deleteMenu.activeInHierarchy);
        }*/

        RealityFlowAPI.DeleteMenu.SetActive(!RealityFlowAPI.DeleteMenu.activeInHierarchy);
        //Debug.Log("Near Menu Toolbox " + (!isActive ? "enabled" : "disabled") + ".");

        CallDeleteCheckAction?.Invoke();
    }
}