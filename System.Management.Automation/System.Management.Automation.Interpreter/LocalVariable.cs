namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal sealed class LocalVariable
    {
        private int _flags;
        private const int InClosureFlag = 2;
        public readonly int Index;
        private const int IsBoxedFlag = 1;

        internal LocalVariable(int index, bool closure, bool boxed)
        {
            this.Index = index;
            this._flags = (closure ? 2 : 0) | (boxed ? 1 : 0);
        }

        internal Expression LoadFromArray(Expression frameData, Expression closure)
        {
            Expression expression = Expression.ArrayAccess(this.InClosure ? closure : frameData, new Expression[] { Expression.Constant(this.Index) });
            if (!this.IsBoxed)
            {
                return expression;
            }
            return Expression.Convert(expression, typeof(StrongBox<object>));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}: {1} {2}", new object[] { this.Index, this.IsBoxed ? "boxed" : null, this.InClosure ? "in closure" : null });
        }

        public bool InClosure
        {
            get
            {
                return ((this._flags & 2) != 0);
            }
        }

        public bool InClosureOrBoxed
        {
            get
            {
                return (this.InClosure | this.IsBoxed);
            }
        }

        public bool IsBoxed
        {
            get
            {
                return ((this._flags & 1) != 0);
            }
            set
            {
                if (value)
                {
                    this._flags |= 1;
                }
                else
                {
                    this._flags &= -2;
                }
            }
        }
    }
}

