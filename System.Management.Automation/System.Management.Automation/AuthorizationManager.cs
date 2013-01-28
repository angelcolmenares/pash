namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Host;
    using System.Runtime.InteropServices;

    public class AuthorizationManager
    {
        private object policyCheckLock = new object();
        private string shellId;

        public AuthorizationManager(string shellId)
        {
            this.shellId = shellId;
        }

        protected internal virtual bool ShouldRun(CommandInfo commandInfo, CommandOrigin origin, PSHost host, out Exception reason)
        {
            reason = null;
            return true;
        }

        internal void ShouldRunInternal(CommandInfo commandInfo, CommandOrigin origin, PSHost host)
        {
            bool flag = false;
            bool flag2 = false;
            Exception reason = null;
            try
            {
                lock (this.policyCheckLock)
                {
                    flag = this.ShouldRun(commandInfo, origin, host, out reason);
                }
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                reason = exception2;
                flag2 = true;
                flag = false;
            }
            if (!flag)
            {
                if (reason == null)
                {
                    throw new PSSecurityException(AuthorizationManagerBase.AuthorizationManagerDefaultFailureReason);
                }
                if (reason is PSSecurityException)
                {
                    throw reason;
                }
                string message = reason.Message;
                if (flag2)
                {
                    message = AuthorizationManagerBase.AuthorizationManagerDefaultFailureReason;
                }
                PSSecurityException exception3 = new PSSecurityException(message, reason);
                throw exception3;
            }
        }

        internal string ShellId
        {
            get
            {
                return this.shellId;
            }
        }
    }
}

