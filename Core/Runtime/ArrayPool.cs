using System;
using System.Collections.Generic;

namespace Common.Core
{
    public static class ArrayPool<T>
    {
        private const int InitialCapacity = 10;
        private const int MaxCapacity = 1000;

        private static readonly Stack<T[]> s_stack = new(InitialCapacity);
        public static T[] Get(int capacity) => s_stack.Count == 0 ? Create(capacity) : s_stack.Pop();

        public static PooledArray<T> Get(int capacity, out T[] pooledInstance, bool clear = false) => new(pooledInstance = Get(capacity), clear);

        public static void Release(T[] pooledInstance, bool clear = false)
        {
            if (s_stack.Count > MaxCapacity || (s_stack.Count > 0 && s_stack.Contains(pooledInstance)))
                return;

            if (clear)
                Array.Clear(pooledInstance, 0, pooledInstance.Length);

            s_stack.Push(pooledInstance);
        }

        public static void Clear() => s_stack.Clear();

        private static T[] Create(int capacity) => new T[capacity];
    }
}