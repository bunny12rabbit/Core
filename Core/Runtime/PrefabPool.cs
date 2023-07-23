using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Common.Core
{
    public class PrefabPool : MonoBehaviour
    {
        private const int _initialSize = 10;

        private readonly Dictionary<int, Stack<MonoBehaviour>> _pools = new Dictionary<int, Stack<MonoBehaviour>>();

        public TPrefab Get<TPrefab, TInputParams>(TPrefab prefab, TInputParams args, Transform parent = null)
            where TPrefab : MonoBehaviour, IInitializableMonoBehaviour<TInputParams>
        {
            var (obj, pool) = GetOrCreate(prefab);
            obj.OnDispose = () => OnObjectDispose(obj, pool);
            obj.gameObject.SetActive(true);
            obj.transform.SetParent(parent, false);
            obj.Init(args);
            return obj;
        }

        public void Prewarm<TPrefab>(TPrefab prefab, int initialCount)
            where TPrefab : MonoBehaviour
        {
            var pool = GetOrCreatePool(prefab);
            var needToAddCount = math.max(0, initialCount - pool.Count);

            for (var i = 0; i < needToAddCount; ++i)
                pool.Push(InstantiatePrefab(prefab));
        }

        protected (TPrefab prefab, Stack<MonoBehaviour> pool) GetOrCreate<TPrefab>(TPrefab prefab)
            where TPrefab : MonoBehaviour
        {
            Debug.Assert(!prefab.gameObject.scene.IsValid(), $"{prefab.name} is not a prefab, it is instance!", prefab);

            var pool = GetOrCreatePool(prefab);

            var obj = (TPrefab) (pool.Count > 0 ? pool.Pop() : InstantiatePrefab(prefab));
            return (obj, pool);
        }

        private TPrefab InstantiatePrefab<TPrefab>(TPrefab prefab)
            where TPrefab : MonoBehaviour
        {
            var instance = Instantiate(prefab, transform);
            instance.gameObject.SetActive(false);
            return instance;
        }

        private Stack<MonoBehaviour> GetOrCreatePool<TPrefab>(TPrefab prefab)
            where TPrefab : MonoBehaviour
        {
            var id = prefab.GetInstanceID();

            if (!_pools.TryGetValue(id, out var pool))
            {
                pool = new Stack<MonoBehaviour>(_initialSize);
                _pools.Add(id, pool);
            }

            return pool;
        }

        protected void OnObjectDispose(MonoBehaviour obj, Stack<MonoBehaviour> pool)
        {
            pool.Push(obj);
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(transform, false);
        }
    }
}