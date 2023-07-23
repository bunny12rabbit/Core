using System;

namespace Common.Core
{
    public interface ICustomDisposable : IDisposable
    {
        Action OnDispose { get; set; }
    }
}