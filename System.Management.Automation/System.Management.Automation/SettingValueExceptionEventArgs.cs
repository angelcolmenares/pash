namespace System.Management.Automation
{
    using System;

    public class SettingValueExceptionEventArgs : EventArgs
    {
        private System.Exception _exception;
        private bool _shouldThrow;

        internal SettingValueExceptionEventArgs(System.Exception exception)
        {
            this._exception = exception;
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
    }
}

