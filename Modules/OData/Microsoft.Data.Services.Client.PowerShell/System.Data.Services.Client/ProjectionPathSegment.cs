namespace System.Data.Services.Client
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("Segment {ProjectionType} {Member}")]
    internal class ProjectionPathSegment
    {
        internal ProjectionPathSegment(ProjectionPath startPath, MemberExpression memberExpression)
        {
            this.StartPath = startPath;
            Expression expression = ResourceBinder.StripTo<Expression>(memberExpression.Expression);
            this.Member = memberExpression.Member.Name;
            this.ProjectionType = memberExpression.Type;
            this.SourceTypeAs = (expression.NodeType == ExpressionType.TypeAs) ? expression.Type : null;
        }

        internal ProjectionPathSegment(ProjectionPath startPath, string member, Type projectionType)
        {
            this.Member = member;
            this.StartPath = startPath;
            this.ProjectionType = projectionType;
        }

        internal string Member { get; private set; }

        internal Type ProjectionType { get; set; }

        internal Type SourceTypeAs { get; set; }

        internal ProjectionPath StartPath { get; private set; }
    }
}

