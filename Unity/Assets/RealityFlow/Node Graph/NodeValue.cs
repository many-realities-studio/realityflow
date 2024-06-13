using System;
using System.Linq;
using NaughtyAttributes;
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
        [Dropdown("validTypes")]
        NodeValueType type;

        /// <summary>
        /// Used to determine which types to show in the inspector dropdown 
        /// (just excludes the Any type)
        /// </summary>
        static readonly NodeValueType[] validTypes =
            Enum.GetValues(typeof(NodeValueType))
            .Cast<NodeValueType>()
            .Where(ty => ty != NodeValueType.Any)
            .ToArray();

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

        public NodeValue(int val)
        {
            type = NodeValueType.Int;
            value = new BoxedInt(val);
        }

        public NodeValue(float val)
        {
            type = NodeValueType.Float;
            value = new BoxedFloat(val);
        }

        public NodeValue(Vector2 val)
        {
            type = NodeValueType.Vector2;
            value = new BoxedVector2(val);
        }

        public NodeValue(Vector3 val)
        {
            type = NodeValueType.Vector3;
            value = new BoxedVector3(val);
        }

        public NodeValue(Quaternion val)
        {
            type = NodeValueType.Quaternion;
            value = new BoxedQuaternion(val);
        }

        public NodeValue(ReadonlyGraph val)
        {
            type = NodeValueType.Graph;
            value = val;
        }

        public NodeValue(bool val)
        {
            type = NodeValueType.Bool;
            value = new BoxedBool(val);
        }

        public NodeValue(string str)
        {
            type = NodeValueType.String;
            value = str;
        }

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
            else if (
                Type is NodeValueType.GameObject
                && this.value is string name
                && GameObject.Find(name) is T tObj
            )
            {
                value = tObj;
                return true;
            }
            else if (Type is NodeValueType.Prefab)
                throw new NotImplementedException("Prefab lookup not implemented");
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
            NodeValueType.Graph => typeof(ReadonlyGraph),
            NodeValueType.Bool => typeof(bool),
            NodeValueType.Any => typeof(object),
            NodeValueType.GameObject => typeof(string),
            NodeValueType.Prefab => typeof(string),
            NodeValueType.String => typeof(string),
            _ => throw new ArgumentException(),
        };

        /// <summary>
        /// Get the C# type that this value's type corresponds to.
        /// </summary>
        public Type GetValueType() => GetValueType(Type);

        public static Type GetEvalTimeType(NodeValueType type) => type switch
        {
            NodeValueType.Int => typeof(int),
            NodeValueType.Float => typeof(float),
            NodeValueType.Vector2 => typeof(Vector2),
            NodeValueType.Vector3 => typeof(Vector3),
            NodeValueType.Quaternion => typeof(Quaternion),
            NodeValueType.Graph => typeof(ReadonlyGraph),
            NodeValueType.Bool => typeof(bool),
            NodeValueType.Any => typeof(object),
            NodeValueType.GameObject => typeof(GameObject),
            NodeValueType.Prefab => typeof(GameObject),
            NodeValueType.String => typeof(string),
            _ => throw new ArgumentException(),
        };

        public Type GetEvalTimeType() => GetEvalTimeType(Type);

        static object InternalDefaultFor(NodeValueType type) => type switch
        {
            NodeValueType.Int => new BoxedInt(0),
            NodeValueType.Float => new BoxedFloat(0f),
            NodeValueType.Vector2 => new BoxedVector2(Vector2.zero),
            NodeValueType.Vector3 => new BoxedVector3(Vector3.zero),
            NodeValueType.Quaternion => new BoxedQuaternion(Quaternion.identity),
            NodeValueType.Graph => new ReadonlyGraph(),
            NodeValueType.Bool => new BoxedBool(false),
            NodeValueType.GameObject => null,
            NodeValueType.Prefab => null,
            NodeValueType.String => string.Empty,
            _ => throw new ArgumentException(),
        };

        /// <summary>
        /// Get the default value for a NodeValue of the given type.
        /// </summary>
        public static NodeValue DefaultFor(NodeValueType type) =>
            new() { type = type, value = InternalDefaultFor(type) };

        public static bool TryGetDefaultFor(NodeValueType type, out NodeValue value) 
        {
            try
            {
                value = DefaultFor(type);
                return true;
            }
            catch (ArgumentException)
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Get an instance of a NodeValue from a given value. May fail if the given value is not
        /// of a type that NodeValue may represent, or if the value is ambiguous between multiple
        /// types.
        /// </summary>
        public static NodeValue From<T>(T value) => value switch
        {
            int val => new(val),
            float val => new(val),
            Vector2 val => new(val),
            Vector3 val => new(val),
            Quaternion val => new(val),
            ReadonlyGraph val => new(val),
            bool val => new(val),
            string val => new(val),
            _ => throw new ArgumentException(),
        };

        public static NodeValue Null => new()
        {
            value = null,
            type = NodeValueType.Any,
        };

        public void OnBeforeSerialize()
        {
            if (value is null || GetValueType() != Value.GetType())
                value = InternalDefaultFor(Type);
        }

        public void OnAfterDeserialize()
        {
            if (value is null || GetValueType() != Value.GetType())
                value = InternalDefaultFor(Type);
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