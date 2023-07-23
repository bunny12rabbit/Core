using System;

namespace Common.Core
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ShowDebugValueAttribute : Attribute
    {
        public readonly bool IsDebugToggle;

        public ShowDebugValueAttribute(bool isDebugToggle)
        {
            IsDebugToggle = isDebugToggle;
        }

        public ShowDebugValueAttribute()
        {
        }
    }
}