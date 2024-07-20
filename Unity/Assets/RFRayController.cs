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

        gameObject.GetComponent<XRInteractorLineVisual>().enabled = true;
        
    }

    public void EnableMRTKRay()
    {
        gameObject.GetComponent<XRInteractorLineVisual>().enabled = false;
        if(leftControllerActiveOnDisable)
        {
            leftRayInteractor.SetActive(true);
        }
        
        if(rightControllerActiveOnDisable)
        {
            rightRayInteractor.SetActive(true);
        }
        
    }
}
