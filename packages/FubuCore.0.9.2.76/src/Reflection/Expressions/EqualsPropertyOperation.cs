using System.Linq.Expressions;

namespace FubuCore.Reflection.Expressions
{
    public class EqualsPropertyOperation : BinaryComparisonPropertyOperation
    {
        public EqualsPropertyOperation()
            : base(ExpressionType.Equal)
        {
        }

        public override string OperationName { get { return "Is"; } }
        public override string Text
        {
            get { return "is"; }
        }
    }
}