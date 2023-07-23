using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Core.Logs;
using UnityEditor;

namespace Common.Core.Editor
{
    public static class UnityEditorExtensions
    {
        public static IEnumerable<KeyValuePair<TAttribute, SerializedProperty>> GetSerializedPropertiesWithAttribute<TAttribute>(
            this UnityEditor.Editor editor, UnityEngine.Object target) where TAttribute : Attribute
        {
            if (target == null)
            {
                Log.Error("The target object is null. Check for missing scripts.");
                yield break;
            }

            var types = GetSelfAndBaseTypes(target);

            for (var i = types.Count - 1; i >= 0; i--)
            {
                var type = types[i];
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in fields)
                {
                    var attribute = field.GetCustomAttribute<TAttribute>();

                    if (attribute == null)
                        continue;

                    yield return new KeyValuePair<TAttribute, SerializedProperty>(attribute, editor.serializedObject.FindProperty(field.Name));
                }

                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<TAttribute>();

                    if (attribute == null)
                        continue;

                    yield return new KeyValuePair<TAttribute, SerializedProperty>(attribute,
                        editor.serializedObject.FindProperty(property.Name));
                }
            }
        }

        /// <summary>
        ///		Get type and all base types of target, sorted as following:
        ///		[target's type, base type, base's base type, ...]
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static List<Type> GetSelfAndBaseTypes(object target)
        {
            List<Type> types = new List<Type>()
            {
                target.GetType()
            };

            while (types.Last().BaseType != null)
                types.Add(types.Last().BaseType);

            return types;
        }
    }
}