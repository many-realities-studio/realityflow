using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.UX;

public class SendString : MonoBehaviour
{
    InputField iField;
    string text;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        iField = GetComponent<InputField>();
    }

}
