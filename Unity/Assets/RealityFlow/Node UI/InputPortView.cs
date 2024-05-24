using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;

public class InputPortView : MonoBehaviour
{
    NodePortDefinition def;
    public NodePortDefinition Definition 
    {
        get => def;
        set
        {
            def = value;
            Render();
        }
    }

    [SerializeField]
    TextMeshProUGUI portName;

    void Render()
    {
        portName.text = Definition.Name;
    }
}
