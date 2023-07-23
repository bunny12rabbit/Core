using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace Common.Core
{
    public static class SceneExtensions
    {
        public static T GetComponentOnAnyRoot<T>(this Scene scene)
            where T : Component
        {
            using (ListPool<GameObject>.Get(out var rootGameObjects))
            {
                scene.GetRootGameObjects(rootGameObjects);

                foreach (var rootGameObject in rootGameObjects)
                {
                    if (rootGameObject.TryGetComponent<T>(out var component))
                        return component;
                }
            }

            return null;
        }

        public static T GetComponentInChildren<T>(this Scene scene)
            where T : Component
        {
            using (ListPool<GameObject>.Get(out var rootGameObjects))
            {
                scene.GetRootGameObjects(rootGameObjects);

                foreach (var rootGameObject in rootGameObjects)
                {
                    var component = rootGameObject.GetComponentInChildren<T>();

                    if (component != null)
                        return component;
                }
            }

            return null;
        }
    }
}