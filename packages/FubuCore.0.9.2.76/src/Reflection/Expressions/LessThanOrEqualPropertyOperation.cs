using System.Linq.Expressions;

namespace FubuCore.Reflection.Expressions
{
    public class LessThanOrEqualPropertyOperation : BinaryComparisonPropertyOperation
    {
        public LessThanOrEqualPropertyOperation()
            : base(ExpressionType.LessThanOrEqual)
        {
        }

        public override string OperationName { get { return "LessThanOrEqual"; } }

        public override string Text
        {
            get { return "less than or equal to"; }
        }
    }
}