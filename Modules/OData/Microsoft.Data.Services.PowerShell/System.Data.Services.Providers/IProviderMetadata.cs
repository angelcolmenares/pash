namespace System.Data.Services.Providers
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;

    internal interface IProviderMetadata
    {
        Type GetClrType(StructuralType structuralType);
        IProviderType GetProviderType(string providerTypeName);
    }
}

