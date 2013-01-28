namespace Microsoft.Data.OData
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [Serializable, DebuggerDisplay("{ErrorCode}: {Message}")]
    internal sealed class ODataError
    {
        public string ErrorCode { get; set; }

        public ODataInnerError InnerError { get; set; }

        public string Message { get; set; }

        public string MessageLanguage { get; set; }
    }
}

