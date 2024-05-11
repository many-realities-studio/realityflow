using System;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class NodeValueType 
    {
        [SerializeReference]
        public Kind Data = new Kind.Int();

        [Serializable]
        public class Kind 
        {
            public virtual NodeValueTypeConstraint[] Constraints 
                => Array.Empty<NodeValueTypeConstraint>();

            public virtual object DefaultValue { get; }

            [Serializable]
            public class Int : Kind 
            {
                public override NodeValueTypeConstraint[] Constraints => new[] { 
                    NodeValueTypeConstraint.Addable(NodeValueType.Int),
                    NodeValueTypeConstraint.Subtractable(NodeValueType.Int),
                    NodeValueTypeConstraint.Multiplicable(NodeValueType.Int),
                    NodeValueTypeConstraint.Divisible(NodeValueType.Int),
                    NodeValueTypeConstraint.Addable(NodeValueType.Float),
                    NodeValueTypeConstraint.Subtractable(NodeValueType.Float),
                    NodeValueTypeConstraint.Multiplicable(NodeValueType.Float),
                    NodeValueTypeConstraint.Divisible(NodeValueType.Float),
                    NodeValueTypeConstraint.Multiplicable(NodeValueType.Vector2),
                    NodeValueTypeConstraint.Divisible(NodeValueType.Vector2),
                    NodeValueTypeConstraint.Multiplicable(NodeValueType.Vector3),
                    NodeValueTypeConstraint.Divisible(NodeValueType.Vector3),
                };

                public override object DefaultValue => 0L;
            }

            [Serializable]
            public class Float : Kind {
                public override NodeValueTypeConstraint[] Constraints => new[] {
                    NodeValueTypeConstraint.Addable(NodeValueType.Float),
                    NodeValueTypeConstraint.Subtractable(NodeValueType.Float),
                    NodeValueTypeConstraint.Multiplicable(NodeValueType.Float),
                    NodeValueTypeConstraint.Divisible(NodeValueType.Float),
                    NodeValueTypeConstraint.Addable(NodeValueType.Int),
                    NodeValueTypeConstraint.Subtractable(NodeValueType.Int),
                    NodeValueTypeConstraint.Multiplicable(NodeValueType.Int),
                    NodeValueTypeConstraint.Divisible(NodeValueType.Int),
                    NodeValueTypeConstraint.Multiplicable(NodeValueType.Vector2),
                    NodeValueTypeConstraint.Divisible(NodeValueType.Vector2),
                    NodeValueTypeConstraint.Multiplicable(NodeValueType.Vector3),
                    NodeValueTypeConstraint.Divisible(NodeValueType.Vector3),
                };

                public override object DefaultValue => 0d;
            }

            [Serializable]
            public class Vector2 : Kind { }

            [Serializable]
            public class Vector3 : Kind { }

            [Serializable]
            public class Quaternion : Kind { }

            [Serializable]
            public class TypeVariable : Kind
            {
                public int Index;
            }
        }

        public static NodeValueType Int { get; } = new() { Data = new Kind.Int() };
        public static NodeValueType Float { get; } = new() { Data = new Kind.Float() };
        public static NodeValueType Vector2 { get; } = new() { Data = new Kind.Vector2() };
        public static NodeValueType Vector3 { get; } = new() { Data = new Kind.Vector3() };
        public static NodeValueType Quaternion { get; } = new() { Data = new Kind.Quaternion() };
        public static NodeValueType TypeVariable(int index) => new() { 
            Data = new Kind.TypeVariable() { Index = index } 
        };
    }
}