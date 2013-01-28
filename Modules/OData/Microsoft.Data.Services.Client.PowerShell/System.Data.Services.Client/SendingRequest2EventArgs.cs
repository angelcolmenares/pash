namespace System.Data.Services.Client
{
    using Microsoft.Data.OData;
    using System;
    using System.Runtime.CompilerServices;

    internal class SendingRequest2EventArgs : EventArgs
    {
        internal SendingRequest2EventArgs(IODataRequestMessage requestMessage, System.Data.Services.Client.Descriptor descriptor, bool isBatchPart)
        {
            this.RequestMessage = requestMessage;
            this.Descriptor = descriptor;
            this.IsBatchPart = isBatchPart;
        }

        public System.Data.Services.Client.Descriptor Descriptor { get; private set; }

        public bool IsBatchPart { get; private set; }

        public IODataRequestMessage RequestMessage { get; private set; }
    }
}

