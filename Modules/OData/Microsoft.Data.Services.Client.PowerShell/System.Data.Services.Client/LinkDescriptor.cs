namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("State = {state}")]
    internal sealed class LinkDescriptor : Descriptor
    {
        internal static readonly IEqualityComparer<LinkDescriptor> EquivalenceComparer = new Equivalent();
        private object source;
        private string sourceProperty;
        private object target;

        internal LinkDescriptor(object source, string sourceProperty, object target, EntityStates state) : base(state)
        {
            this.source = source;
            this.sourceProperty = sourceProperty;
            this.target = target;
        }

        internal LinkDescriptor(object source, string sourceProperty, object target, DataServiceProtocolVersion maxProtocolVersion) : this(source, sourceProperty, target, EntityStates.Unchanged)
        {
            ClientEdmModel model = ClientEdmModel.GetModel(maxProtocolVersion);
            this.IsSourcePropertyCollection = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(source.GetType())).GetProperty(sourceProperty, false).IsEntityCollection;
        }

        internal override void ClearChanges()
        {
        }

        internal bool IsEquivalent(object src, string srcPropName, object targ)
        {
            return (((this.source == src) && (this.target == targ)) && (this.sourceProperty == srcPropName));
        }

        internal override System.Data.Services.Client.DescriptorKind DescriptorKind
        {
            get
            {
                return System.Data.Services.Client.DescriptorKind.Link;
            }
        }

        internal bool IsSourcePropertyCollection { get; private set; }

        public object Source
        {
            get
            {
                return this.source;
            }
        }

        public string SourceProperty
        {
            get
            {
                return this.sourceProperty;
            }
        }

        public object Target
        {
            get
            {
                return this.target;
            }
        }

        private sealed class Equivalent : IEqualityComparer<LinkDescriptor>
        {
            public bool Equals(LinkDescriptor x, LinkDescriptor y)
            {
                return (((x != null) && (y != null)) && x.IsEquivalent(y.source, y.sourceProperty, y.target));
            }

            public int GetHashCode(LinkDescriptor obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                return ((obj.Source.GetHashCode() ^ ((obj.Target != null) ? obj.Target.GetHashCode() : 0)) ^ obj.SourceProperty.GetHashCode());
            }
        }
    }
}

