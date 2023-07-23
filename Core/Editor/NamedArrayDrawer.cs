using System.Globalization;
using UnityEditor;
using UnityEngine;
using Common.Core;

namespace Common.Editor
{
    [CustomPropertyDrawer(typeof(NamedArrayAttribute))]
    public class NamedArrayDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Properly configure height for expanded contents.
            return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                var config = attribute as NamedArrayAttribute;
                var propertyField = property.FindPropertyRelative(config.FieldName);
                label = new GUIContent(GetPropertyLabel(propertyField));
            }
            catch
            {
                // keep default label
            }

            EditorGUI.PropertyField(position, property, label, property.isExpanded);
        }

        private static string GetPropertyLabel(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString();

                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString();

                case SerializedPropertyType.Float:
                    return property.floatValue.ToString(CultureInfo.InvariantCulture);

                case SerializedPropertyType.String:
                    return property.stringValue;

                case SerializedPropertyType.Color:
                    return property.colorValue.ToString();

                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue.ToString();

                case SerializedPropertyType.Enum:
                    return property.enumDisplayNames[property.enumValueIndex];

                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString();

                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString();

                case SerializedPropertyType.Vector4:
                    return property.vector4Value.ToString();

                case SerializedPropertyType.Rect:
                    return property.rectValue.ToString();

                case SerializedPropertyType.ArraySize:
                    return property.arraySize.ToString();

                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue.ToString();

                case SerializedPropertyType.Bounds:
                    return property.boundsValue.ToString();

                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue.ToString();

                case SerializedPropertyType.ExposedReference:
                    return property.exposedReferenceValue.ToString();

                case SerializedPropertyType.FixedBufferSize:
                    return property.fixedBufferSize.ToString();

                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue.ToString();

                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue.ToString();

                case SerializedPropertyType.RectInt:
                    return property.rectIntValue.ToString();

                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue.ToString();

                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceValue.ToString();

                case SerializedPropertyType.Hash128:
                    return property.hash128Value.ToString();
            }

            return property.displayName;
        }
    }
}