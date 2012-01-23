using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FubuCore
{
    public static class BooleanExtensions
    {
        public static string If(this string html, Expression<Func<bool>> modelBooleanValue)
        {
            return GetBooleanPropertyValue(modelBooleanValue) ? html : string.Empty;
        }

        public static string IfNot(this string html, Expression<Func<bool>> modelBooleanValue)
        {
            return !GetBooleanPropertyValue(modelBooleanValue) ? html : string.Empty;
        }

        private static bool GetBooleanPropertyValue(Expression<Func<bool>> modelBooleanValue)
        {
            var prop = modelBooleanValue.Body as MemberExpression;
            if (prop != null)
            {
                var info = prop.Member as PropertyInfo;
                if (info != null)
                {
                    return modelBooleanValue.Compile().Invoke();
                }
            }
            throw new ArgumentException(
                "The modelBooleanValue parameter should be a single property, validation logic is not allowed, only 'x => x.BooleanValue' usage is allowed, if more is needed do that in the Controller");
        }
    }
}