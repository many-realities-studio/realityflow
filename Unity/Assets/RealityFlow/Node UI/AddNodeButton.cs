using System;
using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class AddNodeButton : MonoBehaviour
    {
        public TMP_Text displayName;

        [NonSerialized]
        public GraphView view;
        [NonSerialized]
        public NodeDefinition definition;

        public void Add()
        {
            RealityFlowAPI.Instance.AddNodeToGraph(view.Graph, definition);
            view.MarkDirty();
        }
    }
}