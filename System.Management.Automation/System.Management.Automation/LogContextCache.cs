namespace System.Management.Automation
{
    using System;

    internal class LogContextCache
    {
        private string user;

        internal string User
        {
            get
            {
                return this.user;
            }
            set
            {
                this.user = value;
            }
        }
    }
}

