namespace Microsoft.Data.OData
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [Serializable, DebuggerDisplay("{Message}")]
    internal sealed class ODataInnerError
    {
        public ODataInnerError()
        {
        }

        public ODataInnerError(Exception exception)
        {
            ExceptionUtils.CheckArgumentNotNull<Exception>(exception, "exception");
            this.Message = exception.Message ?? string.Empty;
            this.TypeName = exception.GetType().FullName;
            this.StackTrace = exception.StackTrace;
            if (exception.InnerException != null)
            {
                this.InnerError = new ODataInnerError(exception.InnerException);
            }
        }

        public ODataInnerError InnerError { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public string TypeName { get; set; }
    }
}

