using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Common.Core
{
    public static class DictionaryExtensions
    {
        public static void RemoveKeysWhichNotExistsIn<TKey, TValue1, TValue2>(this IDictionary<TKey, TValue1> removeKeysFrom,
            IReadOnlyDictionary<TKey, TValue2> whereToSearchKeys, Action<TValue1> onRemoveValue = null)
        {
            using (ListPool<TKey>.Get(out var toRemove))
            {
                foreach (var entry in removeKeysFrom)
                {
                    if (!whereToSearchKeys.ContainsKey(entry.Key))
                    {
                        onRemoveValue?.Invoke(entry.Value);
                        toRemove.Add(entry.Key);
                    }
                }

                foreach (var key in toRemove)
                    removeKeysFrom.Remove(key);
            }
        }

        public static void Swap<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey a, TKey b)
        {
            var temp = dictionary[a];
            dictionary[a] = dictionary[b];
            dictionary[b] = temp;
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            if (!dictionary.TryGetValue(key, out var value))
                dictionary[key] = value = new TValue();

            return value;
        }
    }
}