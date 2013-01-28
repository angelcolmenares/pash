namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Remoting;

    public abstract class Repository<T> where T: class
    {
        private string identifier;
        private Dictionary<Guid, T> repository;
        private object syncObject;

        protected Repository(string identifier)
        {
            this.repository = new Dictionary<Guid, T>();
            this.syncObject = new object();
            this.identifier = identifier;
        }

        public void Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(this.identifier);
            }
            lock (this.syncObject)
            {
                Guid key = this.GetKey(item);
                if (this.repository.ContainsKey(key))
                {
                    throw new ArgumentException(this.identifier);
                }
                this.repository.Add(key, item);
            }
        }

        public T GetItem(Guid instanceId)
        {
            lock (this.syncObject)
            {
                if (this.repository.ContainsKey(instanceId))
                {
                    return this.repository[instanceId];
                }
                return default(T);
            }
        }

        public List<T> GetItems()
        {
            return this.Items;
        }

        protected abstract Guid GetKey(T item);
        public void Remove(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(this.identifier);
            }
            lock (this.syncObject)
            {
                Guid key = this.GetKey(item);
                if (!this.repository.ContainsKey(key))
                {
                    throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.ItemNotFoundInRepository, new object[] { "Job repository", key.ToString() }));
                }
                this.repository.Remove(key);
            }
        }

        internal Dictionary<Guid, T> Dictionary
        {
            get
            {
                return this.repository;
            }
        }

        internal List<T> Items
        {
            get
            {
                lock (this.syncObject)
                {
                    return new List<T>(this.repository.Values);
                }
            }
        }
    }
}

