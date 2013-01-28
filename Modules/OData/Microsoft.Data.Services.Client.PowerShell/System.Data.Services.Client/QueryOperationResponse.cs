namespace System.Data.Services.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class QueryOperationResponse : OperationResponse, IEnumerable
    {
        private readonly DataServiceRequest query;
        private readonly MaterializeAtom results;

        internal QueryOperationResponse(Dictionary<string, string> headers, DataServiceRequest query, MaterializeAtom results) : base(headers)
        {
            this.query = query;
            this.results = results;
        }

        public DataServiceQueryContinuation GetContinuation()
        {
            return this.results.GetContinuation(null);
        }

        public DataServiceQueryContinuation<T> GetContinuation<T>(IEnumerable<T> collection)
        {
            return (DataServiceQueryContinuation<T>) this.results.GetContinuation(collection);
        }

        public DataServiceQueryContinuation GetContinuation(IEnumerable collection)
        {
            return this.results.GetContinuation(collection);
        }

        public IEnumerator GetEnumerator()
        {
            return this.GetEnumeratorHelper<IEnumerator>(() => this.Results.GetEnumerator());
        }

        protected T GetEnumeratorHelper<T>(Func<T> getEnumerator) where T: IEnumerator
        {
            if (getEnumerator == null)
            {
                throw new ArgumentNullException("getEnumerator");
            }
            if (this.Results.Context != null)
            {
                bool? singleResult = this.Query.QueryComponents(this.Results.Context.MaxProtocolVersion).SingleResult;
                if (singleResult.HasValue && !singleResult.Value)
                {
                    IEnumerator enumerator = this.Results.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        ICollection is2 = current as ICollection;
                        if (is2 == null)
                        {
                            throw new DataServiceClientException(Strings.AtomMaterializer_CollectionExpectedCollection(current.GetType().ToString()));
                        }
                        return (T) is2.GetEnumerator();
                    }
                }
            }
            return getEnumerator();
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        internal static QueryOperationResponse GetInstance(Type elementType, Dictionary<string, string> headers, DataServiceRequest query, MaterializeAtom results)
        {
            return (QueryOperationResponse) Activator.CreateInstance(typeof(QueryOperationResponse<>).MakeGenericType(new Type[] { elementType }), BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { headers, query, results }, CultureInfo.InvariantCulture);
        }

        public DataServiceRequest Query
        {
            get
            {
                return this.query;
            }
        }

        internal MaterializeAtom Results
        {
            get
            {
                if (base.Error != null)
                {
					throw new InvalidOperationException(Strings.Context_BatchExecuteError, base.Error);
                }
                return this.results;
            }
        }

        public virtual long TotalCount
        {
            get
            {
                throw new NotSupportedException();
            }
        }
    }
}

