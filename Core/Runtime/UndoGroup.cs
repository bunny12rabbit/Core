using System;

namespace Common.Core
{

    public readonly struct UndoGroup : IDisposable
    {
        private readonly string _name;
        public readonly int Index;

        public UndoGroup(string name)
        {
#if UNITY_EDITOR
                UnityEditor.Undo.IncrementCurrentGroup();
                Index = UnityEditor.Undo.GetCurrentGroup();
#else
            Index = 0;
#endif

            _name = name;
        }

        void IDisposable.Dispose()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.SetCurrentGroupName(_name);
            UnityEditor.Undo.CollapseUndoOperations(Index);
#endif
        }
    }
}