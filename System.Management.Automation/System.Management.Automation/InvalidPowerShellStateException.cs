namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidPowerShellStateException : SystemException
    {
        [NonSerialized]
        private PSInvocationState currState;

        public InvalidPowerShellStateException() : base(StringUtil.Format(PowerShellStrings.InvalidPowerShellStateGeneral, new object[0]))
        {
        }

        internal InvalidPowerShellStateException(PSInvocationState currentState) : base(StringUtil.Format(PowerShellStrings.InvalidPowerShellStateGeneral, new object[0]))
        {
            this.currState = currentState;
        }

        public InvalidPowerShellStateException(string message) : base(message)
        {
        }

        protected InvalidPowerShellStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidPowerShellStateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PSInvocationState CurrentState
        {
            get
            {
                return this.currState;
            }
        }
    }
}

