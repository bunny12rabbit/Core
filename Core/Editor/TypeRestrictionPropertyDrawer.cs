using Common.Core;
using UnityEditor;
using UnityEngine;

namespace Common.Core.Editor
{
    [CustomPropertyDrawer(typeof(TypeRestrictionAttribute))]
    public class TypeRestrictionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label);

            if (property.objectReferenceValue == null)
                return;

            var requiredBaseType = ((TypeRestrictionAttribute) attribute).RequiredBaseType;
            if (requiredBaseType.IsInstanceOfType(property.objectReferenceValue))
                return;

            if (property.objectReferenceValue is GameObject gameObject && gameObject.GetComponent(requiredBaseType) != null)
                return;

            property.objectReferenceValue = null;
        }
    }
}