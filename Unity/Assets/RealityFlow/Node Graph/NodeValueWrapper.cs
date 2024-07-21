using UnityEngine;
using System;

namespace RealityFlow.NodeGraph{
    [Serializable]
    public struct NodeValueWrapper
    {
            [SerializeReference]
            [DiscUnion]
            NodeValue value;
            public NodeValue Value => value;

            public NodeValueWrapper(NodeValue value)
            {
                this.value = value;
            }
    }
}