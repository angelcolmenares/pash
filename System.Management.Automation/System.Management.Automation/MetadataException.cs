namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.Serialization;

    [Serializable]
    public class MetadataException : RuntimeException
    {
        internal const string BaseName = "Metadata";
        internal const string MetadataMemberInitialization = "MetadataMemberInitialization";

        public MetadataException() : base(typeof(MetadataException).FullName)
        {
            base.SetErrorCategory(ErrorCategory.MetadataError);
        }

        public MetadataException(string message) : base(message)
        {
            base.SetErrorCategory(ErrorCategory.MetadataError);
        }

        protected MetadataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            base.SetErrorCategory(ErrorCategory.MetadataError);
        }

        public MetadataException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCategory(ErrorCategory.MetadataError);
        }

        internal MetadataException(string errorId, Exception innerException, string resourceStr, params object[] arguments) : base(StringUtil.Format(resourceStr, arguments), innerException)
        {
            base.SetErrorCategory(ErrorCategory.MetadataError);
            base.SetErrorId(errorId);
        }
    }
}

