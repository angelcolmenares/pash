namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class SessionStateScopeEnumerator : IEnumerator<SessionStateScope>, IDisposable, IEnumerator, IEnumerable<SessionStateScope>, IEnumerable
    {
        private SessionStateScope _currentEnumeratedScope;
        private readonly SessionStateScope _initialScope;

        internal SessionStateScopeEnumerator(SessionStateScope scope)
        {
            this._initialScope = scope;
        }

        public void Dispose()
        {
            this.Reset();
        }

        public bool MoveNext()
        {
            this._currentEnumeratedScope = (this._currentEnumeratedScope == null) ? this._initialScope : this._currentEnumeratedScope.Parent;
            return (this._currentEnumeratedScope != null);
        }

        public void Reset()
        {
            this._currentEnumeratedScope = null;
        }

        IEnumerator<SessionStateScope> IEnumerable<SessionStateScope>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        SessionStateScope IEnumerator<SessionStateScope>.Current
        {
            get
            {
                if (this._currentEnumeratedScope == null)
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                return this._currentEnumeratedScope;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        public SessionStateScope Current
        {
            get { return this._currentEnumeratedScope; }
        }
    }
}

