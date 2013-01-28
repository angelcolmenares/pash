namespace System.Management.Automation.Tracing
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Runtime.CompilerServices;

    public class EtwEventArgs : EventArgs
    {
        public EtwEventArgs(EventDescriptor descriptor, bool success, object[] payload)
        {
            this.Descriptor = descriptor;
            this.Payload = payload;
            this.Success = success;
        }

        public EventDescriptor Descriptor { get; private set; }

        public object[] Payload { get; private set; }

        public bool Success { get; private set; }
    }
}

