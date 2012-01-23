using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FubuCore.Reflection
{
    public static class ReflectionHelper
    {
        public static bool MeetsSpecialGenericConstraints(Type genericArgType, Type proposedSpecificType)
        {
            GenericParameterAttributes gpa = genericArgType.GenericParameterAttributes;
            GenericParameterAttributes constraints = gpa & GenericParameterAttributes.SpecialConstraintMask;

            // No constraints, away we go!
            if (constraints == GenericParameterAttributes.None)
                return true;

            // "class" constraint and this is a value type
            if ((constraints & GenericParameterAttributes.ReferenceTypeConstraint) != 0
                && proposedSpecificType.IsValueType)
            {
                return false;
            }

            // "struct" constraint and this is not a value type
            if ((constraints & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0
                && ! proposedSpecificType.IsValueType)
            {
                return false;
            }

            // "new()" constraint and this type has no default constructor
            if ((constraints & GenericParameterAttributes.DefaultConstructorConstraint) != 0
                && proposedSpecificType.GetConstructor(Type.EmptyTypes) == null)
            {
                return false;
            }

            return true;
        }

        public static PropertyInfo GetProperty<TModel>(Expression<Func<TModel, object>> expression)
        {
            MemberExpression memberExpression = getMemberExpression(expression);
            return (PropertyInfo) memberExpression.Member;
        }

        public static PropertyInfo GetProperty<TModel, T>(Expression<Func<TModel, T>> expression)
        {
            MemberExpression memberExpression = getMemberExpression(expression);
            return (PropertyInfo) memberExpression.Member;
        }

        public static PropertyInfo GetProperty(LambdaExpression expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression, true);
            return (PropertyInfo)memberExpression.Member;
        }

        private static MemberExpression getMemberExpression<TModel, T>(Expression<Func<TModel, T>> expression)
        {
            MemberExpression memberExpression = null;
            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                var body = (UnaryExpression) expression.Body;
                memberExpression = body.Operand as MemberExpression;
            }
            else if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression.Body as MemberExpression;
            }


            if (memberExpression == null) throw new ArgumentException("Not a member access", "member");
            return memberExpression;
        }

        public static Accessor GetAccessor(LambdaExpression expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression, true);

            return GetAccessor(memberExpression);
        }

        public static MemberExpression GetMemberExpression(this LambdaExpression expression, bool enforceMemberExpression)
        {
            MemberExpression memberExpression = null;
            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                var body = (UnaryExpression)expression.Body;
                memberExpression = body.Operand as MemberExpression;
            }
            else if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression.Body as MemberExpression;
            }


            if (enforceMemberExpression && memberExpression == null) throw new ArgumentException("Not a member access", "member");
            return memberExpression;
        }

        public static bool IsMemberExpression<T>(Expression<Func<T, object>> expression)
        {
            return IsMemberExpression<T, object>(expression);
        }

        public static bool IsMemberExpression<T, U>(Expression<Func<T, U>> expression)
        {
            return GetMemberExpression(expression, false) != null;
        }

        public static Accessor GetAccessor<TModel>(Expression<Func<TModel, object>> expression)
        {
            MemberExpression memberExpression = getMemberExpression(expression);

            return GetAccessor(memberExpression);
        }

        public static Accessor GetAccessor(MemberExpression memberExpression)
        {
            var list = new List<IValueGetter>();

            buildValueGetters(memberExpression, list);

            if (list.Count == 1)
            {
                var propertyValueGetter = list[0] as PropertyValueGetter;
                return propertyValueGetter != null ? new SingleProperty(propertyValueGetter.PropertyInfo) : null;
            }

            list.Reverse();
            return new PropertyChain(list.ToArray());
        }

        private static void buildValueGetters(Expression expression, IList<IValueGetter> list)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
            {
                var propertyInfo = (PropertyInfo) memberExpression.Member;
                list.Add(new PropertyValueGetter(propertyInfo));
                if (memberExpression.Expression != null)
                {
                    buildValueGetters(memberExpression.Expression, list);
                }
            }

            //deals with collection indexers, an indexer [0] will look like a get(0) method call expression
            var methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression != null)
            {
                var methodInfo = methodCallExpression.Method;
                //only supporting constant expressions as means of providing index
                var firstArgumentExpression = methodCallExpression.Arguments.First() as ConstantExpression;
                if (firstArgumentExpression != null)
                {
                    var value = firstArgumentExpression.Value;
                    var methodValueGetter = new MethodValueGetter(methodInfo, value);
                    list.Add(methodValueGetter);
                }
                if (methodCallExpression.Object != null)
                {
                    buildValueGetters(methodCallExpression.Object, list);
                }
            }
        }

        public static Accessor GetAccessor<TModel, T>(Expression<Func<TModel, T>> expression)
        {
            MemberExpression memberExpression = getMemberExpression(expression);

            return GetAccessor(memberExpression);
        }

        public static MethodInfo GetMethod<T>(Expression<Func<T, object>> expression)
        {
            return new FindMethodVisitor(expression).Method;
        }

        public static MethodInfo GetMethod(Expression<Func<object>> expression)
        {
            return GetMethod<Func<object>>(expression);
        }

        public static MethodInfo GetMethod(Expression expression)
        {
            return new FindMethodVisitor(expression).Method;
        }

        public static MethodInfo GetMethod<TDelegate>(Expression<TDelegate> expression)
        {
            return new FindMethodVisitor(expression).Method;
        }

        public static MethodInfo GetMethod<T, U>(Expression<Func<T, U>> expression)
        {
            return new FindMethodVisitor(expression).Method;
        }

        public static MethodInfo GetMethod<T, U, V>(Expression<Func<T, U, V>> expression)
        {
            return new FindMethodVisitor(expression).Method;
        }

        public static MethodInfo GetMethod<T>(Expression<Action<T>> expression)
        {
            return new FindMethodVisitor(expression).Method;
        }

    }

    public class FindMethodVisitor : ExpressionVisitorBase
    {
        private MethodInfo _method;

        public FindMethodVisitor(Expression expression)
        {
            Visit(expression);
        }

        public MethodInfo Method { get { return _method; } }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            _method = m.Method;
            return m;
        }
    }

    /// <summary>
    /// Provides virtual methods that can be used by subclasses to parse an expression tree.
    /// </summary>
    /// <remarks>
    /// This class actually already exists in the System.Core assembly...as an internal class.
    /// I can only speculate as to why it is internal, but it is obviously much too dangerous
    /// for anyone outside of Microsoft to be using...
    /// </remarks>
    [DebuggerStepThrough, DebuggerNonUserCode]
    public abstract class ExpressionVisitorBase
    {
        public virtual Expression Visit(Expression exp)
        {
            if (exp == null) return exp;

            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary((UnaryExpression) exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression) exp);
                case ExpressionType.TypeIs:
                    return VisitTypeIs((TypeBinaryExpression) exp);
                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression) exp);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression) exp);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression) exp);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression) exp);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression) exp);
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression) exp);
                case ExpressionType.New:
                    return VisitNew((NewExpression) exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression) exp);
                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression) exp);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression) exp);
                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression) exp);
                default:
                    throw new NotSupportedException(String.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment) binding);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding) binding);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding) binding);
                default:
                    throw new NotSupportedException(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            ReadOnlyCollection<Expression> arguments = VisitList(initializer.Arguments);
            if (arguments != initializer.Arguments)
            {
                return Expression.ElementInit(initializer.AddMethod, arguments);
            }
            return initializer;
        }

        protected virtual Expression VisitUnary(UnaryExpression u)
        {
            Expression operand = Visit(u.Operand);
            if (operand != u.Operand)
            {
                return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            }
            return u;
        }

        protected virtual Expression VisitBinary(BinaryExpression b)
        {
            Expression left = Visit(b.Left);
            Expression right = Visit(b.Right);
            Expression conversion = Visit(b.Conversion);

            if (left != b.Left || right != b.Right || conversion != b.Conversion)
            {
                if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                else
                    return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
            }
            return b;
        }

        protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            Expression expr = Visit(b.Expression);
            if (expr != b.Expression)
            {
                return Expression.TypeIs(expr, b.TypeOperand);
            }
            return b;
        }

        protected virtual Expression VisitConstant(ConstantExpression c)
        {
            return c;
        }

        protected virtual Expression VisitConditional(ConditionalExpression c)
        {
            Expression test = Visit(c.Test);
            Expression ifTrue = Visit(c.IfTrue);
            Expression ifFalse = Visit(c.IfFalse);

            if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }

            return c;
        }

        protected virtual Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }

        protected virtual Expression VisitMemberAccess(MemberExpression m)
        {
            Expression exp = Visit(m.Expression);
            if (exp != m.Expression)
            {
                return Expression.MakeMemberAccess(exp, m.Member);
            }
            return m;
        }

        protected virtual Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression obj = Visit(m.Object);
            IEnumerable<Expression> args = VisitList(m.Arguments);

            if (obj != m.Object || args != m.Arguments)
            {
                return Expression.Call(obj, m.Method, args);
            }

            return m;
        }

        protected virtual ReadOnlyCollection<Expression> VisitList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                Expression p = Visit(original[i]);
                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != original[i])
                {
                    list = new List<Expression>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(p);
                }
            }

            if (list != null)
                return list.AsReadOnly();

            return original;
        }

        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression e = Visit(assignment.Expression);

            if (e != assignment.Expression)
            {
                return Expression.Bind(assignment.Member, e);
            }

            return assignment;
        }

        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> bindings = VisitBindingList(binding.Bindings);

            if (bindings != binding.Bindings)
            {
                return Expression.MemberBind(binding.Member, bindings);
            }

            return binding;
        }

        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(binding.Initializers);

            if (initializers != binding.Initializers)
            {
                return Expression.ListBind(binding.Member, initializers);
            }
            return binding;
        }

        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                MemberBinding b = VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }

            if (list != null)
                return list;

            return original;
        }

        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                ElementInit init = VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }

            if (list != null)
                return list;

            return original;
        }

        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body = Visit(lambda.Body);
            if (body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }
            return lambda;
        }

        protected virtual NewExpression VisitNew(NewExpression nex)
        {
            IEnumerable<Expression> args = VisitList(nex.Arguments);
            if (args != nex.Arguments)
            {
                if (nex.Members != null)
                    return Expression.New(nex.Constructor, args, nex.Members);
                else
                    return Expression.New(nex.Constructor, args);
            }

            return nex;
        }

        protected virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> bindings = VisitBindingList(init.Bindings);

            if (n != init.NewExpression || bindings != init.Bindings)
            {
                return Expression.MemberInit(n, bindings);
            }

            return init;
        }

        protected virtual Expression VisitListInit(ListInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(init.Initializers);

            if (n != init.NewExpression || initializers != init.Initializers)
            {
                return Expression.ListInit(n, initializers);
            }

            return init;
        }

        protected virtual Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> exprs = VisitList(na.Expressions);
            if (exprs != na.Expressions)
            {
                if (na.NodeType == ExpressionType.NewArrayInit)
                {
                    return Expression.NewArrayInit(na.Type.GetElementType(), exprs);
                }
                else
                {
                    return Expression.NewArrayBounds(na.Type.GetElementType(), exprs);
                }
            }

            return na;
        }

        protected virtual Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<Expression> args = VisitList(iv.Arguments);
            Expression expr = Visit(iv.Expression);

            if (args != iv.Arguments || expr != iv.Expression)
            {
                return Expression.Invoke(expr, args);
            }

            return iv;
        }
    }
}