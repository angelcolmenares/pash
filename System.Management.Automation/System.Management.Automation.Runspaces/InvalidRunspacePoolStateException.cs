namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidRunspacePoolStateException : SystemException
    {
        [NonSerialized]
        private RunspacePoolState currentState;
        [NonSerialized]
        private RunspacePoolState expectedState;

        public InvalidRunspacePoolStateException() : base(StringUtil.Format(RunspacePoolStrings.InvalidRunspacePoolStateGeneral, new object[0]))
        {
        }

        public InvalidRunspacePoolStateException(string message) : base(message)
        {
        }

        protected InvalidRunspacePoolStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidRunspacePoolStateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal InvalidRunspacePoolStateException(string message, RunspacePoolState currentState, RunspacePoolState expectedState) : base(message)
        {
            this.expectedState = expectedState;
            this.currentState = currentState;
        }

        private static RunspaceState RunspacePoolStateToRunspaceState(RunspacePoolState state)
        {
            switch (state)
            {
                case RunspacePoolState.BeforeOpen:
                    return RunspaceState.BeforeOpen;

                case RunspacePoolState.Opening:
                    return RunspaceState.Opening;

                case RunspacePoolState.Opened:
                    return RunspaceState.Opened;

                case RunspacePoolState.Closed:
                    return RunspaceState.Closed;

                case RunspacePoolState.Closing:
                    return RunspaceState.Closing;

                case RunspacePoolState.Broken:
                    return RunspaceState.Broken;

                case RunspacePoolState.Disconnecting:
                    return RunspaceState.Disconnecting;

                case RunspacePoolState.Disconnected:
                    return RunspaceState.Disconnected;

                case RunspacePoolState.Connecting:
                    return RunspaceState.Connecting;
            }
            return RunspaceState.BeforeOpen;
        }

        internal InvalidRunspaceStateException ToInvalidRunspaceStateException()
        {
            return new InvalidRunspaceStateException(RunspaceStrings.InvalidRunspaceStateGeneral, this) { CurrentState = RunspacePoolStateToRunspaceState(this.CurrentState), ExpectedState = RunspacePoolStateToRunspaceState(this.ExpectedState) };
        }

        public RunspacePoolState CurrentState
        {
            get
            {
                return this.currentState;
            }
        }

        public RunspacePoolState ExpectedState
        {
            get
            {
                return this.expectedState;
            }
        }
    }
}

