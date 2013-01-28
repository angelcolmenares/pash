namespace System.Management.Automation.Remoting
{
    using System;
    using System.Threading;

    internal class AsyncObject<T> where T: class
    {
        private T _value;
        private ManualResetEvent _valueWasSet;

        internal AsyncObject()
        {
            this._valueWasSet = new ManualResetEvent(false);
        }

        internal T Value
        {
            get
            {
                if (!this._valueWasSet.WaitOne())
                {
                    this._value = default(T);
                }
                return this._value;
            }
            set
            {
                this._value = value;
                this._valueWasSet.Set();
            }
        }
    }
}

