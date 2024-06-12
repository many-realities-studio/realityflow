using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using RealityFlow.NodeGraph;
using UnityEngine;
using UnityEngine.UI;

namespace RealityFlow.NodeUI
{
    public class NodePalette : MonoBehaviour
    {
        public Paginator page;
        public GraphView graphView;

        List<NodeDefinition> defs;

        void Start()
        {
            defs = RealityFlowAPI.Instance.GetAvailableNodeDefinitions();

            page.OnShow = (element, index) =>
            {
                element.name = defs[index].Name;
                AddNodeButton button = element.GetComponent<AddNodeButton>();
                button.displayName.text = defs[index].Name;
                button.view = graphView;
                button.definition = defs[index];
            };
            page.ItemCount = defs.Count;
        }
    }
}