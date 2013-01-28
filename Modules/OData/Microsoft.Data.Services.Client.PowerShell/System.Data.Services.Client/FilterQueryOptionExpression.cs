namespace System.Data.Services.Client
{
    using System;
    using System.Linq.Expressions;

    internal class FilterQueryOptionExpression : QueryOptionExpression
    {
        private Expression predicate;

        internal FilterQueryOptionExpression(Type type, Expression predicate) : base(type)
        {
            this.predicate = predicate;
        }

        public override ExpressionType NodeType
        {
            get
            {
                return (ExpressionType) 0x2716;
            }
        }

        internal Expression Predicate
        {
            get
            {
                return this.predicate;
            }
        }
    }
}

