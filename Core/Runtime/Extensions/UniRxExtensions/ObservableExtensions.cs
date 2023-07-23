using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace UniRx
{
    public static partial class Observable
    {
        public static IObservable<long> TimerSeconds(double seconds)
        {
            return Timer(TimeSpan.FromSeconds(seconds));
        }

        public static IObservable<long> IntervalSeconds(double seconds)
        {
            return Interval(TimeSpan.FromSeconds(seconds));
        }

        public static void DisposeItemsAndClear<T>(this ReactiveCollection<T> reactiveCollection)
            where T : IDisposable
        {
            foreach (var disposable in reactiveCollection)
                disposable.Dispose();

            reactiveCollection.Clear();
        }

        /// <summary>
        /// Подписка срабатывает в конце кадра, если в словаре произошли какие-то изменения.
        /// </summary>
        public static IObservable<Unit> ObserveAnyChangeBatched<TKey, TValue>(this IReadOnlyReactiveDictionary<TKey, TValue> reactiveDictionary)
        {
            return BatchAtTheEndOfFrame(reactiveDictionary.ObserveReset(),
                reactiveDictionary.ObserveAdd().AsUnitObservable(),
                reactiveDictionary.ObserveRemove().AsUnitObservable(),
                reactiveDictionary.ObserveReplace().AsUnitObservable());
        }

        /// <summary>
        /// Подписка срабатывает в конце кадра, если в коллекции произошли какие-то изменения.
        /// </summary>
        public static IObservable<Unit> ObserveAnyChangeBatched<T>(this IReadOnlyReactiveCollection<T> reactiveCollection)
        {
            return BatchAtTheEndOfFrame(reactiveCollection.ObserveReset(),
                reactiveCollection.ObserveAdd().AsUnitObservable(),
                reactiveCollection.ObserveRemove().AsUnitObservable(),
                reactiveCollection.ObserveReplace().AsUnitObservable());
        }

        public static IObservable<T> BatchAtTheEndOfFrame<T>(params IObservable<T>[] other)
        {
            return Merge(other).SampleFrame(0, FrameCountType.EndOfFrame);
        }

        private static IObservable<T> BatchAtTheEndOfFrame<T>(this IObservable<T> source, params IObservable<T>[] other)
        {
            return source.Merge(other).SampleFrame(0, FrameCountType.EndOfFrame);
        }

        public static IObservable<int> Threshold(this IObservable<int> source, int threshold)
        {
            return source
                .DistinctUntilChanged()
                .Pairwise()
                .Where(pair => Mathf.Abs(pair.Current - pair.Previous) >= threshold)
                .Select(pair => pair.Current);
        }

        public static IObservable<float> Threshold(this IObservable<float> source, float threshold)
        {
            return source
                .DistinctUntilChanged()
                .Pairwise()
                .Where(pair => Mathf.Abs(pair.Current - pair.Previous) >= threshold)
                .Select(pair => pair.Current);
        }
    }
}