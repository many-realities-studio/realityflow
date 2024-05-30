using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachGameObjectToParameter : MonoBehaviour
{

    public void SendObject()
    {
        // on manipulation end, sends the object to the parameter
    }
    
    public void ToggleParamSetting(){
        //gameObject.GetComponent<ObjectManipulator>().OnManipulationEnded += SendObject;
    }
}
