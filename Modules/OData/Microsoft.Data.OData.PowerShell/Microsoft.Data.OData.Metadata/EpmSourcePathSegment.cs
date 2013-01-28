namespace Microsoft.Data.OData.Metadata
{
    using System;
    using System.Collections.Generic;

    internal sealed class EpmSourcePathSegment
    {
        private EntityPropertyMappingInfo epmInfo;
        private readonly string propertyName;
        private readonly List<EpmSourcePathSegment> subProperties;

        internal EpmSourcePathSegment() : this(null)
        {
        }

        internal EpmSourcePathSegment(string propertyName)
        {
            this.propertyName = propertyName;
            this.subProperties = new List<EpmSourcePathSegment>();
        }

        internal EntityPropertyMappingInfo EpmInfo
        {
            get
            {
                return this.epmInfo;
            }
            set
            {
                this.epmInfo = value;
            }
        }

        internal string PropertyName
        {
            get
            {
                return this.propertyName;
            }
        }

        internal List<EpmSourcePathSegment> SubProperties
        {
            get
            {
                return this.subProperties;
            }
        }
    }
}

