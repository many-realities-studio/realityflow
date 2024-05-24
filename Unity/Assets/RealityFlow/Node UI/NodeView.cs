using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeView : MonoBehaviour
{
    Node node;
    public Node Node
    {
        get => node;
        set
        {
            node = value;
            Render();
        }
    }

    [SerializeField]
    GameObject fieldPrefab;
    [SerializeField]
    GameObject inputPortPrefab;
    [SerializeField]
    GameObject outputPortPrefab;

    [SerializeField]
    TextMeshProUGUI title;
    [SerializeField]
    Transform fields;
    [SerializeField]
    Transform inputPorts;
    [SerializeField]
    Transform outputPorts;

    void Render()
    {
        ClearChildren(fields);
        ClearChildren(inputPorts);
        ClearChildren(outputPorts);

        title.text = Node.Definition.Name;
        
        foreach (var def in Node.Definition.Inputs)
        {
            GameObject port = Instantiate(inputPortPrefab);
            port.transform.SetParent(inputPorts);

            InputPortView view = port.GetComponent<InputPortView>();
            view.Definition = def;
        }

        foreach (var def in Node.Definition.Outputs)
        {
            GameObject port = Instantiate(outputPortPrefab);
            port.transform.SetParent(outputPorts);

            OutputPortView view = port.GetComponent<OutputPortView>();
            view.Definition = def;
        }
    }

    void ClearChildren(Transform tf)
    {
        foreach (Transform child in tf)
            Destroy(child.gameObject);
    }
}
