namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidRunspaceStateException : SystemException
    {
        [NonSerialized]
        private RunspaceState _currentState;
        [NonSerialized]
        private RunspaceState _expectedState;

        public InvalidRunspaceStateException() : base(StringUtil.Format(RunspaceStrings.InvalidRunspaceStateGeneral, new object[0]))
        {
        }

        public InvalidRunspaceStateException(string message) : base(message)
        {
        }

        protected InvalidRunspaceStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidRunspaceStateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal InvalidRunspaceStateException(string message, RunspaceState currentState, RunspaceState expectedState) : base(message)
        {
            this._expectedState = expectedState;
            this._currentState = currentState;
        }

        public RunspaceState CurrentState
        {
            get
            {
                return this._currentState;
            }
            internal set
            {
                this._currentState = value;
            }
        }

        public RunspaceState ExpectedState
        {
            get
            {
                return this._expectedState;
            }
            internal set
            {
                this._expectedState = value;
            }
        }
    }
}

