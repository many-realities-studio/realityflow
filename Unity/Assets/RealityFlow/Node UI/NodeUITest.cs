using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using UnityEngine;

public class NodeUITest : MonoBehaviour
{
    Node node;

    public NodeDefinition AddDef;
    public GameObject NodeUI;

    // Start is called before the first frame update
    void Start()
    {
        node = new(AddDef);

        var ui = Instantiate(NodeUI);
        ui.transform.SetParent(transform);
        ui.GetComponent<NodeView>().Node = node;
    }
}
