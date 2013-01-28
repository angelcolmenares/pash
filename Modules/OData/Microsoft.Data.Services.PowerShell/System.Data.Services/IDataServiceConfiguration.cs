namespace System.Data.Services
{
    using System;

    internal interface IDataServiceConfiguration
    {
        void RegisterKnownType(Type type);
        void SetEntitySetAccessRule(string name, EntitySetRights rights);
        void SetServiceOperationAccessRule(string name, ServiceOperationRights rights);

        bool DisableValidationOnMetadataWrite { get; set; }

        int MaxBatchCount { get; set; }

        int MaxChangesetCount { get; set; }

        int MaxExpandCount { get; set; }

        int MaxExpandDepth { get; set; }

        int MaxObjectCountOnInsert { get; set; }

        int MaxResultsPerCollection { get; set; }

        bool UseVerboseErrors { get; set; }
    }
}

