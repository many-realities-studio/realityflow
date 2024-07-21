using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Unity.XR.CoreUtils;

public class RFRayController : MonoBehaviour
{
    private GameObject leftHand;
    private GameObject rightHand;
    private GameObject leftRayInteractor;
    private GameObject rightRayInteractor;
    private bool leftControllerActiveOnDisable = true;
    private bool rightControllerActiveOnDisable = true;
    private byte alternateEnableDisable = 0;

    // Start is called before the first frame update
    void Start()
    {
        var rig = UnityEngine.Object.FindFirstObjectByType<XROrigin>().gameObject;
        leftHand = rig.transform.Find("Camera Offset/MRTK LeftHand Controller").gameObject;
        rightHand = rig.transform.Find("Camera Offset/MRTK RightHand Controller").gameObject;
        leftRayInteractor = rightHand.GetComponentInChildren<MRTKRayInteractor>().gameObject;
        rightRayInteractor = leftHand.GetComponentInChildren<MRTKRayInteractor>().gameObject;

        //Debug.Log("THIS IS FOR THE INTERACTORS:");
        //Debug.Log(leftRayInteractor);
        //Debug.Log(rightRayInteractor);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisableMRTKRay()
    {
        Debug.Log("MRTK RAYS DISABLED");
        if(alternateEnableDisable == 0)
        {
            // If the lefthandray and righthandray interactor(s) are already active, set 
            // that it was active on disable and disable. If the ray interactor isn't active,
            // set that it was not active on disable.
            if(leftRayInteractor.activeInHierarchy)
            {
                leftControllerActiveOnDisable = true;
                leftRayInteractor.SetActive(false);
            } else
            {
                leftControllerActiveOnDisable = false;
            }
            
            if(rightRayInteractor.activeInHierarchy)
            {
                rightControllerActiveOnDisable = true;
                rightRayInteractor.SetActive(false);
            } else
            {
                rightControllerActiveOnDisable = false;
            }

            // Enable the XRI line visual because the MRTK ones are disabled.
            gameObject.GetComponent<XRInteractorLineVisual>().enabled = true;
            alternateEnableDisable = 1;
        }
        
    }

    public void EnableMRTKRay()
    {
        Debug.Log("MRTK RAYS ENABLED");

        if(alternateEnableDisable == 1)
        {
            // Disable the XRI line visual because the MRTK ones are endabled.
            gameObject.GetComponent<XRInteractorLineVisual>().enabled = false;
            
            // If the controllers were active when disable was set, enable it
            if(leftControllerActiveOnDisable)
            {
                leftRayInteractor.SetActive(true);
            }
            
            if(rightControllerActiveOnDisable)
            {
                rightRayInteractor.SetActive(true);
            }
            alternateEnableDisable = 0;
        }
    }
}
