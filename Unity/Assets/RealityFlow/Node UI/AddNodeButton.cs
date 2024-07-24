using System;
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
            if (view.Graph == null)
                return;

            RealityFlowAPI.Instance.AddNodeToGraph(view.Graph, definition);
            view.MarkDirty();
        }
    }
}