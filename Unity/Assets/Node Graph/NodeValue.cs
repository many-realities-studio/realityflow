using System;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class NodeValue : ISerializationCallbackReceiver
    {
        [SerializeField]
        NodeValueType type;
        
        [SerializeField]
        [SerializeReference]
        object value;

        NodeValue() {}

        public NodeValueType Type => type;
        public object Value => value is Boxed box ? box.DynValue : value;

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

        public Type GetValueType() => GetValueType(Type);

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
            if (Type is NodeValueType.Graph && value is null)
                value = new Graph();
        }

        abstract class Boxed 
        {
            public abstract object DynValue { get; }
        }

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

        class BoxedInt : Boxed<int>
        {
            public BoxedInt(int value) : base(value) { }
        }
        class BoxedFloat : Boxed<float>
        {
            public BoxedFloat(float value) : base(value) { }
        }
        class BoxedVector2 : Boxed<Vector2>
        {
            public BoxedVector2(Vector2 value) : base(value) { }
        }
        class BoxedVector3 : Boxed<Vector3>
        {
            public BoxedVector3(Vector3 value) : base(value) { }
        }
        class BoxedQuaternion : Boxed<Quaternion>
        {
            public BoxedQuaternion(Quaternion value) : base(value) { }
        }
        class BoxedBool : Boxed<bool>
        {
            public BoxedBool(bool value) : base(value) { }
        }
    }
}