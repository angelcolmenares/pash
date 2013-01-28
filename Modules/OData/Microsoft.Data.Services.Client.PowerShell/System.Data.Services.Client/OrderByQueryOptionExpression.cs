namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;

    internal class OrderByQueryOptionExpression : QueryOptionExpression
    {
        private List<Selector> selectors;

        internal OrderByQueryOptionExpression(Type type, List<Selector> selectors) : base(type)
        {
            this.selectors = selectors;
        }

        public override ExpressionType NodeType
        {
            get
            {
                return (ExpressionType) 0x2715;
            }
        }

        internal List<Selector> Selectors
        {
            get
            {
                return this.selectors;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Selector
        {
            internal readonly System.Linq.Expressions.Expression Expression;
            internal readonly bool Descending;
            internal Selector(System.Linq.Expressions.Expression e, bool descending)
            {
                this.Expression = e;
                this.Descending = descending;
            }
        }
    }
}

