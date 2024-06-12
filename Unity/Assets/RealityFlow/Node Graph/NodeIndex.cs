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
    public struct NodeIndex : IEquatable<NodeIndex>
    {
        [SerializeField]
        Arena<Node>.Index index;

        public NodeIndex(Arena<Node>.Index index)
        {
            this.index = index;
        }

        public override string ToString()
        {
            return $"NodeIndex({index.Value})";
        }

        public override bool Equals(object obj)
        {
            if (obj is NodeIndex index)
                return Equals(index);
            return false;
        }

        public override int GetHashCode()
        {
            return index.GetHashCode();
        }

        public readonly bool Equals(NodeIndex other)
        {
            return index == other.index;
        }

        public static implicit operator Arena<Node>.Index(NodeIndex index) => index.index;
        public static implicit operator NodeIndex(Arena<Node>.Index index) => new(index);

        public static bool operator ==(NodeIndex lhs, NodeIndex rhs) => lhs.Equals(rhs);
        public static bool operator !=(NodeIndex lhs, NodeIndex rhs) => !lhs.Equals(rhs);
    }
}