namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;

    public class PSEventArgsCollection : IEnumerable<PSEventArgs>, IEnumerable
    {
        private List<PSEventArgs> eventCollection = new List<PSEventArgs>();
        private object syncRoot = new object();

        public event PSEventReceivedEventHandler PSEventReceived;

        internal void Add(PSEventArgs eventToAdd)
        {
            if (eventToAdd == null)
            {
                throw new ArgumentNullException("eventToAdd");
            }
            this.eventCollection.Add(eventToAdd);
            this.OnPSEventReceived(eventToAdd.Sender, eventToAdd);
        }

        public IEnumerator<PSEventArgs> GetEnumerator()
        {
            return this.eventCollection.GetEnumerator();
        }

        private void OnPSEventReceived(object sender, PSEventArgs e)
        {
            if (this.PSEventReceived != null)
            {
                this.PSEventReceived(sender, e);
            }
        }

        public void RemoveAt(int index)
        {
            this.eventCollection.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.eventCollection.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.eventCollection.Count;
            }
        }

        public PSEventArgs this[int index]
        {
            get
            {
                return this.eventCollection[index];
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.syncRoot;
            }
        }
    }
}

