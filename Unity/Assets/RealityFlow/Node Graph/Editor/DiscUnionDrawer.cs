using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace RealityFlow.NodeGraph.Editor
{
    [CustomPropertyDrawer(typeof(DiscUnionAttribute))]
    public class DiscUnionDrawer : PropertyDrawer
    {
        List<Type> concreteSubtypes;

        bool InitSubtypes(Type superType)
        {
            concreteSubtypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .Where(type =>
                !type.IsAbstract
                && !type.IsInterface
                && superType.IsAssignableFrom(type)
            )
            .ToList();
            concreteSubtypes.Sort((lhs, rhs) => lhs.Name.CompareTo(rhs.Name));

            return true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue == null)
                return EditorGUIUtility.singleLineHeight;

            return EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (concreteSubtypes == null)
                if (!InitSubtypes(fieldInfo.FieldType))
                    return;

            if (fieldInfo.CustomAttributes.Where(data => data.AttributeType == typeof(SerializeReference)).Count() == 0)
                Debug.LogError("When using DiscUnion attribute must also use SerializeReference");

            Type currentType = property.managedReferenceValue?.GetType();

            GenericMenu variantMenu = new();
            variantMenu.AddItem(new("null"), false, () =>
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });
            foreach (Type type in concreteSubtypes)
                variantMenu.AddItem(new(type.Name), false, () =>
                {
                    try
                    {
                        property.managedReferenceValue = Activator.CreateInstance(type, true);
                        property.serializedObject.ApplyModifiedProperties();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to construct variant: {e.Message}");
                    }
                });

            Rect dropdownPos = EditorGUI.PrefixLabel(
                new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), 
                label
            );

            if (EditorGUI.DropdownButton(dropdownPos, new(currentType?.Name ?? "null"), FocusType.Keyboard))
                variantMenu.DropDown(position);

            EditorGUI.indentLevel++;

            Rect newPos = EditorGUI.IndentedRect(new(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight,
                position.width,
                position.height - EditorGUIUtility.singleLineHeight
            ));

            if (currentType != null)
                EditorGUI.PropertyField(newPos, property, true);

            EditorGUI.indentLevel--;
        }
    }
}