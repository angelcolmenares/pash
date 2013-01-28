namespace Microsoft.Data.OData
{
    using System;

    internal sealed class ODataMessageQuotas
    {
        private int maxEntityPropertyMappingsPerType;
        private int maxNestingDepth;
        private int maxOperationsPerChangeset;
        private int maxPartsPerBatch;
        private long maxReceivedMessageSize;

        public ODataMessageQuotas()
        {
            this.maxPartsPerBatch = 100;
            this.maxOperationsPerChangeset = 0x3e8;
            this.maxNestingDepth = 100;
            this.maxReceivedMessageSize = 0x100000L;
            this.maxEntityPropertyMappingsPerType = 100;
        }

        public ODataMessageQuotas(ODataMessageQuotas other)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataMessageQuotas>(other, "other");
            this.maxPartsPerBatch = other.maxPartsPerBatch;
            this.maxOperationsPerChangeset = other.maxOperationsPerChangeset;
            this.maxNestingDepth = other.maxNestingDepth;
            this.maxReceivedMessageSize = other.maxReceivedMessageSize;
            this.maxEntityPropertyMappingsPerType = other.maxEntityPropertyMappingsPerType;
        }

        public int MaxEntityPropertyMappingsPerType
        {
            get
            {
                return this.maxEntityPropertyMappingsPerType;
            }
            set
            {
                ExceptionUtils.CheckIntegerNotNegative(value, "MaxEntityPropertyMappingsPerType");
                this.maxEntityPropertyMappingsPerType = value;
            }
        }

        public int MaxNestingDepth
        {
            get
            {
                return this.maxNestingDepth;
            }
            set
            {
                ExceptionUtils.CheckIntegerPositive(value, "MaxNestingDepth");
                this.maxNestingDepth = value;
            }
        }

        public int MaxOperationsPerChangeset
        {
            get
            {
                return this.maxOperationsPerChangeset;
            }
            set
            {
                ExceptionUtils.CheckIntegerNotNegative(value, "MaxOperationsPerChangeset");
                this.maxOperationsPerChangeset = value;
            }
        }

        public int MaxPartsPerBatch
        {
            get
            {
                return this.maxPartsPerBatch;
            }
            set
            {
                ExceptionUtils.CheckIntegerNotNegative(value, "MaxPartsPerBatch");
                this.maxPartsPerBatch = value;
            }
        }

        public long MaxReceivedMessageSize
        {
            get
            {
                return this.maxReceivedMessageSize;
            }
            set
            {
                ExceptionUtils.CheckLongPositive(value, "MaxReceivedMessageSize");
                this.maxReceivedMessageSize = value;
            }
        }
    }
}

