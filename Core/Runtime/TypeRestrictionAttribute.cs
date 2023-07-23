using System;
using UnityEngine;

namespace Common.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TypeRestrictionAttribute : PropertyAttribute
    {
        public readonly Type RequiredBaseType;

        public TypeRestrictionAttribute(Type requiredBaseType)
        {
            RequiredBaseType = requiredBaseType;
        }
    }
}