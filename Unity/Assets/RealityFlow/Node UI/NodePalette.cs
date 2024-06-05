using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using RealityFlow.NodeGraph;
using UnityEngine;

public class NodePalette : MonoBehaviour
{
    List<NodeDefinition> defs;
    VirtualizedScrollRectList list;

    void Start()
    {
        defs = RealityFlowAPI.Instance.GetAvailableNodeDefinitions();
        list = GetComponent<VirtualizedScrollRectList>();

        list.SetItemCount(defs.Count);
        list.OnVisible += OnVisible;
    }

    void OnVisible(GameObject element, int index)
    {

    }
}
