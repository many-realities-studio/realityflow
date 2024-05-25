using System;
using NaughtyAttributes;
using RealityFlow.Scripting;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class NodePortDefinition
    {
        public string Name;
        public bool Optional;
        public NodeValueType Type;
    }
}