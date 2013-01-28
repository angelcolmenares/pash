namespace System.Management.Automation.Internal
{
    using System;
    using System.Management.Automation;

    internal class TestHelperSession : RemoteSession
    {
        internal override void CompleteKeyExchange()
        {
        }

        internal override void StartKeyExchange()
        {
        }

        internal override RemotingDestination MySelf
        {
            get
            {
                return RemotingDestination.InvalidDestination;
            }
        }
    }
}

