namespace System.Data.Services.Providers
{
    using System;
    using System.Linq.Expressions;

    internal sealed class OrderingExpression
    {
        private readonly bool isAscending;
        private readonly System.Linq.Expressions.Expression orderingExpression;

        public OrderingExpression(System.Linq.Expressions.Expression orderingExpression, bool isAscending)
        {
            this.orderingExpression = orderingExpression;
            this.isAscending = isAscending;
        }

        public System.Linq.Expressions.Expression Expression
        {
            get
            {
                return this.orderingExpression;
            }
        }

        public bool IsAscending
        {
            get
            {
                return this.isAscending;
            }
        }
    }
}

