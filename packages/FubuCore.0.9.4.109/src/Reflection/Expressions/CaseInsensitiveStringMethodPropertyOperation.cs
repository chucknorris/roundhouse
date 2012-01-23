using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FubuCore.Reflection.Expressions
{
    public abstract class CaseInsensitiveStringMethodPropertyOperation : IPropertyOperation
    {
        private readonly MethodInfo _method;
        private readonly bool _negate;

        protected CaseInsensitiveStringMethodPropertyOperation(MethodInfo method) : this(method, false) { }

        protected CaseInsensitiveStringMethodPropertyOperation(MethodInfo method, bool negate)
        {
            _method = method;
            _negate = negate;
        }

        public virtual string OperationName
        {
            get { return _method.Name; }
        }
        public abstract string Text { get; }

        public Func<object, Expression<Func<T, bool>>> GetPredicateBuilder<T>(MemberExpression propertyPath)
        {
            return valueToCheck =>
            {
                ConstantExpression valueToCheckConstant = Expression.Constant(valueToCheck);
                BinaryExpression binaryExpression = Expression.Coalesce(propertyPath, Expression.Constant(string.Empty));
                ConstantExpression invariantCulture = Expression.Constant(StringComparison.InvariantCultureIgnoreCase);
                Expression expression = Expression.Call(binaryExpression, _method, valueToCheckConstant, invariantCulture);
                if (_negate)
                {
                    expression = Expression.Not(expression);
                }

                ParameterExpression lambdaParameter = propertyPath.GetParameter<T>();
                return Expression.Lambda<Func<T, bool>>(expression, lambdaParameter);
            };
        }
    }
}