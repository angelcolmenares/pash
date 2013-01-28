namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ArgumentTransformationMetadataException : MetadataException
    {
        internal const string ArgumentTransformationArgumentsShouldBeStrings = "ArgumentTransformationArgumentsShouldBeStrings";

        public ArgumentTransformationMetadataException() : base(typeof(ArgumentTransformationMetadataException).FullName)
        {
        }

        public ArgumentTransformationMetadataException(string message) : base(message)
        {
        }

        protected ArgumentTransformationMetadataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ArgumentTransformationMetadataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal ArgumentTransformationMetadataException(string errorId, Exception innerException, string resourceStr, params object[] arguments) : base(errorId, innerException, resourceStr, arguments)
        {
        }
    }
}

