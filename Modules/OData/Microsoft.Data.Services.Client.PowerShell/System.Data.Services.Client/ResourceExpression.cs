namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal abstract class ResourceExpression : Expression
    {
        private System.Data.Services.Client.CountOption countOption;
        private Dictionary<ConstantExpression, ConstantExpression> customQueryOptions;
        private List<string> expandPaths;
        protected InputReferenceExpression inputRef;
        private ProjectionQueryOptionExpression projection;
        protected readonly Expression source;
        private System.Type type;
        private Version uriVersion;

        internal ResourceExpression(Expression source, System.Type type, List<string> expandPaths, System.Data.Services.Client.CountOption countOption, Dictionary<ConstantExpression, ConstantExpression> customQueryOptions, ProjectionQueryOptionExpression projection, System.Type resourceTypeAs, Version uriVersion)
        {
            this.source = source;
            this.type = type;
            this.expandPaths = expandPaths ?? new List<string>();
            this.countOption = countOption;
            this.customQueryOptions = customQueryOptions ?? new Dictionary<ConstantExpression, ConstantExpression>(ReferenceEqualityComparer<ConstantExpression>.Instance);
            this.projection = projection;
            this.ResourceTypeAs = resourceTypeAs;
            this.uriVersion = uriVersion ?? Util.DataServiceVersion1;
        }

        internal abstract ResourceExpression CreateCloneWithNewType(System.Type type);
        internal InputReferenceExpression CreateReference()
        {
            if (this.inputRef == null)
            {
                this.inputRef = new InputReferenceExpression(this);
            }
            return this.inputRef;
        }

        internal void RaiseUriVersion(Version newVersion)
        {
            WebUtil.RaiseVersion(ref this.uriVersion, newVersion);
        }

        internal virtual System.Data.Services.Client.CountOption CountOption
        {
            get
            {
                return this.countOption;
            }
            set
            {
                this.countOption = value;
            }
        }

        internal virtual Dictionary<ConstantExpression, ConstantExpression> CustomQueryOptions
        {
            get
            {
                return this.customQueryOptions;
            }
            set
            {
                this.customQueryOptions = value;
            }
        }

        internal virtual List<string> ExpandPaths
        {
            get
            {
                return this.expandPaths;
            }
            set
            {
                this.expandPaths = value;
            }
        }

        internal abstract bool HasQueryOptions { get; }

        internal abstract bool IsSingleton { get; }

        internal ProjectionQueryOptionExpression Projection
        {
            get
            {
                return this.projection;
            }
            set
            {
                this.projection = value;
            }
        }

        internal abstract System.Type ResourceType { get; }

        internal System.Type ResourceTypeAs { get; set; }

        internal Expression Source
        {
            get
            {
                return this.source;
            }
        }

        public override System.Type Type
        {
            get
            {
                return this.type;
            }
        }

        internal Version UriVersion
        {
            get
            {
                return this.uriVersion;
            }
        }
    }
}

