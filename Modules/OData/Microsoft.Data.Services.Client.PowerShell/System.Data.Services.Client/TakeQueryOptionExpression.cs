namespace System.Data.Services.Client
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;

    [DebuggerDisplay("TakeQueryOptionExpression {TakeAmount}")]
    internal class TakeQueryOptionExpression : QueryOptionExpression
    {
        private ConstantExpression takeAmount;

        internal TakeQueryOptionExpression(Type type, ConstantExpression takeAmount) : base(type)
        {
            this.takeAmount = takeAmount;
        }

        internal override QueryOptionExpression ComposeMultipleSpecification(QueryOptionExpression previous)
        {
            int num = (int) this.takeAmount.Value;
            int num2 = (int) ((TakeQueryOptionExpression) previous).takeAmount.Value;
            if (num >= num2)
            {
                return previous;
            }
            return this;
        }

        public override ExpressionType NodeType
        {
            get
            {
                return (ExpressionType) 0x2713;
            }
        }

        internal ConstantExpression TakeAmount
        {
            get
            {
                return this.takeAmount;
            }
        }
    }
}

