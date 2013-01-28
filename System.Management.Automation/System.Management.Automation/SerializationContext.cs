namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;

    internal class SerializationContext
    {
        internal readonly CimClassSerializationCache<CimClassSerializationId> cimClassSerializationIdCache;
        internal readonly PSRemotingCryptoHelper cryptoHelper;
        private const int DefaultSerializationDepth = 2;
        internal readonly int depth;
        internal readonly SerializationOptions options;

        internal SerializationContext() : this(2, true)
        {
        }

        internal SerializationContext(int depth, bool useDepthFromTypes) : this(depth, (useDepthFromTypes ? SerializationOptions.UseDepthFromTypes : SerializationOptions.None) | SerializationOptions.PreserveSerializationSettingOfOriginal, null)
        {
        }

        internal SerializationContext(int depth, SerializationOptions options, PSRemotingCryptoHelper cryptoHelper)
        {
            this.cimClassSerializationIdCache = new CimClassSerializationCache<CimClassSerializationId>();
            if (depth < 1)
            {
                throw PSTraceSource.NewArgumentException("writer", "serialization", "DepthOfOneRequired", new object[0]);
            }
            this.depth = depth;
            this.options = options;
            this.cryptoHelper = cryptoHelper;
        }
    }
}

