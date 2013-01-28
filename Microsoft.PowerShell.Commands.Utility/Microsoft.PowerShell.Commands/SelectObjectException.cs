namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    internal class SelectObjectException : SystemException
    {
        private System.Management.Automation.ErrorRecord errorRecord;

        internal SelectObjectException(System.Management.Automation.ErrorRecord errorRecord)
        {
            this.errorRecord = errorRecord;
        }

        internal System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                return this.errorRecord;
            }
        }
    }
}

