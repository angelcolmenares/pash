namespace Microsoft.Data.OData.Metadata
{
    using System;
    using System.Collections.Generic;

    internal sealed class CachedPrimitiveKeepInContentAnnotation
    {
        private readonly HashSet<string> keptInContentPropertyNames;

        internal CachedPrimitiveKeepInContentAnnotation(IEnumerable<string> keptInContentPropertyNames)
        {
            this.keptInContentPropertyNames = (keptInContentPropertyNames == null) ? null : new HashSet<string>(keptInContentPropertyNames, StringComparer.Ordinal);
        }

        internal bool IsKeptInContent(string propertyName)
        {
            return ((this.keptInContentPropertyNames != null) && this.keptInContentPropertyNames.Contains(propertyName));
        }
    }
}

