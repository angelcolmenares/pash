namespace System.Management.Automation
{
    using System;

    internal class AssertException : SystemException
    {
        private string stackTrace;

        internal AssertException(string message) : base(message)
        {
            this.stackTrace = Diagnostics.StackTrace(3);
        }

        public override string StackTrace
        {
            get
            {
                return this.stackTrace;
            }
        }
    }
}

