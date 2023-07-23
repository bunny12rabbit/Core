using System;
using System.Collections.Generic;
using Common.Core;
using Common.Core.Logs;
using UniRx;

namespace Common.Core.StateMachine.States
{
    public abstract class State : IDisposable
    {
        public static State Empty = new Empty(null);

        protected readonly StateMachineBase stateMachine;

        protected readonly IReadOnlyDictionary<string, State> availableStates;

        private readonly ReactiveCommand _stateCompleted = new();
        public IObservable<Unit> StateCompleted => _stateCompleted;

        private readonly CompositeDisposable _disposables = new();
        protected ICollection<IDisposable> Disposables => _disposables;

        public abstract string Name { get; }

        protected State(StateMachineBase stateMachine, IReadOnlyDictionary<string, State> availableStates)
        {
            this.stateMachine = stateMachine;
            this.availableStates = availableStates;
        }

        public void Dispose()
        {
            _disposables.Clear();
            OnDispose();
        }

        public virtual void Enter()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void Exit()
        {
            _stateCompleted.Execute();
            Dispose();
        }

        protected virtual void OnDispose()
        {
        }

        protected State TryGetState(string stateName)
        {
            if (availableStates.IsNullOrEmpty() || !availableStates.TryGetValue(stateName, out var state))
            {
                Log.Warning($"Threre's no state {stateName} in {nameof(availableStates)}");
                return Empty;
            }

            return state;
        }
    }
}