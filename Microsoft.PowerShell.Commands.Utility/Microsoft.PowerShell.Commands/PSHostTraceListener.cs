namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Management.Automation.Internal.Host;
    using System.Security.Permissions;
    using System.Text;

    internal class PSHostTraceListener : TraceListener
    {
        private StringBuilder cachedWrite;
        private InternalHostUserInterface ui;

        internal PSHostTraceListener(PSCmdlet cmdlet) : base("")
        {
            this.cachedWrite = new StringBuilder();
            if (cmdlet == null)
            {
                throw new PSArgumentNullException("cmdlet");
            }
            this.ui = cmdlet.Host.UI as InternalHostUserInterface;
        }

        [SecurityPermission(SecurityAction.LinkDemand)]
        public override void Close()
        {
            base.Close();
        }

        [SecurityPermission(SecurityAction.LinkDemand)]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.Close();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        ~PSHostTraceListener()
        {
            this.Dispose(false);
        }

        [SecurityPermission(SecurityAction.LinkDemand)]
        public override void Write(string output)
        {
            try
            {
                this.cachedWrite.Append(output);
            }
            catch (Exception exception)
            {
                UtilityCommon.CheckForSevereException(null, exception);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand)]
        public override void WriteLine(string output)
        {
            try
            {
                this.cachedWrite.Append(output);
                this.ui.WriteDebugLine(this.cachedWrite.ToString());
                this.cachedWrite.Remove(0, this.cachedWrite.Length);
            }
            catch (Exception exception)
            {
                UtilityCommon.CheckForSevereException(null, exception);
            }
        }
    }
}

