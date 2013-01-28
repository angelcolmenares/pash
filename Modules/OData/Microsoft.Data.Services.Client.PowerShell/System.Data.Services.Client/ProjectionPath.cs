namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Text;

    [DebuggerDisplay("{ToString()}")]
    internal class ProjectionPath : List<ProjectionPathSegment>
    {
        internal ProjectionPath()
        {
        }

        internal ProjectionPath(ParameterExpression root, Expression expectedRootType, Expression rootEntry)
        {
            this.Root = root;
            this.RootEntry = rootEntry;
            this.ExpectedRootType = expectedRootType;
        }

        internal ProjectionPath(ParameterExpression root, Expression expectedRootType, Expression rootEntry, IEnumerable<Expression> members) : this(root, expectedRootType, rootEntry)
        {
            foreach (Expression expression in members)
            {
                base.Add(new ProjectionPathSegment(this, (MemberExpression) expression));
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.Root.ToString());
            builder.Append("->");
            for (int i = 0; i < base.Count; i++)
            {
                if (base[i].SourceTypeAs != null)
                {
                    builder.Insert(0, "(");
                    builder.Append(" as " + base[i].SourceTypeAs.Name + ")");
                }
                if (i > 0)
                {
                    builder.Append('.');
                }
                builder.Append((base[i].Member == null) ? "*" : base[i].Member);
            }
            return builder.ToString();
        }

        internal Expression ExpectedRootType { get; private set; }

        internal ParameterExpression Root { get; private set; }

        internal Expression RootEntry { get; private set; }
    }
}

