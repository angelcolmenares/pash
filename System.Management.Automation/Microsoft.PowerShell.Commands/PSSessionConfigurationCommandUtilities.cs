namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Text;

    internal static class PSSessionConfigurationCommandUtilities
    {
        internal const string PSCustomShellTypeName = "Microsoft.PowerShell.Commands.PSSessionConfigurationCommands#PSSessionConfiguration";
        internal const string restartWSManFormat = "restart-service winrm -force -confirm:$false";

        internal static Version CalculateMaxPSVersion(Version psVersion)
        {
            Version version = null;
            if ((psVersion != null) && (psVersion.Major == 2))
            {
                version = new Version(2, 0);
            }
            return version;
        }

        internal static void CheckIfPowerShellVersionIsInstalled(Version version)
        {
            if ((version != null) && (version.Major == 2))
            {
                try
                {
                    PSSnapInReader.GetPSEngineKey(PSVersionInfo.RegistryVersion1Key);
                    if (!PsUtils.FrameworkRegistryInstallation.IsFrameworkInstalled(2, 0, 0))
                    {
                        throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.NetFrameWorkV2NotInstalled, new object[0]));
                    }
                }
                catch (PSArgumentException)
                {
                    throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.PowerShellNotInstalled, new object[] { version, "PSVersion" }));
                }
            }
        }

        internal static void CollectShouldProcessParameters(PSCmdlet cmdlet, out bool whatIf, out bool confirm)
        {
            whatIf = false;
            confirm = true;
            MshCommandRuntime commandRuntime = cmdlet.CommandRuntime as MshCommandRuntime;
            if (commandRuntime != null)
            {
                whatIf = (bool) commandRuntime.WhatIf;
                if (commandRuntime.IsConfirmFlagSet)
                {
                    confirm = (bool) commandRuntime.Confirm;
                }
            }
        }

        internal static Version ConstructVersionFormatForConfigXml(Version psVersion)
        {
            Version version = null;
            if (psVersion != null)
            {
                version = new Version(psVersion.Major, psVersion.Minor);
            }
            return version;
        }

        internal static string GetModulePathAsString(string[] modulePath)
        {
            if ((modulePath == null) || (modulePath.Length <= 0))
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            foreach (string str in modulePath)
            {
                builder.Append(str);
                builder.Append(',');
            }
            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        internal static void RestartWinRMService(PSCmdlet cmdlet, bool isErrorReported, bool force, bool noServiceRestart)
        {
            if (!isErrorReported && !noServiceRestart)
            {
                string restartWSManServiceAction = RemotingErrorIdStrings.RestartWSManServiceAction;
                string target = StringUtil.Format(RemotingErrorIdStrings.RestartWSManServiceTarget, "WinRM");
                if (force || cmdlet.ShouldProcess(target, restartWSManServiceAction))
                {
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.RestartWSManServiceMessageV, new object[0]));
                    cmdlet.InvokeCommand.NewScriptBlock("restart-service winrm -force -confirm:$false").InvokeUsingCmdlet(cmdlet, true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, new object[0], AutomationNull.Value, new object[0]);
                }
            }
        }

        internal static void ThrowIfNotAdministrator()
        {
			if (OSHelper.IsUnix) return;
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                throw new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.EDcsRequiresElevation, new object[0]));
            }
        }
    }
}

