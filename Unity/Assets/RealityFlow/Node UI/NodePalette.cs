using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            string[] whitelist = RealityFlowAPI.Instance.GetNodeWhitelist();
            defs = RealityFlowAPI.Instance
                .NodeDefinitionDict
                .Values
                .Where(def => whitelist == null || whitelist.Contains(def.Name))
                .OrderBy(def => def.Name)
                .ToList();

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

    internal class NewClass
    {
        public NewClass()
        {
        }

        public override bool Equals(object obj)
        {
            return obj is NewClass other;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}