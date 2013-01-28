namespace System.Management.Automation
{
    using System;

    public class GettingValueExceptionEventArgs : EventArgs
    {
        private System.Exception _exception;
        private bool _shouldThrow;
        private object _valueReplacement;

        internal GettingValueExceptionEventArgs(System.Exception exception)
        {
            this._exception = exception;
            this._valueReplacement = null;
            this._shouldThrow = true;
        }

        public System.Exception Exception
        {
            get
            {
                return this._exception;
            }
        }

        public bool ShouldThrow
        {
            get
            {
                return this._shouldThrow;
            }
            set
            {
                this._shouldThrow = value;
            }
        }

        public object ValueReplacement
        {
            get
            {
                return this._valueReplacement;
            }
            set
            {
                this._valueReplacement = value;
            }
        }
    }
}

