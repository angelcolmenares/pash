namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal sealed class OrderingInfo
    {
        private readonly List<OrderingExpression> orderingExpressions;
        private readonly bool paged;

        internal OrderingInfo(bool paged)
        {
            this.paged = paged;
            this.orderingExpressions = new List<OrderingExpression>();
        }

        internal void Add(OrderingExpression orderingExpression)
        {
            this.orderingExpressions.Add(orderingExpression);
        }

        public bool IsPaged
        {
            get
            {
                return this.paged;
            }
        }

        public ReadOnlyCollection<OrderingExpression> OrderingExpressions
        {
            get
            {
                return this.orderingExpressions.AsReadOnly();
            }
        }
    }
}

