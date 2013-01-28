namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Threading;

    [OutputType(new Type[] { typeof(PSEventArgs) }), Cmdlet("Wait", "Event", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135276")]
    public class WaitEventCommand : PSCmdlet
    {
        private AutoResetEvent eventArrived = new AutoResetEvent(false);
        private WildcardPattern matchPattern;
        private PSEventArgs receivedEvent;
        private object receivedEventLock = new object();
        private string sourceIdentifier;
        private int timeoutInSeconds = -1;

        private void NotifyEvent(PSEventArgs e)
        {
            if (this.receivedEvent == null)
            {
                lock (this.receivedEventLock)
                {
                    if (this.receivedEvent == null)
                    {
                        this.receivedEvent = e;
                        this.eventArrived.Set();
                    }
                }
            }
        }

        protected override void ProcessRecord()
        {
            DateTime utcNow = DateTime.UtcNow;
            base.Events.ReceivedEvents.PSEventReceived += new PSEventReceivedEventHandler(this.ReceivedEvents_PSEventReceived);
            bool flag = false;
            this.ScanEventQueue();
            PSLocalEventManager events = (PSLocalEventManager) base.Events;
            while (!flag)
            {
                if (this.timeoutInSeconds >= 0)
                {
                    TimeSpan span = (TimeSpan) (DateTime.UtcNow - utcNow);
                    if (span.TotalSeconds > this.timeoutInSeconds)
                    {
                        break;
                    }
                }
                flag = this.eventArrived.WaitOne(200, false);
                events.ProcessPendingActions();
            }
            base.Events.ReceivedEvents.PSEventReceived -= new PSEventReceivedEventHandler(this.ReceivedEvents_PSEventReceived);
            if (this.receivedEvent != null)
            {
                base.WriteObject(this.receivedEvent);
            }
        }

        private void ReceivedEvents_PSEventReceived(object sender, PSEventArgs e)
        {
            if (this.sourceIdentifier == null)
            {
                this.NotifyEvent(e);
            }
            else
            {
                this.ScanEventQueue();
            }
        }

        private void ScanEventQueue()
        {
            lock (base.Events.ReceivedEvents.SyncRoot)
            {
                foreach (PSEventArgs args in base.Events.ReceivedEvents)
                {
                    if ((this.matchPattern == null) || this.matchPattern.IsMatch(args.SourceIdentifier))
                    {
                        this.NotifyEvent(args);
                        break;
                    }
                }
            }
        }

        protected override void StopProcessing()
        {
            this.eventArrived.Set();
        }

        [Parameter(Position=0, ValueFromPipelineByPropertyName=true)]
        public string SourceIdentifier
        {
            get
            {
                return this.sourceIdentifier;
            }
            set
            {
                this.sourceIdentifier = value;
                this.matchPattern = new WildcardPattern(value, WildcardOptions.IgnoreCase);
            }
        }

        [Parameter, Alias(new string[] { "TimeoutSec" }), ValidateRange(-1, 0x7fffffff)]
        public int Timeout
        {
            get
            {
                return this.timeoutInSeconds;
            }
            set
            {
                this.timeoutInSeconds = value;
            }
        }
    }
}

