using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using System;
using System.Text.RegularExpressions;

namespace Common.Core.Editor
{
    public static class EditorGUILayoutUtils
    {
        public static void BeginBoxGroupLayout(string label = "")
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (!string.IsNullOrEmpty(label))
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            }
        }

        public static void EndBoxGroupLayout()
        {
            EditorGUILayout.EndVertical();
        }

        public static void ReadOnlyTextArea(string text, bool useRichText = false)
        {
            var style = new GUIStyle(EditorStyles.textArea);
            style.richText = useRichText;
            style.wordWrap = true;

            var content = new GUIContent(text);
            var position = GUILayoutUtility.GetRect(content, style);
            EditorGUI.SelectableLabel(position, text, style);
        }

        public static void ReadOnlyTextArea(Rect rect, string text, bool useRichText = false)
        {
            var style = new GUIStyle(EditorStyles.textArea);
            style.richText = useRichText;
            style.wordWrap = true;
            EditorGUI.SelectableLabel(rect, text, style);
        }

        public static string TextArea(string text, bool useRichText = false)
        {
            var style = new GUIStyle(EditorStyles.textArea);
            style.richText = useRichText;
            style.wordWrap = true;

            var content = new GUIContent(text);
            var position = GUILayoutUtility.GetRect(content, style);
            return EditorGUI.TextArea(position, text, style);
        }

        public static string TextArea(Rect rect, string text, bool useRichText = false)
        {
            var style = new GUIStyle(EditorStyles.textArea);
            style.richText = useRichText;
            style.wordWrap = true;
            return EditorGUI.TextArea(rect, text, style);
        }

        public static void ReadonlyTextField(string text, bool useRichText = false, params GUILayoutOption[] options)
        {
            var style = new GUIStyle(EditorStyles.textField);
            style.richText = useRichText;
            EditorGUILayout.TextField(text, style, options);
        }

        public static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            using (ListPool<string>.Get(out var layers))
            using (ListPool<int>.Get(out var layerNumbers))
            {
                for (var i = 0; i < 32; i++)
                {
                    var layerName = LayerMask.LayerToName(i);

                    if (layerName == string.Empty)
                        continue;

                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }

                var maskWithoutEmpty = 0;

                for (var i = 0; i < layerNumbers.Count; i++)
                {
                    if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                        maskWithoutEmpty |= 1 << i;
                }

                maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());

                var mask = 0;

                for (var i = 0; i < layerNumbers.Count; i++)
                {
                    if ((maskWithoutEmpty & (1 << i)) > 0)
                        mask |= (1 << layerNumbers[i]);
                }

                layerMask.value = mask;
            }

            return layerMask;
        }

        public  static float GetTextAreaHeight(string text) => (EditorGUIUtility.singleLineHeight - 3f) * GetNumberOfLines(text) + 3f;

        public static int GetNumberOfLines(string text)
        {
            var content = Regex.Replace(text, @"\r\n|\n\r|\r|\n", Environment.NewLine);
            var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            return lines.Length;
        }
    }
}