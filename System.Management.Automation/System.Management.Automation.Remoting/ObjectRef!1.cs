namespace System.Management.Automation.Remoting
{
    using System;

    internal class ObjectRef<T> where T: class
    {
        private T _newValue;
        private T _oldValue;

        internal ObjectRef(T oldValue)
        {
            this._oldValue = oldValue;
        }

        internal void Override(T newValue)
        {
            this._newValue = newValue;
        }

        internal void Revert()
        {
            this._newValue = default(T);
        }

        internal bool IsOverridden
        {
            get
            {
                return (this._newValue != null);
            }
        }

        internal T OldValue
        {
            get
            {
                return this._oldValue;
            }
        }

        internal T Value
        {
            get
            {
                if (this._newValue == null)
                {
                    return this._oldValue;
                }
                return this._newValue;
            }
        }
    }
}

