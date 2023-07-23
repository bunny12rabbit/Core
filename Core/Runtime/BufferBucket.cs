using System.Collections.Concurrent;
using System.Threading;

namespace Common.Core
{
    internal class BufferBucket<T> : IBufferOwner<T>
    {
        public readonly int ArraySize;

        private readonly ConcurrentQueue<ArrayBuffer<T>> _buffers;

        private int _current;

        public BufferBucket(int arraySize)
        {
            ArraySize = arraySize;
            _buffers = new ConcurrentQueue<ArrayBuffer<T>>();
        }

        public ArrayBuffer<T> Take()
        {
            IncrementCreated();
            return _buffers.TryDequeue(out var buffer) ? buffer : new ArrayBuffer<T>(this, ArraySize);
        }

        public void Return(ArrayBuffer<T> buffer)
        {
            DecrementCreated();
            buffer.Validate(ArraySize);
            _buffers.Enqueue(buffer);
        }

        private void IncrementCreated() => Interlocked.Increment(ref _current);
        private void DecrementCreated() => Interlocked.Decrement(ref _current);
    }
}