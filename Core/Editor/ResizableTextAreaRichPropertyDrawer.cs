using Common.Core;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace Common.Core.Editor
{
    [CustomPropertyDrawer(typeof(ResizableTextAreaRichAttribute))]
    public class ResizableTextAreaRichPropertyDrawer : PropertyDrawerBase
    {
        protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                var labelHeight = EditorGUIUtility.singleLineHeight;
                var textAreaHeight = EditorGUILayoutUtils.GetTextAreaHeight(property.stringValue);
                return labelHeight + textAreaHeight;
            }
            else
            {
                return GetPropertyHeight(property) + GetHelpBoxHeight();
            }
        }

        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            if (property.propertyType == SerializedPropertyType.String)
            {
                var labelRect = new Rect {x = rect.x, y = rect.y, width = rect.width, height = EditorGUIUtility.singleLineHeight};

                EditorGUI.LabelField(labelRect, label.text);

                EditorGUI.BeginChangeCheck();

                var textAreaRect = new Rect
                {
                    x = labelRect.x,
                    y = labelRect.y + EditorGUIUtility.singleLineHeight,
                    width = labelRect.width,
                    height = EditorGUILayoutUtils.GetTextAreaHeight(property.stringValue)
                };

                var resizableTextAreaRichAttribute = attribute as ResizableTextAreaRichAttribute;
                var isReadOnly = resizableTextAreaRichAttribute.IsReadOnly;
                var textAreaValue = property.stringValue;

                if (isReadOnly)
                    EditorGUILayoutUtils.ReadOnlyTextArea(textAreaRect, property.stringValue, true);
                else
                    textAreaValue = EditorGUILayoutUtils.TextArea(textAreaRect, property.stringValue, true);

                if (EditorGUI.EndChangeCheck())
                    property.stringValue = textAreaValue;
            }
            else
            {
                var message = $"{nameof(ResizableTextAreaRichAttribute)} can only be used with string fields";
                DrawDefaultPropertyAndHelpBox(rect, property, message, MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }


    }
}