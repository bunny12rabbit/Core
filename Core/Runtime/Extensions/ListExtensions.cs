using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace Common.Core
{
	public static class ListExtensions
	{
		public static void Resize<T>(this List<T> list, int size, T fillValue = default)
		{
			var currentCount = list.Count;

			if (size < currentCount)
				list.RemoveRange(size, currentCount - size);
			else if (size > currentCount)
				list.AddRange(Enumerable.Repeat(fillValue, size - currentCount));
		}

		/// <summary>
		/// Вставляет элемент в отсортированный массив в нужную позицию.
		/// </summary>
		public static void InsertSorted<T>(this List<T> list, T item)
		{
			var index = list.BinarySearch(item);

			if (index < 0)
				index = ~index;

			list.Insert(index, item);
		}

		/// <summary>
		/// Вставляет элемент в отсортированный массив в нужную позицию.
		/// </summary>
		public static void InsertSorted<T>(this List<T> list, T item, IComparer<T> comparer)
		{
			var index = list.BinarySearch(item, comparer);

			if (index < 0)
				index = ~index;

			list.Insert(index, item);
		}

        /// <summary>
        /// Truncates the list by replacing the item at the specified index with the last item in the list. The list
        /// is shortened by one.
        /// </summary>
        /// <typeparam name="T">Source type of elements</typeparam>
        /// <param name="list">List to perform removal.</param>
        /// <param name="index">The index of the item to delete.</param>
        public static void RemoveAtSwapBack<T>(this List<T> list, int index)
        {
            int lastIndex = list.Count - 1;
            list[index] = list[lastIndex];
            list.RemoveAt(lastIndex);
        }

		public static TValue AddTo<TValue, TListElement>(this TValue value, List<TListElement> list)
			where TValue : TListElement
		{
			list.Add(value);
			return value;
		}

		public static T GetRandomElement<T>(this IReadOnlyList<T> list)
		{
			if (list.Count == 0)
				throw new ArgumentException($"List must be non empty");

			return list[Random.Range(0, list.Count)];
		}

        public static void DisposeAndClear<T>(this IList<T> list)
            where T : IDisposable
        {
            foreach (var item in list)
                item.Dispose();

            list.Clear();
        }
	}
}