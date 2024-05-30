using Packages.realityflow_package.Runtime.scripts;
using RealityFlow.Plugin.Scripts;
using GraphProcessor; // TODO: Fix reference
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RuntimeVSGraphManipulation : MonoBehaviour
{
    private TMP_InputField input;

    void Start()
    {
        input = this.gameObject.GetComponent<TMP_InputField>();
    }

    public void CreateVSGraph()
    {
        if(input == null)
        {
            Debug.Log("Could not find input field");
        }
        else if(input.text.ToString() == "")
        {
            Debug.Log("Input field empty");
        }

        FlowVSGraph graph = new FlowVSGraph(input.text.ToString());
        Debug.Log(graph.Name);
        graph.Name = input.text.ToString();
        Debug.Log(graph);
        Debug.Log(JsonUtility.ToJson(graph));
        // Debug.Log(graph.Id);
        // Debug.Log(graph._id);

        Operations.CreateVSGraph(graph, ConfigurationSingleton.SingleInstance.CurrentProject.Id, (_, e) => Debug.Log(e.message));
    }

    public void DeleteVSGraph()
    {

    }
}
