using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Binding;

namespace FubuCore.Binding
{
    public class BindResult
    {
        public IList<ConvertProblem> Problems = new List<ConvertProblem>();
        public object Value;

        public override string ToString()
        {
            return string.Format("BindResult: {0}, Problems:  {1}", Value, Problems.Count);
        }

        public void AssertNoProblems(Type type)
        {
            if (Problems.Any())
            {
                throw new BindResultAssertionException(type, Problems);
            }
        }
    }
}