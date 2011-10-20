using System;
using System.Linq.Expressions;

namespace FubuCore.Reflection
{
    public interface IValueGetter
    {
        object GetValue(object target);
        string Name { get; }
        Type DeclaringType { get; }

        Expression ChainExpression(Expression body);
    }
}