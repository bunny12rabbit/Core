using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Core
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Get sat flags for enum, marked as [Flags]
        /// </summary>
        /// <param name="input">Enum, marked as [Flags]</param>
        /// <typeparam name="T">Enum type</typeparam>
        /// <returns>Enumerable of sat flags</returns>
        public static IEnumerable<T> GetFlags<T>(this T input) where T : Enum
        {
            return Enum.GetValues(input.GetType()).Cast<Enum>()
                .Where(value => !Equals(value, (T) Convert.ChangeType(0, Enum.GetUnderlyingType(typeof(T)))) && input.HasFlag(value)).Cast<T>();
        }
    }
}