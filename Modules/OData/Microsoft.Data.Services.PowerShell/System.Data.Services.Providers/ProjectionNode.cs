namespace System.Data.Services.Providers
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("ProjectionNode {PropertyName}")]
    internal class ProjectionNode
    {
        private readonly ResourceProperty property;
        private readonly string propertyName;
        private ResourceType targetResourceType;

        internal ProjectionNode(string propertyName, ResourceProperty property, ResourceType targetResourceType)
        {
            this.propertyName = propertyName;
            this.property = property;
            this.targetResourceType = targetResourceType;
        }

        public ResourceProperty Property
        {
            get
            {
                return this.property;
            }
        }

        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }
        }

        public ResourceType TargetResourceType
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
    }
}

