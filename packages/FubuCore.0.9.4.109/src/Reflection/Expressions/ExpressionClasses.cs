using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FubuCore.Reflection.Expressions
{
    public interface IArguments
    {
        T Get<T>(string propertyName);
        bool Has(string propertyName);
    }

    public static class ConstructorBuilder
    {
        public static LambdaExpression CreateSingleStringArgumentConstructor(Type concreteType)
        {
            var constructor = concreteType.GetConstructor(new Type[]{typeof (string)});
            if (constructor == null)
            {
                throw new ArgumentOutOfRangeException("concreteType", concreteType, "Only types with a ctor(string) can be used here");
            }

            var argument = Expression.Parameter(typeof (string), "x");

            NewExpression ctorCall = Expression.New(constructor, argument);

            var funcType = typeof (Func<,>).MakeGenericType(typeof (string), concreteType);
            return Expression.Lambda(funcType, ctorCall, argument);
        }
    }

    public class ConstructorFunctionBuilder<T>
    {
        public Func<IArguments, T> CreateBuilder(ConstructorInfo constructor)
        {
            ParameterExpression args = Expression.Parameter(typeof(IArguments), "x");


            IEnumerable<Expression> arguments =
                constructor.GetParameters().Select(
                    param => ToParameterValueGetter(args, param.ParameterType, param.Name));

            NewExpression ctorCall = Expression.New(constructor, arguments);

            LambdaExpression lambda = Expression.Lambda(typeof(Func<IArguments, T>), ctorCall, args);
            return (Func<IArguments, T>)lambda.Compile();
        }

        public static Expression ToParameterValueGetter(ParameterExpression args, Type type, string argName)
        {
            MethodInfo method = typeof(IArguments).GetMethod("Get").MakeGenericMethod(type);
            return Expression.Call(args, method, Expression.Constant(argName));
        }
    }
}