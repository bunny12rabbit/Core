using System;
using Common.Core.StateMachine.States;

namespace Common.Core.StateMachine
{
    public class StateMachineBase : IDisposable
    {
        private State _currentState;

        public void Initialize(State startingState) => ChangeState(startingState);

        public void Dispose()
        {
            _currentState?.Dispose();
            _currentState = State.Empty;
        }

        public void UpdateState() => _currentState?.OnUpdate();

        public void ChangeState(State newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            OnStateChanged(newState);
            newState.Enter();
        }

        protected virtual void OnStateChanged(State newState)
        {
        }
    }
}