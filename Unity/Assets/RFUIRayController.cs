using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Unity.XR.CoreUtils;

public class RFUIRayController : MonoBehaviour
{
    private GameObject leftHand;
    private GameObject rightHand;
    [SerializeField] private GameObject leftRayInteractor;
    [SerializeField] private GameObject rightRayInteractor;

    // Start is called before the first frame update
    void Start()
    {
        var rig = UnityEngine.Object.FindFirstObjectByType<XROrigin>().gameObject;
        leftHand = rig.transform.Find("Camera Offset/MRTK LeftHand Controller").gameObject;
        rightHand = rig.transform.Find("Camera Offset/MRTK RightHand Controller").gameObject;
        if(leftRayInteractor == null || rightRayInteractor == null)
        {
            leftRayInteractor = rightHand.transform.Find("Far Ray XR UI").gameObject;
            rightRayInteractor = leftHand.transform.Find("Far Ray XR UI").gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisableXRRay()
    {
        if(leftRayInteractor.activeInHierarchy)
        {
            leftRayInteractor.SetActive(false);
        }
            
        if(rightRayInteractor.activeInHierarchy)
        {
            rightRayInteractor.SetActive(false);
        }
    }

    public void EnableXRRay()
    {        
        leftRayInteractor.SetActive(true);
        rightRayInteractor.SetActive(true);
    }
}
