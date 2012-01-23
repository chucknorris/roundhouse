using System;
using System.Reflection;

namespace FubuCore.Reflection.Expressions
{
    public class StringDoesNotStartWithPropertyOperation : CaseInsensitiveStringMethodPropertyOperation
    {
        private static readonly MethodInfo _method =
            ReflectionHelper.GetMethod<string>(s => s.StartsWith("", StringComparison.CurrentCulture));

        public StringDoesNotStartWithPropertyOperation()
            : base(_method, true)
        {
        }

        public override string OperationName
        {
            get { return "DoesNotStartWith"; }
        }

        public override string Text
        {
            get { return "does not start with"; }
        }
    }
}