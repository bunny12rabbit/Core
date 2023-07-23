using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Common.Core.Logs;
using UnityEngine;

namespace Common.Core
{
    public sealed class ArrayBuffer<T> : IDisposable, IReadOnlyList<T>
    {
        public static ArrayBuffer<T> Empty => new(Array.Empty<T>());

        private readonly IBufferOwner<T> _owner;

        public readonly T[] array;

        private int _releaseRequired;

        public ArrayBuffer(IBufferOwner<T> owner, int size)
        {
            _owner = owner;
            array = new T[size];
        }

        private ArrayBuffer(T[] array)
        {
            _owner = null;
            this.array = array;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return array[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => array.Length;

        public T this[int index] => array[index];

        /// <summary>
        /// How many times <see cref="Release"/> needs to be called before buffer is returned to pool
        /// This allows the buffer to be used in multiple places at the same time
        /// </summary>
        /// <param name="required">This value is normally 0 by default, but can be changed to require <see cref="Release"/>
        /// to be called multiple times</param>
        public void SetReleaseRequired(int required) => _releaseRequired = required;

        void IDisposable.Dispose() => Release();

        public void Release()
        {
            var newValue = Interlocked.Decrement(ref _releaseRequired);

            if (newValue > 0)
                return;

            _owner?.Return(this);
        }

        public ArraySegment<T> ToSegment(int offset, int count) => new(array, offset, count);
        public ArraySegment<T> ToSegment() => new(array, 0, Count);

        [Conditional("UNITY_ASSERTIONS")]
        internal void Validate(int arraySize)
        {
            if (array.Length == arraySize)
                return;

            Log.Debug($"Buffer that was returned had an array of wrong size: should be {array.Length}, but is {arraySize}", LogType.Error);
        }
    }
}