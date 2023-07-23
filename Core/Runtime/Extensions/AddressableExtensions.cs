using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Common
{
    public class DisposableOperationHandle<T> : IDisposable
    {
        private AsyncOperationHandle<T> _asyncOperationHandle;
        private Action<T> _onSuccess;

        public DisposableOperationHandle(AsyncOperationHandle<T> asyncOperationHandle, Action<T> onSuccess)
        {
            _asyncOperationHandle = asyncOperationHandle;
            _onSuccess = onSuccess;

            _asyncOperationHandle.Completed += OnCompleted;
        }

        public void Dispose()
        {
            _asyncOperationHandle.Completed -= OnCompleted;
            _onSuccess = null;

            Addressables.Release(_asyncOperationHandle);
        }

        private void OnCompleted(AsyncOperationHandle<T> asyncOperationHandle)
        {
            if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
                _onSuccess.Invoke(asyncOperationHandle.Result);
        }
    }

    public static class AddressableExtensions
    {
        public static IDisposable Subscribe<T>(this AsyncOperationHandle<T> asyncOperationHandle, Action<T> onSuccess)
        {
            return new DisposableOperationHandle<T>(asyncOperationHandle, onSuccess);
        }
    }
}