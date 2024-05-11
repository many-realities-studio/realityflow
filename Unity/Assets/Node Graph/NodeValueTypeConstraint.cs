using System;
using System.Linq;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class NodeValueTypeConstraint : IEquatable<NodeValueTypeConstraint>
    {
        [Serializable]
        public enum Kind
        {
            Addable,
            Subtractable,
            Multiplicable,
            Divisible,
        }

        [SerializeField]
        Kind kind;
        [SerializeField]
        NodeValueType[] typeArgs;

        public Kind Name => kind;
        public NodeValueType[] Args => typeArgs;

        NodeValueTypeConstraint(Kind name, params NodeValueType[] typeArgs)
        {
            kind = name;
            this.typeArgs = typeArgs;
        }

        public static NodeValueTypeConstraint Addable(NodeValueType other)
            => new(Kind.Addable, other);

        public static NodeValueTypeConstraint Subtractable(NodeValueType other)
            => new(Kind.Subtractable, other);

        public static NodeValueTypeConstraint Multiplicable(NodeValueType other)
            => new(Kind.Multiplicable, other);

        public static NodeValueTypeConstraint Divisible(NodeValueType other)
            => new(Kind.Divisible, other);

        public bool Equals(NodeValueTypeConstraint other)
        {
            return kind == other.kind && typeArgs.SequenceEqual(other.typeArgs);
        }
    }
}
