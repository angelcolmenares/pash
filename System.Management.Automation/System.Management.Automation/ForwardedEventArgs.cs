namespace System.Management.Automation
{
    using System;

    public class ForwardedEventArgs : EventArgs
    {
        private PSObject serializedRemoteEventArgs;

        internal ForwardedEventArgs(PSObject serializedRemoteEventArgs)
        {
            this.serializedRemoteEventArgs = serializedRemoteEventArgs;
        }

        internal static bool IsRemoteSourceEventArgs(object argument)
        {
            return Deserializer.IsDeserializedInstanceOfType(argument, typeof(EventArgs));
        }

        public PSObject SerializedRemoteEventArgs
        {
            get
            {
                return this.serializedRemoteEventArgs;
            }
        }
    }
}

