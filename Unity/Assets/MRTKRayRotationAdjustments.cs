using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MRTKRayRotationAdjustments : MonoBehaviour
{

    void Awake()
    {
        if(UnityEngine.XR.XRSettings.enabled == true)
        {
            gameObject.transform.localRotation = Quaternion.Euler(60, 0, 0);
        }
        //gameObject.transform.localRotation = Quaternion.Euler(60, 0, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        //if(UnityEngine.XR.XRSettings.enabled == true)
        //{
            //gameObject.transform.localRotation = Quaternion.Euler(60, 0, 0);
        //}
        //transform.localRotation = Quaternion.Euler(60, 0, 0);
        //gameObject.transform.localRotation = Quaternion.Euler(60, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
