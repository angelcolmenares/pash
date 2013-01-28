namespace System.Data.Services.Serializers
{
    using System;
    using System.Collections;
    using System.Data.Services;

    internal class CollectionPropertyValueEnumerable : IEnumerable
    {
        private IEnumerable sourceEnumerable;

        internal CollectionPropertyValueEnumerable(IEnumerable sourceEnumerable)
        {
            this.sourceEnumerable = sourceEnumerable;
        }

        public IEnumerator GetEnumerator()
        {
            if (this.sourceEnumerable == null)
            {
                throw new InvalidOperationException(Strings.CollectionCanOnlyBeEnumeratedOnce);
            }
            IEnumerator enumerator = new CollectionPropertyValueEnumerator(this.sourceEnumerable.GetEnumerator());
            this.sourceEnumerable = null;
            return enumerator;
        }

        private class CollectionPropertyValueEnumerator : IEnumerator, IDisposable
        {
            private IEnumerator sourceEnumerator;

            internal CollectionPropertyValueEnumerator(IEnumerator sourceEnumerator)
            {
                this.sourceEnumerator = sourceEnumerator;
            }

            public void Dispose()
            {
                WebUtil.Dispose(this.sourceEnumerator);
                this.sourceEnumerator = null;
            }

            public bool MoveNext()
            {
                if (this.sourceEnumerator == null)
                {
                    throw new ObjectDisposedException("CollectionPropertyValueEnumerator");
                }
                return this.sourceEnumerator.MoveNext();
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public object Current
            {
                get
                {
                    if (this.sourceEnumerator == null)
                    {
                        throw new ObjectDisposedException("CollectionPropertyValueEnumerator");
                    }
                    return this.sourceEnumerator.Current;
                }
            }
        }
    }
}

