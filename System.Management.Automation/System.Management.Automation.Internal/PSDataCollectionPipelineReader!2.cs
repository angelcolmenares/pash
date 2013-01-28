namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    internal class PSDataCollectionPipelineReader<DataStoreType, ReturnType> : ObjectReaderBase<ReturnType>
    {
        private string computerName;
        private PSDataCollection<DataStoreType> datastore;
        private Guid runspaceId;

        internal PSDataCollectionPipelineReader(PSDataCollectionStream<DataStoreType> stream, string computerName, Guid runspaceId) : base(stream)
        {
            this.datastore = stream.ObjectStore;
            this.computerName = computerName;
            this.runspaceId = runspaceId;
        }

        private ReturnType ConvertToReturnType(object inputObject)
        {
            Type o = typeof(ReturnType);
            if (!typeof(PSObject).Equals(o) && !typeof(object).Equals(o))
            {
                throw PSTraceSource.NewNotSupportedException();
            }
            ReturnType result = default(ReturnType);
            LanguagePrimitives.TryConvertTo<ReturnType>(inputObject, out result);
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.datastore.Dispose();
            }
        }

        public override Collection<ReturnType> NonBlockingRead()
        {
            return this.NonBlockingRead(0x7fffffff);
        }

        public override Collection<ReturnType> NonBlockingRead(int maxRequested)
        {
            if (maxRequested < 0)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("maxRequested", maxRequested);
            }
            if (maxRequested == 0)
            {
                return new Collection<ReturnType>();
            }
            Collection<ReturnType> collection = new Collection<ReturnType>();
            for (int i = maxRequested; i > 0; i--)
            {
                if (this.datastore.Count <= 0)
                {
                    return collection;
                }
                collection.Add(this.ConvertToReturnType(this.datastore.ReadAndRemove(1)[0]));
            }
            return collection;
        }

        public override ReturnType Peek()
        {
            throw new NotSupportedException();
        }

        public override ReturnType Read()
        {
            object inputObject = AutomationNull.Value;
            if (this.datastore.Count > 0)
            {
                Collection<DataStoreType> collection = this.datastore.ReadAndRemove(1);
                if (collection.Count == 1)
                {
                    inputObject = collection[0];
                }
            }
            return this.ConvertToReturnType(inputObject);
        }

        public override Collection<ReturnType> Read(int count)
        {
            throw new NotSupportedException();
        }

        public override Collection<ReturnType> ReadToEnd()
        {
            throw new NotSupportedException();
        }

        internal string ComputerName
        {
            get
            {
                return this.computerName;
            }
        }

        internal Guid RunspaceId
        {
            get
            {
                return this.runspaceId;
            }
        }
    }
}

