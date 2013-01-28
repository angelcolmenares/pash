namespace System.Management.Automation
{
    using System;

    internal class CallsiteCacheEntry : IEquatable<CallsiteCacheEntry>
    {
        private string methodName;
        private CallsiteSignature signature;

        internal CallsiteCacheEntry(string methodName, CallsiteSignature signature)
        {
            this.methodName = methodName;
            this.signature = signature;
        }

        public bool Equals(CallsiteCacheEntry other)
        {
            if (!this.methodName.Equals(other.methodName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return this.signature.Equals(other.signature);
        }

        public override bool Equals(object other)
        {
            CallsiteCacheEntry entry = other as CallsiteCacheEntry;
            return ((entry != null) && this.Equals(entry));
        }

        public override int GetHashCode()
        {
            return (this.methodName.ToLowerInvariant().GetHashCode() ^ this.signature.GetHashCode());
        }
    }
}

