using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAPI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject gameObject = RealityFlowAPI.Instance.GetPrefabByName("TreeStump");
        if (gameObject != null)
        {
            Debug.Log(gameObject.name);
        }
        else
        {
            Debug.Log("Prefab not found.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
