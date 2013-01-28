namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [Serializable]
    public class ProviderNameAmbiguousException : ProviderNotFoundException
    {
        private ReadOnlyCollection<ProviderInfo> _possibleMatches;

        public ProviderNameAmbiguousException()
        {
        }

        public ProviderNameAmbiguousException(string message) : base(message)
        {
        }

        protected ProviderNameAmbiguousException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ProviderNameAmbiguousException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal ProviderNameAmbiguousException(string providerName, string errorIdAndResourceId, string resourceStr, Collection<ProviderInfo> possibleMatches, params object[] messageArgs) : base(providerName, SessionStateCategory.CmdletProvider, errorIdAndResourceId, resourceStr, messageArgs)
        {
            this._possibleMatches = new ReadOnlyCollection<ProviderInfo>(possibleMatches);
        }

        public ReadOnlyCollection<ProviderInfo> PossibleMatches
        {
            get
            {
                return this._possibleMatches;
            }
        }
    }
}

