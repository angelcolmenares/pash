namespace System.Management.Automation
{
    using System;

    public class PSEventHandler
    {
        protected PSEventManager eventManager;
        protected PSObject extraData;
        protected object sender;
        protected string sourceIdentifier;

        public PSEventHandler()
        {
        }

        public PSEventHandler(PSEventManager eventManager, object sender, string sourceIdentifier, PSObject extraData)
        {
            this.eventManager = eventManager;
            this.sender = sender;
            this.sourceIdentifier = sourceIdentifier;
            this.extraData = extraData;
        }
    }
}

