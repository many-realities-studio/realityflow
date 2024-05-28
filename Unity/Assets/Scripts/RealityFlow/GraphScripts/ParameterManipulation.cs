using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using UnityEngine.EventSystems;
using Microsoft.MixedReality.Toolkit;


// Used to assign a gameobject value to a parameter, works very similarly to NodeManipulation
public class ParameterManipulation : MonoBehaviour
{
    Vector3 position;
    Quaternion rotation;
    public ParameterView pv;
    public static ParameterManipulation instance;
    void Awake()
    {
        instance = this;
        Physics.IgnoreLayerCollision(3, 3);
    }

    public void NodeRaycastCreation()
    {
        GameObject spherePoint = this.gameObject;
        GameObject ptr = null;
        // TODO UPDATE WITH XRI
        //foreach (IMixedRealityController controller in MixedRealityToolkit.InputSystem.DetectedControllers)
        //{
        //    if (controller.Visualizer?.GameObjectProxy != null)
        //    {
        //        Debug.Log("Visualizer Game Object: " + controller.Visualizer.GameObjectProxy);
        //    }
        //    else
        //    {
        //        Debug.Log("Controller has no visualizer!");
        //    }


        //    foreach (IMixedRealityPointer pointer in controller.InputSource.Pointers)
        //    {
        //        if (pointer is MonoBehaviour)
        //        {
        //            var monoBehavior = pointer as MonoBehaviour;
        //            if( monoBehavior.gameObject.name.Contains("ParabolicPointer"))
        //            {
        //                ptr = monoBehavior.gameObject;
        //            }
        //        }
        //    }
        //    if(ptr == null)
        //    {
        //        ptr = spherePoint;
        //    }
        //}
        RaycastHit hit;
        if( Physics.Raycast(spherePoint.transform.position, ptr.transform.forward, out hit, LayerMask.NameToLayer("RFObject")) && ptr != null )
        {
            pv.SetParameterValue(hit.collider.gameObject);
            Destroy(spherePoint);
        }
        else
        {
            Destroy(spherePoint);
        }
    }
}