using System;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    /// <summary>
    /// Represents any possible value that can be passed around or stored in a node graph.
    /// Specially built to be serialized correctly despite containing polymorphic values.
    /// A slightly weird class, because unfortunately C# lacks good discriminated unions.
    /// All implementations should be immutable.
    /// Improvements to this class' design welcome.
    /// </summary>
    [Serializable]
    public abstract class NodeValue
    {
        public abstract NodeValueType ValueType { get; }

        public abstract object DynValue { get; }

        protected virtual bool TryGetValue<T>(out T value)
        {
            if (DynValue is T tValue)
            {
                value = tValue;
                return true;
            }

            value = default;
            return false;
        }

        public virtual string CastToString() => DynValue.ToString();

        public static bool TryGetValue<T>(NodeValue nodeValue, out T value)
        {
            if (nodeValue != null && nodeValue.TryGetValue(out value))
                return true;
            else
            {
                value = default;
                return false;
            }
        }

        public static T UnwrapValue<T>(NodeValue nodeValue)
        {
            if (TryGetValue(nodeValue, out T value))
                return value;
            else
                throw new ArgumentException($"Could not read NodeValue as type {typeof(T).FullName}");
        }

        public static readonly NodeValueType[] valueTypes =
            new[] {
                NodeValueType.Bool,
                NodeValueType.Int,
                NodeValueType.Float,
                NodeValueType.Vector2,
                NodeValueType.Vector3,
                NodeValueType.Quaternion,
                NodeValueType.GameObject,
                NodeValueType.TemplateObject,
                NodeValueType.String,
            };

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

        public Type GetEvalTimeType() => GetEvalTimeType(ValueType);

        /// <summary>
        /// Get the default value for a NodeValue of the given type.
        /// </summary>
        public static NodeValue DefaultFor(NodeValueType type) => type switch
        {
            NodeValueType.Int => new IntValue(0),
            NodeValueType.Float => new FloatValue(0f),
            NodeValueType.Vector2 => new Vector2Value(Vector2.zero),
            NodeValueType.Vector3 => new Vector3Value(Vector3.zero),
            NodeValueType.Quaternion => new QuaternionValue(Quaternion.identity),
            // NodeValueType.Graph => new ReadonlyGraph(),
            NodeValueType.Bool => new BoolValue(false),
            NodeValueType.GameObject => new GameObjectValue(null),
            NodeValueType.TemplateObject => new TemplateObjectValue(null),
            NodeValueType.String => new StringValue(string.Empty),
            NodeValueType.Variable => new VariableValue(null),
            _ => throw new ArgumentException(),
        };

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
            NodeValueType.Int => new IntValue(value.AsType<T, int>()),
            NodeValueType.Float => new FloatValue(value.AsType<T, float>()),
            NodeValueType.String => new StringValue(value.AsType<T, string>()),
            NodeValueType.Bool => new BoolValue(value.AsType<T, bool>()),
            NodeValueType.Vector2 => new Vector2Value(value.AsType<T, Vector2>()),
            NodeValueType.Vector3 => new Vector3Value(value.AsType<T, Vector3>()),
            NodeValueType.Quaternion => new QuaternionValue(value.AsType<T, Quaternion>()),
            // NodeValueType.Graph => new GraphValue(value.AsType<T, ReadonlyGraph>()),
            NodeValueType.GameObject => new GameObjectValue(value.AsType<T, GameObject>()),
            NodeValueType.TemplateObject => new TemplateObjectValue(value.AsType<T, GameObject>()),
            NodeValueType.Variable => new VariableValue(value.AsType<T, string>()),
            _ => throw new ArgumentException(),
        };
    }

    [Serializable]
    public class IntValue : NodeValue
    {
        [SerializeField]
        int value;
        public int Value => value;

        public IntValue()
        {
            value = 0;
        }

        public IntValue(int value) 
        {
            this.value = value;
        }

        public override NodeValueType ValueType => NodeValueType.Int;

        public override object DynValue => value;
    }

    [Serializable]
    public class FloatValue : NodeValue
    {
        [SerializeField]
        float value;
        public float Value => value;

        public FloatValue()
        {
            value = 0;
        }

        public FloatValue(float value)
        {
            this.value = value;
        }

        public override NodeValueType ValueType => NodeValueType.Float;

        public override object DynValue => value;
    }

    [Serializable]
    public class StringValue : NodeValue
    {
        [SerializeField]
        string value;
        public string Value => value;

        public StringValue()
        {
            value = "";
        }

        public StringValue(string value)
        {
            this.value = value;
        }

        public override NodeValueType ValueType => NodeValueType.String;

        public override object DynValue => value;
    }

    [Serializable]
    public class BoolValue : NodeValue
    {
        [SerializeField]
        bool value;
        public bool Value => value;

        public BoolValue()
        {
            value = false;
        }

        public BoolValue(bool value)
        {
            this.value = value;
        }

        public override NodeValueType ValueType => NodeValueType.Bool;

        public override object DynValue => value;
    }

    [Serializable]
    public class Vector2Value : NodeValue
    {
        [SerializeField]
        Vector2 value;
        public Vector2 Value => value;

        public Vector2Value()
        {
            value = Vector2.zero;
        }

        public Vector2Value(Vector2 value)
        {
            this.value = value;
        }

        public override NodeValueType ValueType => NodeValueType.Vector2;

        public override object DynValue => value;
    }

    [Serializable]
    public class Vector3Value : NodeValue
    {
        [SerializeField]
        Vector3 value;
        public Vector3 Value => value;

        public Vector3Value()
        {
            value = Vector3.zero;
        }

        public Vector3Value(Vector3 value)
        {
            this.value = value;
        }

        public override NodeValueType ValueType => NodeValueType.Vector3;

        public override object DynValue => value;
    }

    [Serializable]
    public class QuaternionValue : NodeValue
    {
        [SerializeField]
        Quaternion value;
        public Quaternion Value => value;

        public QuaternionValue()
        {
            value = Quaternion.identity;
        }

        public QuaternionValue(Quaternion value)
        {
            this.value = value;
        }

        public override NodeValueType ValueType => NodeValueType.Quaternion;

        public override object DynValue => value;
    }

    [Serializable]
    public class GameObjectValue : NodeValue
    {
        [SerializeField]
        string realityflowId;
        public string RealityFlowId => realityflowId;

        public GameObjectValue()
        {
            realityflowId = null;
        }

        public GameObjectValue(GameObject obj)
        {
            if (RealityFlowAPI.Instance.SpawnedObjects.TryGetValue(obj, out RfObject rfObj))
                realityflowId = rfObj.id;
            else
            {
                realityflowId = null;
                Debug.LogError("Failed to lookup object ID");
            }
        }

        public override NodeValueType ValueType => NodeValueType.GameObject;

        public GameObject Value
        {
            get
            {
                if (RealityFlowAPI.Instance.SpawnedObjectsById.TryGetValue(realityflowId, out GameObject obj))
                    return obj;
                else
                {
                    Debug.LogError($"Failed to lookup object by id {realityflowId}");
                    return null;
                }
            }
        }

        protected override bool TryGetValue<T>(out T value)
        {
            if (Value is T tValue)
            {
                value = tValue;
                return true;
            }
            value = default;
            return false;
        }

        public override object DynValue => Value;
    }

    [Serializable]
    public class TemplateObjectValue : GameObjectValue
    {
        public TemplateObjectValue() : base() { }

        public TemplateObjectValue(GameObject obj) : base(obj) { }

        public override NodeValueType ValueType => NodeValueType.TemplateObject;
    }

    [Serializable]
    public class VariableValue : NodeValue
    {
        [SerializeField]
        string varName;
        public string VarName => varName;

        public VariableValue()
        {
            varName = null;
        }

        public VariableValue(string varName)
        {
            this.varName = varName;
        }

        public override NodeValueType ValueType => NodeValueType.Variable;

        public override object DynValue => varName;
    }
}