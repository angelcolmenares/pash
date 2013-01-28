namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SetValueInvocationException : SetValueException
    {
        internal const string CannotSetNonManagementObjectMsg = "CannotSetNonManagementObject";
        internal const string ExceptionWhenSettingMsg = "ExceptionWhenSetting";

        public SetValueInvocationException() : base(typeof(SetValueInvocationException).FullName)
        {
        }

        public SetValueInvocationException(string message) : base(message)
        {
        }

        protected SetValueInvocationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SetValueInvocationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal SetValueInvocationException(string errorId, Exception innerException, string resourceString, params object[] arguments) : base(errorId, innerException, resourceString, arguments)
        {
        }
    }
}

