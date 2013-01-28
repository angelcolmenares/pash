namespace System.Management.Automation.Internal
{
    using System;
    using System.Management.Automation;

    internal class InformationalMessage
    {
        private RemotingDataType dataType;
        private object message;

        internal InformationalMessage(object message, RemotingDataType dataType)
        {
            this.dataType = dataType;
            this.message = message;
        }

        internal RemotingDataType DataType
        {
            get
            {
                return this.dataType;
            }
        }

        internal object Message
        {
            get
            {
                return this.message;
            }
        }
    }
}

