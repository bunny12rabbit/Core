using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Common.Core
{
    public interface IInitializableMonoBehaviour<TInputParams> : IInitializable<TInputParams>
    {
    }

    public abstract class InitializableMonoBehaviour<TInputParams> : MonoBehaviour, IInitializableMonoBehaviour<TInputParams>
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private readonly ReactiveProperty<bool> _isInitialized = new();
        public IReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized;

        private Action _onDispose;
        public ICollection<IDisposable> Disposables => _disposables;

        protected TInputParams InputParams { get; private set; }

        protected virtual void OnDestroy()
        {
            DisposeUtils.EnsureIsDisposed(this, gameObject);
        }

        Action ICustomDisposable.OnDispose
        {
            get => _onDispose;
            set => _onDispose = value;
        }

        protected virtual void Awake()
        {
            if (!_isInitialized.Value)
                enabled = false; // Чтобы не добавлять в каждом наследнике проверки на инициализацию в Update/FixedUpdate.
        }

        public virtual void Dispose()
        {
            _disposables.Clear();
            InputParams = default;
            enabled = false;
            _isInitialized.Value = false;

            DisposeUtils.InvokeAndSetNull(ref _onDispose);
        }

        public IDisposable Init(TInputParams inputParams)
        {
            InputParams = inputParams;
            enabled = true;
            Init();
            _isInitialized.Value = true;

            return this;
        }

        protected virtual void Init()
        {
        }
    }
}