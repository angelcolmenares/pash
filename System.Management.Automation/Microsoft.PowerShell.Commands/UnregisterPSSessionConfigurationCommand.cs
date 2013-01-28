namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Tracing;
    using System.Security.Principal;

    [Cmdlet("Unregister", "PSSessionConfiguration", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High, HelpUri="http://go.microsoft.com/fwlink/?LinkID=144308")]
    public sealed class UnregisterPSSessionConfigurationCommand : PSCmdlet
    {
        private bool force;
        private bool isErrorReported;
        private bool noRestart;
        private static readonly ScriptBlock removePluginSb = ScriptBlock.Create(string.Format(CultureInfo.InvariantCulture, "\r\nfunction Unregister-PSSessionConfiguration\r\n{{\r\n    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\n    param(\r\n       $filter,\r\n       $action,\r\n       $targetTemplate,\r\n       $shellNotErrMsgFormat,\r\n       [bool]$force,\r\n       [bool]$serviceRestart,\r\n       [string]$serviceRestartWarning)\r\n\r\n    begin\r\n    {{\r\n        $RestartWarningShown = $false\r\n    }}\r\n\r\n    process\r\n    {{\r\n        $shellsFound = 0\r\n        dir 'WSMan:\\localhost\\Plugin\\' | ? {{ $_.Name -like \"$filter\" }} | % {{\r\n            $pluginFileNamePath = join-path \"$($_.pspath)\" 'FileName'\r\n            if (!(test-path \"$pluginFileNamePath\"))\r\n            {{\r\n                return\r\n            }}\r\n\r\n           $pluginFileName = get-item -literalpath \"$pluginFileNamePath\"\r\n           if ((!$pluginFileName) -or ($pluginFileName.Value -notmatch '{0}'))\r\n           {{\r\n                return  \r\n           }}\r\n           \r\n           $shellsFound++\r\n\r\n           $shouldProcessTargetString = $targetTemplate -f $_.Name\r\n           if ($serviceRestart -and !$force -and !$RestartWarningShown) {{\r\n               Write-Warning $serviceRestartWarning \r\n               $RestartWarningShown = $true \r\n           }}\r\n\r\n           $DISCConfigFilePath = [System.IO.Path]::Combine($_.PSPath, \"InitializationParameters\")\r\n           $DISCConfigFile = get-childitem -literalpath \"$DISCConfigFilePath\" | ? {{$_.Name -like \"configFilePath\"}}\r\n        \r\n           if($DISCConfigFile -ne $null)\r\n           {{\r\n               if(test-path -LiteralPath \"$($DISCConfigFile.Value)\") {{                      \r\n                       remove-item -literalpath \"$($DISCConfigFile.Value)\" -recurse -force -confirm:$false\r\n               }}\r\n           }}\r\n \r\n           if($force -or $pscmdlet.ShouldProcess($shouldProcessTargetString, $action))\r\n           {{\r\n                remove-item -literalpath \"$($_.pspath)\" -recurse -force\r\n           }}\r\n        }}\r\n\r\n        if (!$shellsFound)\r\n        {{\r\n            $errMsg = $shellNotErrMsgFormat -f $filter\r\n            Write-Error $errMsg \r\n        }}\r\n    }} # end of Process block\r\n}}\r\n\r\nUnregister-PSSessionConfiguration -filter $args[0] -whatif:$args[1] -confirm:$args[2] -action $args[3] -targetTemplate $args[4] -shellNotErrMsgFormat $args[5] -force $args[6] -serviceRestart $args[7] -serviceRestartWarning $args[8]\r\n", new object[] { "pwrshplugin.dll" }));
        private const string removePluginSbFormat = "\r\nfunction Unregister-PSSessionConfiguration\r\n{{\r\n    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\n    param(\r\n       $filter,\r\n       $action,\r\n       $targetTemplate,\r\n       $shellNotErrMsgFormat,\r\n       [bool]$force,\r\n       [bool]$serviceRestart,\r\n       [string]$serviceRestartWarning)\r\n\r\n    begin\r\n    {{\r\n        $RestartWarningShown = $false\r\n    }}\r\n\r\n    process\r\n    {{\r\n        $shellsFound = 0\r\n        dir 'WSMan:\\localhost\\Plugin\\' | ? {{ $_.Name -like \"$filter\" }} | % {{\r\n            $pluginFileNamePath = join-path \"$($_.pspath)\" 'FileName'\r\n            if (!(test-path \"$pluginFileNamePath\"))\r\n            {{\r\n                return\r\n            }}\r\n\r\n           $pluginFileName = get-item -literalpath \"$pluginFileNamePath\"\r\n           if ((!$pluginFileName) -or ($pluginFileName.Value -notmatch '{0}'))\r\n           {{\r\n                return  \r\n           }}\r\n           \r\n           $shellsFound++\r\n\r\n           $shouldProcessTargetString = $targetTemplate -f $_.Name\r\n           if ($serviceRestart -and !$force -and !$RestartWarningShown) {{\r\n               Write-Warning $serviceRestartWarning \r\n               $RestartWarningShown = $true \r\n           }}\r\n\r\n           $DISCConfigFilePath = [System.IO.Path]::Combine($_.PSPath, \"InitializationParameters\")\r\n           $DISCConfigFile = get-childitem -literalpath \"$DISCConfigFilePath\" | ? {{$_.Name -like \"configFilePath\"}}\r\n        \r\n           if($DISCConfigFile -ne $null)\r\n           {{\r\n               if(test-path -LiteralPath \"$($DISCConfigFile.Value)\") {{                      \r\n                       remove-item -literalpath \"$($DISCConfigFile.Value)\" -recurse -force -confirm:$false\r\n               }}\r\n           }}\r\n \r\n           if($force -or $pscmdlet.ShouldProcess($shouldProcessTargetString, $action))\r\n           {{\r\n                remove-item -literalpath \"$($_.pspath)\" -recurse -force\r\n           }}\r\n        }}\r\n\r\n        if (!$shellsFound)\r\n        {{\r\n            $errMsg = $shellNotErrMsgFormat -f $filter\r\n            Write-Error $errMsg \r\n        }}\r\n    }} # end of Process block\r\n}}\r\n\r\nUnregister-PSSessionConfiguration -filter $args[0] -whatif:$args[1] -confirm:$args[2] -action $args[3] -targetTemplate $args[4] -shellNotErrMsgFormat $args[5] -force $args[6] -serviceRestart $args[7] -serviceRestartWarning $args[8]\r\n";
        private string shellName;
        private bool shouldOfferRestart;

        static UnregisterPSSessionConfigurationCommand()
        {
            removePluginSb.LanguageMode = 0;
        }

        protected override void BeginProcessing()
        {
            RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
            PSSessionConfigurationCommandUtilities.ThrowIfNotAdministrator();
        }

        protected override void EndProcessing()
        {
            PSSessionConfigurationCommandUtilities.RestartWinRMService(this, this.shouldOfferRestart ? this.isErrorReported : true, (bool) this.Force, this.shouldOfferRestart ? this.noRestart : true);
            if (!this.isErrorReported && this.noRestart)
            {
                string o = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, base.CommandInfo.Name);
                base.WriteWarning(StringUtil.Format(RemotingErrorIdStrings.WinRMRequiresRestart, o));
            }
            new Tracer().EndpointUnregistered(this.Name, WindowsIdentity.GetCurrent().Name);
        }

        protected override void ProcessRecord()
        {
            base.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.RcsScriptMessageV, "\r\nfunction Unregister-PSSessionConfiguration\r\n{{\r\n    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\n    param(\r\n       $filter,\r\n       $action,\r\n       $targetTemplate,\r\n       $shellNotErrMsgFormat,\r\n       [bool]$force,\r\n       [bool]$serviceRestart,\r\n       [string]$serviceRestartWarning)\r\n\r\n    begin\r\n    {{\r\n        $RestartWarningShown = $false\r\n    }}\r\n\r\n    process\r\n    {{\r\n        $shellsFound = 0\r\n        dir 'WSMan:\\localhost\\Plugin\\' | ? {{ $_.Name -like \"$filter\" }} | % {{\r\n            $pluginFileNamePath = join-path \"$($_.pspath)\" 'FileName'\r\n            if (!(test-path \"$pluginFileNamePath\"))\r\n            {{\r\n                return\r\n            }}\r\n\r\n           $pluginFileName = get-item -literalpath \"$pluginFileNamePath\"\r\n           if ((!$pluginFileName) -or ($pluginFileName.Value -notmatch '{0}'))\r\n           {{\r\n                return  \r\n           }}\r\n           \r\n           $shellsFound++\r\n\r\n           $shouldProcessTargetString = $targetTemplate -f $_.Name\r\n           if ($serviceRestart -and !$force -and !$RestartWarningShown) {{\r\n               Write-Warning $serviceRestartWarning \r\n               $RestartWarningShown = $true \r\n           }}\r\n\r\n           $DISCConfigFilePath = [System.IO.Path]::Combine($_.PSPath, \"InitializationParameters\")\r\n           $DISCConfigFile = get-childitem -literalpath \"$DISCConfigFilePath\" | ? {{$_.Name -like \"configFilePath\"}}\r\n        \r\n           if($DISCConfigFile -ne $null)\r\n           {{\r\n               if(test-path -LiteralPath \"$($DISCConfigFile.Value)\") {{                      \r\n                       remove-item -literalpath \"$($DISCConfigFile.Value)\" -recurse -force -confirm:$false\r\n               }}\r\n           }}\r\n \r\n           if($force -or $pscmdlet.ShouldProcess($shouldProcessTargetString, $action))\r\n           {{\r\n                remove-item -literalpath \"$($_.pspath)\" -recurse -force\r\n           }}\r\n        }}\r\n\r\n        if (!$shellsFound)\r\n        {{\r\n            $errMsg = $shellNotErrMsgFormat -f $filter\r\n            Write-Error $errMsg \r\n        }}\r\n    }} # end of Process block\r\n}}\r\n\r\nUnregister-PSSessionConfiguration -filter $args[0] -whatif:$args[1] -confirm:$args[2] -action $args[3] -targetTemplate $args[4] -shellNotErrMsgFormat $args[5] -force $args[6] -serviceRestart $args[7] -serviceRestartWarning $args[8]\r\n"));
            string o = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, base.CommandInfo.Name);
            string cSShouldProcessTarget = RemotingErrorIdStrings.CSShouldProcessTarget;
            string customShellNotFound = RemotingErrorIdStrings.CustomShellNotFound;
            bool whatIf = false;
            bool confirm = true;
            PSSessionConfigurationCommandUtilities.CollectShouldProcessParameters(this, out whatIf, out confirm);
            ArrayList dollarErrorVariable = (ArrayList) base.Context.DollarErrorVariable;
            int count = dollarErrorVariable.Count;
            removePluginSb.InvokeUsingCmdlet(this, true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, new object[0], AutomationNull.Value, new object[] { this.shellName, whatIf, confirm, o, cSShouldProcessTarget, customShellNotFound, this.force, !this.noRestart, StringUtil.Format(RemotingErrorIdStrings.WinRMRestartWarning, o) });
            dollarErrorVariable = (ArrayList) base.Context.DollarErrorVariable;
            this.isErrorReported = dollarErrorVariable.Count > count;
            this.shouldOfferRestart = true;
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

        [ValidateNotNullOrEmpty, Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true)]
        public string Name
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

        [Parameter]
        public SwitchParameter NoServiceRestart
        {
            get
            {
                return this.noRestart;
            }
            set
            {
                this.noRestart = (bool) value;
            }
        }
    }
}

