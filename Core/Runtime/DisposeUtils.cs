using System;
using Common.Core.Logs;
using UnityEngine;

namespace Common.Core
{
    public static class DisposeUtils
    {
        public static void DisposeAndSetNull<T>(ref T disposable)
            where T : IDisposable
        {
            disposable?.Dispose();
            disposable = default;
        }

        public static void EnsureIsDisposed(ICustomDisposable customDisposable, GameObject gameObject)
        {
#if UNITY_EDITOR
            // При выходе из Play Mode порядок выгрузки сцен не определён, поэтому проверка ниже не имеет смысла.
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;
#endif

            if (customDisposable.OnDispose != null)
                Log.Error($"<b>{gameObject.name}</b> was not disposed before destruction.", gameObject);
        }

        public static void InvokeAndSetNull(ref Action onDispose)
        {
            var onDisposeCopy = onDispose;
            onDispose = null;
            onDisposeCopy?.Invoke();
        }
    }
}