using System.Linq.Expressions;

namespace FubuCore.Reflection.Expressions
{
    public class GreaterThanOrEqualPropertyOperation : BinaryComparisonPropertyOperation
    {
        public GreaterThanOrEqualPropertyOperation()
            : base(ExpressionType.GreaterThanOrEqual)
        {
        }

        public override string OperationName { get { return "GreaterThanOrEqual"; } }

        public override string Text
        {
            get { return "greater than or equal to"; }
        }
    }
}