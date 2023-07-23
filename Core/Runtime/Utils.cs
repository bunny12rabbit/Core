using System;
using UniRx;
using UnityEngine;
using Unity.Mathematics;
using Common.Core;
using Common.Core.Logs;
using Random = UnityEngine.Random;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Common.Core
{
    public static class Utils
    {
        /// <summary>
        /// Converts slider value to logarithmic mixer value
        /// </summary>
        /// <param name="value">amount (will be clamped between 0.0001 and 1)</param>
        /// <returns>Log from <paramref name="value"/></returns>
        public static float GetLogValueForMixer(float value) => Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;

        // TODO: Переделать на Tween
        public static Subject<float> Fade(float from, float to, float duration, float tolerance = 0.0001f)
        {
            var subject = new Subject<float>();
            var fadeInterpolator = 0f;
            IDisposable tickSub = null;
            tickSub = Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    fadeInterpolator += Time.unscaledDeltaTime;

                    if (duration == 0)
                        duration = fadeInterpolator;

                    var fraction = fadeInterpolator / duration;
                    var value = Mathf.Lerp(@from, to, fraction);

                    subject.OnNext(value);

                    if (Math.Abs(value - to) < tolerance)
                    {
                        subject.OnCompleted();
                        tickSub?.Dispose();
                    }
                });

            return subject;
        }

        public static float GetNormalizedPercentFromRange(float x, float minThresholdRange, float maxThresholdRange)
        {
            var range = maxThresholdRange - minThresholdRange;

            var rangeFraction = x < minThresholdRange
                ? 0
                : (x - minThresholdRange) / range;

            return math.clamp(rangeFraction, 0, 1);
        }

        private const string AllChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string GetRandomString(int length)
        {
            using (StringBuilderPool.Get(out var sb))
            {
                for (var i = 0; i < length; i++)
                    sb.Append(AllChars[Random.Range(0, AllChars.Length)]);

                return sb.ToString();
            }
        }

        /// <summary>
        /// This method generates an <see cref="Action{T}"/> delegate for a given <see cref="MethodInfo"/>, at target instance,
        /// with generic type parameter <typeparamref name="T"/> where use of
        /// <see cref="System.Delegate.CreateDelegate(System.Type,object,MethodInfo)"/> is impossible
        /// due to <see cref="Action{T}"/> is not covariant (method parameter is an implementation and <typeparamref name="T"/> is interface).
        /// The generated delegate can be used to invoke the specified method with the argument of type <typeparamref name="T"/> or it's inheritors or implementations.
        /// If the method has wrong signature, an error message is logged and null is returned.
        /// </summary>
        /// <typeparam name="T">The type of the argument</typeparam>
        /// <typeparam name="TInstance">The type of the target instance with method</typeparam>
        /// <param name="method">The <see cref="MethodInfo"/> of the method to be invoked</param>
        /// <param name="target">The target instance on which the method will be invoked</param>
        /// <returns>An <see cref="Action{T}"/> delegate that can invoke the specified method with the argument</returns>
        [CanBeNull]
        public static Action<T> CreateAction<T, TInstance>(MethodInfo method, TInstance target)
        {
            var parameters = method.GetParameters();

            if (parameters.Length != 1)
            {
                Log.Error($"::{nameof(CreateAction)}:: Wrong number of parameters in method {method.Name}");
                return null;
            }

            var parameter = parameters[0];
            var argType = parameter.ParameterType;

            if (method.ReturnType != typeof(void) || !typeof(T).IsAssignableFrom(argType))
            {
                Log.Error($"::{nameof(CreateAction)}:: Wrong signature in method {method.Name}");
                return null;
            }

            var instance = target is Unit or null ? null : Expression.Constant(target);
            var arg = Expression.Parameter(typeof(T), parameter.Name);
            var castArg = Expression.Convert(arg, argType);
            var methodCall = Expression.Call(instance, method, castArg);
            var lambda = Expression.Lambda<Action<T>>(methodCall, arg);
            return lambda.Compile();
        }

        /// <summary>
        /// This method generates an <see cref="Action{T}"/> delegate for a given <see cref="MethodInfo"/>
        /// with generic type parameter <typeparamref name="T"/>.
        /// The generated delegate can be used to invoke the specified static method with the argument of type
        /// <typeparamref name="T"/> or it's inheritors or implementations where use of
        /// <see cref="System.Delegate.CreateDelegate(System.Type,MethodInfo)"/> is impossible
        /// due to <see cref="Action{T}"/> is not covariant (method parameter is an implementation and <typeparamref name="T"/> is interface).
        /// If the method has wrong signature, an error message is logged and null is returned.
        /// </summary>
        /// <typeparam name="T">The type of the argument</typeparam>
        /// <param name="method">The <see cref="MethodInfo"/> of the static method to be invoked</param>
        /// <returns>An <see cref="Action{T}"/> delegate that can invoke the specified static method with the argument</returns>
        [CanBeNull]
        public static Action<T> CreateAction<T>(MethodInfo method) => CreateAction<T, Unit>(method, Unit.Default);

        /// <summary>
        /// This method generates an <see cref="Action{T1, T2}"/> delegate for a given <see cref="MethodInfo"/>, at target instance,
        /// with generic type parameters <typeparamref name="T1"/> and <typeparamref name="T2"/> where use of
        /// <see cref="System.Delegate.CreateDelegate(System.Type, System.Type, object,MethodInfo)"/> is impossible
        /// due to <see cref="Action{T}"/> is not covariant (method parameter is an implementation and <typeparamref name="T1"/> is interface).
        /// The generated delegate can be used to invoke the specified method with two arguments of types <typeparamref name="T1"/> and <typeparamref name="T2"/>.
        /// or it's inheritors or implementations.
        /// If the method has wrong signature, an error message is logged and null is returned.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument</typeparam>
        /// <typeparam name="T2">The type of the second argument</typeparam>
        /// <typeparam name="TInstance">The type of the target instance with method</typeparam>
        /// <param name="method">The <see cref="MethodInfo"/> of the method to be invoked</param>
        /// <param name="target">The target instance on which the method will be invoked</param>
        /// <returns>An <see cref="Action{T1, T2}"/> delegate that can invoke the specified method with two arguments</returns>
        [CanBeNull]
        public static Action<T1, T2> CreateAction<T1, T2, TInstance>(MethodInfo method, TInstance target)
        {
            var parameters = method.GetParameters();

            if (parameters.Length != 2)
            {
                Log.Error($"::{nameof(CreateAction)}:: Wrong number of parameters in method {method.Name}");
                return null;
            }

            var parameter0 = parameters[0];
            var parameter1 = parameters[1];
            var arg0Type = parameter0.ParameterType;
            var arg1Type = parameter1.ParameterType;

            if (method.ReturnType != typeof(void) || !typeof(T1).IsAssignableFrom(arg0Type) || !typeof(T2).IsAssignableFrom(arg1Type))
            {
                Log.Error($"::{nameof(CreateAction)}:: Wrong signature in method {method.Name}");
                return null;
            }

            var instance = target is Unit or null ? null : Expression.Constant(target);
            var arg0 = Expression.Parameter(typeof(T1), parameter0.Name);
            var arg1 = Expression.Parameter(typeof(T2), parameter1.Name);
            var castArg0 = Expression.Convert(arg0, arg0Type);
            var castArg1 = Expression.Convert(arg1, arg1Type);
            var methodCall = Expression.Call(instance, method, castArg0, castArg1);
            var lambda = Expression.Lambda<Action<T1, T2>>(methodCall, arg0, arg1);
            return lambda.Compile();
        }

        /// <summary>
        /// This method generates an <see cref="Action{T1, T2}"/> delegate for a given <see cref="MethodInfo"/>.
        /// The generated delegate can be used to invoke the specified static method with two arguments of types
        /// <typeparamref name="T1"/> and <typeparamref name="T2"/> or it's inheritors or implementations where use of
        /// <see cref="System.Delegate.CreateDelegate(System.Type,System.Type,MethodInfo)"/> is impossible
        /// due to <see cref="Action{T}"/> is not covariant (method parameter is an implementation and <typeparamref name="T1"/> is interface).
        /// If the method has wrong signature, an error message is logged and null is returned.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument</typeparam>
        /// <typeparam name="T2">The type of the second argument</typeparam>
        /// <param name="method">The <see cref="MethodInfo"/> of the static method to be invoked</param>
        /// <returns>An <see cref="Action{T1, T2}"/> delegate that can invoke the specified static method with two arguments</returns>
        [CanBeNull]
        public static Action<T1, T2> CreateAction<T1, T2>(MethodInfo method) => CreateAction<T1, T2, Unit>(method, Unit.Default);
    }
}