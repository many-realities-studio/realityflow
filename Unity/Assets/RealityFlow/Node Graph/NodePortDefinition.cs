using System;
using NaughtyAttributes;
using RealityFlow.Scripting;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class NodeInputPortDefinition
    {
        public string Name;

        [SerializeField]
        NodeValueType typeSerialized;
        public INodeInputType Type
        {
            get => (INodeInputType)typeSerialized;
            set => typeSerialized = (NodeValueType)value;
        }
    }

    [Serializable]
    public class NodeOutputPortDefinition
    {
        public string Name;

        [SerializeField]
        NodeValueType typeSerialized;
        public INodeOutputType Type
        {
            get => (INodeOutputType)typeSerialized;
            set => typeSerialized = (NodeValueType)value;
        }
    }
}