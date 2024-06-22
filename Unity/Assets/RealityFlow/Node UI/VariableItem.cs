using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using RealityFlow.NodeUI;
using TMPro;
using UnityEngine;

public class VariableItem : MonoBehaviour
{
    public TMP_Text title;
    public string varName;
    public NodeValueType type;
    public GraphView view;

    public void Select()
    {
        view.SetSelectedVariable(varName);
    }

    public void AddGetNode()
    {
        view.AddGetVariableNode(varName, type);
    }

    public void AddSetNode()
    {
        view.AddSetVariableNode(varName, type);
    }
}
