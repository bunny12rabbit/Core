using System;
using Common.Core.Logs;

namespace Common.Core
{
    public class EnumBuildParameter<T> : BuildParameter<T> where T : struct
    {
        public EnumBuildParameter(string name, Action<T> apply)
            : base(name, Enum.TryParse, apply)
        {
        }
    }

    public class BoolBuildParameter : BuildParameter<bool>
    {
        public BoolBuildParameter(string name, Action<bool> apply)
            : base(name, bool.TryParse, apply)
        {
        }
    }

    public class IntBuildParameter : BuildParameter<int>
    {
        public IntBuildParameter(string name, Action<int> apply)
            : base(name, int.TryParse, apply)
        {
        }
    }

    public class StringBuildParameter : BuildParameter<string>
    {
        public StringBuildParameter(string name, Action<string> apply)
            : base(name, (string str, out string result) => { result = str; return true; }, apply)
        {
        }
    }

    public interface IBuildParameter
    {
        string Name { get; }

        void Parse(string stringValue);
    }

    public abstract class BuildParameter<T> : IBuildParameter
    {
        protected delegate bool TryParse(string str, out T result);

        public string Name { get; }

        private readonly TryParse _tryParse;
        private readonly Action<T> _apply;

        protected BuildParameter(string name, TryParse tryParse, Action<T> apply)
        {
            Name = name;
            _tryParse = tryParse;
            _apply = apply;
        }

        public void Parse(string stringValue)
        {
            if (_tryParse(stringValue, out var value))
                _apply(value);
            else
                Log.Error($"Failed to parse '{typeof(T).Name}' from '{stringValue}'");
        }
    }
}