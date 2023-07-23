using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.Core.Logs
{
    /// <summary>
    /// Вырезает из колстека бесполезные строки, чтобы облегчить читаемость логов.
    /// </summary>
    public class ReducedCallStackLogHandler : ILogHandler
    {
        private readonly Regex _redundantLinesRegex = new Regex(
            "^" + // Начало строки
            "(" + // Любая из следующих подстрок (подстроки разделяются вертикальной чертой)
            "Common.Core.Logs" + "|" +
            "Cysharp.Threading.Tasks" + "|" +
            "UnityEngine.StackTraceUtility" + "|" +
            "UnityEngine.Logger" +
            ")" +
            ".*" + // Любые символы (кроме переноса строки) в любом количестве
            "$" + // Окончание строки (оно находится перед непостредственно символом переноса строки)
            "(\r\n|\n)", // Перевод строки
            RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>
        /// Вырезать строки из колстека не имеет смысла в редакторе, там консолька сама отдельно формирует колстек с ссылками в IDE.
        /// </summary>
        public static bool IsEnabled => !Application.isEditor || Application.isBatchMode;

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            if (IsEnabled && logType == LogType.Log)
            {
                var stackTrace = StackTraceUtility.ExtractStackTrace();
                stackTrace = _redundantLinesRegex.Replace(stackTrace, "");

                Debug.unityLogger.logHandler.LogFormat(logType, context, $"{format}\n{stackTrace}\n", args);
            }
            else
            {
                Debug.unityLogger.logHandler.LogFormat(logType, context, format, args);
            }
        }

        public void LogException(Exception exception, Object context)
        {
            Debug.unityLogger.logHandler.LogException(exception, context);
        }
    }
}