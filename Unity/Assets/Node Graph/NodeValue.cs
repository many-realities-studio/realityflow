using System;
using System.Diagnostics;
using NaughtyAttributes;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class NodeValue : ISerializationCallbackReceiver
    {
        public NodeValueType Type;

        [ShowIf("Type", NodeValueType.Int)]
        [AllowNesting]
        public int IntValue;
        [ShowIf("Type", NodeValueType.Float)]
        [AllowNesting]
        public float FloatValue;
        [ShowIf("Type", NodeValueType.Vector2)]
        [AllowNesting]
        public Vector2 Vector2Value;
        [ShowIf("Type", NodeValueType.Vector3)]
        [AllowNesting]
        public Vector3 Vector3Value;
        [ShowIf("Type", NodeValueType.Quaternion)]
        [AllowNesting]
        public Quaternion QuaternionValue;
        [ShowIf("Type", NodeValueType.Graph)]
        [AllowNesting]
        [SerializeReference]
        public Graph GraphValue;
        [ShowIf("Type", NodeValueType.Bool)]
        [AllowNesting]
        public bool BoolValue;

        public bool TryGetValue<T>(out T value)
        {
            if (IntValue is T intValue)
                value = intValue;
            else if (FloatValue is T floatValue)
                value = floatValue;
            else if (Vector2Value is T vector2Value)
                value = vector2Value;
            else if (Vector3Value is T vector3Value)
                value = vector3Value;
            else if (QuaternionValue is T quaternionValue)
                value = quaternionValue;
            else if (GraphValue is T graphValue)
                value = graphValue;
            else if (BoolValue is T boolValue)
                value = boolValue;
            else
            {
                value = default;
                return false;
            }

            return true;
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
            NodeValueType.Quaternion => new() { 
                Type = type, 
                QuaternionValue = Quaternion.identity 
            },
            NodeValueType.Graph => new() { Type = type, GraphValue = new() },
            _ => throw new ArgumentException(),
        };

        public static NodeValue From<T>(T value) => value switch
        {
            int val => new() { Type = NodeValueType.Int, IntValue = val },
            float val => new() { Type = NodeValueType.Float, FloatValue = val },
            Vector2 val => new() { Type = NodeValueType.Vector2, Vector2Value = val },
            Vector3 val => new() { Type = NodeValueType.Vector3, Vector3Value = val },
            Quaternion val => new() { Type = NodeValueType.Quaternion, QuaternionValue = val },
            Graph val => new() { Type = NodeValueType.Graph, GraphValue = val },
            bool val => new() { Type = NodeValueType.Bool, BoolValue = val },
            _ => throw new ArgumentException(),
        };

        public void OnBeforeSerialize()
        {
            if (Type != NodeValueType.Graph)
                GraphValue = null;
        }

        public void OnAfterDeserialize()
        {
            if (Type == NodeValueType.Graph && GraphValue is null)
                GraphValue = new();
        }
    }
}