using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace FubuCore.Reflection.Expressions
{
    public class ComposableOrOperation
    {
        private List<Tuple<IPropertyOperation, MemberExpression, object>> _listOfOperations = new List<Tuple<IPropertyOperation, MemberExpression, object>>();

        public void Set<T>(Expression<Func<T, object>> path, object value)
        {
            //why am I falling into here
            var memberExpression = path.GetMemberExpression(true);
            var operation = new EqualsPropertyOperation();
            _listOfOperations.Add(new Tuple<IPropertyOperation, MemberExpression, object>(operation, memberExpression, value));
        }


        public void Set<T>(Expression<Func<T, object>> path, IEnumerable<object> value)
        {
            var memberExpression = path.GetMemberExpression(true);
            var operation = new CollectionContainsPropertyOperation();
            _listOfOperations.Add(new Tuple<IPropertyOperation, MemberExpression, object>(operation, memberExpression, value));
        }

        public Expression<Func<T, bool>> GetPredicateBuilder<T>()
        {
            if(_listOfOperations.Count() == 0)
            {
                throw new Exception("You must have at least one operation registered for an 'or' operation (you have {0})".ToFormat(_listOfOperations.Count));
            }

            //the parameter to use
            var lambdaParameter = Expression.Parameter(typeof (T));

            var initialPredicate = Expression.Constant(false);
            Expression builtUpPredicate = initialPredicate;

            foreach (var operation in _listOfOperations)
            {
                var predBuilder = operation.Item1.GetPredicateBuilder<T>(operation.Item2);
                var predicate = predBuilder(operation.Item3);
                var expPredicate = rebuild(predicate, lambdaParameter);
                builtUpPredicate = Expression.MakeBinary(ExpressionType.OrElse, builtUpPredicate, expPredicate);
            }

            return Expression.Lambda<Func<T,bool>>(builtUpPredicate, lambdaParameter);
        }

        Expression rebuild(Expression exp, ParameterExpression parameter)
        {
            var lb = (LambdaExpression) exp;
            var targetBody = lb.Body;

            return new RewriteToLambda(parameter).Visit(targetBody);
        }

    }

    public class OrOperation
    {
        
        public Expression<Func<T, bool>> GetPredicateBuilder<T>(Expression<Func<T, object>> leftPath, object leftValue, Expression<Func<T, object>> rightPath, object rightValue)
        {
            var comp = new ComposableOrOperation();
            comp.Set(leftPath, leftValue);
            comp.Set(rightPath, rightValue);
            return comp.GetPredicateBuilder<T>();
        }

        public Expression<Func<T, bool>> GetPredicateBuilder<T>(Expression<Func<T, object>> leftPath, IEnumerable<object> leftValue, Expression<Func<T, object>> rightPath, object rightValue)
        {
            var comp = new ComposableOrOperation();
            comp.Set(leftPath, leftValue);
            comp.Set(rightPath, rightValue);
            return comp.GetPredicateBuilder<T>();
        }

        public Expression<Func<T, bool>> GetPredicateBuilder<T>(Expression<Func<T, object>> leftPath, object leftValue, Expression<Func<T, object>> rightPath, IEnumerable<object> rightValue)
        {
            var comp = new ComposableOrOperation();
            comp.Set(leftPath, leftValue);
            comp.Set(rightPath, rightValue);
            return comp.GetPredicateBuilder<T>();
        }
    }

    public class RewriteToLambda : ExpressionVisitorBase
    {
        private ParameterExpression _parameter;

        public RewriteToLambda(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitBinary(BinaryExpression exp)
        {
            var a = VisitMemberAccess((MemberExpression) exp.Left);
            return Expression.Equal(a, exp.Right);    
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            Expression exp = null;
            if(m.Expression.NodeType == ExpressionType.Parameter)
            {
                //c.IsThere
                exp = Expression.MakeMemberAccess(_parameter, m.Member);
            }

            if(m.Expression.NodeType == ExpressionType.MemberAccess)
            {
                //c.Thing.IsThere

                //rewrite c.Thing
                var intermediate = VisitMemberAccess((MemberExpression)m.Expression);

                //now combine 'c.Thing' with '.IsThere'
                exp = Expression.MakeMemberAccess(intermediate, m.Member);
            }

            return exp;
        }

        protected override Expression VisitMethodCall(MethodCallExpression exp)
        {
            if (exp.Method.IsStatic)
            {
                var aa = exp.Arguments.Skip(1).First();
                if(aa.NodeType != ExpressionType.Constant)
                {
                    aa = VisitMemberAccess((MemberExpression) exp.Arguments.Skip(1).First());
                }

                //if second arg is a constant of our type swap other wise continue down the rabbit hole
                var args = new[] {exp.Arguments.First(), aa};
                return Expression.Call(exp.Method, args);
            }

            return Expression.Call(_parameter, exp.Method, exp.Arguments.First());
        }
    }
}