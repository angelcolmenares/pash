namespace Microsoft.Data.OData
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class WriterUtils
    {
        internal static bool ShouldSkipProperty(this ProjectedPropertiesAnnotation projectedProperties, string propertyName)
        {
            return ((projectedProperties != null) && !projectedProperties.IsPropertyProjected(propertyName));
        }
    }
}

