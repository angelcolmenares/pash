namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;

    [Serializable]
    public class ScriptBlockToPowerShellNotSupportedException : RuntimeException
    {
        public ScriptBlockToPowerShellNotSupportedException() : base(typeof(ScriptBlockToPowerShellNotSupportedException).FullName)
        {
        }

        public ScriptBlockToPowerShellNotSupportedException(string message) : base(message)
        {
        }

        protected ScriptBlockToPowerShellNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ScriptBlockToPowerShellNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal ScriptBlockToPowerShellNotSupportedException(string errorId, Exception innerException, string message, params object[] arguments) : base(string.Format(CultureInfo.CurrentCulture, message, arguments), innerException)
        {
            base.SetErrorId(errorId);
        }
    }
}

