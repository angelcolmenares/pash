namespace System.Management.Automation
{
    using System;
    using System.Threading;

    public class PSEventArgs : EventArgs
    {
        private string computerName;
        private PSObject data;
        private int eventIdentifier;
        private ManualResetEventSlim eventProcessed;
        private bool forwardEvent;
        private Guid runspaceId;
        private object sender;
        private object[] sourceArgs;
        private EventArgs sourceEventArgs;
        private string sourceIdentifier;
        private DateTime timeGenerated;

        internal PSEventArgs(string computerName, Guid runspaceId, int eventIdentifier, string sourceIdentifier, object sender, object[] originalArgs, PSObject additionalData)
        {
            if (originalArgs != null)
            {
                foreach (object obj2 in originalArgs)
                {
                    EventArgs args = obj2 as EventArgs;
                    if (args != null)
                    {
                        this.sourceEventArgs = args;
                        break;
                    }
                    if (ForwardedEventArgs.IsRemoteSourceEventArgs(obj2))
                    {
                        this.sourceEventArgs = new ForwardedEventArgs((PSObject) obj2);
                        break;
                    }
                }
            }
            this.computerName = computerName;
            this.runspaceId = runspaceId;
            this.eventIdentifier = eventIdentifier;
            this.sender = sender;
            this.sourceArgs = originalArgs;
            this.sourceIdentifier = sourceIdentifier;
            this.timeGenerated = DateTime.Now;
            this.data = additionalData;
            this.forwardEvent = false;
        }

        public string ComputerName
        {
            get
            {
                return this.computerName;
            }
            internal set
            {
                this.computerName = value;
            }
        }

        public int EventIdentifier
        {
            get
            {
                return this.eventIdentifier;
            }
            internal set
            {
                this.eventIdentifier = value;
            }
        }

        internal ManualResetEventSlim EventProcessed
        {
            get
            {
                return this.eventProcessed;
            }
            set
            {
                this.eventProcessed = value;
            }
        }

        internal bool ForwardEvent
        {
            get
            {
                return this.forwardEvent;
            }
            set
            {
                this.forwardEvent = value;
            }
        }

        public PSObject MessageData
        {
            get
            {
                return this.data;
            }
        }

        public Guid RunspaceId
        {
            get
            {
                return this.runspaceId;
            }
            internal set
            {
                this.runspaceId = value;
            }
        }

        public object Sender
        {
            get
            {
                return this.sender;
            }
        }

        public object[] SourceArgs
        {
            get
            {
                return this.sourceArgs;
            }
        }

        public EventArgs SourceEventArgs
        {
            get
            {
                return this.sourceEventArgs;
            }
        }

        public string SourceIdentifier
        {
            get
            {
                return this.sourceIdentifier;
            }
        }

        public DateTime TimeGenerated
        {
            get
            {
                return this.timeGenerated;
            }
            internal set
            {
                this.timeGenerated = value;
            }
        }
    }
}

