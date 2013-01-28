namespace System.Management.Automation
{
    using System;

    internal enum ParameterBinderAssociation
    {
        DeclaredFormalParameters,
        DynamicParameters,
        CommonParameters,
        ShouldProcessParameters,
        TransactionParameters,
        PagingParameters
    }
}

