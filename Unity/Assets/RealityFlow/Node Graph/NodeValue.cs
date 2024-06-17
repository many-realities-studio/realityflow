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

        public NodeValue(GameObject go)
        {
            type = NodeValueType.GameObject;
            value = go.name;
        }

        public static NodeValue TemplateObject(GameObject template)
            => new() { type = NodeValueType.TemplateObject, value = template };

        public static NodeValue Variable(string name)
            => new() { type = NodeValueType.Variable, value = name };

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
                // TODO: RealityFlowId Lookup
                Type is NodeValueType.GameObject
                && this.value is string goName
                && GameObject.Find(goName) is T gameObj
            )
            {
                value = gameObj;
                return true;
            }
            else if (
                Type is NodeValueType.TemplateObject
                && this.value is string toName
                && GameObject.Find(toName) is T tempObj
            )
            {
                value = tempObj;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public T UnwrapValue<T>()
        {
            if (TryGetValue(out T value))
                return value;
            else
                throw new ArgumentException($"Could not read NodeValue as type {typeof(T).FullName}");
        }

        public static bool IsAssignableTo(NodeValueType assigned, NodeValueType to)
        {
            return
                to is NodeValueType.Any
                || to is NodeValueType.String
                || assigned == to
                || (assigned == NodeValueType.Int && to == NodeValueType.Float);
        }

        public static bool IsNotAssignableTo(NodeValueType subtype, NodeValueType supertype)
            => !IsAssignableTo(subtype, supertype);

        public string CastToString()
        {
            switch (Type)
            {
                case NodeValueType.Int:
                    return UnwrapValue<int>().ToString();
                case NodeValueType.Float:
                    return UnwrapValue<float>().ToString();
                case NodeValueType.Bool:
                    return UnwrapValue<bool>().ToString();
                case NodeValueType.GameObject:
                    return UnwrapValue<GameObject>().name;
                case NodeValueType.Graph:
                    return UnwrapValue<ReadonlyGraph>().ToString();
                case NodeValueType.Quaternion:
                    return UnwrapValue<Quaternion>().ToString();
                case NodeValueType.String:
                    return UnwrapValue<string>().ToString();
                case NodeValueType.Vector2:
                    return UnwrapValue<Vector2>().ToString();
                case NodeValueType.Vector3:
                    return UnwrapValue<Vector3>().ToString();
                case NodeValueType.TemplateObject:
                    return UnwrapValue<GameObject>().name;
                case NodeValueType.Variable:
                    return UnwrapValue<string>();
            }

            throw new ArgumentException();
        }

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
            NodeValueType.TemplateObject => typeof(string),
            NodeValueType.String => typeof(string),
            NodeValueType.Variable => typeof(string),
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
            NodeValueType.TemplateObject => typeof(GameObject),
            NodeValueType.String => typeof(string),
            NodeValueType.Variable => typeof(string),
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
            NodeValueType.TemplateObject => null,
            NodeValueType.String => string.Empty,
            NodeValueType.Variable => string.Empty,
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
        public static NodeValue From<T>(T value, NodeValueType type) => type switch
        {
            NodeValueType.Int => new(value.AsType<T, int>()),
            NodeValueType.Float => new(value.AsType<T, float>()),
            NodeValueType.Vector2 => new(value.AsType<T, Vector2>()),
            NodeValueType.Vector3 => new(value.AsType<T, Vector3>()),
            NodeValueType.Quaternion => new(value.AsType<T, Quaternion>()),
            NodeValueType.Graph => new(value.AsType<T, ReadonlyGraph>()),
            NodeValueType.Bool => new(value.AsType<T, bool>()),
            NodeValueType.GameObject => new(value.AsType<T, GameObject>()),
            NodeValueType.TemplateObject => TemplateObject(value.AsType<T, GameObject>()),
            NodeValueType.String => new(value.AsType<T, string>()),
            NodeValueType.Variable => Variable(value.AsType<T, string>()),
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