namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidPipelineStateException : SystemException
    {
        [NonSerialized]
        private PipelineState _currentState;
        [NonSerialized]
        private PipelineState _expectedState;

        public InvalidPipelineStateException() : base(StringUtil.Format(RunspaceStrings.InvalidPipelineStateStateGeneral, new object[0]))
        {
        }

        public InvalidPipelineStateException(string message) : base(message)
        {
        }

        private InvalidPipelineStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidPipelineStateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal InvalidPipelineStateException(string message, PipelineState currentState, PipelineState expectedState) : base(message)
        {
            this._expectedState = expectedState;
            this._currentState = currentState;
        }

        public PipelineState CurrentState
        {
            get
            {
                return this._currentState;
            }
        }

        public PipelineState ExpectedState
        {
            get
            {
                return this._expectedState;
            }
        }
    }
}

