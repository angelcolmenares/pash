namespace System.Data.Services.Client
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Linq.Expressions;

    internal abstract class DataServiceQuery : DataServiceRequest, IQueryable, IEnumerable
    {
        internal DataServiceQuery()
        {
        }

        public IAsyncResult BeginExecute(AsyncCallback callback, object state)
        {
            return this.BeginExecuteInternal(callback, state);
        }

        internal abstract IAsyncResult BeginExecuteInternal(AsyncCallback callback, object state);
        public IEnumerable EndExecute(IAsyncResult asyncResult)
        {
            return this.EndExecuteInternal(asyncResult);
        }

        internal abstract IEnumerable EndExecuteInternal(IAsyncResult asyncResult);
        public IEnumerable Execute()
        {
            return this.ExecuteInternal();
        }

        internal abstract IEnumerable ExecuteInternal();
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw System.Data.Services.Client.Error.NotImplemented();
        }

        public abstract System.Linq.Expressions.Expression Expression { get; }

        public abstract IQueryProvider Provider { get; }
    }
}

