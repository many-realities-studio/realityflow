using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRTKRayRotationAdjustments : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(UnityEngine.XR.XRSettings.enabled)
        {
            gameObject.transform.localRotation = Quaternion.Euler(60, 0, 0);
        }
        //gameObject.transform.localRotation = Quaternion.Euler(60, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
