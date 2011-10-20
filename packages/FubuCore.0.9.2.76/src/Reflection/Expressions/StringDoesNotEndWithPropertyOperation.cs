using System;
using System.Reflection;

namespace FubuCore.Reflection.Expressions
{
    public class StringDoesNotEndWithPropertyOperation : CaseInsensitiveStringMethodPropertyOperation
    {
        private static readonly MethodInfo _method =
            ReflectionHelper.GetMethod<string>(s => s.EndsWith("", StringComparison.CurrentCulture));

        public StringDoesNotEndWithPropertyOperation()
            : base(_method, true)
        {
        }

        public override string OperationName
        {
            get { return "DoesNotEndWith"; }
        }

        public override string Text
        {
            get { return "does not end with"; }
        }
    }
}