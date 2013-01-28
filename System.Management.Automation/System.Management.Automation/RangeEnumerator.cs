namespace System.Management.Automation
{
    using System;
    using System.Collections;

    internal class RangeEnumerator : IEnumerator
    {
        private int _current;
        private int _lowerBound;
        private int _upperBound;
        private bool firstElement = true;
        private int increment = 1;

        public RangeEnumerator(int lowerBound, int upperBound)
        {
            this._lowerBound = lowerBound;
            this._current = this._lowerBound;
            this._upperBound = upperBound;
            if (lowerBound > upperBound)
            {
                this.increment = -1;
            }
        }

        public bool MoveNext()
        {
            if (this.firstElement)
            {
                this.firstElement = false;
                return true;
            }
            if (this._current == this._upperBound)
            {
                return false;
            }
            this._current += this.increment;
            return true;
        }

        public void Reset()
        {
            this._current = this._lowerBound;
            this.firstElement = true;
        }

        public object Current
        {
            get
            {
                return this._current;
            }
        }

        internal int CurrentValue
        {
            get
            {
                return this._current;
            }
        }

        internal int LowerBound
        {
            get
            {
                return this._lowerBound;
            }
        }

        internal int UpperBound
        {
            get
            {
                return this._upperBound;
            }
        }
    }
}

