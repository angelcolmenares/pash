namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class NavigationPropertySingletonExpression : ResourceExpression
    {
        private readonly Expression memberExpression;
        private readonly Type resourceType;

        internal NavigationPropertySingletonExpression(Type type, Expression source, Expression memberExpression, Type resourceType, List<string> expandPaths, CountOption countOption, Dictionary<ConstantExpression, ConstantExpression> customQueryOptions, ProjectionQueryOptionExpression projection, Type resourceTypeAs, Version uriVersion) : base(source, type, expandPaths, countOption, customQueryOptions, projection, resourceTypeAs, uriVersion)
        {
            this.memberExpression = memberExpression;
            this.resourceType = resourceType;
        }

        internal override ResourceExpression CreateCloneWithNewType(Type type)
        {
            return new NavigationPropertySingletonExpression(type, base.source, this.MemberExpression, TypeSystem.GetElementType(type), this.ExpandPaths.ToList<string>(), this.CountOption, this.CustomQueryOptions.ToDictionary<KeyValuePair<ConstantExpression, ConstantExpression>, ConstantExpression, ConstantExpression>(kvp => kvp.Key, kvp => kvp.Value), base.Projection, base.ResourceTypeAs, base.UriVersion);
        }

        internal override bool HasQueryOptions
        {
            get
            {
                if (((this.ExpandPaths.Count <= 0) && (this.CountOption != CountOption.InlineAll)) && (this.CustomQueryOptions.Count <= 0))
                {
                    return (base.Projection != null);
                }
                return true;
            }
        }

        internal override bool IsSingleton
        {
            get
            {
                return true;
            }
        }

        internal System.Linq.Expressions.MemberExpression MemberExpression
        {
            get
            {
                return (System.Linq.Expressions.MemberExpression) this.memberExpression;
            }
        }

        public override ExpressionType NodeType
        {
            get
            {
                return (ExpressionType) 0x2712;
            }
        }

        internal override Type ResourceType
        {
            get
            {
                return this.resourceType;
            }
        }
    }
}

