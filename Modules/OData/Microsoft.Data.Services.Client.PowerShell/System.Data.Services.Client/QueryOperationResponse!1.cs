namespace System.Data.Services.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class QueryOperationResponse<T> : QueryOperationResponse, IEnumerable<T>, IEnumerable
    {
        internal QueryOperationResponse(Dictionary<string, string> headers, DataServiceRequest query, MaterializeAtom results) : base(headers, query, results)
        {
        }

        public DataServiceQueryContinuation<T> GetContinuation()
        {
            return (DataServiceQueryContinuation<T>) base.GetContinuation();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return base.GetEnumeratorHelper<IEnumerator<T>>(() => base.Results.Cast<T>().GetEnumerator());
        }

        public override long TotalCount
        {
            get
            {
                if ((base.Results == null) || !base.Results.IsCountable)
                {
                    throw new InvalidOperationException(System.Data.Services.Client.Strings.MaterializeFromAtom_CountNotPresent);
                }
                return base.Results.CountValue();
            }
        }
    }
}

