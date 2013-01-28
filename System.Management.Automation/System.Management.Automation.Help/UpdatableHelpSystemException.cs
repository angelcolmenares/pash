namespace System.Management.Automation.Help
{
    using System;
    using System.Management.Automation;
    using System.Runtime.Serialization;

    [Serializable]
    internal class UpdatableHelpSystemException : Exception
    {
        private System.Management.Automation.ErrorCategory _cat;
        private string _errorId;
        private object _targetObject;

        protected UpdatableHelpSystemException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        internal UpdatableHelpSystemException(string errorId, string message, System.Management.Automation.ErrorCategory cat, object targetObject, Exception innerException) : base(message, innerException)
        {
            this._errorId = errorId;
            this._cat = cat;
            this._targetObject = targetObject;
        }

        internal System.Management.Automation.ErrorCategory ErrorCategory
        {
            get
            {
                return this._cat;
            }
        }

        internal string FullyQualifiedErrorId
        {
            get
            {
                return this._errorId;
            }
        }

        internal object TargetObject
        {
            get
            {
                return this._targetObject;
            }
        }
    }
}

