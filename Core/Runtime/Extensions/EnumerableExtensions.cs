using System;
using System.Collections.Generic;
using System.Linq;
using Common.Core;
using Common.Core.Logs;

namespace Common.Core
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Determines whether the collection is null or contains no elements.
        /// </summary>
        /// <typeparam name="T">The IEnumerable type.</typeparam>
        /// <param name="enumerable">The enumerable, which may be null or empty.</param>
        /// <returns>
        ///     <c>true</c> if the IEnumerable is null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable) => enumerable == null || enumerable.IsEmpty();

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            //If this is a list, use the Count property for efficiency. The Count property is O(1) while IEnumerable.Count() is O(N).
            return enumerable is ICollection<T> collection ? collection.Count < 1 : !enumerable.Any();
        }

        /// <summary>
        /// Computes the discrete cumulative density function (CDF) for <paramref name="collection"/>,
        /// or in simple terms the array of cumulative sums of elements.
        /// </summary>
        /// <param name="collection">Collection of weights to compute CDF for</param>
        /// <returns> Collection of computed CDF weights for <paramref name="collection"/></returns>
        public static IEnumerable<int> ComputeCumulativeDensityFunction(this ICollection<int> collection)
        {
            var result = new int[collection.Count];
            var cumulativeSum = 0;

            for (var i = 0; i < collection.Count; i++)
            {
                cumulativeSum += collection.ElementAt(i);
                result[i] = cumulativeSum;
            }

            return result;
        }

        /// <summary>
        /// Computes the discrete cumulative density function (CDF) for <paramref name="collection"/>,
        /// or in simple terms the array of cumulative sums of elements.
        /// </summary>
        /// <param name="collection">Collection of weights to compute CDF for</param>
        /// <returns> Collection of computed CDF weights for <paramref name="collection"/></returns>
        public static IEnumerable<float> ComputeCumulativeDensityFunction(this ICollection<float> collection)
        {
            var result = new float[collection.Count];

            var cumulativeSum = 0f;

            for (var i = 0; i < collection.Count; i++)
            {
                cumulativeSum += collection.ElementAt(i);
                result[i] = cumulativeSum;
            }

            return result;
        }

        /// <summary>
        /// Finds the index in the <paramref name="collection"/> (unsorted, being sorted inside) equals to <paramref name="value"/>
        /// Use only array with numeric values, such as int, float, double, etc, except decimal
        /// </summary>
        /// <param name="collection">Collection on numerics. Can be unsorted, being sorted inside</param>
        /// <param name="value">Target numeric value</param>
        /// <typeparam name="T">Numeric values, such as int, float, double, etc, except decimal</typeparam>
        /// <returns> The index of element equals to <paramref name="value"/>, or -1 if no exact match found
        /// or if <paramref name="collection"/> is null or empty</returns>
        public static int FindIndexWithBinarySearch<T>(this IEnumerable<T> collection, T value) where T : IComparable<T>
        {
            var orderedArray = collection?.OrderBy(t => t).ToArray();
            var last = orderedArray?.Length - 1 ?? 0;

            if (Log.Assert.IsNotNullOrEmpty(orderedArray))
                return -1;

            if (orderedArray[0].CompareTo(value) >= 0)
                return 0;

            if (orderedArray[last].CompareTo(value) <= 0)
                return last;

            return orderedArray.FindIndexWithBinarySearch(value, 0, last);
        }

        /// <summary>
        /// Finds the index in the <paramref name="orderedArray"/> equals to <paramref name="value"/>
        /// Use only array with numeric values, such as int, float, double, etc, except decimal
        /// </summary>
        /// <param name="orderedArray">Ordered array on numerics</param>
        /// <param name="value">Target numeric value</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <typeparam name="T">Numeric values, such as int, float, double, etc, except decimal</typeparam>
        /// <returns> The index of element equals to <paramref name="value"/>, or -1 if no exact match found
        /// or if <paramref name="orderedArray"/> is null or empty</returns>
        public static int FindIndexWithBinarySearch<T>(this T[] orderedArray, T value, int start, int end)
            where T : IComparable<T>
        {
            var middle = start + (end - start) / 2;

            if (start > end || orderedArray.IsNullOrEmpty())
                return -1;

            var elementAtMiddle = orderedArray[middle];

            return value.CompareTo(elementAtMiddle) switch
            {
                0 => middle,
                < 0 => orderedArray.FindIndexWithBinarySearch(value, start, middle - 1),
                > 0 => orderedArray.FindIndexWithBinarySearch(value, middle + 1, end)
            };
        }

        /// <summary>
        /// Finds the closest index in the <paramref name="collection"/> (unsorted, being sorted inside) to <paramref name="value"/>
        /// Use only array with numeric values, such as int, float, double, etc, except decimal
        /// </summary>
        /// <param name="collection">Collection on numerics. Can be unsorted, being sorted inside</param>
        /// <param name="value">Target numeric value</param>
        /// <typeparam name="T">Numeric values, such as int, float, double, etc, except decimal</typeparam>
        /// <returns> The index of element equals to <paramref name="value"/>, or closest to if no exact match found,
        /// or -1 if <paramref name="collection"/> is null or empty</returns>
        public static int FindClosestIndexWithBinarySearch<T>(this IEnumerable<T> collection, T value) where T : IComparable<T>
        {
            var orderedArray = collection?.OrderBy(t => t).ToArray();
            var last = orderedArray?.Length - 1 ?? 0;

            if (Log.Assert.IsNotNullOrEmpty(orderedArray))
                return -1;

            if (orderedArray[0].CompareTo(value) >= 0)
                return 0;

            if (orderedArray[last].CompareTo(value) <= 0)
                return last;

            return orderedArray.FindClosestIndexWithBinarySearch(value, 0, last);
        }

        /// <summary>
        /// Finds the closest index in the <paramref name="orderedArray"/> to <paramref name="value"/> in range
        /// <paramref name="start"/> - <paramref name="end"/>, using binary search, recursively calling self.
        /// Use only array with numeric values, such as int, float, double, etc, except decimal
        /// </summary>
        /// <param name="orderedArray">Ordered array on numerics</param>
        /// <param name="value">Target numeric value</param>
        /// <param name="start">Start range</param>
        /// <param name="end">End range</param>
        /// <typeparam name="T">Numeric values, such as int, float, double, etc, except decimal</typeparam>
        /// <returns> The index of element equals to <paramref name="value"/>, or closest to if no exact match found
        ///  or -1 if <paramref name="orderedArray"/> is null or empty</returns>
        public static int FindClosestIndexWithBinarySearch<T>(this T[] orderedArray, T value, int start, int end)
            where T : IComparable<T>
        {
            if (start > end || orderedArray.IsNullOrEmpty())
                return -1;

            var middle = start + (end - start) / 2;

            if (orderedArray[middle].CompareTo(value) == 0)
                return middle;

            if (orderedArray[middle].CompareTo(value) < 0)
            {
                return middle < orderedArray.Length - 1 && orderedArray[middle + 1].CompareTo(value) > 0
                    ? GetClosestIndexInRange(orderedArray, middle, middle + 1, value)
                    : orderedArray.FindClosestIndexWithBinarySearch(value, middle + 1, end);
            }

            if (middle > 0 && orderedArray[middle - 1].CompareTo(value) < 0)
                return GetClosestIndexInRange(orderedArray, middle - 1, middle, value);

            return orderedArray.FindClosestIndexWithBinarySearch(value, start, middle - 1);
        }

        /// <summary>
        /// Returns the closest of <paramref name="leftIndex"/> or <paramref name="rightIndex"/> to <paramref name="value"/>.
        /// Use only array with numeric values, such as int, float, double, etc, except decimal
        /// </summary>
        /// <param name="orderedArray">Ordered array on numerics</param>
        /// <param name="leftIndex">Left index to choose between</param>
        /// <param name="rightIndex">Right index to choose between</param>
        /// <param name="value">Target numeric value</param>
        /// <typeparam name="T">Numeric values, such as int, float, double, etc, except decimal</typeparam>
        /// <returns> The closest of <paramref name="leftIndex"/> or <paramref name="rightIndex"/> to <paramref name="value"/></returns>
        private static int GetClosestIndexInRange<T>(IReadOnlyList<T> orderedArray, int leftIndex, int rightIndex, T value)
            where T : IComparable<T>
        {
            var left = orderedArray[leftIndex];
            var right = orderedArray[rightIndex];
            var closest = GetClosest(left, right, value);

            return closest.CompareTo(left) == 0 ? leftIndex : rightIndex;
        }

        /// <summary>
        /// Returns the closest value of <paramref name="a"/> or <paramref name="b"/> to <paramref name="value"/>. Use only numeric values,
        /// such as int, float, double, etc, except decimal
        /// </summary>
        /// <param name="a">Numeric value</param>
        /// <param name="b">Numeric value</param>
        /// <param name="value">Target numeric value</param>
        /// <typeparam name="T">Numeric values, such as int, float, double, etc, except decimal
        /// </typeparam>
        /// <returns>Returns the closest value of <paramref name="a"/> or <paramref name="b"/> to <paramref name="value"/></returns>
        private static T GetClosest<T>(T a, T b, T value) where T : IComparable<T>
        {
            dynamic val1 = Convert.ToDouble(a);
            dynamic val2 = Convert.ToDouble(b);
            dynamic target = Convert.ToDouble(value);

            return target - val1 > val2 - target ? (T) val2 : (T) val1;
        }

        /// <summary>
        /// Shuffles elements in <paramref name="collection"/> using
        /// <a href="http://en.wikipedia.org/wiki/Fisher-Yates_shuffle">Fisher-Yates shuffle</a>
        /// </summary>
        /// <param name="collection">Array or List, or anything that implements indexers</param>
        /// <typeparam name="T">Type</typeparam>
        public static void Shuffle<T>(this IList<T> collection)
        {
            var n = collection.Count;

            if (n <= 2)
                return;

            while (n > 1)
            {
                n--;
                var k = UnityEngine.Random.Range(0, n + 1);
                Swap(collection, k, n);
            }
        }

        public static void Swap<T>(this IList<T> collection, int leftIndex, int rightIndex) =>
            (collection[leftIndex], collection[rightIndex]) = (collection[rightIndex], collection[leftIndex]);

        /// <summary>
        /// Splits the original collection into a number of chunks with size of <paramref name="chunkSize"/>
        /// </summary>
        /// <param name="source">Original collection</param>
        /// <param name="chunkSize">Elements in chunk</param>
        /// <typeparam name="T">Type of element</typeparam>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
        {
            return chunkSize > 0
                ? source?.Select((x, i) => new {Index = i, Value = x})
                    .GroupBy(x => x.Index / chunkSize)
                    .Select(x => x.Select(v => v.Value))
                : Enumerable.Repeat(source, 1);
        }
    }
}