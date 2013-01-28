namespace Microsoft.Management.Infrastructure
{
    using Microsoft.Management.Infrastructure.Internal;
    using Microsoft.Management.Infrastructure.Native;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable]
    public class CimException : Exception, IDisposable
    {
        private bool _disposed;
        private CimInstance _errorData;
        private const string serializationId_ErrorData = "ErrorData";

        public CimException() : this(string.Empty, null)
        {
        }

        public CimException(CimInstance cimError) : base(GetExceptionMessage(cimError))
        {
            if (cimError == null)
            {
                throw new ArgumentNullException("cimError");
            }
            this._errorData = new CimInstance(cimError);
        }

        public CimException(string message) : this(message, null)
        {
        }

        protected CimException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this._errorData = (CimInstance) info.GetValue("ErrorData", typeof(CimInstance));
        }

        public CimException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal CimException(MiResult errorCode, string errorMessage, InstanceHandle errorDetailsHandle) : this(errorCode, errorMessage, errorDetailsHandle, null)
        {
        }

        internal CimException(MiResult errorCode, string errorMessage, InstanceHandle errorDetailsHandle, string exceptionMessage) : base(exceptionMessage ?? GetExceptionMessage(errorCode, errorMessage, errorDetailsHandle))
        {
            this.NativeErrorCode = errorCode.ToNativeErrorCode();
            if (errorDetailsHandle != null)
            {
                this._errorData = new CimInstance(errorDetailsHandle, null);
            }
        }

        internal void AssertNotDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing && (this._errorData != null))
                {
                    this._errorData.Dispose();
                    this._errorData = null;
                }
                this._disposed = true;
            }
        }

        internal static CimException GetExceptionIfMiResultFailure(MiResult result, string errorMessage, InstanceHandle errorData)
        {
            if (result != MiResult.OK)
            {
                return new CimException(result, errorMessage, errorData);
            }
            return null;
        }

        private static string GetExceptionMessage(CimInstance cimError)
        {
            if (cimError == null)
            {
                throw new ArgumentNullException("cimError");
            }
            return GetExceptionMessage(MiResult.FAILED, null, cimError.InstanceHandle);
        }

        private static string GetExceptionMessage(InstanceHandle errorDetailsHandle)
        {
            if (errorDetailsHandle != null)
            {
                string str;
                CimInstance errorData = new CimInstance(errorDetailsHandle, null);
                if (TryGetErrorDataProperty<string>(errorData, "Message", out str))
                {
                    return str;
                }
            }
            return null;
        }

        private static string GetExceptionMessage(MiResult errorCode, string errorMessage, InstanceHandle errorDetailsHandle)
        {
            string exceptionMessage = GetExceptionMessage(errorDetailsHandle);
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                return exceptionMessage;
            }
            ApplicationMethods.GetCimErrorFromMiResult(errorCode, errorMessage, out errorDetailsHandle);
            try
            {
                exceptionMessage = GetExceptionMessage(errorDetailsHandle);
                if (!string.IsNullOrEmpty(exceptionMessage))
                {
                    return exceptionMessage;
                }
            }
            finally
            {
                if (errorDetailsHandle != null)
                {
                    var disosable = errorDetailsHandle as IDisposable;
                    if (disosable != null)
                    {
                        disosable.Dispose();
                    }
                }
            }
            return errorCode.ToString();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("ErrorData", this.ErrorData);
        }

        internal static void ThrowIfMiResultFailure(MiResult result)
        {
            ThrowIfMiResultFailure(result, null, null);
        }

        internal static void ThrowIfMiResultFailure(MiResult result, InstanceHandle errorData)
        {
            ThrowIfMiResultFailure(result, null, errorData);
        }

        internal static void ThrowIfMiResultFailure(MiResult result, string errorMessage, InstanceHandle errorData)
        {
            CimException exception = GetExceptionIfMiResultFailure(result, errorMessage, errorData);
            if (exception != null)
            {
                throw exception;
            }
        }

        private bool TryGetErrorDataProperty<T>(string propertyName, out T propertyValue)
        {
            return TryGetErrorDataProperty<T>(this.ErrorData, propertyName, out propertyValue);
        }

        private static bool TryGetErrorDataProperty<T>(CimInstance errorData, string propertyName, out T propertyValue)
        {
            propertyValue = default(T);
            if (errorData == null)
            {
                return false;
            }
            try
            {
                CimProperty property = errorData.CimInstanceProperties[propertyName];
                if (property == null)
                {
                    return false;
                }
                if (!(property.Value is T))
                {
                    return false;
                }
                propertyValue = (T) property.Value;
                return true;
            }
            catch (CimException)
            {
                return false;
            }
        }

        public CimInstance ErrorData
        {
            get
            {
                this.AssertNotDisposed();
                return this._errorData;
            }
        }

        public string ErrorSource
        {
            get
            {
                string str;
                this.AssertNotDisposed();
                if (!this.TryGetErrorDataProperty<string>("ErrorSource", out str))
                {
                    return null;
                }
                return str;
            }
        }

        public ushort ErrorType
        {
            get
            {
                ushort num;
                this.AssertNotDisposed();
                if (!this.TryGetErrorDataProperty<ushort>("ErrorType", out num))
                {
                    return 0;
                }
                return num;
            }
        }

        public string MessageId
        {
            get
            {
                string str;
                this.AssertNotDisposed();
                if (!this.TryGetErrorDataProperty<string>("MessageId", out str))
                {
                    return null;
                }
                return str;
            }
        }

        public Microsoft.Management.Infrastructure.NativeErrorCode NativeErrorCode { get; private set; }

        public int StatusCode
        {
            get
            {
                int num;
                this.AssertNotDisposed();
                if (!this.TryGetErrorDataProperty<int>("CIMStatusCode", out num))
                {
                    return 0;
                }
                return num;
            }
        }
    }
}

