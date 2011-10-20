using System;

namespace FubuCore
{
    public static class BasicExtensions
    {
        public static void SafeDispose(this IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception)
            {
                // That's right, swallow that exception
            }
        }

        public static TOut IfNotNull<TTarget, TOut>(this TTarget target, Func<TTarget, TOut> valueFunc)
            where TTarget : class
        {
            return target == null ? default(TOut) : valueFunc(target);
        }

        public static void CallIfNotNull<TTarget>(this TTarget target, Action<TTarget> actionToPerform)
            where TTarget : class
        {
            if(target != null)
            {
                actionToPerform(target);   
            }
        }

        public static T IfNotNull<T>(this object target, Func<T> valueFunc)
            where T : class
        {
            return target == null ? null : valueFunc();
        }

        public static void IfNotNull<T>(this T? target, Action<T> action) where T : struct
        {
            if (target.HasValue)
            {
                action(target.Value);
            }
        }
    }
}