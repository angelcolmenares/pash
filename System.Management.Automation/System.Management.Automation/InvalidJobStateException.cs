namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Remoting;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidJobStateException : SystemException
    {
        [NonSerialized]
        private JobState currState;

        public InvalidJobStateException() : base(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.InvalidJobStateGeneral, new object[0]))
        {
        }

        internal InvalidJobStateException(JobState currentState) : base(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.InvalidJobStateGeneral, new object[0]))
        {
            this.currState = currentState;
        }

        public InvalidJobStateException(string message) : base(message)
        {
        }

        public InvalidJobStateException(JobState currentState, string actionMessage) : base(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.InvalidJobStateSpecific, new object[] { currentState, actionMessage }))
        {
            this.currState = currentState;
        }

        protected InvalidJobStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidJobStateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public JobState CurrentState
        {
            get
            {
                return this.currState;
            }
        }
    }
}

