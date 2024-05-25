using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpawnButtonHandler : MonoBehaviour
{
    public RealityFlowAPITest tester;

    void Start()
    {
        // Get the Button component and add a listener to it
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        // Call the method in RealityFlowAPITest to spawn the object
        if (tester != null)
        {
            return;
        }
    }
}
