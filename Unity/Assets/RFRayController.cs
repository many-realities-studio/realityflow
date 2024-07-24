using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Unity.XR.CoreUtils;

public class RFRayController : MonoBehaviour
{
    private GameObject thisHand;
    private GameObject rightHand;

    // Highly recommend these are set in editor
    [SerializeField] private GameObject mRTKRayInteractor;
    [SerializeField] private GameObject uIRayInteractor;

    // Start is called before the first frame update
    void Start()
    {
        thisHand = transform.parent.gameObject; // not recommended way of finding controller but should go unused if serialized fields are set
        if(mRTKRayInteractor == null || uIRayInteractor == null)
        {
            mRTKRayInteractor = gameObject.GetComponentInChildren<MRTKRayInteractor>().gameObject;
            uIRayInteractor = gameObject;
        }

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
        // disable the mrtk ray and visualize XR ray
        mRTKRayInteractor.SetActive(false);
        uIRayInteractor.GetComponent<XRInteractorLineVisual>().enabled = true;
    }

    public void EnableMRTKRay()
    {
        // make XR ray invisible and the enable the mrtk ray 
        uIRayInteractor.GetComponent<XRInteractorLineVisual>().enabled = false;
        mRTKRayInteractor.SetActive(true);
    }
}
