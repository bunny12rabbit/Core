using UnityEngine;

namespace Common.Core
{
    public class NamedArrayAttribute : PropertyAttribute
    {
        public readonly string FieldName;

        public NamedArrayAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}