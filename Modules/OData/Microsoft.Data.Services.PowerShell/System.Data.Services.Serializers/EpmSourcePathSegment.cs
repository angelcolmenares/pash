namespace System.Data.Services.Serializers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class EpmSourcePathSegment
    {
        private readonly string propertyName;
        private readonly List<EpmSourcePathSegment> subProperties;

        internal EpmSourcePathSegment()
        {
            this.propertyName = null;
            this.subProperties = new List<EpmSourcePathSegment>();
        }

        internal EpmSourcePathSegment(string propertyName)
        {
            this.propertyName = propertyName;
            this.subProperties = new List<EpmSourcePathSegment>();
        }

        internal EntityPropertyMappingInfo EpmInfo { get; set; }

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

