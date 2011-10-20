using System;
using System.Reflection;

namespace FubuCore.Reflection.Expressions
{
    public class StringEqualsPropertyOperation : CaseInsensitiveStringMethodPropertyOperation
    {
        private static readonly MethodInfo _method =
            ReflectionHelper.GetMethod<string>(s => s.Equals("", StringComparison.CurrentCulture));

        public StringEqualsPropertyOperation()
            : base(_method)
        {
        }

        public override string Text
        {
            get { return "is"; }
        }
    }
}