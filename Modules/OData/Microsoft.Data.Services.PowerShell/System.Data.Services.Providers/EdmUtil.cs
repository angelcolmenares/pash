namespace System.Data.Services.Providers
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;

    internal static class EdmUtil
    {
        internal static MetadataProperty FindExtendedProperty(MetadataItem metadataItem, string propertyName)
        {
            MetadataProperty property;
            string identity = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata:" + propertyName;
            if (metadataItem.MetadataProperties.TryGetValue(identity, false, out property))
            {
                return property;
            }
            return null;
        }
    }
}

