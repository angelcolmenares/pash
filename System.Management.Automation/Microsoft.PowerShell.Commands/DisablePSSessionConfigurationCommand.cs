namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Tracing;
    using System.Security.Principal;
    using System.Text;

    [Cmdlet("Disable", "PSSessionConfiguration", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High, HelpUri="http://go.microsoft.com/fwlink/?LinkID=144299")]
    public sealed class DisablePSSessionConfigurationCommand : PSCmdlet
    {
        private static ScriptBlock disablePluginSb = ScriptBlock.Create(string.Format(CultureInfo.InvariantCulture, "\r\nfunction Disable-PSSessionConfiguration\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter(Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]\r\n    [System.String]\r\n    $Name,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $restartWinRMMessage,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $setEnabledTarget,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $setEnabledAction\r\n)\r\n    \r\n    begin\r\n    {{\r\n        $needWinRMRestart = $false\r\n        if ($force -or $pscmdlet.ShouldProcess($restartWinRMMessage))\r\n        {{\r\n            $svc = get-service winrm\r\n            if ($svc.Status -match \"Stopped\")\r\n            {{\r\n               Restart-Service winrm -force -confirm:$false\r\n            }}\r\n        }}       \r\n    }} #end of Begin block   \r\n\r\n    process\r\n    {{\r\n       Get-PSSessionConfiguration $name | % {{\r\n           \r\n           if ($_.Enabled -and ($force -or $pscmdlet.ShouldProcess($setEnabledTarget, $setEnabledAction)))\r\n           {{\r\n                Set-Item -WarningAction SilentlyContinue -Path \"WSMan:\\localhost\\Plugin\\$name\\Enabled\" -Value $false -confirm:$false\r\n                $needWinRMRestart = $true\r\n           }}\r\n       }} # end of foreach block\r\n    }} #end of process block\r\n\r\n    # restart the winrm to make the config change takes effect immediately\r\n    End\r\n    {{\r\n        if ($needWinRMRestart)\r\n        {{\r\n            Restart-Service winrm -force -confirm:$false\r\n        }}\r\n    }}\r\n}}\r\n\r\n$_ | Disable-PSSessionConfiguration -force $args[0] -whatif:$args[1] -confirm:$args[2] -restartWinRMMessage $args[3] -setEnabledTarget $args[4] -setEnabledAction $args[5]\r\n", new object[0]));
        private const string disablePluginSbFormat = "\r\nfunction Disable-PSSessionConfiguration\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter(Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]\r\n    [System.String]\r\n    $Name,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $restartWinRMMessage,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $setEnabledTarget,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $setEnabledAction\r\n)\r\n    \r\n    begin\r\n    {{\r\n        $needWinRMRestart = $false\r\n        if ($force -or $pscmdlet.ShouldProcess($restartWinRMMessage))\r\n        {{\r\n            $svc = get-service winrm\r\n            if ($svc.Status -match \"Stopped\")\r\n            {{\r\n               Restart-Service winrm -force -confirm:$false\r\n            }}\r\n        }}       \r\n    }} #end of Begin block   \r\n\r\n    process\r\n    {{\r\n       Get-PSSessionConfiguration $name | % {{\r\n           \r\n           if ($_.Enabled -and ($force -or $pscmdlet.ShouldProcess($setEnabledTarget, $setEnabledAction)))\r\n           {{\r\n                Set-Item -WarningAction SilentlyContinue -Path \"WSMan:\\localhost\\Plugin\\$name\\Enabled\" -Value $false -confirm:$false\r\n                $needWinRMRestart = $true\r\n           }}\r\n       }} # end of foreach block\r\n    }} #end of process block\r\n\r\n    # restart the winrm to make the config change takes effect immediately\r\n    End\r\n    {{\r\n        if ($needWinRMRestart)\r\n        {{\r\n            Restart-Service winrm -force -confirm:$false\r\n        }}\r\n    }}\r\n}}\r\n\r\n$_ | Disable-PSSessionConfiguration -force $args[0] -whatif:$args[1] -confirm:$args[2] -restartWinRMMessage $args[3] -setEnabledTarget $args[4] -setEnabledAction $args[5]\r\n";
        private bool force;
        private string[] shellName;
        private Collection<string> shellsToDisable = new Collection<string>();

        static DisablePSSessionConfigurationCommand()
        {
            disablePluginSb.LanguageMode = 0;
        }

        protected override void BeginProcessing()
        {
            RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
            PSSessionConfigurationCommandUtilities.ThrowIfNotAdministrator();
        }

        protected override void EndProcessing()
        {
            if (this.shellsToDisable.Count == 0)
            {
                this.shellsToDisable.Add("Microsoft.PowerShell");
            }
            base.WriteWarning(StringUtil.Format(RemotingErrorIdStrings.DcsWarningMessage, new object[0]));
            base.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.EcsScriptMessageV, "\r\nfunction Disable-PSSessionConfiguration\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter(Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]\r\n    [System.String]\r\n    $Name,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $restartWinRMMessage,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $setEnabledTarget,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $setEnabledAction\r\n)\r\n    \r\n    begin\r\n    {{\r\n        $needWinRMRestart = $false\r\n        if ($force -or $pscmdlet.ShouldProcess($restartWinRMMessage))\r\n        {{\r\n            $svc = get-service winrm\r\n            if ($svc.Status -match \"Stopped\")\r\n            {{\r\n               Restart-Service winrm -force -confirm:$false\r\n            }}\r\n        }}       \r\n    }} #end of Begin block   \r\n\r\n    process\r\n    {{\r\n       Get-PSSessionConfiguration $name | % {{\r\n           \r\n           if ($_.Enabled -and ($force -or $pscmdlet.ShouldProcess($setEnabledTarget, $setEnabledAction)))\r\n           {{\r\n                Set-Item -WarningAction SilentlyContinue -Path \"WSMan:\\localhost\\Plugin\\$name\\Enabled\" -Value $false -confirm:$false\r\n                $needWinRMRestart = $true\r\n           }}\r\n       }} # end of foreach block\r\n    }} #end of process block\r\n\r\n    # restart the winrm to make the config change takes effect immediately\r\n    End\r\n    {{\r\n        if ($needWinRMRestart)\r\n        {{\r\n            Restart-Service winrm -force -confirm:$false\r\n        }}\r\n    }}\r\n}}\r\n\r\n$_ | Disable-PSSessionConfiguration -force $args[0] -whatif:$args[1] -confirm:$args[2] -restartWinRMMessage $args[3] -setEnabledTarget $args[4] -setEnabledAction $args[5]\r\n"));
            bool whatIf = false;
            bool confirm = true;
            PSSessionConfigurationCommandUtilities.CollectShouldProcessParameters(this, out whatIf, out confirm);
            string restartWinRMMessage = RemotingErrorIdStrings.RestartWinRMMessage;
            string setEnabledFalseTarget = RemotingErrorIdStrings.SetEnabledFalseTarget;
            string str3 = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, "Set-Item");
            disablePluginSb.InvokeUsingCmdlet(this, true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, this.shellsToDisable, new object[0], AutomationNull.Value, new object[] { this.force, whatIf, confirm, restartWinRMMessage, setEnabledFalseTarget, str3 });
            Tracer tracer = new Tracer();
            StringBuilder builder = new StringBuilder();
            foreach (string str4 in this.Name ?? new string[0])
            {
                builder.Append(str4);
                builder.Append(", ");
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 2, 2);
            }
            tracer.EndpointDisabled(builder.ToString(), WindowsIdentity.GetCurrent().Name);
        }

        protected override void ProcessRecord()
        {
            if (this.shellName != null)
            {
                foreach (string str in this.shellName)
                {
                    this.shellsToDisable.Add(str);
                }
            }
        }

        [Parameter]
        public SwitchParameter Force
        {
            get
            {
                return this.force;
            }
            set
            {
                this.force = (bool) value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
        public string[] Name
        {
            get
            {
                return this.shellName;
            }
            set
            {
                this.shellName = value;
            }
        }
    }
}

