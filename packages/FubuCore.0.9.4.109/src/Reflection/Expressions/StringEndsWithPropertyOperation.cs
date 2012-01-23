using System;
using System.Reflection;

namespace FubuCore.Reflection.Expressions
{
    public class StringEndsWithPropertyOperation : CaseInsensitiveStringMethodPropertyOperation
    {
        private static readonly MethodInfo _method =
            ReflectionHelper.GetMethod<string>(s => s.EndsWith("", StringComparison.CurrentCulture));

        public StringEndsWithPropertyOperation()
            : base(_method)
        {
        }

        public override string Text
        {
            get { return "ends with"; }
        }
    }
}