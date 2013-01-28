namespace System.Management.Automation.Internal
{
    using System;
    using System.Runtime.Serialization;
    using System.Text;

    [Serializable]
    internal class PSCryptoException : Exception
    {
        private int _errorCode;

        public PSCryptoException() : this(0, new StringBuilder(string.Empty))
        {
        }

        public PSCryptoException(string message) : this(message, null)
        {
        }

        protected PSCryptoException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._errorCode = 0xfffffff;
        }

        public PSCryptoException(string message, Exception innerException) : base(message, innerException)
        {
            this._errorCode = int.MaxValue;
        }

        public PSCryptoException(int errorCode, StringBuilder message) : base(message.ToString())
        {
            this._errorCode = errorCode;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        internal int ErrorCode
        {
            get
            {
                return this._errorCode;
            }
        }
    }
}

