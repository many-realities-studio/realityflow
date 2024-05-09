using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace RealityFlow.NodeGraph
{
    [CustomPropertyDrawer(typeof(NodeValueType))]
    public class NodeValueTypeEditor : PropertyDrawer
    {
        static readonly Type[] ValueTypes;
        static readonly bool[] NonEmptyValueTypes;
        static readonly Dictionary<Type, int> ValueTypeIndices;
        static readonly string[] ValueTypeNames;

        static NodeValueTypeEditor()
        {
            ValueTypes =
                typeof(NodeValueType.Kind)
                .GetNestedTypes();

            NonEmptyValueTypes = ValueTypes
                .Select(type => type.GetFields().Length != 0)
                .ToArray();

            ValueTypeIndices =
                new Dictionary<Type, int>(
                    ValueTypes
                    .Select((t, i) => KeyValuePair.Create(t, i))
                );

            ValueTypeNames = ValueTypes.Select(type => type.Name).ToArray();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty kindProperty = 
                property.FindPropertyRelative(nameof(NodeValueType.Data));
            kindProperty.managedReferenceValue ??= new NodeValueType.Kind.Int();
            int kindIndex = 
                ValueTypeIndices[kindProperty.managedReferenceValue.GetType()];
            float childHeight = 
                NonEmptyValueTypes[kindIndex] ? EditorGUI.GetPropertyHeight(kindProperty) : 0;
            return 18 + childHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect labelRect = new(position.x, position.y, 50, 18);
            EditorGUI.PrefixLabel(labelRect, label);

            SerializedProperty kindProperty =
                property.FindPropertyRelative(nameof(NodeValueType.Data));

            kindProperty.managedReferenceValue ??= new NodeValueType.Kind.Int();

            int currentIndex = ValueTypeIndices[kindProperty.managedReferenceValue?.GetType()];

            Rect dropdownRect = new(
                position.x + labelRect.width,
                position.y,
                position.width - labelRect.width,
                labelRect.height
            );
            int newIndex = EditorGUI.Popup(dropdownRect, currentIndex, ValueTypeNames);

            if (newIndex != currentIndex)
            {
                kindProperty.managedReferenceValue =
                    Activator.CreateInstance(ValueTypes[newIndex]);
            }

            if (NonEmptyValueTypes[newIndex])
            {
                Rect kindRect =
                    new(
                        position.x,
                        labelRect.height,
                        position.width,
                        position.height - labelRect.height
                    );
                EditorGUI.PropertyField(kindRect, kindProperty, true);
            }

            EditorGUI.EndProperty();
        }
    }
}