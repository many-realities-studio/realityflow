using System;
using RealityFlow.Scripting;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class NodePortDefinition
    {
        public string Name;
        public bool Optional;
        public NodeValueType Type;

        public string GetDescriptor()
        {
            return $@"  {{
                    name: ""{Name}"",
                    type: ""{Type}"",
                }}";
        }
    }
}