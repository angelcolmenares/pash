namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.Serialization;

    [Serializable]
    public class WriteErrorException : SystemException
    {
        public WriteErrorException() : base(StringUtil.Format(WriteErrorStrings.WriteErrorException, new object[0]))
        {
        }

        public WriteErrorException(string message) : base(message)
        {
        }

        protected WriteErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public WriteErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

