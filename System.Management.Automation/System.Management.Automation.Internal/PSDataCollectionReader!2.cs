namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    internal class PSDataCollectionReader<DataStoreType, ReturnType> : ObjectReaderBase<ReturnType>
    {
        private PSDataCollectionEnumerator<DataStoreType> enumerator;

        public PSDataCollectionReader(PSDataCollectionStream<DataStoreType> stream) : base(stream)
        {
            this.enumerator = (PSDataCollectionEnumerator<DataStoreType>) stream.ObjectStore.GetEnumerator();
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
                base._stream.Close();
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
            int num = maxRequested;
            while (num > 0)
            {
                if (!this.enumerator.MoveNext(false))
                {
                    return collection;
                }
                collection.Add(this.ConvertToReturnType(this.enumerator.Current));
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
            if (this.enumerator.MoveNext())
            {
                inputObject = this.enumerator.Current;
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
    }
}

