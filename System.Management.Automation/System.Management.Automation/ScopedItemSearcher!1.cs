namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal abstract class ScopedItemSearcher<T> : IEnumerator<T>, IDisposable, IEnumerator, IEnumerable<T>, IEnumerable
    {
        private T current;
        private SessionStateScope currentScope;
        private SessionStateScope initialScope;
        private bool isInitialized;
        private bool isSingleScopeLookup;
        private VariablePath lookupPath;
        private SessionStateScopeEnumerator scopeEnumerable;
        protected SessionStateInternal sessionState;

        internal ScopedItemSearcher(SessionStateInternal sessionState, VariablePath lookupPath)
        {
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            if (lookupPath == null)
            {
                throw PSTraceSource.NewArgumentNullException("lookupPath");
            }
            this.sessionState = sessionState;
            this.lookupPath = lookupPath;
            this.InitializeScopeEnumerator();
        }

        public void Dispose()
        {
            this.current = default(T);
            this.scopeEnumerable.Dispose();
            this.scopeEnumerable = null;
            this.isInitialized = false;
            GC.SuppressFinalize(this);
        }

        protected abstract bool GetScopeItem(SessionStateScope scope, VariablePath name, out T newCurrentItem);
        private void InitializeScopeEnumerator()
        {
            this.initialScope = this.sessionState.CurrentScope;
            if (this.lookupPath.IsGlobal)
            {
                this.initialScope = this.sessionState.GlobalScope;
                this.isSingleScopeLookup = true;
            }
            else if (this.lookupPath.IsLocal || this.lookupPath.IsPrivate)
            {
                this.initialScope = this.sessionState.CurrentScope;
                this.isSingleScopeLookup = true;
            }
            else if (this.lookupPath.IsScript)
            {
                this.initialScope = this.sessionState.ScriptScope;
                this.isSingleScopeLookup = true;
            }
            this.scopeEnumerable = new SessionStateScopeEnumerator(this.initialScope);
            this.isInitialized = true;
        }

        public bool MoveNext()
        {
            bool flag = true;
            if (!this.isInitialized)
            {
                this.InitializeScopeEnumerator();
            }
            while (this.scopeEnumerable.MoveNext())
            {
                T local;
                if (this.TryGetNewScopeItem(this.scopeEnumerable.Current, out local))
                {
                    this.currentScope = this.scopeEnumerable.Current;
                    this.current = local;
                    return true;
                }
                flag = false;
                if (this.isSingleScopeLookup)
                {
                    return flag;
                }
            }
            return flag;
        }

        public void Reset()
        {
            this.InitializeScopeEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        private bool TryGetNewScopeItem(SessionStateScope lookupScope, out T newCurrentItem)
        {
            return this.GetScopeItem(lookupScope, this.lookupPath, out newCurrentItem);
        }

        public object Current
        {
            get
            {
                return this.current;
            }
        }

        internal SessionStateScope CurrentLookupScope
        {
            get
            {
                return this.currentScope;
            }
        }

        internal SessionStateScope InitialScope
        {
            get
            {
                return this.initialScope;
            }
        }

        T IEnumerator<T>.Current
        {
            get
            {
                return this.current;
            }
        }
    }
}

