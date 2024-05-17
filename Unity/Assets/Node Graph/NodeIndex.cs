using System;
using RealityFlow.Collections;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    /// <summary>
    /// A typesafe index into the arena of nodes of a given graph. Avoid mixing up which graph a 
    /// node index belongs to.
    /// </summary>
    [Serializable]
    public struct NodeIndex
    {
        [SerializeField]
        Arena<Node>.Index index;

        public NodeIndex(Arena<Node>.Index index)
        {
            this.index = index;
        }

        public static implicit operator Arena<Node>.Index(NodeIndex index) => index.index;
        public static implicit operator NodeIndex(Arena<Node>.Index index) => new(index);
    }
}