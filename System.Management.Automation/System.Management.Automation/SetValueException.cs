namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SetValueException : ExtendedTypeSystemException
    {
        internal const string ReadOnlyProperty = "ReadOnlyProperty";
        internal const string SetWithoutSetterExceptionMsg = "SetWithoutSetterException";
        internal const string XmlNodeSetRestrictions = "XmlNodeSetShouldBeAString";
        internal const string XmlNodeSetShouldBeAString = "XmlNodeSetShouldBeAString";

        public SetValueException() : base(typeof(SetValueException).FullName)
        {
        }

        public SetValueException(string message) : base(message)
        {
        }

        protected SetValueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SetValueException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal SetValueException(string errorId, Exception innerException, string resourceString, params object[] arguments) : base(errorId, innerException, resourceString, arguments)
        {
        }
    }
}

