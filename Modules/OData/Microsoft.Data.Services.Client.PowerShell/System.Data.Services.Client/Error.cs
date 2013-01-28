namespace System.Data.Services.Client
{
    using System;
    using System.Linq.Expressions;

    internal static class Error
    {
        internal static ArgumentException Argument(string message, string parameterName)
        {
            return Trace<ArgumentException>(new ArgumentException(message, parameterName));
        }

        internal static Exception ArgumentNull(string paramName)
        {
            return new ArgumentNullException(paramName);
        }

        internal static Exception ArgumentOutOfRange(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName);
        }

        internal static InvalidOperationException HttpHeaderFailure(int errorCode, string message)
        {
            return Trace<InvalidOperationException>(new InvalidOperationException(message));
        }

        internal static InvalidOperationException InternalError(System.Data.Services.Client.InternalError value)
        {
            return InvalidOperation(System.Data.Services.Client.Strings.Context_InternalError((int) value));
        }

        internal static InvalidOperationException InvalidOperation(string message)
        {
            return Trace<InvalidOperationException>(new InvalidOperationException(message));
        }

        internal static InvalidOperationException InvalidOperation(string message, Exception innerException)
        {
            return Trace<InvalidOperationException>(new InvalidOperationException(message, innerException));
        }

        internal static NotSupportedException MethodNotSupported(MethodCallExpression m)
        {
            return NotSupported(System.Data.Services.Client.Strings.ALinq_MethodNotSupported(m.Method.Name));
        }

        internal static Exception NotImplemented()
        {
            return new NotImplementedException();
        }

        internal static Exception NotSupported()
        {
            return new NotSupportedException();
        }

        internal static NotSupportedException NotSupported(string message)
        {
            return Trace<NotSupportedException>(new NotSupportedException(message));
        }

        internal static void ThrowBatchExpectedResponse(System.Data.Services.Client.InternalError value)
        {
            throw InvalidOperation(System.Data.Services.Client.Strings.Batch_ExpectedResponse((int) value));
        }

        internal static void ThrowBatchUnexpectedContent(System.Data.Services.Client.InternalError value)
        {
            throw InvalidOperation(System.Data.Services.Client.Strings.Batch_UnexpectedContent((int) value));
        }

        internal static void ThrowInternalError(System.Data.Services.Client.InternalError value)
        {
            throw InternalError(value);
        }

        internal static void ThrowObjectDisposed(Type type)
        {
            throw Trace<ObjectDisposedException>(new ObjectDisposedException(type.ToString()));
        }

        private static T Trace<T>(T exception) where T: Exception
        {
            return exception;
        }
    }
}

