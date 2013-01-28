namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Providers;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;

    [DebuggerDisplay("ExpandSegment ({name},Filter={filter})]")]
    internal class ExpandSegment
    {
        private readonly ResourceSetWrapper container;
        private readonly ResourceProperty expandedProperty;
        private readonly Expression filter;
        private readonly int maxResultsExpected;
        private readonly string name;
        private readonly System.Data.Services.Providers.OrderingInfo orderingInfo;
        private readonly ResourceType targetResourceType;

        public ExpandSegment(string name, Expression filter) : this(name, filter, 0x7fffffff, null, null, null, null)
        {
        }

        internal ExpandSegment(string name, Expression filter, int maxResultsExpected, ResourceSetWrapper container, ResourceType targetResourceType, ResourceProperty expandedProperty, System.Data.Services.Providers.OrderingInfo orderingInfo)
        {
            WebUtil.CheckArgumentNull<string>(name, "name");
            CheckFilterType(filter);
            this.name = name;
            this.filter = filter;
            this.container = container;
            this.maxResultsExpected = maxResultsExpected;
            this.expandedProperty = expandedProperty;
            this.orderingInfo = orderingInfo;
            this.targetResourceType = targetResourceType;
        }

        private static void CheckFilterType(Expression filter)
        {
            if (filter != null)
            {
                if (filter.NodeType != ExpressionType.Lambda)
                {
                    throw new ArgumentException(System.Data.Services.Strings.ExpandSegment_FilterShouldBeLambda(filter.NodeType), "filter");
                }
                LambdaExpression expression = (LambdaExpression) filter;
                if ((expression.Body.Type != typeof(bool)) && (expression.Body.Type != typeof(bool?)))
                {
                    throw new ArgumentException(System.Data.Services.Strings.ExpandSegment_FilterBodyShouldReturnBool(expression.Body.Type), "filter");
                }
                if (expression.Parameters.Count != 1)
                {
                    throw new ArgumentException(System.Data.Services.Strings.ExpandSegment_FilterBodyShouldTakeOneParameter(expression.Parameters.Count), "filter");
                }
            }
        }

        public static bool PathHasFilter(IEnumerable<ExpandSegment> path)
        {
            WebUtil.CheckArgumentNull<IEnumerable<ExpandSegment>>(path, "path");
            return path.Any<ExpandSegment>(segment => segment.HasFilter);
        }

        internal ResourceSetWrapper Container
        {
            get
            {
                return this.container;
            }
        }

        public ResourceProperty ExpandedProperty
        {
            get
            {
                return this.expandedProperty;
            }
        }

        public Expression Filter
        {
            get
            {
                return this.filter;
            }
        }

        public bool HasFilter
        {
            get
            {
                return (this.Filter != null);
            }
        }

        public int MaxResultsExpected
        {
            get
            {
                return this.maxResultsExpected;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        internal System.Data.Services.Providers.OrderingInfo OrderingInfo
        {
            get
            {
                return this.orderingInfo;
            }
        }

        internal ResourceType TargetResourceType
        {
            get
            {
                return this.targetResourceType;
            }
        }
    }
}

