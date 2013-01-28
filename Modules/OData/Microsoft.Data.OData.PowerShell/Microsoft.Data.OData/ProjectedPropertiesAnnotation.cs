namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;

    internal sealed class ProjectedPropertiesAnnotation
    {
        private static readonly ProjectedPropertiesAnnotation emptyProjectedPropertiesMarker = new ProjectedPropertiesAnnotation(new string[0]);
        private readonly HashSet<string> projectedProperties;

        public ProjectedPropertiesAnnotation(IEnumerable<string> projectedPropertyNames)
        {
            ExceptionUtils.CheckArgumentNotNull<IEnumerable<string>>(projectedPropertyNames, "projectedPropertyNames");
            this.projectedProperties = new HashSet<string>(projectedPropertyNames, StringComparer.Ordinal);
        }

        internal bool IsPropertyProjected(string propertyName)
        {
            return this.projectedProperties.Contains(propertyName);
        }

        internal static ProjectedPropertiesAnnotation EmptyProjectedPropertiesMarker
        {
            get
            {
                return emptyProjectedPropertiesMarker;
            }
        }
    }
}

