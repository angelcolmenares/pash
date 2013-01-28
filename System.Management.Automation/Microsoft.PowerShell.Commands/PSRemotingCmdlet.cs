namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;

    public abstract class PSRemotingCmdlet : PSCmdlet
    {
        private bool _skipWinRMCheck;
        protected const string ComputerNameParameterSet = "ComputerName";
        protected const string DefaultPowerShellRemoteShellAppName = "WSMan";
        protected const string DefaultPowerShellRemoteShellName = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
        private static string LOCALHOST = "localhost";
        protected const string SessionParameterSet = "Session";

        protected PSRemotingCmdlet()
        {
        }

        protected override void BeginProcessing()
        {
            if (!this._skipWinRMCheck)
            {
                RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
            }
        }

        internal string GetMessage(string resourceString)
        {
            return this.GetMessage(resourceString, null);
        }

        internal string GetMessage(string resourceString, params object[] args)
        {
            if (args != null)
            {
                return StringUtil.Format(resourceString, args);
            }
            return resourceString;
        }

        protected string ResolveAppName(string appName)
        {
            if (!string.IsNullOrEmpty(appName))
            {
                return appName;
            }
            return (string) base.SessionState.Internal.ExecutionContext.GetVariableValue(SpecialVariables.PSSessionApplicationNameVarPath, "WSMan");
        }

        protected string ResolveComputerName(string computerName)
        {
            if (string.Equals(computerName, ".", StringComparison.OrdinalIgnoreCase))
            {
                return LOCALHOST;
            }
            return computerName;
        }

        protected void ResolveComputerNames(string[] computerNames, out string[] resolvedComputerNames)
        {
            if (computerNames == null)
            {
                resolvedComputerNames = new string[] { this.ResolveComputerName(".") };
            }
            else if (computerNames.Length == 0)
            {
                resolvedComputerNames = new string[0];
            }
            else
            {
                resolvedComputerNames = new string[computerNames.Length];
                for (int i = 0; i < resolvedComputerNames.Length; i++)
                {
                    resolvedComputerNames[i] = this.ResolveComputerName(computerNames[i]);
                }
            }
        }

        protected string ResolveShell(string shell)
        {
            if (!string.IsNullOrEmpty(shell))
            {
                return shell;
            }
            return (string) base.SessionState.Internal.ExecutionContext.GetVariableValue(SpecialVariables.PSSessionConfigurationNameVarPath, "http://schemas.microsoft.com/powershell/Microsoft.PowerShell");
        }

        internal void WriteStreamObject(Action<Cmdlet> action)
        {
            action(this);
        }

        internal bool SkipWinRMCheck
        {
            get
            {
                return this._skipWinRMCheck;
            }
            set
            {
                this._skipWinRMCheck = value;
            }
        }
    }
}

