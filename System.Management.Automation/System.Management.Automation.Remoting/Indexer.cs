namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections;

    internal class Indexer : IEnumerable, IEnumerator
    {
        private int[] _current;
        private int[] _lengths;

        internal Indexer(int[] lengths)
        {
            this._lengths = lengths;
            this._current = new int[lengths.Length];
        }

        private bool CheckLengthsNonNegative(int[] lengths)
        {
            for (int i = 0; i < lengths.Length; i++)
            {
                if (lengths[i] < 0)
                {
                    return false;
                }
            }
            return true;
        }

        public IEnumerator GetEnumerator()
        {
            this.Reset();
            return this;
        }

        public bool MoveNext()
        {
            for (int i = this._lengths.Length - 1; i >= 0; i--)
            {
                if (this._current[i] < (this._lengths[i] - 1))
                {
                    this._current[i]++;
                    return true;
                }
                this._current[i] = 0;
            }
            return false;
        }

        public void Reset()
        {
            for (int i = 0; i < this._current.Length; i++)
            {
                this._current[i] = 0;
            }
            if (this._current.Length > 0)
            {
                this._current[this._current.Length - 1] = -1;
            }
        }

        public object Current
        {
            get
            {
                return this._current;
            }
        }
    }
}

