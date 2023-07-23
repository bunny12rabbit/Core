using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.Core.Logs
{
    public static class Log
    {
        public const string DebugCondition = "DEBUG";

        public struct Assert
        {
            [AssertionMethod]
            public static bool IsFalse([AssertionCondition(AssertionConditionType.IS_FALSE)]bool condition, Object context = default,
                [CallerFilePath] string tag = "")
            {
                if (condition)
                    return false;

                Log.AssertionLog("Assertion failed!", context, tag);
                return true;
            }

            [AssertionMethod]
            public static bool IsNotNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]T obj, Object context = default,
                [CallerFilePath] string tag = "")
            {
                if (obj != null)
                    return false;

                Log.AssertionLog($"{nameof(obj)} is null!", context, tag);
                return true;
            }

            [AssertionMethod]
            public static bool IsNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]T obj, Object context = default,
                [CallerFilePath] string tag = "")
            {
                if (obj == null)
                    return false;

                Log.AssertionLog($"{nameof(obj)} is not null!", context, tag);
                return true;
            }

            [AssertionMethod]
            public static bool IsNotNullOrEmpty<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]IEnumerable<T> enumerable,
                Object context = default, [CallerFilePath] string tag = "")
            {
                if (enumerable == null)
                {
                    Log.AssertionLog($"{nameof(enumerable)} is null!", context, tag);
                    return true;
                }

                if (enumerable.IsEmpty())
                {
                    Log.AssertionLog($"{nameof(enumerable)} is empty!", context, tag);
                    return true;
                }

                return false;
            }
        }

        private static ILogger s_logger;

        static Log()
        {
            s_logger = new UnityLogger();
        }

        [DebuggerHidden, Conditional(DebugCondition)]
        public static void Debug(string message, LogType logType = LogType.Log, Object context = default, [CallerFilePath] string tag = "") =>
            s_logger.PrettyLog(logType, message, context, tag);

        [DebuggerHidden]
        public static void Info(string message, Object context = default, [CallerFilePath] string tag = "") =>
            s_logger.PrettyLog(LogType.Log, message, context, tag);

        [DebuggerHidden, Conditional("UNITY_EDITOR")]
        public static void Editor(string message, Object context = default, [CallerFilePath] string tag = "") =>
            s_logger.PrettyLog(LogType.Log, message, context, tag);

        [DebuggerHidden, Conditional(DebugCondition)]
        public static void Warning(string message, Object context = default, [CallerFilePath] string tag = "") =>
            s_logger.PrettyLog(LogType.Warning, message, context, tag);

        [DebuggerHidden]
        public static void Error(string message, Object context = default, [CallerFilePath] string tag = "") =>
            s_logger.PrettyLog(LogType.Error, message, context, tag);

        [DebuggerHidden]
        public static void Exception(Exception exception, Object context = default, [CallerFilePath] string tag = "") =>
            s_logger.PrettyLog(LogType.Exception, StackTraceUtility.ExtractStringFromException(exception), context, tag);


        [DebuggerHidden, Conditional("UNITY_ASSERTIONS")]
        internal static void AssertionLog(string message, Object context, string tag = "") =>
            s_logger.PrettyLog(LogType.Assert, message, context, tag);
    }
}