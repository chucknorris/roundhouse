using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

namespace FubuCore.Reflection.Expressions
{
    public class StringStartsWithPropertyOperation : CaseInsensitiveStringMethodPropertyOperation
    {
        private static readonly MethodInfo _method =
            ReflectionHelper.GetMethod<string>(s => s.StartsWith("", StringComparison.CurrentCulture));

        public StringStartsWithPropertyOperation()
            : base(_method)
        {
        }

        public override string Text
        {
            get { return "starts with"; }
        }
    }

    public class CollectionContainsPropertyOperation : IPropertyOperation
    {
        private const string _operationName = "Contains";
        private const string _description = "contains";
                private MethodInfo method =
                    typeof (Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(
                        m => m.Name.EqualsIgnoreCase("Contains")).First();

        public string OperationName { get { return _operationName; } }
        
        public string Text
        {
            get { return _description; }
        }

        public Func<object, Expression<Func<T, bool>>> GetPredicateBuilder<T>(MemberExpression propertyPath)
        {
            return valuesToCheck =>
            {
                var enumerationOfObjects = (IEnumerable<object>) valuesToCheck;
                if (enumerationOfObjects == null) return c => false;

                //what's the type of the collection?
                var valuesToCheckType = valuesToCheck.GetType();
                var collectionOf = valuesToCheckType.IsAnEnumerationOf();
                
                

                //capture and close the Enumerbable.Contains method
                var closedMethod = method.MakeGenericMethod(collectionOf);

                //the list that we need to call contains on
                var list = Expression.Constant(enumerationOfObjects);


                //lambda parameter
                var param = Expression.Parameter(typeof(T));

                //this should be a property call
                var memberAccess = Expression.MakeMemberAccess(param, propertyPath.Member);
                
                
                //call 'Contains' with the desired 'value' to check on the 'list'
                var call = Expression.Call(closedMethod, list, memberAccess);


                var lambda = Expression.Lambda<Func<T, bool>>(call, param);
                //return enumerationOfObjects.Contains(((PropertyInfo) propertyPath.Member).GetValue(c, null));
                return lambda;

            };
        }
    }

    public class StringContainsPropertyOperation : StringContainsPropertyOperationBase
    {
        public StringContainsPropertyOperation() : base("Contains", "contains", false)
        {
        }
    }

    public class StringDoesNotContainPropertyOperation : StringContainsPropertyOperationBase
    {
        public StringDoesNotContainPropertyOperation() : base("DoesNotContain", "does not contain", true)
        {
        }
    }

    public abstract class StringContainsPropertyOperationBase : IPropertyOperation
    {
        private static readonly MethodInfo _indexOfMethod;
        private readonly string _operation;
        private readonly string _description;
        private readonly bool _negate;

        static StringContainsPropertyOperationBase()
        {
            _indexOfMethod =
                ReflectionHelper.GetMethod<string>(s => s.IndexOf("", StringComparison.InvariantCultureIgnoreCase));
        }

        protected StringContainsPropertyOperationBase(string operation, string description, bool negate)
        {
            _operation = operation;
            _description = description;
            _negate = negate;
        }

        public string OperationName { get { return _operation; } }
        public string Text
        {
            get { return _description; }
        }

        public Func<object, Expression<Func<T, bool>>> GetPredicateBuilder<T>(MemberExpression propertyPath)
        {
            return valueToCheck =>
            {
                ConstantExpression valueToCheckConstant = Expression.Constant(valueToCheck);
                MethodCallExpression indexOfCall =
                    Expression.Call(Expression.Coalesce(propertyPath, Expression.Constant(String.Empty)), _indexOfMethod,
                                    valueToCheckConstant,
                                    Expression.Constant(StringComparison.InvariantCultureIgnoreCase));
                var operation = _negate ? ExpressionType.LessThan : ExpressionType.GreaterThanOrEqual;
                BinaryExpression comparison = Expression.MakeBinary(operation, indexOfCall,
                                                                    Expression.Constant(0));
                ParameterExpression lambdaParameter = propertyPath.GetParameter<T>();
                return Expression.Lambda<Func<T, bool>>(comparison, lambdaParameter);
            };
        }
    }
}