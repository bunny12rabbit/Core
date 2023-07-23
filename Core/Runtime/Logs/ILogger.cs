using System.Diagnostics;
using UnityEngine;

namespace Common.Core.Logs
{
    public interface ILogger
    {
        [DebuggerHidden]
        void PrettyLog(LogType type, string message, Object context = null, string tag = "");
    }
}