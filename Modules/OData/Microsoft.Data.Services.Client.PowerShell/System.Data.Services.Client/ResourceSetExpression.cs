namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
	using System.Reflection;

    [DebuggerDisplay("ResourceSetExpression {Source}.{MemberExpression}")]
    internal class ResourceSetExpression : ResourceExpression
    {
        private Dictionary<PropertyInfo, ConstantExpression> keyFilter;
        private readonly Expression member;
        private readonly Type resourceType;
        private List<QueryOptionExpression> sequenceQueryOptions;
        private TransparentAccessors transparentScope;

        internal ResourceSetExpression(Type type, Expression source, Expression memberExpression, Type resourceType, List<string> expandPaths, CountOption countOption, Dictionary<ConstantExpression, ConstantExpression> customQueryOptions, ProjectionQueryOptionExpression projection, Type resourceTypeAs, Version uriVersion) : base(source, type, expandPaths, countOption, customQueryOptions, projection, resourceTypeAs, uriVersion)
        {
            this.member = memberExpression;
            this.resourceType = resourceType;
            this.sequenceQueryOptions = new List<QueryOptionExpression>();
        }

        internal void AddSequenceQueryOption(QueryOptionExpression qoe)
        {
            QueryOptionExpression previous = (from o in this.sequenceQueryOptions
                where o.GetType() == qoe.GetType()
                select o).FirstOrDefault<QueryOptionExpression>();
            if (previous != null)
            {
                qoe = qoe.ComposeMultipleSpecification(previous);
                this.sequenceQueryOptions.Remove(previous);
            }
            this.sequenceQueryOptions.Add(qoe);
        }

        internal ResourceSetExpression CreateCloneForTransparentScope(Type type)
        {
            Type elementType = TypeSystem.GetElementType(type);
            Type newType = typeof(IOrderedQueryable<>).MakeGenericType(new Type[] { elementType });
            return this.CreateCloneWithNewTypes(newType, this.ResourceType);
        }

        internal override ResourceExpression CreateCloneWithNewType(Type type)
        {
            return this.CreateCloneWithNewTypes(type, TypeSystem.GetElementType(type));
        }

        private ResourceSetExpression CreateCloneWithNewTypes(Type newType, Type newResourceType)
        {
            return new ResourceSetExpression(newType, base.source, this.MemberExpression, newResourceType, this.ExpandPaths.ToList<string>(), this.CountOption, this.CustomQueryOptions.ToDictionary<KeyValuePair<ConstantExpression, ConstantExpression>, ConstantExpression, ConstantExpression>(kvp => kvp.Key, kvp => kvp.Value), base.Projection, base.ResourceTypeAs, base.UriVersion) { keyFilter = this.keyFilter, sequenceQueryOptions = this.sequenceQueryOptions, transparentScope = this.transparentScope };
        }

        internal void OverrideInputReference(ResourceSetExpression newInput)
        {
            InputReferenceExpression inputRef = newInput.inputRef;
            if (inputRef != null)
            {
                base.inputRef = inputRef;
                inputRef.OverrideTarget(this);
            }
        }

        internal FilterQueryOptionExpression Filter
        {
            get
            {
                return this.sequenceQueryOptions.OfType<FilterQueryOptionExpression>().SingleOrDefault<FilterQueryOptionExpression>();
            }
        }

        internal bool HasKeyPredicate
        {
            get
            {
                return (this.keyFilter != null);
            }
        }

        internal override bool HasQueryOptions
        {
            get
            {
                if (((this.sequenceQueryOptions.Count <= 0) && (this.ExpandPaths.Count <= 0)) && ((this.CountOption != CountOption.InlineAll) && (this.CustomQueryOptions.Count <= 0)))
                {
                    return (base.Projection != null);
                }
                return true;
            }
        }

        internal bool HasSequenceQueryOptions
        {
            get
            {
                return (this.sequenceQueryOptions.Count > 0);
            }
        }

        internal bool HasTransparentScope
        {
            get
            {
                return (this.transparentScope != null);
            }
        }

        internal override bool IsSingleton
        {
            get
            {
                return this.HasKeyPredicate;
            }
        }

        internal Dictionary<PropertyInfo, ConstantExpression> KeyPredicate
        {
            get
            {
                return this.keyFilter;
            }
            set
            {
                this.keyFilter = value;
            }
        }

        internal Expression MemberExpression
        {
            get
            {
                return this.member;
            }
        }

        public override ExpressionType NodeType
        {
            get
            {
                if (base.source == null)
                {
                    return (ExpressionType) 0x2710;
                }
                return (ExpressionType) 0x2711;
            }
        }

        internal OrderByQueryOptionExpression OrderBy
        {
            get
            {
                return this.sequenceQueryOptions.OfType<OrderByQueryOptionExpression>().SingleOrDefault<OrderByQueryOptionExpression>();
            }
        }

        internal override Type ResourceType
        {
            get
            {
                return this.resourceType;
            }
        }

        internal IEnumerable<QueryOptionExpression> SequenceQueryOptions
        {
            get
            {
                return this.sequenceQueryOptions.ToList<QueryOptionExpression>();
            }
        }

        internal SkipQueryOptionExpression Skip
        {
            get
            {
                return this.sequenceQueryOptions.OfType<SkipQueryOptionExpression>().SingleOrDefault<SkipQueryOptionExpression>();
            }
        }

        internal TakeQueryOptionExpression Take
        {
            get
            {
                return this.sequenceQueryOptions.OfType<TakeQueryOptionExpression>().SingleOrDefault<TakeQueryOptionExpression>();
            }
        }

        internal TransparentAccessors TransparentScope
        {
            get
            {
                return this.transparentScope;
            }
            set
            {
                this.transparentScope = value;
            }
        }

        [DebuggerDisplay("{ToString()}")]
        internal class TransparentAccessors
        {
            internal readonly string Accessor;
            internal readonly Dictionary<string, Expression> SourceAccessors;

            internal TransparentAccessors(string acc, Dictionary<string, Expression> sourceAccesors)
            {
                this.Accessor = acc;
                this.SourceAccessors = sourceAccesors;
            }

            public override string ToString()
            {
                return (("SourceAccessors=[" + string.Join(",", this.SourceAccessors.Keys.ToArray<string>())) + "] ->* Accessor=" + this.Accessor);
            }
        }
    }
}

