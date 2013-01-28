namespace System.Data.Services.Client
{
    using System;
    using System.Linq.Expressions;

    internal abstract class QueryOptionExpression : Expression
    {
        private System.Type type;

        internal QueryOptionExpression(System.Type type)
        {
            this.type = type;
        }

        internal virtual QueryOptionExpression ComposeMultipleSpecification(QueryOptionExpression previous)
        {
            return this;
        }

        public override System.Type Type
        {
            get
            {
                return this.type;
            }
        }
    }
}

