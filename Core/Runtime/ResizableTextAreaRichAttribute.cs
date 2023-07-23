using System;
using UnityEngine;

namespace Common.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ResizableTextAreaRichAttribute : PropertyAttribute
    {
        public readonly bool IsReadOnly;

        public ResizableTextAreaRichAttribute(bool isReadOnly)
        {
            IsReadOnly = isReadOnly;
        }
    }
}