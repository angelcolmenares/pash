namespace System.Management.Automation
{
    using System;

    public class PSEventUnsubscribedEventArgs : EventArgs
    {
        private PSEventSubscriber eventSubscriber;

        internal PSEventUnsubscribedEventArgs(PSEventSubscriber eventSubscriber)
        {
            this.eventSubscriber = eventSubscriber;
        }

        public PSEventSubscriber EventSubscriber
        {
            get
            {
                return this.eventSubscriber;
            }
            internal set
            {
                this.eventSubscriber = value;
            }
        }
    }
}

