namespace Microsoft.Data.OData
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, DebuggerDisplay("{Message}")]
    internal sealed class ODataErrorException : ODataException
    {
        [NonSerialized]
        private ODataErrorExceptionSafeSerializationState state;

        public ODataErrorException() : this(Strings.ODataErrorException_GeneralError)
        {
        }

        public ODataErrorException(ODataError error) : this(Strings.ODataErrorException_GeneralError, null, error)
        {
        }

        public ODataErrorException(string message) : this(message, (Exception) null)
        {
        }

        public ODataErrorException(string message, ODataError error) : this(message, null, error)
        {
        }

        public ODataErrorException(string message, Exception innerException) : this(message, innerException, new ODataError())
        {
        }

        public ODataErrorException(string message, Exception innerException, ODataError error) : base(message, innerException)
        {
            EventHandler<SafeSerializationEventArgs> handler = null;
            this.state.ODataError = error;
            if (handler == null)
            {
                handler = (exception, eventArgs) => eventArgs.AddSerializedState(this.state);
            }
            base.SerializeObjectState += handler;
        }

        public ODataError Error
        {
            get
            {
                return this.state.ODataError;
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        private struct ODataErrorExceptionSafeSerializationState : ISafeSerializationData
        {
            public Microsoft.Data.OData.ODataError ODataError { get; set; }
            void ISafeSerializationData.CompleteDeserialization(object obj)
            {
                ODataErrorException exception = obj as ODataErrorException;
                exception.state = this;
            }
        }
    }
}

