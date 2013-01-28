namespace System.Data.Services
{
    using System;

    internal static class Error
    {
        internal static Exception ArgumentNull(string paramName)
        {
            return new ArgumentNullException(paramName);
        }

        internal static Exception ArgumentOutOfRange(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName);
        }

        internal static DataServiceException HttpHeaderFailure(int errorCode, string message)
        {
            return Trace<DataServiceException>(new DataServiceException(errorCode, message));
        }

        internal static Exception NotImplemented()
        {
            return new NotImplementedException();
        }

        internal static Exception NotSupported()
        {
            return new NotSupportedException();
        }

        private static T Trace<T>(T exception) where T: Exception
        {
            return exception;
        }
    }
}

