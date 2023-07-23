using System;
using System.Collections.Generic;
using Common.Core.Logs;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Common.Core
{
    public static class GameObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject, string prefix = "") where T : Component
        {
            if (gameObject == null)
                return null;
            var component = gameObject.GetComponent<T>();
            return component ? component : gameObject.AddComponentWithProfiler<T>(prefix);
        }

        public static T AddComponentWithProfiler<T>(this GameObject go, string prefix = "") where T : Component
        {
            Profiler.BeginSample($"{prefix} AddComponentWithProfiler {typeof(T).Name} to {go.name}");
            var component = go.AddComponent<T>();
            Profiler.EndSample();
            Log.Debug($"AddComponent: {typeof(T).Name} was added to {go.name}");
            return component;
        }

        public static void DestroyAllChildren(this Transform transform)
        {
            transform.ForEachChild(c => Object.Destroy(c.gameObject));
        }

        public static void DestroyAllChildrenImmediate(this Transform transform)
        {
            while (transform.childCount > 0)
                Object.DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject);
        }

        public static List<T> GetComponentsInChildrenNonRecursive<T>(this Component component, bool includeSelf = false)
            where T : Component
        {
            var result = new List<T>();
            component.FillWithComponentsInChildrenNonRecursive(result, includeSelf);
            return result;
        }

        public static void FillWithComponentsInChildrenNonRecursive<T>(this Component component, List<T> childComponents, bool includeSelf = false)
            where T : Component
        {
            childComponents.Clear();

            if (includeSelf)
            {
                if (component.TryGetComponent<T>(out var componentOnSelf))
                    childComponents.Add(componentOnSelf);
            }

            foreach (Transform child in component.transform)
            {
                if (child.TryGetComponent<T>(out var childComponent))
                    childComponents.Add(childComponent);
            }
        }

        public static void ForEachChild(this Transform transform, Action<Transform> visit, bool recursive = true)
        {
            foreach (var child in transform.EnumerateChildren(recursive))
                visit(child);
        }

        public static IEnumerable<Transform> EnumerateChildren(this Transform transform, bool recursive = true)
        {
            for (var i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                yield return child;

                if (recursive)
                {
                    foreach (var c in child.EnumerateChildren(true))
                        yield return c;
                }
            }
        }

        public static string GetPath(this GameObject gameObject) => GetPath(gameObject.transform);

        public static string GetPath(this Transform transform) =>
            transform.parent == null ? transform.name : $"{GetPath(transform.parent)}/{transform.name}";

        public static bool TryGetComponentInParents<T>(this GameObject gameObject, out T component) =>
            TryGetComponentInParents(gameObject, false, out component);

        public static bool TryGetComponentInParents<T>(this GameObject gameObject, bool includeSelf, out T component) =>
            TryGetComponentInParents(gameObject.transform, includeSelf, out component);

        public static bool TryGetComponentInParents<T>(this Transform transform, out T component) =>
            TryGetComponentInParents(transform, false, out component);

        public static bool TryGetComponentInParents<T>(this Transform transform, bool includeSelf, out T component)
        {
            if (includeSelf)
                return transform.TryGetComponent(out component) || TryGetComponentInParents(transform, false, out component);

            var parent = transform.parent;

            if (parent != null)
                return parent.TryGetComponent(out component) || TryGetComponentInParents(parent, true, out component);

            component = default;
            return component != null;
        }
    }
}