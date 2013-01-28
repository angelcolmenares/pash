namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class ProjectionQueryOptionExpression : QueryOptionExpression
    {
        private readonly LambdaExpression lambda;
        private readonly List<string> paths;

        internal ProjectionQueryOptionExpression(Type type, LambdaExpression lambda, List<string> paths) : base(type)
        {
            this.lambda = lambda;
            this.paths = paths;
        }

        public override ExpressionType NodeType
        {
            get
            {
                return (ExpressionType) 0x2718;
            }
        }

        internal List<string> Paths
        {
            get
            {
                return this.paths;
            }
        }

        internal LambdaExpression Selector
        {
            get
            {
                return this.lambda;
            }
        }
    }
}

