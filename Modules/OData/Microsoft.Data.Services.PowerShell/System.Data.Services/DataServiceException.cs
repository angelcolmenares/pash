namespace System.Data.Services
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, DebuggerDisplay("{statusCode}: {Message}")]
    internal sealed class DataServiceException : InvalidOperationException
    {
        [NonSerialized]
        private DataServiceExceptionSerializationState state;

        public DataServiceException() : this(500, Strings.DataServiceException_GeneralError)
        {
        }

        public DataServiceException(string message) : this(500, message)
        {
        }

        public DataServiceException(int statusCode, string message) : this(statusCode, null, message, null, null)
        {
        }

        public DataServiceException(string message, Exception innerException) : this(500, null, message, null, innerException)
        {
        }

        public DataServiceException(int statusCode, string errorCode, string message, string messageXmlLang, Exception innerException) : base(message, innerException)
        {
            EventHandler<SafeSerializationEventArgs> handler = null;
            this.state = new DataServiceExceptionSerializationState();
            this.state.ErrorCode = errorCode ?? string.Empty;
            this.state.MessageLanguage = messageXmlLang ?? CultureInfo.CurrentCulture.Name;
            this.state.StatusCode = statusCode;
            if (handler == null)
            {
                handler = (sender, e) => e.AddSerializedState(this.state);
            }
            base.SerializeObjectState += handler;
        }

        internal static DataServiceException CreateBadRequestError(string message)
        {
            return new DataServiceException(400, message);
        }

        internal static DataServiceException CreateBadRequestError(string message, Exception innerException)
        {
            return new DataServiceException(400, null, message, null, innerException);
        }

        internal static DataServiceException CreateDeepRecursion(int recursionLimit)
        {
            return CreateBadRequestError(Strings.BadRequest_DeepRecursion(recursionLimit));
        }

        internal static DataServiceException CreateDeepRecursion_General()
        {
            return CreateBadRequestError(Strings.BadRequest_DeepRecursion_General);
        }

        internal static DataServiceException CreateForbidden()
        {
			return new DataServiceException(0x193, Strings.RequestUriProcessor_Forbidden);
        }

        internal static DataServiceException CreateMethodNotAllowed(string message, string allow)
        {
            DataServiceException exception = new DataServiceException(0x195, message);
            exception.state.ResponseAllowHeader = allow;
            return exception;
        }

        internal static DataServiceException CreateMethodNotImplemented(string message)
        {
            return new DataServiceException(0x1f5, message);
        }

        internal static DataServiceException CreatePreConditionFailedError(string message)
        {
            return new DataServiceException(0x19c, message);
        }

        internal static DataServiceException CreatePreConditionFailedError(string message, Exception innerException)
        {
            return new DataServiceException(0x19c, null, message, null, innerException);
        }

        internal static DataServiceException CreateResourceNotFound(string identifier)
        {
			return new DataServiceException(0x194, Strings.RequestUriProcessor_ResourceNotFound(identifier));
        }

        internal static DataServiceException CreateSyntaxError()
        {
			return CreateSyntaxError(Strings.RequestUriProcessor_SyntaxError);
        }

        internal static DataServiceException CreateSyntaxError(string message)
        {
            return CreateBadRequestError(message);
        }

        internal static DataServiceException ResourceNotFoundError(string errorMessage)
        {
            return new DataServiceException(0x194, errorMessage);
        }

        public string ErrorCode
        {
            get
            {
                return this.state.ErrorCode;
            }
        }

        public string MessageLanguage
        {
            get
            {
                return this.state.MessageLanguage;
            }
        }

        internal string ResponseAllowHeader
        {
            get
            {
                return this.state.ResponseAllowHeader;
            }
        }

        public int StatusCode
        {
            get
            {
                return this.state.StatusCode;
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        private struct DataServiceExceptionSerializationState : ISafeSerializationData
        {
            public string MessageLanguage { get; set; }
            public string ErrorCode { get; set; }
            public int StatusCode { get; set; }
            public string ResponseAllowHeader { get; set; }
            void ISafeSerializationData.CompleteDeserialization(object deserialized)
            {
                DataServiceException exception = (DataServiceException) deserialized;
                exception.state = this;
            }
        }
    }
}

