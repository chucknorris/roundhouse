using System;
using System.Collections.Generic;
using System.Reflection;

namespace FubuCore
{
    public class DisplayConversionRegistry
    {
        private readonly IList<StringifierStrategy> _strategies = new List<StringifierStrategy>();


        public Stringifier BuildStringifier()
        {
            var stringifier = new Stringifier();
            Configure(stringifier);
            return stringifier;
        }

        public void Configure(Stringifier stringifier)
        {
            _strategies.Each(s => stringifier.AddStrategy(s));
        }


        private MakeDisplayExpression makeDisplay(Func<GetStringRequest, bool> filter)
        {
            return new MakeDisplayExpression(func =>
            {
                _strategies.Add(new StringifierStrategy
                {
                    Matches = filter,
                    StringFunction = func
                });
            });
        }

        private MakeDisplayExpression<T> makeDisplay<T>(Func<GetStringRequest, bool> filter)
        {
            return new MakeDisplayExpression<T>(func =>
            {
                _strategies.Add(new StringifierStrategy
                {
                    Matches = filter,
                    StringFunction = func
                });
            });
        }

        public MakeDisplayExpression IfTypeMatches(Func<Type, bool> filter)
        {
            return makeDisplay(request => filter(request.PropertyType));
        }

        public MakeDisplayExpression<T> IfIsType<T>()
        {
            return makeDisplay<T>(request => request.PropertyType == typeof (T));
        }

        public MakeDisplayExpression<T> IfCanBeCastToType<T>()
        {
            return makeDisplay<T>(t => t.PropertyType.CanBeCastTo<T>());
        }

        public MakeDisplayExpression IfPropertyMatches(Func<PropertyInfo, bool> matches)
        {
            return makeDisplay(request => request.Property != null && matches(request.Property));
        }

        public MakeDisplayExpression<T> IfPropertyMatches<T>(Func<PropertyInfo, bool> matches)
        {
            return
                makeDisplay<T>(
                    request =>
                    request.Property != null && request.PropertyType == typeof (T) && matches(request.Property));
        }

        #region Nested type: MakeDisplayExpression

        public class MakeDisplayExpression : MakeDisplayExpressionBase
        {
            public MakeDisplayExpression(Action<Func<GetStringRequest, string>> callback)
                : base(callback)
            {
            }

            public void ConvertBy(Func<GetStringRequest, string> display)
            {
                _callback(display);
            }

            public void ConvertWith<TService>(Func<TService, GetStringRequest, string> display)
            {
                apply(o => display(o.Get<TService>(), o));
            }
        }

        public class MakeDisplayExpression<T> : MakeDisplayExpressionBase
        {
            public MakeDisplayExpression(Action<Func<GetStringRequest, string>> callback)
                : base(callback)
            {
            }

            public void ConvertBy(Func<T, string> display)
            {
                apply(o => display((T) o.RawValue));
            }

            public void ConvertBy(Func<GetStringRequest, T, string> display)
            {
                apply(o => display(o, (T) o.RawValue));
            }

            public void ConvertWith<TService>(Func<TService, T, string> display)
            {
                apply(o => display(o.Get<TService>(), (T) o.RawValue));
            }
        }

        #endregion

        #region Nested type: MakeDisplayExpressionBase

        public abstract class MakeDisplayExpressionBase
        {
            protected Action<Func<GetStringRequest, string>> _callback;

            public MakeDisplayExpressionBase(Action<Func<GetStringRequest, string>> callback)
            {
                _callback = callback;
            }

            protected void apply(Func<GetStringRequest, string> func)
            {
                _callback(func);
            }
        }

        #endregion
    }
}