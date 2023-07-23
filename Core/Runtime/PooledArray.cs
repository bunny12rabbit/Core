using System;

namespace Common.Core
{
    public readonly struct PooledArray<T> : IDisposable
    {
        private readonly T[] _toReturn;
        private readonly bool _clear;

        internal PooledArray(T[] toReturn, bool clear)
        {
            _toReturn = toReturn;
            _clear = clear;
        }

        void IDisposable.Dispose() => ArrayPool<T>.Release(_toReturn, _clear);
    }
}