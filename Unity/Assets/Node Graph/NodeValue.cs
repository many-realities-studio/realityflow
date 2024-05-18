using System;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    /// <summary>
    /// Represents any possible value that can be passed around or stored in a node graph.
    /// Specially built to be serialized correctly despite containing polymorphic values.
    /// A weird class, because unfortunately C# lacks good discriminated unions.
    /// Is immutable.
    /// Improvements to this class' design welcome.
    /// </summary>
    [Serializable]
    public class NodeValue : ISerializationCallbackReceiver
    {
        /// <summary>
        /// A discriminant for the exact type that this value is.
        /// Note: cannot be `Any`, which is equivalent to `Object`.
        /// </summary>
        [SerializeField]
        NodeValueType type;

        /// <summary>
        /// The stored value. If the value is a value type (aka struct or enum, as opposed to a reference type/class)
        /// then it will be boxed using the relevant Box class below. This is necessary because unity
        /// doesn't serialize value types properly with SerializeReference, even through an object.
        /// 
        /// This does put every value type through 3 levels of indirection; once through NodeValue, then through
        /// .value, then through the Box.
        /// </summary>
        [SerializeField]
        [SerializeReference]
        object value;

        NodeValue() { }

        public NodeValueType Type => type;
        public object Value => value is Boxed box ? box.DynValue : value;

        /// <summary>
        /// Attempts to read this value as the given type. May fail, if this value is not that type.
        /// </summary>
        public bool TryGetValue<T>(out T value)
        {
            if (this.value is T tValue)
            {
                value = tValue;
                return true;
            }
            else if (this.value is Boxed box && box.DynValue is T boxedValue)
            {
                value = boxedValue;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public static bool IsAssignableTo(NodeValueType assigned, NodeValueType to)
        {
            return
                to is NodeValueType.Any
                || assigned == to
                || (assigned == NodeValueType.Int && to == NodeValueType.Float);
        }

        public static bool IsNotAssignableTo(NodeValueType subtype, NodeValueType supertype)
            => !IsAssignableTo(subtype, supertype);

        /// <summary>
        /// Get the C# type that the given NodeValueType corresponds to.
        /// </summary>
        public static Type GetValueType(NodeValueType type) => type switch
        {
            NodeValueType.Int => typeof(int),
            NodeValueType.Float => typeof(float),
            NodeValueType.Vector2 => typeof(Vector2),
            NodeValueType.Vector3 => typeof(Vector3),
            NodeValueType.Quaternion => typeof(Quaternion),
            NodeValueType.Graph => typeof(Graph),
            NodeValueType.Bool => typeof(bool),
            NodeValueType.Any => typeof(object),
            _ => throw new ArgumentException(),
        };

        /// <summary>
        /// Get the C# type that this value's type corresponds to.
        /// </summary>
        public Type GetValueType() => GetValueType(Type);

        /// <summary>
        /// Get the default value for a NodeValue of the given type.
        /// </summary>
        public static NodeValue DefaultFor(NodeValueType type) => type switch
        {
            NodeValueType.Int => From(0),
            NodeValueType.Float => From(0f),
            NodeValueType.Vector2 => From(Vector2.zero),
            NodeValueType.Vector3 => From(Vector3.zero),
            NodeValueType.Quaternion => From(Quaternion.identity),
            NodeValueType.Graph => From(new Graph()),
            NodeValueType.Bool => From(false),
            _ => throw new ArgumentException(),
        };

        /// <summary>
        /// Get an instance of a NodeValue from a given value. May fail if the given value is not
        /// of a type that NodeValue may represent.
        /// </summary>
        public static NodeValue From<T>(T value) => value switch
        {
            int val => From(val),
            float val => From(val),
            Vector2 val => From(val),
            Vector3 val => From(val),
            Quaternion val => From(val),
            Graph val => From(val),
            bool val => From(val),
            _ => throw new ArgumentException(),
        };
        public static NodeValue From(int value) => new() { type = NodeValueType.Int, value = new BoxedInt(value) };
        public static NodeValue From(float value) => new() { type = NodeValueType.Float, value = new BoxedFloat(value) };
        public static NodeValue From(Vector2 value) => new() { type = NodeValueType.Vector2, value = new BoxedVector2(value) };
        public static NodeValue From(Vector3 value) => new() { type = NodeValueType.Vector3, value = new BoxedVector3(value) };
        public static NodeValue From(Quaternion value) => new() { type = NodeValueType.Quaternion, value = new BoxedQuaternion(value) };
        public static NodeValue From(Graph value) => new() { type = NodeValueType.Graph, value = value };
        public static NodeValue From(bool value) => new() { type = NodeValueType.Bool, value = new BoxedBool(value) };

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            // Initialize all reference types, NodeValue cannot be null.
            if (Type is NodeValueType.Graph && value is null)
                value = new Graph();
        }

        // See the comment on `value` above for more details on these classes and why they're needed.

        /// <summary>
        /// This abstract box is used to retrieve the value of a box without branching on every possible 
        /// type, like with a switch case.
        /// </summary>
        abstract class Boxed
        {
            public abstract object DynValue { get; }
        }

        /// <summary>
        /// Reusable implementation to cut down on repetition.
        /// </summary>
        [Serializable]
        class Boxed<T> : Boxed
        {
            public T value;

            public Boxed(T value)
            {
                this.value = value;
            }

            public override object DynValue => value;
        }

        [Serializable]
        class BoxedInt : Boxed<int>
        {
            public BoxedInt(int value) : base(value) { }
        }
        [Serializable]
        class BoxedFloat : Boxed<float>
        {
            public BoxedFloat(float value) : base(value) { }
        }
        [Serializable]
        class BoxedVector2 : Boxed<Vector2>
        {
            public BoxedVector2(Vector2 value) : base(value) { }
        }
        [Serializable]
        class BoxedVector3 : Boxed<Vector3>
        {
            public BoxedVector3(Vector3 value) : base(value) { }
        }
        [Serializable]
        class BoxedQuaternion : Boxed<Quaternion>
        {
            public BoxedQuaternion(Quaternion value) : base(value) { }
        }
        [Serializable]
        class BoxedBool : Boxed<bool>
        {
            public BoxedBool(bool value) : base(value) { }
        }
    }
}