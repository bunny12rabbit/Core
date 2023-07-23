using System;

namespace Common.Core
{
    public interface IInitializable<in TInputParams> : ICustomDisposable
    {
        IDisposable Init(TInputParams inputParams);
    }
}