namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class CmdletProviderInvocationException : CmdletInvocationException
    {
        [NonSerialized]
        private System.Management.Automation.ProviderInvocationException _providerInvocationException;

        public CmdletProviderInvocationException()
        {
        }

        public CmdletProviderInvocationException(string message) : base(message)
        {
        }

        internal CmdletProviderInvocationException(System.Management.Automation.ProviderInvocationException innerException, InvocationInfo myInvocation) : base(GetInnerException(innerException), myInvocation)
        {
            if (innerException == null)
            {
                throw new ArgumentNullException("innerException");
            }
            this._providerInvocationException = innerException;
        }

        protected CmdletProviderInvocationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._providerInvocationException = base.InnerException as System.Management.Automation.ProviderInvocationException;
        }

        public CmdletProviderInvocationException(string message, Exception innerException) : base(message, innerException)
        {
            this._providerInvocationException = innerException as System.Management.Automation.ProviderInvocationException;
        }

        private static Exception GetInnerException(Exception e)
        {
            if (e != null)
            {
                return e.InnerException;
            }
            return null;
        }

        public System.Management.Automation.ProviderInfo ProviderInfo
        {
            get
            {
                if (this._providerInvocationException != null)
                {
                    return this._providerInvocationException.ProviderInfo;
                }
                return null;
            }
        }

        public System.Management.Automation.ProviderInvocationException ProviderInvocationException
        {
            get
            {
                return this._providerInvocationException;
            }
        }
    }
}

