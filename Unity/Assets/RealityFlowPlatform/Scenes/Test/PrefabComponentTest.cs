using UnityEditor;
using UnityEngine;
using System.IO;
using Ubiq.Messaging;
using Microsoft.MixedReality.Toolkit.UX;
using Microsoft.MixedReality.Toolkit.Examples.Demos;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using RealityFlow.NodeUI;
using UnityEngine.XR.Interaction.Toolkit;

public class PrefabComponentTest : MonoBehaviour
{

    public GameObject prefab;
    // Start is called before the first frame update
    void Start()
    {
        GameObject instance = GameObject.Instantiate(prefab);
        // Add All the expected components, check if a component already exists for each type
        // Add MyNetworkedObject script

        instance.AddComponent<ObjectManipulator>();


        ObjectManipulator objManip = instance.GetComponent<ObjectManipulator>();
        Debug.Log("Instantiated");
        if (objManip != null)
        {
            Debug.Log("Before adding listener");
            objManip.firstSelectEntered.AddListener((args) => Debug.Log("Added first select entered"));
            Debug.Log(objManip.firstSelectEntered);
            objManip.lastSelectExited.AddListener((args) => Debug.Log("Added last select exited"));
            objManip.firstSelectEntered.Invoke(new SelectEnterEventArgs());
        }
        else
            Debug.Log("ObjectManipulator not found");

    }
}
