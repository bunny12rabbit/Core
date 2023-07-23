using System.Collections.Generic;
using JetBrains.Annotations;

namespace Common.Core
{
    public static class Empty
    {
        public static T[] Array<T>()
        {
            return EmptyArray<T>.Instance;
        }

        public static IReadOnlyList<T> List<T>()
        {
            return EmptyList<T>.Instance;
        }

        public static IReadOnlyCollection<T> HashSet<T>()
        {
            return EmptyHashSet<T>.Instance;
        }

        public static IReadOnlyDictionary<TKey, TValue> Dictionary<TKey, TValue>()
        {
            return EmptyDictionary<TKey, TValue>.Instance;
        }

        [UsedImplicitly]
        private class EmptyList<TElement>
        {
            public static readonly List<TElement> Instance = new List<TElement>(0);
        }

        [UsedImplicitly]
        private class EmptyArray<TElement>
        {
            public static readonly TElement[] Instance = System.Array.Empty<TElement>();
        }

        [UsedImplicitly]
        private class EmptyHashSet<TElement>
        {
            public static readonly HashSet<TElement> Instance = new HashSet<TElement>();
        }

        [UsedImplicitly]
        private class EmptyDictionary<TKey, TElement>
        {
            public static readonly Dictionary<TKey, TElement> Instance = new Dictionary<TKey, TElement>();
        }
    }
}