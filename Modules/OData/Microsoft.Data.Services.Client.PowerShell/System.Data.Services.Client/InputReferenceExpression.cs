namespace System.Data.Services.Client
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;

    [DebuggerDisplay("InputReferenceExpression -> {Type}")]
    internal sealed class InputReferenceExpression : Expression
    {
        private ResourceExpression target;

        internal InputReferenceExpression(ResourceExpression target)
        {
            this.target = target;
        }

        internal void OverrideTarget(ResourceSetExpression newTarget)
        {
            this.target = newTarget;
        }

        public override ExpressionType NodeType
        {
            get
            {
                return (ExpressionType) 0x2717;
            }
        }

        internal ResourceExpression Target
        {
            get
            {
                return this.target;
            }
        }

        public override System.Type Type
        {
            get
            {
                return this.target.ResourceType;
            }
        }
    }
}

