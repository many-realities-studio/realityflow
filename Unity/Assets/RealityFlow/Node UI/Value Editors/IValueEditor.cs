using System;
using RealityFlow.NodeGraph;

namespace RealityFlow.NodeUI
{
    public interface IValueEditor
    {
        public NodeValue NodeValue { set; }

        public Action<NodeValue> OnTick { get; set; }
    }
}