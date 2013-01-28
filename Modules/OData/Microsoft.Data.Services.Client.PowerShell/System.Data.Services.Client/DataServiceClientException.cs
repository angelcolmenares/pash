namespace System.Data.Services.Client
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, DebuggerDisplay("{Message}")]
    internal sealed class DataServiceClientException : InvalidOperationException
    {
        [NonSerialized]
        private DataServiceClientExceptionSerializationState state;

        public DataServiceClientException() : this(Strings.DataServiceException_GeneralError)
        {
        }

        public DataServiceClientException(string message) : this(message, (Exception) null)
        {
        }

        public DataServiceClientException(string message, Exception innerException) : this(message, innerException, 500)
        {
        }

        public DataServiceClientException(string message, int statusCode) : this(message, null, statusCode)
        {
        }

        public DataServiceClientException(string message, Exception innerException, int statusCode) : base(message, innerException)
        {
            EventHandler<SafeSerializationEventArgs> handler = null;
            this.state.StatusCode = statusCode;
            if (handler == null)
            {
                handler = (sender, e) => e.AddSerializedState(this.state);
            }
            base.SerializeObjectState += handler;
        }

        public int StatusCode
        {
            get
            {
                return this.state.StatusCode;
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        private struct DataServiceClientExceptionSerializationState : ISafeSerializationData
        {
            public int StatusCode { get; set; }
            void ISafeSerializationData.CompleteDeserialization(object deserialized)
            {
                DataServiceClientException exception = (DataServiceClientException) deserialized;
                exception.state = this;
            }
        }
    }
}

