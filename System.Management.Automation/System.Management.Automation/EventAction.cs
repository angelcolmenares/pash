namespace System.Management.Automation
{
    using System;

    internal class EventAction
    {
        private PSEventArgs args;
        private PSEventSubscriber sender;

        public EventAction(PSEventSubscriber sender, PSEventArgs args)
        {
            this.sender = sender;
            this.args = args;
        }

        public PSEventArgs Args
        {
            get
            {
                return this.args;
            }
        }

        public PSEventSubscriber Sender
        {
            get
            {
                return this.sender;
            }
        }
    }
}

