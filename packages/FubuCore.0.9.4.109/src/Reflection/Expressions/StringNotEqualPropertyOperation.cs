using System;
using System.Reflection;

namespace FubuCore.Reflection.Expressions
{
    public class StringNotEqualPropertyOperation : CaseInsensitiveStringMethodPropertyOperation
    {
        private static readonly MethodInfo _method =
            ReflectionHelper.GetMethod<string>(s => s.Equals("", StringComparison.CurrentCulture));

        public StringNotEqualPropertyOperation()
            : base(_method, true)
        {
        }

        public override string OperationName
        {
            get { return "DoesNotEqual"; }
        }

        public override string Text
        {
            get { return "is not"; }
        }
    }
}