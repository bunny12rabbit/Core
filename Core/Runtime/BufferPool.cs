using System;
using System.Diagnostics;
using Common.Core.Logs;

namespace Common.Core
{
    /// <summary>
    /// Collection of different sized buffers. Creates multiple groups of buffers covering the range of allowed sizes
    /// and splits range exponentially (using math.log) so that there are more groups for a small buffers.
    /// </summary>
    /// <typeparam name="T">Type of an array</typeparam>
    public class BufferPool<T>
    {
        private readonly BufferBucket<T>[] _buckets;

        private readonly int _bucketsCount;
        private readonly int _smallest;
        private readonly int _largest;

        public BufferPool(int bucketsCount, int smallest, int largest, int multipleOf = 1)
        {
            if (bucketsCount < 2)
            {
                Log.Error($"{nameof(bucketsCount)} must be at least 2");
                return;
            }

            if (smallest < 1)
            {
                Log.Error($"{nameof(smallest)} must be at least 1");
                return;
            }

            if (largest < smallest)
            {
                Log.Error($"{nameof(largest)} must be greater than {nameof(smallest)}");
                return;
            }

            if (multipleOf < 1)
            {
                Log.Error($"{nameof(multipleOf)} must be greater than 0");
                return;
            }

            _bucketsCount = bucketsCount;
            _smallest = smallest;
            _largest = largest;

            // Split range over Log scale (more buckets for smaller sizes)

            var minLog = Math.Log(_smallest);
            var maxLog = Math.Log(_largest);

            var range = maxLog - minLog;
            var each = range / (bucketsCount - 1);

            _buckets = new BufferBucket<T>[bucketsCount];

            for (var i = 0; i < bucketsCount; i++)
            {
                var size = smallest * Math.Pow(Math.E, each * i);
                var sizeMultipleOf = (int) Math.Ceiling(size / multipleOf) * multipleOf;
                _buckets[i] = new BufferBucket<T>(sizeMultipleOf);
            }

            Validate();

            // Example
            // 5         count
            // 20        smallest
            // 16400     largest

            // 3.0       log 20
            // 9.7       log 16400

            // 6.7       range 9.7 - 3
            // 1.675     each  6.7 / (5-1)

            // 20        e^ (3 + 1.675 * 0)
            // 107       e^ (3 + 1.675 * 1)
            // 572       e^ (3 + 1.675 * 2)
            // 3056      e^ (3 + 1.675 * 3)
            // 16,317    e^ (3 + 1.675 * 4)

            // precision wont be lose when using doubles
        }

        [Conditional("UNITY_ASSERTIONS")]
        private void Validate()
        {
            if (_buckets[0].ArraySize < _smallest)
                Log.Error($"Failed to create bucket for smallest. Bucket:{_buckets[0].ArraySize} smallest {_smallest}");

            var largestBucket = _buckets[_bucketsCount - 1].ArraySize;

            if (largestBucket < _largest)
                Log.Error($"Failed to create bucket for largest. Bucket:{largestBucket} largest {_largest}");
        }

        public ArrayBuffer<T> Take(int size)
        {
            if (size > _largest)
                return GetEmptyArrayWithError(size);

            for (var i = 0; i < _bucketsCount; i++)
            {
                if (size <= _buckets[i].ArraySize)
                    return _buckets[i].Take();
            }

            return GetEmptyArrayWithError(size);
        }

        private ArrayBuffer<T> GetEmptyArrayWithError(int size)
        {
            Log.Error($"Size ({size}) is greater than largest ({_largest})");
            return ArrayBuffer<T>.Empty;
        }
    }
}