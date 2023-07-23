namespace Common.Core.StateMachine.States
{
    public class Empty : State
    {
        public Empty(StateMachineBase stateMachine) : base(stateMachine, Core.Empty.Dictionary<string, State>())
        {
        }

        public override string Name => nameof(Empty);
    }
}