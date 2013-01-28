namespace System.Data.Services.Providers
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;

    internal class ObjectContextMetadata : IProviderMetadata
    {
        private readonly MetadataWorkspace metadataWorkspace;

        public ObjectContextMetadata(MetadataWorkspace metadataWorkspace)
        {
            this.metadataWorkspace = metadataWorkspace;
        }

        public Type GetClrType(StructuralType structuralType)
        {
            return ObjectContextServiceProvider.GetClrTypeForCSpaceType(this.metadataWorkspace, structuralType);
        }

        public IProviderType GetProviderType(string providerTypeName)
        {
            return new ObjectContextType(this.metadataWorkspace.GetItem<StructuralType>(providerTypeName, DataSpace.CSpace));
        }
    }
}

