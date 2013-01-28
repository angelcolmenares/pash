namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Xml;

    internal class DeserializationContext
    {
        internal readonly CimClassDeserializationCache<CimClassSerializationId> cimClassSerializationIdCache;
        internal readonly PSRemotingCryptoHelper cryptoHelper;
        private int? maxAllowedMemory;
        internal static int MaxItemsInCimClassCache = 100;
        internal readonly DeserializationOptions options;
        private int totalDataProcessedSoFar;

        internal DeserializationContext() : this(DeserializationOptions.None, null)
        {
        }

        internal DeserializationContext(DeserializationOptions options, PSRemotingCryptoHelper cryptoHelper)
        {
            this.cimClassSerializationIdCache = new CimClassDeserializationCache<CimClassSerializationId>();
            this.options = options;
            this.cryptoHelper = cryptoHelper;
        }

        internal void LogExtraMemoryUsage(int amountOfExtraMemory)
        {
            if ((amountOfExtraMemory >= 0) && this.maxAllowedMemory.HasValue)
            {
                if (amountOfExtraMemory > (this.maxAllowedMemory.Value - this.totalDataProcessedSoFar))
                {
                    throw new XmlException(StringUtil.Format(Serialization.DeserializationMemoryQuota, new object[] { ((double) this.maxAllowedMemory.Value) / 1048576.0, "PSMaximumReceivedObjectSizeMB", "PSMaximumReceivedDataSizePerCommandMB" }));
                }
                this.totalDataProcessedSoFar += amountOfExtraMemory;
            }
        }

        internal int? MaximumAllowedMemory
        {
            get
            {
                return this.maxAllowedMemory;
            }
            set
            {
                this.maxAllowedMemory = value;
            }
        }
    }
}

