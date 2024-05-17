using System;
using NaughtyAttributes;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class NodeValue : ISerializationCallbackReceiver
    {
        public NodeValueType Type;
        
        [SerializeField]
        [SerializeReference]
        object value;

        public object Value => value;

        public bool TryGetValue<T>(out T value)
        {
            if (this.value is T tValue)
            {
                value = tValue;
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
            NodeValueType.Int
            or NodeValueType.Float
            or NodeValueType.Vector2
            or NodeValueType.Vector3
            or NodeValueType.Bool
            => new() { Type = type },
            NodeValueType.Quaternion => new()
            {
                Type = type,
                value = Quaternion.identity
            },
            NodeValueType.Graph => new() { Type = type, value = new Graph() },
            _ => throw new ArgumentException(),
        };

        public static NodeValue From<T>(T value) => value switch
        {
            int val => new() { Type = NodeValueType.Int, value = val },
            float val => new() { Type = NodeValueType.Float, value = val },
            Vector2 val => new() { Type = NodeValueType.Vector2, value = val },
            Vector3 val => new() { Type = NodeValueType.Vector3, value = val },
            Quaternion val => new() { Type = NodeValueType.Quaternion, value = val },
            Graph val => new() { Type = NodeValueType.Graph, value = val },
            bool val => new() { Type = NodeValueType.Bool, value = val },
            _ => throw new ArgumentException(),
        };
        public static NodeValue From(int value) => new() { Type = NodeValueType.Int, value = value };
        public static NodeValue From(float value) => new() { Type = NodeValueType.Float, value = value };
        public static NodeValue From(Vector2 value) => new() { Type = NodeValueType.Vector2, value = value };
        public static NodeValue From(Vector3 value) => new() { Type = NodeValueType.Vector3, value = value };
        public static NodeValue From(Quaternion value) => new() { Type = NodeValueType.Quaternion, value = value };
        public static NodeValue From(Graph value) => new() { Type = NodeValueType.Graph, value = value };
        public static NodeValue From(bool value) => new() { Type = NodeValueType.Bool, value = value };

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (Type is NodeValueType.Graph && value is null)
                value = new Graph();
        }
    }
}