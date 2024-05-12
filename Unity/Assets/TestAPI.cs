using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAPI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var gameObject = RealityFlowAPI.Instance.GetGameObject("Test Object");
        Debug.Log(gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
