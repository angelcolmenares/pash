namespace System.Data.Services
{
    using System;
    using System.Collections;
    using System.Data.Services.Providers;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("SegmentInfo={Identifier} -> {TargetKind} '{TargetResourceType.InstanceType}'")]
    internal class SegmentInfo
    {
        private string identifier;
        private KeyInstance key;
        private OperationWrapper operation;
        private ResourceProperty projectedProperty;
        private bool singleResult;
        private ResourceSetWrapper targetContainer;
        private RequestTargetKind targetKind;
        private ResourceType targetResourceType;
        private RequestTargetSource targetSource;

        internal SegmentInfo()
        {
        }

        internal SegmentInfo(SegmentInfo other)
        {
            this.Identifier = other.Identifier;
            this.Key = other.Key;
            this.Operation = other.Operation;
            this.ProjectedProperty = other.ProjectedProperty;
            this.RequestExpression = other.RequestExpression;
            this.RequestEnumerable = other.RequestEnumerable;
            this.SingleResult = other.SingleResult;
            this.TargetContainer = other.TargetContainer;
            this.TargetKind = other.TargetKind;
            this.TargetSource = other.TargetSource;
            this.targetResourceType = other.targetResourceType;
        }

        internal bool HasKeyValues
        {
            get
            {
                return ((this.Key != null) && !this.Key.IsEmpty);
            }
        }

        internal string Identifier
        {
            get
            {
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }

        internal bool IsDirectReference
        {
            get
            {
                if ((this.TargetKind != RequestTargetKind.PrimitiveValue) && (this.TargetKind != RequestTargetKind.OpenPropertyValue))
                {
                    return this.HasKeyValues;
                }
                return true;
            }
        }

        internal bool IsTypeIdentifierSegment { get; set; }

        internal KeyInstance Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = value;
            }
        }

        internal OperationWrapper Operation
        {
            get
            {
                return this.operation;
            }
            set
            {
                this.operation = value;
            }
        }

        internal ResourceProperty ProjectedProperty
        {
            get
            {
                return this.projectedProperty;
            }
            set
            {
                this.projectedProperty = value;
            }
        }

        internal IEnumerable RequestEnumerable { get; set; }

        internal Expression RequestExpression { get; set; }

        internal bool SingleResult
        {
            get
            {
                return this.singleResult;
            }
            set
            {
                this.singleResult = value;
            }
        }

        internal ResourceSetWrapper TargetContainer
        {
            get
            {
                return this.targetContainer;
            }
            set
            {
                this.targetContainer = value;
            }
        }

        internal RequestTargetKind TargetKind
        {
            get
            {
                return this.targetKind;
            }
            set
            {
                this.targetKind = value;
            }
        }

        internal ResourceType TargetResourceType
        {
            get
            {
                return this.targetResourceType;
            }
            set
            {
                this.targetResourceType = value;
            }
        }

        internal RequestTargetSource TargetSource
        {
            get
            {
                return this.targetSource;
            }
            set
            {
                this.targetSource = value;
            }
        }
    }
}

