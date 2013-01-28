namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ParsingMetadataException : MetadataException
    {
        internal const string ParsingTooManyParameterSets = "ParsingTooManyParameterSets";

        public ParsingMetadataException() : base(typeof(ParsingMetadataException).FullName)
        {
        }

        public ParsingMetadataException(string message) : base(message)
        {
        }

        protected ParsingMetadataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ParsingMetadataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal ParsingMetadataException(string errorId, Exception innerException, string resourceStr, params object[] arguments) : base(errorId, innerException, resourceStr, arguments)
        {
        }
    }
}

