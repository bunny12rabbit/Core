using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Common
{
    public class ComponentReference<TComponent> : AssetReference
    {
        public ComponentReference(string guid) : base(guid)
        {
        }

        public new AsyncOperationHandle<TComponent> InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return Addressables.ResourceManager.CreateChainOperation(base.InstantiateAsync(position, Quaternion.identity, parent),
                GameObjectReady);
        }

        public new AsyncOperationHandle<TComponent> InstantiateAsync(Transform parent = null, bool instantiateInWorldSpace = false)
        {
            return Addressables.ResourceManager.CreateChainOperation(base.InstantiateAsync(parent, instantiateInWorldSpace),
                GameObjectReady);
        }

        public AsyncOperationHandle<TComponent> LoadAssetAsync()
        {
            return Addressables.ResourceManager.CreateChainOperation(base.LoadAssetAsync<GameObject>(), GameObjectReady);
        }

        private AsyncOperationHandle<TComponent> GameObjectReady(AsyncOperationHandle<GameObject> arg)
        {
            var comp = arg.Result.GetComponent<TComponent>();
            return Addressables.ResourceManager.CreateCompletedOperation(comp, string.Empty);
        }

        public override bool ValidateAsset(Object obj)
        {
            var go = obj as GameObject;
            return go != null && go.GetComponent<TComponent>() != null;
        }

        public override bool ValidateAsset(string path)
        {
#if UNITY_EDITOR
            //this load can be expensive...
            var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return go != null && go.GetComponent<TComponent>() != null;
#else
            return false;
#endif
        }

        public void ReleaseInstance(AsyncOperationHandle<TComponent> op)
        {
            // Release the instance
            var component = op.Result as Component;

            if (component != null)
            {
                Addressables.ReleaseInstance(component.gameObject);
            }

            // Release the handle
            Addressables.Release(op);
        }
    }
}