namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class PSDataCollectionEnumerator<W> : IBlockingEnumerator<W>, IEnumerator<W>, IDisposable, IEnumerator
    {
        private bool _neverBlock;
        private PSDataCollection<W> collToEnumerate;
        private W currentElement;
        private int index;

        internal PSDataCollectionEnumerator(PSDataCollection<W> collection, bool neverBlock)
        {
            this.collToEnumerate = collection;
            this.index = 0;
            this.currentElement = default(W);
            this.collToEnumerate.IsEnumerated = true;
            this._neverBlock = neverBlock;
        }

        public bool MoveNext()
        {
            return this.MoveNext(!this._neverBlock);
        }

        public bool MoveNext(bool block)
        {
            lock (this.collToEnumerate.SyncObject)
            {
            Label_0016:
                if (this.index < this.collToEnumerate.Count)
                {
                    this.currentElement = this.collToEnumerate[this.index];
                    if (this.collToEnumerate.ReleaseOnEnumeration)
                    {
                        this.collToEnumerate[this.index] = default(W);
                    }
                    this.index++;
                    return true;
                }
                if (((this.collToEnumerate.RefCount != 0) && this.collToEnumerate.IsOpen) && block)
                {
                    if (this.collToEnumerate.PulseIdleEvent)
                    {
                        this.collToEnumerate.FireIdleEvent();
                        Monitor.Wait(this.collToEnumerate.SyncObject);
                    }
                    else
                    {
                        Monitor.Wait(this.collToEnumerate.SyncObject);
                    }
                    goto Label_0016;
                }
                return false;
            }
        }

        public void Reset()
        {
            this.currentElement = default(W);
            this.index = 0;
        }

        void IDisposable.Dispose()
        {
        }

        public object Current
        {
            get
            {
                return this.currentElement;
            }
        }

        W IEnumerator<W>.Current
        {
            get
            {
                return this.currentElement;
            }
        }
    }
}

