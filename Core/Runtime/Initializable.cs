using System;
using System.Collections.Generic;
using UniRx;

namespace Common.Core
{
    public abstract class Initializable<TInputParams> : IInitializable<TInputParams>
    {
        private readonly CompositeDisposable _disposables = new();

        public bool IsInitialized { get; private set; }

        private Action _onDispose;
        Action ICustomDisposable.OnDispose
        {
            get => _onDispose;
            set => _onDispose = value;
        }

        public ICollection<IDisposable> Disposables => _disposables;
        protected TInputParams InputParams { get; private set; }

        public virtual void Dispose()
        {
            _disposables.Clear();
            InputParams = default;
            IsInitialized = false;

            DisposeUtils.InvokeAndSetNull(ref _onDispose);
        }

        public IDisposable Init(TInputParams inputParams)
        {
            InputParams = inputParams;
            IsInitialized = true;
            Init();

            return this;
        }

        protected virtual void Init()
        {
        }
    }
}