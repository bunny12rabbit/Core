using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Common.Core.Logs
{
    public class UnityLogger : ILogger
    {
        private const int MaxMessageLength = 4000;

        private static readonly Color s_tagColor = new Color(0.38f, 0.78f, 0.95f);
        private static string TimeStamp => $"[{DateTime.Now.TimeOfDay} : {Time.frameCount,4}]";

        private readonly UnityEngine.ILogger _internalLogger;

        public UnityLogger()
        {
            _internalLogger = ReducedCallStackLogHandler.IsEnabled
                ? new Logger(new ReducedCallStackLogHandler())
                : Debug.unityLogger;
        }

        [DebuggerHidden]
        public void PrettyLog(LogType logType, string message, Object context = null, string filePath = "")
        {
            var tag = Path.GetFileNameWithoutExtension(filePath);
            var coloredTag = $"[{tag}]".Colorize(s_tagColor);
            var formattedMessage = $"{TimeStamp} {coloredTag} {message}";

            if (formattedMessage.Length > MaxMessageLength)
            {
                // Консоль юнити обрезает большие сообщения, поэтому вручную разбиваем их на несколько мелких.

                for (var i = 0; i < formattedMessage.Length; i += MaxMessageLength)
                {
                    var s = formattedMessage.Substring(i, Mathf.Min(MaxMessageLength, formattedMessage.Length - i - 1));
                    RawLog(logType, s, context);
                }
            }
            else
            {
                RawLog(logType, formattedMessage, context);
            }
        }

        [DebuggerHidden]
        public void RawLog(LogType logType, object message, Object context = null)
        {
            _internalLogger.Log(logType, message, context);
        }
    }
}