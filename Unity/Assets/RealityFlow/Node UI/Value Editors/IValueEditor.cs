using System;
using RealityFlow.NodeGraph;
using TMPro;

namespace RealityFlow.NodeUI
{
    public interface IValueEditor
    {
        public NodeValue NodeValue { set; }

        public TMP_Text Name { get; }

        public Action<NodeValue> OnTick { get; set; }
    }
}