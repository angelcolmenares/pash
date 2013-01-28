namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Sqm;
    using System.Management.Automation.Tracing;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Text;

    [Cmdlet("Set", "PSSessionConfiguration", DefaultParameterSetName="NameParameterSet", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High, HelpUri="http://go.microsoft.com/fwlink/?LinkID=144307")]
    public sealed class SetPSSessionConfigurationCommand : PSSessionConfigurationCommandBase
    {
        private const string AllowRemoteAccessToken = "Enabled";
        private const string getAssemblyNameDataFormat = @"(Get-Item 'WSMan:\localhost\Plugin\{0}\InitializationParameters\assemblyname').Value";
        private const string getCurrentIdleTimeoutmsFormat = @"(Get-Item 'WSMan:\localhost\Plugin\{0}\Quotas\IdleTimeoutms').Value";
        private const string getSessionConfigurationDataSbFormat = @"(Get-Item 'WSMan:\localhost\Plugin\{0}\InitializationParameters\SessionConfigurationData').Value";
        private const string getSessionTypeFormat = @"(get-item 'WSMan::localhost\Plugin\{0}\InitializationParameters\sessiontype' -ErrorAction SilentlyContinue).Value";
        private static readonly string[] initParametersMap = new string[] { "applicationbase", "assemblyname", "pssessionconfigurationtypename", "startupscript", "psmaximumreceivedobjectsizemb", "psmaximumreceiveddatasizepercommandmb", "pssessionthreadoptions", "pssessionthreadapartmentstate", "PSVersion", "MaxPSVersion", "sessionconfigurationdata" };
        private const string initParamFormat = "<Param Name='{0}' Value='{1}' />";
        private bool isErrorReported;
        private const string privateDataFormat = "<Param Name='PrivateData'>{0}</Param>";
        private const string SessionConfigDataFormat = "<SessionConfigurationData>{0}</SessionConfigurationData>";
        private static readonly ScriptBlock setPluginSb;
        private const string setPluginSbFormat = "\r\nfunction Set-PSSessionConfiguration([PSObject]$customShellObject, \r\n     [Array]$initParametersMap,\r\n     [bool]$force,\r\n     [string]$sddl,\r\n     [bool]$isSddlSpecified,\r\n     [bool]$shouldShowUI,\r\n     [string]$resourceUri,\r\n     [string]$pluginNotFoundErrorMsg,\r\n     [string]$pluginNotPowerShellMsg,\r\n     [System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]$accessMode\r\n)\r\n{{\r\n   $wsmanPluginDir = 'WSMan:\\localhost\\Plugin'\r\n   $pluginName = $customShellObject.Name;\r\n   $pluginDir = Join-Path \"$wsmanPluginDir\" \"$pluginName\"\r\n   if ((!$pluginName) -or !(test-path \"$pluginDir\"))\r\n   {{\r\n      Write-Error $pluginNotFoundErrorMsg\r\n      return\r\n   }}\r\n\r\n   # check if the plugin is a PowerShell plugin   \r\n   $pluginFileNamePath = Join-Path \"$pluginDir\" 'FileName'\r\n   if (!(test-path \"$pluginFileNamePath\"))\r\n   {{\r\n      Write-Error $pluginNotPowerShellMsg\r\n      return\r\n   }}\r\n\r\n   $pluginFileName = get-item -literalpath \"$pluginFileNamePath\"\r\n   if ((!$pluginFileName) -or ($pluginFileName.Value -notmatch '{0}'))\r\n   {{\r\n      Write-Error $pluginNotPowerShellMsg\r\n      return\r\n   }}\r\n\r\n   # set Initialization Parameters\r\n   $initParametersPath = Join-Path \"$pluginDir\" 'InitializationParameters'  \r\n   foreach($initParameterName in $initParametersMap)\r\n   {{         \r\n        if ($customShellObject | get-member $initParameterName)\r\n        {{\r\n            $parampath = Join-Path \"$initParametersPath\" $initParameterName\r\n\r\n            if (test-path $parampath)\r\n            {{\r\n               remove-item -path \"$parampath\"\r\n            }}\r\n                \r\n            # 0 is an accepted value for MaximumReceivedDataSizePerCommandMB and MaximumReceivedObjectSizeMB\r\n            if (($customShellObject.$initParameterName) -or ($customShellObject.$initParameterName -eq 0))\r\n            {{\r\n               new-item -path \"$initParametersPath\" -paramname $initParameterName  -paramValue \"$($customShellObject.$initParameterName)\" -Force\r\n            }}\r\n        }}\r\n   }}\r\n\r\n   # sddl processing\r\n   if ($isSddlSpecified)\r\n   {{\r\n       $resourcesPath = Join-Path \"$pluginDir\" 'Resources'\r\n       dir -literalpath \"$resourcesPath\" | % {{\r\n            $securityPath = Join-Path \"$($_.pspath)\" 'Security'\r\n            if ((@(dir -literalpath \"$securityPath\")).count -gt 0)\r\n            {{\r\n                dir -literalpath \"$securityPath\" | % {{\r\n                    $securityIDPath = \"$($_.pspath)\"\r\n                    remove-item -path \"$securityIDPath\" -recurse -force\r\n                }} #end of securityPath\r\n\r\n                if ($sddl)\r\n                {{\r\n                    new-item -path \"$securityPath\" -Sddl $sddl -force\r\n                }}\r\n            }}\r\n            else\r\n            {{\r\n                if ($sddl)\r\n                {{\r\n                    new-item -path \"$securityPath\" -Sddl $sddl -force\r\n                }}\r\n            }}\r\n       }} # end of resources\r\n       return\r\n   }} #end of sddl processing\r\n   elseif ($shouldShowUI)\r\n   {{\r\n        $null = winrm configsddl $resourceUri\r\n   }}\r\n\r\n   # If accessmode is 'Disabled', we don't bother to check the sddl\r\n   if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Disabled.Equals($accessMode))\r\n   {{\r\n        return\r\n   }}\r\n\r\n   # Construct SID for network users\r\n   [system.security.principal.wellknownsidtype]$evst = \"NetworkSid\"\r\n   $networkSID = new-object system.security.principal.securityidentifier $evst,$null\r\n\r\n   $resPath = Join-Path \"$pluginDir\" 'Resources'\r\n   dir -literalpath \"$resPath\" | % {{\r\n        $securityPath = Join-Path \"$($_.pspath)\" 'Security'\r\n        if ((@(dir -literalpath \"$securityPath\")).count -gt 0)\r\n        {{\r\n            dir -literalpath \"$securityPath\" | % {{\r\n                $sddlPath = Join-Path \"$($_.pspath)\" 'Sddl'\r\n                $curSDDL = (get-item -path $sddlPath).value\r\n                $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$curSDDL\r\n                $newSDDL = $null\r\n                \r\n                $disableNetworkExists = $false\r\n                $securityIdentifierToPurge = $null\r\n                $sd.DiscretionaryAcl | % {{\r\n                    if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $networkSID) -and ($_.AccessMask -eq 268435456))\r\n                    {{\r\n                        $disableNetworkExists = $true\r\n                        $securityIdentifierToPurge = $_.securityidentifier\r\n                    }}\r\n                }}\r\n\r\n                if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode) -and !$disableNetworkExists)\r\n                {{\r\n                    $sd.DiscretionaryAcl.AddAccess(\"deny\", $networkSID, 268435456, \"None\", \"None\")\r\n                    $newSDDL = $sd.GetSddlForm(\"all\")\r\n                }}\r\n                if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Remote.Equals($accessMode) -and $disableNetworkExists)\r\n                {{\r\n                    # Remove the specific ACE\r\n                    $sd.discretionaryacl.RemoveAccessSpecific('Deny', $securityIdentifierToPurge, 268435456, 'none', 'none')\r\n\r\n                    # if there is no discretionaryacl..add Builtin Administrators and Remote Management Users\r\n                    # to the DACL group as this is the default WSMan behavior\r\n                    if ($sd.discretionaryacl.count -eq 0)\r\n                    {{\r\n                        # Built-in administrators\r\n                        [system.security.principal.wellknownsidtype]$bast = \"BuiltinAdministratorsSid\"\r\n                        $basid = new-object system.security.principal.securityidentifier $bast,$null\r\n                        $sd.DiscretionaryAcl.AddAccess('Allow',$basid, 268435456, 'none', 'none')\r\n\r\n                        # Remote Management Users, Win8+ only\r\n                        if ([System.Environment]::OSVersion.Version.Major -ge 6 -and [System.Environment]::OSVersion.Version.Minor -ge 2)\r\n                        {{\r\n                            $rmSidId = new-object system.security.principal.securityidentifier \"{2}\"\r\n                            $sd.DiscretionaryAcl.AddAccess('Allow', $rmSidId, 268435456, 'none', 'none')\r\n                        }}\r\n                    }}\r\n\r\n                    $newSDDL = $sd.GetSddlForm(\"all\")\r\n                }}\r\n\r\n                if ($newSDDL)\r\n                {{\r\n                    set-item -WarningAction SilentlyContinue -path $sddlPath -value $newSDDL -force\r\n                }}\r\n            }}\r\n        }}\r\n        else\r\n        {{\r\n            if (([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode)))\r\n            {{\r\n                new-item -path \"$securityPath\" -Sddl \"{1}\" -force\r\n            }}\r\n        }}\r\n   }}\r\n}}\r\n\r\nSet-PSSessionConfiguration $args[0] $args[1] $args[2] $args[3] $args[4] $args[5] $args[6] $args[7] $args[8] $args[9]\r\n";
        private const string setRunAsSbFormat = "\r\nfunction Set-RunAsCredential{{\r\n    param (\r\n        [string]$runAsUserName,\r\n\t    [system.security.securestring]$runAsPassword\r\n    )\r\n\r\n    $cred = new-object System.Management.Automation.PSCredential($runAsUserName, $runAsPassword)\r\n    set-item -WarningAction SilentlyContinue 'WSMan:\\localhost\\Plugin\\{0}\\RunAsUser' $cred -confirm:$false\r\n}}\r\nSet-RunAsCredential $args[0] $args[1]\r\n";
        private const string setSessionConfigurationDataSbFormat = "\r\nfunction Set-SessionConfigurationData([string] $scd) {{\r\n    if (test-path 'WSMan:\\localhost\\Plugin\\{0}\\InitializationParameters\\sessionconfigurationdata')\r\n    {{\r\n        set-item -WarningAction SilentlyContinue -Force 'WSMan:\\localhost\\Plugin\\{0}\\InitializationParameters\\sessionconfigurationdata' -Value $scd\r\n    }}\r\n    else\r\n    {{\r\n        new-item -WarningAction SilentlyContinue -path 'WSMan:\\localhost\\Plugin\\{0}\\InitializationParameters' -paramname sessionconfigurationdata -paramValue $scd -Force\r\n    }}\r\n}}\r\nSet-SessionConfigurationData $args[0]\r\n";
        private const string setSessionConfigurationOptionsSbFormat = "\r\nfunction Set-SessionPluginOptions([hashtable] $options) {{\r\n    if ($options[\"UsedSharedProcess\"]) {{\r\n        $value = $options[\"UseSharedProcess\"];\r\n        set-item -WarningAction SilentlyContinue 'WSMan:\\localhost\\Plugin\\{0}\\UseSharedProcess' -Value $value -confirm:$false\r\n        $options.Remove(\"UseSharedProcess\");\r\n    }}\r\n    foreach($v in $options.GetEnumerator()) {{\r\n        $name = $v.Name; \r\n        $value = $v.Value\r\n\r\n        if (!$value) {{\r\n            $value = 0;\r\n        }}\r\n\r\n        set-item -WarningAction SilentlyContinue ('WSMan:\\localhost\\Plugin\\{0}\\' + $name) -Value $value -confirm:$false\r\n    }}\r\n}}\r\nSet-SessionPluginOptions $args[0]\r\n";
        private const string setSessionConfigurationQuotaSbFormat = "\r\nfunction Set-SessionPluginQuota([hashtable] $quotas) {{\r\n    foreach($v in $quotas.GetEnumerator()) {{\r\n        $name = $v.Name; \r\n        $value = $v.Value;\r\n        if (!$value) {{\r\n            $value = [string]::empty;\r\n        }}\r\n        set-item -WarningAction SilentlyContinue ('WSMan:\\localhost\\Plugin\\{0}\\Quotas\\' + $name) -Value $value -confirm:$false\r\n    }}\r\n}}\r\nSet-SessionPluginQuota $args[0]\r\n";
        private const string setSessionConfigurationTimeoutQuotasSbFormat = "\r\nfunction Set-SessionPluginIdleTimeoutQuotas([int] $maxIdleTimeoutms, [int] $idleTimeoutms, [bool] $setMaxIdleTimoutFirst) {{\r\n    if ($setMaxIdleTimoutFirst) {{\r\n        set-item -WarningAction SilentlyContinue 'WSMan:\\localhost\\Plugin\\{0}\\Quotas\\MaxIdleTimeoutms' -Value $maxIdleTimeoutms -confirm:$false\r\n        set-item -WarningAction SilentlyContinue 'WSMan:\\localhost\\Plugin\\{0}\\Quotas\\IdleTimeoutms' -Value $idleTimeoutms -confirm:$false\r\n    }}\r\n    else {{\r\n        set-item -WarningAction SilentlyContinue 'WSMan:\\localhost\\Plugin\\{0}\\Quotas\\IdleTimeoutms' -Value $idleTimeoutms -confirm:$false\r\n        set-item -WarningAction SilentlyContinue 'WSMan:\\localhost\\Plugin\\{0}\\Quotas\\MaxIdleTimeoutms' -Value $maxIdleTimeoutms -confirm:$false\r\n    }}\r\n}}\r\nSet-SessionPluginIdleTimeoutQuotas $args[0] $args[1] $args[2]\r\n";
        private const string UseSharedProcessToken = "UseSharedProcess";

        static SetPSSessionConfigurationCommand()
        {
            string localSddl = PSSessionConfigurationCommandBase.GetLocalSddl();
            setPluginSb = ScriptBlock.Create(string.Format(CultureInfo.InvariantCulture, "\r\nfunction Set-PSSessionConfiguration([PSObject]$customShellObject, \r\n     [Array]$initParametersMap,\r\n     [bool]$force,\r\n     [string]$sddl,\r\n     [bool]$isSddlSpecified,\r\n     [bool]$shouldShowUI,\r\n     [string]$resourceUri,\r\n     [string]$pluginNotFoundErrorMsg,\r\n     [string]$pluginNotPowerShellMsg,\r\n     [System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]$accessMode\r\n)\r\n{{\r\n   $wsmanPluginDir = 'WSMan:\\localhost\\Plugin'\r\n   $pluginName = $customShellObject.Name;\r\n   $pluginDir = Join-Path \"$wsmanPluginDir\" \"$pluginName\"\r\n   if ((!$pluginName) -or !(test-path \"$pluginDir\"))\r\n   {{\r\n      Write-Error $pluginNotFoundErrorMsg\r\n      return\r\n   }}\r\n\r\n   # check if the plugin is a PowerShell plugin   \r\n   $pluginFileNamePath = Join-Path \"$pluginDir\" 'FileName'\r\n   if (!(test-path \"$pluginFileNamePath\"))\r\n   {{\r\n      Write-Error $pluginNotPowerShellMsg\r\n      return\r\n   }}\r\n\r\n   $pluginFileName = get-item -literalpath \"$pluginFileNamePath\"\r\n   if ((!$pluginFileName) -or ($pluginFileName.Value -notmatch '{0}'))\r\n   {{\r\n      Write-Error $pluginNotPowerShellMsg\r\n      return\r\n   }}\r\n\r\n   # set Initialization Parameters\r\n   $initParametersPath = Join-Path \"$pluginDir\" 'InitializationParameters'  \r\n   foreach($initParameterName in $initParametersMap)\r\n   {{         \r\n        if ($customShellObject | get-member $initParameterName)\r\n        {{\r\n            $parampath = Join-Path \"$initParametersPath\" $initParameterName\r\n\r\n            if (test-path $parampath)\r\n            {{\r\n               remove-item -path \"$parampath\"\r\n            }}\r\n                \r\n            # 0 is an accepted value for MaximumReceivedDataSizePerCommandMB and MaximumReceivedObjectSizeMB\r\n            if (($customShellObject.$initParameterName) -or ($customShellObject.$initParameterName -eq 0))\r\n            {{\r\n               new-item -path \"$initParametersPath\" -paramname $initParameterName  -paramValue \"$($customShellObject.$initParameterName)\" -Force\r\n            }}\r\n        }}\r\n   }}\r\n\r\n   # sddl processing\r\n   if ($isSddlSpecified)\r\n   {{\r\n       $resourcesPath = Join-Path \"$pluginDir\" 'Resources'\r\n       dir -literalpath \"$resourcesPath\" | % {{\r\n            $securityPath = Join-Path \"$($_.pspath)\" 'Security'\r\n            if ((@(dir -literalpath \"$securityPath\")).count -gt 0)\r\n            {{\r\n                dir -literalpath \"$securityPath\" | % {{\r\n                    $securityIDPath = \"$($_.pspath)\"\r\n                    remove-item -path \"$securityIDPath\" -recurse -force\r\n                }} #end of securityPath\r\n\r\n                if ($sddl)\r\n                {{\r\n                    new-item -path \"$securityPath\" -Sddl $sddl -force\r\n                }}\r\n            }}\r\n            else\r\n            {{\r\n                if ($sddl)\r\n                {{\r\n                    new-item -path \"$securityPath\" -Sddl $sddl -force\r\n                }}\r\n            }}\r\n       }} # end of resources\r\n       return\r\n   }} #end of sddl processing\r\n   elseif ($shouldShowUI)\r\n   {{\r\n        $null = winrm configsddl $resourceUri\r\n   }}\r\n\r\n   # If accessmode is 'Disabled', we don't bother to check the sddl\r\n   if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Disabled.Equals($accessMode))\r\n   {{\r\n        return\r\n   }}\r\n\r\n   # Construct SID for network users\r\n   [system.security.principal.wellknownsidtype]$evst = \"NetworkSid\"\r\n   $networkSID = new-object system.security.principal.securityidentifier $evst,$null\r\n\r\n   $resPath = Join-Path \"$pluginDir\" 'Resources'\r\n   dir -literalpath \"$resPath\" | % {{\r\n        $securityPath = Join-Path \"$($_.pspath)\" 'Security'\r\n        if ((@(dir -literalpath \"$securityPath\")).count -gt 0)\r\n        {{\r\n            dir -literalpath \"$securityPath\" | % {{\r\n                $sddlPath = Join-Path \"$($_.pspath)\" 'Sddl'\r\n                $curSDDL = (get-item -path $sddlPath).value\r\n                $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$curSDDL\r\n                $newSDDL = $null\r\n                \r\n                $disableNetworkExists = $false\r\n                $securityIdentifierToPurge = $null\r\n                $sd.DiscretionaryAcl | % {{\r\n                    if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $networkSID) -and ($_.AccessMask -eq 268435456))\r\n                    {{\r\n                        $disableNetworkExists = $true\r\n                        $securityIdentifierToPurge = $_.securityidentifier\r\n                    }}\r\n                }}\r\n\r\n                if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode) -and !$disableNetworkExists)\r\n                {{\r\n                    $sd.DiscretionaryAcl.AddAccess(\"deny\", $networkSID, 268435456, \"None\", \"None\")\r\n                    $newSDDL = $sd.GetSddlForm(\"all\")\r\n                }}\r\n                if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Remote.Equals($accessMode) -and $disableNetworkExists)\r\n                {{\r\n                    # Remove the specific ACE\r\n                    $sd.discretionaryacl.RemoveAccessSpecific('Deny', $securityIdentifierToPurge, 268435456, 'none', 'none')\r\n\r\n                    # if there is no discretionaryacl..add Builtin Administrators and Remote Management Users\r\n                    # to the DACL group as this is the default WSMan behavior\r\n                    if ($sd.discretionaryacl.count -eq 0)\r\n                    {{\r\n                        # Built-in administrators\r\n                        [system.security.principal.wellknownsidtype]$bast = \"BuiltinAdministratorsSid\"\r\n                        $basid = new-object system.security.principal.securityidentifier $bast,$null\r\n                        $sd.DiscretionaryAcl.AddAccess('Allow',$basid, 268435456, 'none', 'none')\r\n\r\n                        # Remote Management Users, Win8+ only\r\n                        if ([System.Environment]::OSVersion.Version.Major -ge 6 -and [System.Environment]::OSVersion.Version.Minor -ge 2)\r\n                        {{\r\n                            $rmSidId = new-object system.security.principal.securityidentifier \"{2}\"\r\n                            $sd.DiscretionaryAcl.AddAccess('Allow', $rmSidId, 268435456, 'none', 'none')\r\n                        }}\r\n                    }}\r\n\r\n                    $newSDDL = $sd.GetSddlForm(\"all\")\r\n                }}\r\n\r\n                if ($newSDDL)\r\n                {{\r\n                    set-item -WarningAction SilentlyContinue -path $sddlPath -value $newSDDL -force\r\n                }}\r\n            }}\r\n        }}\r\n        else\r\n        {{\r\n            if (([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode)))\r\n            {{\r\n                new-item -path \"$securityPath\" -Sddl \"{1}\" -force\r\n            }}\r\n        }}\r\n   }}\r\n}}\r\n\r\nSet-PSSessionConfiguration $args[0] $args[1] $args[2] $args[3] $args[4] $args[5] $args[6] $args[7] $args[8] $args[9]\r\n", new object[] { "pwrshplugin.dll", localSddl, "S-1-5-32-580" }));
            setPluginSb.LanguageMode = 0;
        }

        protected override void BeginProcessing()
        {
            if (base.isUseSharedProcessSpecified)
            {
                using (PowerShell shell = PowerShell.Create())
                {
                    shell.AddScript(string.Format(CultureInfo.InvariantCulture, @"(get-item 'WSMan::localhost\Plugin\{0}\InitializationParameters\sessiontype' -ErrorAction SilentlyContinue).Value", new object[] { CommandMetadata.EscapeSingleQuotedString(base.Name) }));
                    Collection<PSObject> collection = shell.Invoke(new object[] { base.Name });
                    if (collection != null)
                    {
                        int count = collection.Count;
                    }
                    if (((base.UseSharedProcess == 0) && (collection[0] != null)) && (string.Compare(collection[0].ToString(), "Workflow", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        throw new PSInvalidOperationException(RemotingErrorIdStrings.UseSharedProcessCannotBeFalseForWorkflowSessionType);
                    }
                }
            }
            if (base.isSddlSpecified && base.showUISpecified)
            {
                throw new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.ShowUIAndSDDLCannotExist, "SecurityDescriptorSddl", "ShowSecurityDescriptorUI"));
            }
            if (base.isRunAsCredentialSpecified)
            {
                base.WriteWarning(RemotingErrorIdStrings.RunAsSessionConfigurationSecurityWarning);
            }
            if (base.isSddlSpecified && base.accessModeSpecified)
            {
                CommonSecurityDescriptor descriptor = new CommonSecurityDescriptor(false, false, base.sddl);
                SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.NetworkSid, null);
                bool flag = false;
                AceEnumerator enumerator = descriptor.DiscretionaryAcl.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    CommonAce current = (CommonAce) enumerator.Current;
                    if ((current.AceQualifier.Equals(AceQualifier.AccessDenied) && current.SecurityIdentifier.Equals(sid)) && (current.AccessMask == 0x10000000))
                    {
                        flag = true;
                        break;
                    }
                }
                switch (base.AccessMode)
                {
                    case PSSessionConfigurationAccessMode.Local:
                        if (!flag)
                        {
                            descriptor.DiscretionaryAcl.AddAccess(AccessControlType.Deny, sid, 0x10000000, InheritanceFlags.None, PropagationFlags.None);
                            base.sddl = descriptor.GetSddlForm(AccessControlSections.All);
                        }
                        break;

                    case PSSessionConfigurationAccessMode.Remote:
                        if (flag)
                        {
                            descriptor.DiscretionaryAcl.RemoveAccessSpecific(AccessControlType.Deny, sid, 0x10000000, InheritanceFlags.None, PropagationFlags.None);
                            if (descriptor.DiscretionaryAcl.Count == 0)
                            {
                                SecurityIdentifier identifier2 = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                                descriptor.DiscretionaryAcl.AddAccess(AccessControlType.Allow, identifier2, 0x10000000, InheritanceFlags.None, PropagationFlags.None);
                                if ((Environment.OSVersion.Version.Major >= 6) && (Environment.OSVersion.Version.Minor >= 2))
                                {
                                    SecurityIdentifier identifier3 = new SecurityIdentifier("S-1-5-32-580");
                                    descriptor.DiscretionaryAcl.AddAccess(AccessControlType.Allow, identifier3, 0x10000000, InheritanceFlags.None, PropagationFlags.None);
                                }
                            }
                            base.sddl = descriptor.GetSddlForm(AccessControlSections.All);
                        }
                        break;
                }
            }
            RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
            PSSessionConfigurationCommandUtilities.ThrowIfNotAdministrator();
        }

        private PSObject ConstructPropertiesForUpdate()
        {
            PSObject obj2 = new PSObject();
            obj2.Properties.Add(new PSNoteProperty("Name", base.shellName));
            if (base.isAssemblyNameSpecified)
            {
                obj2.Properties.Add(new PSNoteProperty("assemblyname", base.assemblyName));
            }
            if (base.isApplicationBaseSpecified)
            {
                obj2.Properties.Add(new PSNoteProperty("applicationbase", base.applicationBase));
            }
            if (base.isConfigurationTypeNameSpecified)
            {
                obj2.Properties.Add(new PSNoteProperty("pssessionconfigurationtypename", base.configurationTypeName));
            }
            if (base.isConfigurationScriptSpecified)
            {
                obj2.Properties.Add(new PSNoteProperty("startupscript", base.configurationScript));
            }
            if (base.isMaxCommandSizeMBSpecified)
            {
                object obj3 = this.maxCommandSizeMB.HasValue ? (object)this.maxCommandSizeMB.Value : null;
                obj2.Properties.Add(new PSNoteProperty("psmaximumreceiveddatasizepercommandmb", obj3));
            }
            if (base.isMaxObjectSizeMBSpecified)
            {
                object obj4 = this.maxObjectSizeMB.HasValue ? (object)this.maxObjectSizeMB.Value : null;
                obj2.Properties.Add(new PSNoteProperty("psmaximumreceivedobjectsizemb", obj4));
            }
            if (this.threadAptState.HasValue)
            {
                obj2.Properties.Add(new PSNoteProperty("pssessionthreadapartmentstate", this.threadAptState.Value));
            }
            if (this.threadOptions.HasValue)
            {
                obj2.Properties.Add(new PSNoteProperty("pssessionthreadoptions", this.threadOptions.Value));
            }
            if (base.isPSVersionSpecified)
            {
                obj2.Properties.Add(new PSNoteProperty("PSVersion", PSSessionConfigurationCommandUtilities.ConstructVersionFormatForConfigXml(base.psVersion)));
                base.MaxPSVersion = PSSessionConfigurationCommandUtilities.CalculateMaxPSVersion(base.psVersion);
                obj2.Properties.Add(new PSNoteProperty("MaxPSVersion", PSSessionConfigurationCommandUtilities.ConstructVersionFormatForConfigXml(base.MaxPSVersion)));
            }
            if (base.modulePathSpecified && (base.sessionTypeOption == null))
            {
                bool flag = false;
                string str = null;
                if (((base.modulesToImport == null) || (base.modulesToImport.Length == 0)) || ((base.modulesToImport.Length == 1) && base.modulesToImport[0].Equals(string.Empty, StringComparison.OrdinalIgnoreCase)))
                {
                    flag = true;
                }
                else
                {
                    str = PSSessionConfigurationCommandUtilities.GetModulePathAsString(base.modulesToImport).Trim();
                }
                if (flag || !string.IsNullOrEmpty(str))
                {
                    using (PowerShell shell = PowerShell.Create())
                    {
                        bool flag2 = this.IsWorkflowConfigurationType(shell);
                        if (!string.IsNullOrEmpty(str) && flag2)
                        {
                            List<string> list = new List<string>(base.modulesToImport);
                            list.Insert(0, @"%windir%\system32\windowspowershell\v1.0\Modules\PSWorkflow");
                            str = PSSessionConfigurationCommandUtilities.GetModulePathAsString(list.ToArray()).Trim();
                        }
                        shell.AddScript(string.Format(CultureInfo.InvariantCulture, @"(Get-Item 'WSMan:\localhost\Plugin\{0}\InitializationParameters\SessionConfigurationData').Value", new object[] { CommandMetadata.EscapeSingleQuotedString(base.Name) }));
                        Collection<PSObject> collection = shell.Invoke(new object[] { base.Name });
                        if (collection != null)
                        {
                            int count = collection.Count;
                        }
                        StringBuilder builder = new StringBuilder();
                        if (collection[0] == null)
                        {
                            if (!flag)
                            {
                                builder.Append(string.Format(CultureInfo.InvariantCulture, "<Param Name='{0}' Value='{1}' />", new object[] { "modulestoimport", str }));
                            }
                        }
                        else
                        {
                            PSSessionConfigurationData data = PSSessionConfigurationData.Create(collection[0].BaseObject.ToString());
                            string str2 = string.IsNullOrEmpty(data.PrivateData) ? null : data.PrivateData.Replace('"', '\'');
                            if (flag)
                            {
                                if ((data.ModulesToImport != null) && (data.ModulesToImport.Count != 0))
                                {
                                    string str3 = flag2 ? @"%windir%\system32\windowspowershell\v1.0\Modules\PSWorkflow" : string.Empty;
                                    builder.Append(string.Format(CultureInfo.InvariantCulture, "<Param Name='{0}' Value='{1}' />", new object[] { "modulestoimport", str3 }));
                                    if (!string.IsNullOrEmpty(str2))
                                    {
                                        builder.Append(string.Format(CultureInfo.InvariantCulture, "<Param Name='PrivateData'>{0}</Param>", new object[] { str2 }));
                                    }
                                }
                            }
                            else
                            {
                                builder.Append(string.Format(CultureInfo.InvariantCulture, "<Param Name='{0}' Value='{1}' />", new object[] { "modulestoimport", str }));
                                if (!string.IsNullOrEmpty(str2))
                                {
                                    builder.Append(string.Format(CultureInfo.InvariantCulture, "<Param Name='PrivateData'>{0}</Param>", new object[] { str2 }));
                                }
                            }
                        }
                        if (builder.Length > 0)
                        {
                            string str5 = SecurityElement.Escape(string.Format(CultureInfo.InvariantCulture, "<SessionConfigurationData>{0}</SessionConfigurationData>", new object[] { builder }));
                            obj2.Properties.Add(new PSNoteProperty("sessionconfigurationdata", str5));
                        }
                    }
                }
            }
            if (base.Path != null)
            {
                PSDriveInfo info2;
                string str8;
                ProviderInfo provider = null;
                string fileName = base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(base.Path, out provider, out info2);
                if (!provider.NameEquals(base.Context.ProviderNames.FileSystem) || !fileName.EndsWith(".pssc", StringComparison.OrdinalIgnoreCase))
                {
                    InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.InvalidPSSessionConfigurationFilePath, base.Path));
                    ErrorRecord errorRecord = new ErrorRecord(exception, "InvalidPSSessionConfigurationFilePath", ErrorCategory.InvalidArgument, base.Path);
                    base.ThrowTerminatingError(errorRecord);
                }
                ExternalScriptInfo scriptInfo = DISCUtils.GetScriptInfoForFile(base.Context, fileName, out str8);
                Hashtable hashtable = DISCUtils.LoadConfigFile(base.Context, scriptInfo);
                foreach (object obj5 in hashtable.Keys)
                {
                    if (obj2.Properties[obj5.ToString()] == null)
                    {
                        obj2.Properties.Add(new PSNoteProperty(obj5.ToString(), hashtable[obj5]));
                    }
                    else
                    {
                        obj2.Properties[obj5.ToString()].Value = hashtable[obj5];
                    }
                }
            }
            return obj2;
        }

        protected override void EndProcessing()
        {
            PSSessionConfigurationCommandUtilities.RestartWinRMService(this, this.isErrorReported, (bool) base.Force, base.noRestart);
            if (!this.isErrorReported && base.noRestart)
            {
                string o = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, base.CommandInfo.Name);
                base.WriteWarning(StringUtil.Format(RemotingErrorIdStrings.WinRMRequiresRestart, o));
            }
            new Tracer().EndpointModified(base.Name, WindowsIdentity.GetCurrent().Name);
        }

        private bool IsWorkflowConfigurationType(PowerShell ps)
        {
            ps.AddScript(string.Format(CultureInfo.InvariantCulture, @"(Get-Item 'WSMan:\localhost\Plugin\{0}\InitializationParameters\assemblyname').Value", new object[] { CommandMetadata.EscapeSingleQuotedString(base.Name) }));
            Collection<PSObject> collection = ps.Invoke(new object[] { base.Name });
            if (collection != null)
            {
                int count = collection.Count;
            }
            if (collection[0] == null)
            {
                return false;
            }
            return collection[0].BaseObject.ToString().Equals("Microsoft.PowerShell.Workflow.ServiceCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL", StringComparison.OrdinalIgnoreCase);
        }

        protected override void ProcessRecord()
        {
            string str2;
            base.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.ScsScriptMessageV, "\r\nfunction Set-PSSessionConfiguration([PSObject]$customShellObject, \r\n     [Array]$initParametersMap,\r\n     [bool]$force,\r\n     [string]$sddl,\r\n     [bool]$isSddlSpecified,\r\n     [bool]$shouldShowUI,\r\n     [string]$resourceUri,\r\n     [string]$pluginNotFoundErrorMsg,\r\n     [string]$pluginNotPowerShellMsg,\r\n     [System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]$accessMode\r\n)\r\n{{\r\n   $wsmanPluginDir = 'WSMan:\\localhost\\Plugin'\r\n   $pluginName = $customShellObject.Name;\r\n   $pluginDir = Join-Path \"$wsmanPluginDir\" \"$pluginName\"\r\n   if ((!$pluginName) -or !(test-path \"$pluginDir\"))\r\n   {{\r\n      Write-Error $pluginNotFoundErrorMsg\r\n      return\r\n   }}\r\n\r\n   # check if the plugin is a PowerShell plugin   \r\n   $pluginFileNamePath = Join-Path \"$pluginDir\" 'FileName'\r\n   if (!(test-path \"$pluginFileNamePath\"))\r\n   {{\r\n      Write-Error $pluginNotPowerShellMsg\r\n      return\r\n   }}\r\n\r\n   $pluginFileName = get-item -literalpath \"$pluginFileNamePath\"\r\n   if ((!$pluginFileName) -or ($pluginFileName.Value -notmatch '{0}'))\r\n   {{\r\n      Write-Error $pluginNotPowerShellMsg\r\n      return\r\n   }}\r\n\r\n   # set Initialization Parameters\r\n   $initParametersPath = Join-Path \"$pluginDir\" 'InitializationParameters'  \r\n   foreach($initParameterName in $initParametersMap)\r\n   {{         \r\n        if ($customShellObject | get-member $initParameterName)\r\n        {{\r\n            $parampath = Join-Path \"$initParametersPath\" $initParameterName\r\n\r\n            if (test-path $parampath)\r\n            {{\r\n               remove-item -path \"$parampath\"\r\n            }}\r\n                \r\n            # 0 is an accepted value for MaximumReceivedDataSizePerCommandMB and MaximumReceivedObjectSizeMB\r\n            if (($customShellObject.$initParameterName) -or ($customShellObject.$initParameterName -eq 0))\r\n            {{\r\n               new-item -path \"$initParametersPath\" -paramname $initParameterName  -paramValue \"$($customShellObject.$initParameterName)\" -Force\r\n            }}\r\n        }}\r\n   }}\r\n\r\n   # sddl processing\r\n   if ($isSddlSpecified)\r\n   {{\r\n       $resourcesPath = Join-Path \"$pluginDir\" 'Resources'\r\n       dir -literalpath \"$resourcesPath\" | % {{\r\n            $securityPath = Join-Path \"$($_.pspath)\" 'Security'\r\n            if ((@(dir -literalpath \"$securityPath\")).count -gt 0)\r\n            {{\r\n                dir -literalpath \"$securityPath\" | % {{\r\n                    $securityIDPath = \"$($_.pspath)\"\r\n                    remove-item -path \"$securityIDPath\" -recurse -force\r\n                }} #end of securityPath\r\n\r\n                if ($sddl)\r\n                {{\r\n                    new-item -path \"$securityPath\" -Sddl $sddl -force\r\n                }}\r\n            }}\r\n            else\r\n            {{\r\n                if ($sddl)\r\n                {{\r\n                    new-item -path \"$securityPath\" -Sddl $sddl -force\r\n                }}\r\n            }}\r\n       }} # end of resources\r\n       return\r\n   }} #end of sddl processing\r\n   elseif ($shouldShowUI)\r\n   {{\r\n        $null = winrm configsddl $resourceUri\r\n   }}\r\n\r\n   # If accessmode is 'Disabled', we don't bother to check the sddl\r\n   if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Disabled.Equals($accessMode))\r\n   {{\r\n        return\r\n   }}\r\n\r\n   # Construct SID for network users\r\n   [system.security.principal.wellknownsidtype]$evst = \"NetworkSid\"\r\n   $networkSID = new-object system.security.principal.securityidentifier $evst,$null\r\n\r\n   $resPath = Join-Path \"$pluginDir\" 'Resources'\r\n   dir -literalpath \"$resPath\" | % {{\r\n        $securityPath = Join-Path \"$($_.pspath)\" 'Security'\r\n        if ((@(dir -literalpath \"$securityPath\")).count -gt 0)\r\n        {{\r\n            dir -literalpath \"$securityPath\" | % {{\r\n                $sddlPath = Join-Path \"$($_.pspath)\" 'Sddl'\r\n                $curSDDL = (get-item -path $sddlPath).value\r\n                $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$curSDDL\r\n                $newSDDL = $null\r\n                \r\n                $disableNetworkExists = $false\r\n                $securityIdentifierToPurge = $null\r\n                $sd.DiscretionaryAcl | % {{\r\n                    if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $networkSID) -and ($_.AccessMask -eq 268435456))\r\n                    {{\r\n                        $disableNetworkExists = $true\r\n                        $securityIdentifierToPurge = $_.securityidentifier\r\n                    }}\r\n                }}\r\n\r\n                if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode) -and !$disableNetworkExists)\r\n                {{\r\n                    $sd.DiscretionaryAcl.AddAccess(\"deny\", $networkSID, 268435456, \"None\", \"None\")\r\n                    $newSDDL = $sd.GetSddlForm(\"all\")\r\n                }}\r\n                if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Remote.Equals($accessMode) -and $disableNetworkExists)\r\n                {{\r\n                    # Remove the specific ACE\r\n                    $sd.discretionaryacl.RemoveAccessSpecific('Deny', $securityIdentifierToPurge, 268435456, 'none', 'none')\r\n\r\n                    # if there is no discretionaryacl..add Builtin Administrators and Remote Management Users\r\n                    # to the DACL group as this is the default WSMan behavior\r\n                    if ($sd.discretionaryacl.count -eq 0)\r\n                    {{\r\n                        # Built-in administrators\r\n                        [system.security.principal.wellknownsidtype]$bast = \"BuiltinAdministratorsSid\"\r\n                        $basid = new-object system.security.principal.securityidentifier $bast,$null\r\n                        $sd.DiscretionaryAcl.AddAccess('Allow',$basid, 268435456, 'none', 'none')\r\n\r\n                        # Remote Management Users, Win8+ only\r\n                        if ([System.Environment]::OSVersion.Version.Major -ge 6 -and [System.Environment]::OSVersion.Version.Minor -ge 2)\r\n                        {{\r\n                            $rmSidId = new-object system.security.principal.securityidentifier \"{2}\"\r\n                            $sd.DiscretionaryAcl.AddAccess('Allow', $rmSidId, 268435456, 'none', 'none')\r\n                        }}\r\n                    }}\r\n\r\n                    $newSDDL = $sd.GetSddlForm(\"all\")\r\n                }}\r\n\r\n                if ($newSDDL)\r\n                {{\r\n                    set-item -WarningAction SilentlyContinue -path $sddlPath -value $newSDDL -force\r\n                }}\r\n            }}\r\n        }}\r\n        else\r\n        {{\r\n            if (([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode)))\r\n            {{\r\n                new-item -path \"$securityPath\" -Sddl \"{1}\" -force\r\n            }}\r\n        }}\r\n   }}\r\n}}\r\n\r\nSet-PSSessionConfiguration $args[0] $args[1] $args[2] $args[3] $args[4] $args[5] $args[6] $args[7] $args[8] $args[9]\r\n"));
            string action = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, base.CommandInfo.Name);
            if (!base.isSddlSpecified)
            {
                str2 = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessTarget, base.Name);
            }
            else
            {
                str2 = StringUtil.Format(RemotingErrorIdStrings.ScsShouldProcessTargetSDDL, base.Name, base.sddl);
            }
            if (!base.noRestart && !base.force)
            {
                string o = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, base.CommandInfo.Name);
                base.WriteWarning(StringUtil.Format(RemotingErrorIdStrings.WinRMRestartWarning, o));
            }
            if (base.force || base.ShouldProcess(str2, action))
            {
                string str4 = StringUtil.Format(RemotingErrorIdStrings.CSCmdsShellNotFound, base.shellName);
                string str5 = StringUtil.Format(RemotingErrorIdStrings.CSCmdsShellNotPowerShellBased, base.shellName);
                PSObject obj2 = this.ConstructPropertiesForUpdate();
                ArrayList dollarErrorVariable = (ArrayList) base.Context.DollarErrorVariable;
                int count = dollarErrorVariable.Count;
                setPluginSb.InvokeUsingCmdlet(this, true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, new object[0], AutomationNull.Value, new object[] { obj2, initParametersMap, base.force, base.sddl, base.isSddlSpecified, base.ShowSecurityDescriptorUI.ToBool(), "http://schemas.microsoft.com/powershell/" + base.shellName, str4, str5, base.accessModeSpecified ? ((object) base.AccessMode) : ((object) 0) });
                dollarErrorVariable = (ArrayList) base.Context.DollarErrorVariable;
                this.isErrorReported = dollarErrorVariable.Count > count;
                if (!this.isErrorReported)
                {
                    count = dollarErrorVariable.Count;
                    this.SetSessionConfigurationTypeOptions();
                    dollarErrorVariable = (ArrayList) base.Context.DollarErrorVariable;
                    this.isErrorReported = dollarErrorVariable.Count > count;
                    if (!this.isErrorReported)
                    {
                        count = dollarErrorVariable.Count;
                        this.SetQuotas();
                        dollarErrorVariable = (ArrayList) base.Context.DollarErrorVariable;
                        this.isErrorReported = dollarErrorVariable.Count > count;
                        if (!this.isErrorReported)
                        {
                            count = dollarErrorVariable.Count;
                            this.SetRunAs();
                            dollarErrorVariable = (ArrayList) base.Context.DollarErrorVariable;
                            this.isErrorReported = dollarErrorVariable.Count > count;
                            if (!this.isErrorReported)
                            {
                                count = dollarErrorVariable.Count;
                                this.SetOptions();
                                dollarErrorVariable = (ArrayList) base.Context.DollarErrorVariable;
                                this.isErrorReported = dollarErrorVariable.Count > count;
                            }
                            if (!this.isErrorReported && (base.Path != null))
                            {
                                PSDriveInfo info2;
                                string str7;
                                ProviderInfo provider = null;
                                string fileName = base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(base.Path, out provider, out info2);
                                Guid empty = Guid.Empty;
                                ExternalScriptInfo scriptInfo = DISCUtils.GetScriptInfoForFile(base.Context, fileName, out str7);
                                Hashtable hashtable = DISCUtils.LoadConfigFile(base.Context, scriptInfo);
                                if ((hashtable != null) && hashtable.ContainsKey(ConfigFileContants.Guid))
                                {
                                    empty = Guid.Parse(hashtable[ConfigFileContants.Guid].ToString());
                                }
                                string destFileName = System.IO.Path.Combine(Utils.GetApplicationBase(Utils.DefaultPowerShellShellID), "SessionConfig", base.shellName + "_" + empty.ToString() + ".pssc");
                                File.Copy(fileName, destFileName, true);
                            }
                        }
                    }
                }
            }
        }

        private void SetOptions()
        {
            string str;
            ScriptBlock block = ScriptBlock.Create(string.Format(CultureInfo.InvariantCulture, "\r\nfunction Set-SessionPluginOptions([hashtable] $options) {{\r\n    if ($options[\"UsedSharedProcess\"]) {{\r\n        $value = $options[\"UseSharedProcess\"];\r\n        set-item -WarningAction SilentlyContinue 'WSMan:\\localhost\\Plugin\\{0}\\UseSharedProcess' -Value $value -confirm:$false\r\n        $options.Remove(\"UseSharedProcess\");\r\n    }}\r\n    foreach($v in $options.GetEnumerator()) {{\r\n        $name = $v.Name; \r\n        $value = $v.Value\r\n\r\n        if (!$value) {{\r\n            $value = 0;\r\n        }}\r\n\r\n        set-item -WarningAction SilentlyContinue ('WSMan:\\localhost\\Plugin\\{0}\\' + $name) -Value $value -confirm:$false\r\n    }}\r\n}}\r\nSet-SessionPluginOptions $args[0]\r\n", new object[] { CommandMetadata.EscapeSingleQuotedString(base.Name) }));
            block.LanguageMode = 0;
            Hashtable hashtable = (base.transportOption != null) ? base.transportOption.ConstructOptionsAsHashtable() : new Hashtable();
            if (hashtable.ContainsKey("OutputBufferingMode") && LanguagePrimitives.TryConvertTo<string>(hashtable["OutputBufferingMode"], out str))
            {
                PSSQMAPI.NoteSessionConfigurationOutputBufferingMode(str);
            }
            if (base.accessModeSpecified)
            {
                switch (base.AccessMode)
                {
                    case PSSessionConfigurationAccessMode.Disabled:
                        hashtable["Enabled"] = false.ToString();
                        break;

                    case PSSessionConfigurationAccessMode.Local:
                    case PSSessionConfigurationAccessMode.Remote:
                        hashtable["Enabled"] = true.ToString();
                        break;
                }
            }
            if (base.isUseSharedProcessSpecified)
            {
                hashtable["UseSharedProcess"] = base.UseSharedProcess.ToBool().ToString();
            }
            block.InvokeUsingCmdlet(this, true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, new object[0], AutomationNull.Value, new object[] { hashtable });
        }

        private void SetQuotas()
        {
            if (base.transportOption != null)
            {
                ScriptBlock block = ScriptBlock.Create(string.Format(CultureInfo.InvariantCulture, "\r\nfunction Set-SessionPluginQuota([hashtable] $quotas) {{\r\n    foreach($v in $quotas.GetEnumerator()) {{\r\n        $name = $v.Name; \r\n        $value = $v.Value;\r\n        if (!$value) {{\r\n            $value = [string]::empty;\r\n        }}\r\n        set-item -WarningAction SilentlyContinue ('WSMan:\\localhost\\Plugin\\{0}\\Quotas\\' + $name) -Value $value -confirm:$false\r\n    }}\r\n}}\r\nSet-SessionPluginQuota $args[0]\r\n", new object[] { CommandMetadata.EscapeSingleQuotedString(base.Name) }));
                block.LanguageMode = 0;
                Hashtable hashtable = base.transportOption.ConstructQuotasAsHashtable();
                int result = 0;
                if (hashtable.ContainsKey("IdleTimeoutms") && LanguagePrimitives.TryConvertTo<int>(hashtable["IdleTimeoutms"], out result))
                {
                    PSSQMAPI.NoteSessionConfigurationIdleTimeout(result);
                }
                if ((result != 0) && hashtable.ContainsKey("MaxIdleTimeoutms"))
                {
                    int num2;
                    bool flag = true;
                    if (LanguagePrimitives.TryConvertTo<int>(hashtable["MaxIdleTimeoutms"], out num2))
                    {
                        int? defaultIdleTimeout = WSManConfigurationOption.DefaultIdleTimeout;
                        using (PowerShell shell = PowerShell.Create())
                        {
                            shell.AddScript(string.Format(CultureInfo.InvariantCulture, @"(Get-Item 'WSMan:\localhost\Plugin\{0}\Quotas\IdleTimeoutms').Value", new object[] { CommandMetadata.EscapeSingleQuotedString(base.Name) }));
                            Collection<PSObject> collection = shell.Invoke(new object[] { base.Name });
                            if (collection != null)
                            {
                                int count = collection.Count;
                            }
                            defaultIdleTimeout = new int?(Convert.ToInt32(collection[0].ToString(), CultureInfo.InvariantCulture));
                        }
                        int? nullable2 = defaultIdleTimeout;
                        int num3 = num2;
                        if ((nullable2.GetValueOrDefault() >= num3) && nullable2.HasValue)
                        {
                            int? nullable3 = defaultIdleTimeout;
                            int num4 = result;
                            if ((nullable3.GetValueOrDefault() >= num4) && nullable3.HasValue)
                            {
                                flag = false;
                            }
                        }
                    }
                    ScriptBlock block2 = ScriptBlock.Create(string.Format(CultureInfo.InvariantCulture, "\r\nfunction Set-SessionPluginIdleTimeoutQuotas([int] $maxIdleTimeoutms, [int] $idleTimeoutms, [bool] $setMaxIdleTimoutFirst) {{\r\n    if ($setMaxIdleTimoutFirst) {{\r\n        set-item -WarningAction SilentlyContinue 'WSMan:\\localhost\\Plugin\\{0}\\Quotas\\MaxIdleTimeoutms' -Value $maxIdleTimeoutms -confirm:$false\r\n        set-item -WarningAction SilentlyContinue 'WSMan:\\localhost\\Plugin\\{0}\\Quotas\\IdleTimeoutms' -Value $idleTimeoutms -confirm:$false\r\n    }}\r\n    else {{\r\n        set-item -WarningAction SilentlyContinue 'WSMan:\\localhost\\Plugin\\{0}\\Quotas\\IdleTimeoutms' -Value $idleTimeoutms -confirm:$false\r\n        set-item -WarningAction SilentlyContinue 'WSMan:\\localhost\\Plugin\\{0}\\Quotas\\MaxIdleTimeoutms' -Value $maxIdleTimeoutms -confirm:$false\r\n    }}\r\n}}\r\nSet-SessionPluginIdleTimeoutQuotas $args[0] $args[1] $args[2]\r\n", new object[] { CommandMetadata.EscapeSingleQuotedString(base.Name) }));
                    block2.LanguageMode = 0;
                    block2.InvokeUsingCmdlet(this, true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, new object[0], AutomationNull.Value, new object[] { num2, result, flag });
                    hashtable.Remove("MaxIdleTimeoutms");
                    hashtable.Remove("IdleTimeoutms");
                }
                block.InvokeUsingCmdlet(this, true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, new object[0], AutomationNull.Value, new object[] { hashtable });
            }
        }

        private void SetRunAs()
        {
            if (base.runAsCredential != null)
            {
                ScriptBlock block = ScriptBlock.Create(string.Format(CultureInfo.InvariantCulture, "\r\nfunction Set-RunAsCredential{{\r\n    param (\r\n        [string]$runAsUserName,\r\n\t    [system.security.securestring]$runAsPassword\r\n    )\r\n\r\n    $cred = new-object System.Management.Automation.PSCredential($runAsUserName, $runAsPassword)\r\n    set-item -WarningAction SilentlyContinue 'WSMan:\\localhost\\Plugin\\{0}\\RunAsUser' $cred -confirm:$false\r\n}}\r\nSet-RunAsCredential $args[0] $args[1]\r\n", new object[] { CommandMetadata.EscapeSingleQuotedString(base.Name) }));
                block.LanguageMode = 0;
                block.InvokeUsingCmdlet(this, true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, new object[0], AutomationNull.Value, new object[] { base.runAsCredential.UserName, base.runAsCredential.Password });
            }
        }

        private void SetSessionConfigurationTypeOptions()
        {
            if (base.sessionTypeOption != null)
            {
                using (PowerShell shell = PowerShell.Create())
                {
                    shell.AddScript(string.Format(CultureInfo.InvariantCulture, @"(Get-Item 'WSMan:\localhost\Plugin\{0}\InitializationParameters\SessionConfigurationData').Value", new object[] { CommandMetadata.EscapeSingleQuotedString(base.Name) }));
                    Collection<PSObject> collection = shell.Invoke(new object[] { base.Name });
                    if (collection != null)
                    {
                        int count = collection.Count;
                    }
                    PSSessionConfigurationData data = PSSessionConfigurationData.Create((collection[0] == null) ? string.Empty : collection[0].BaseObject.ToString());
                    PSSessionTypeOption option = base.sessionTypeOption.ConstructObjectFromPrivateData(data.PrivateData);
                    option.CopyUpdatedValuesFrom(base.sessionTypeOption);
                    StringBuilder builder = new StringBuilder();
                    string str = null;
                    string str2 = string.Empty;
                    bool flag = false;
                    if (base.modulePathSpecified)
                    {
                        bool flag2 = this.IsWorkflowConfigurationType(shell);
                        if (((base.modulesToImport == null) || (base.modulesToImport.Length == 0)) || ((base.modulesToImport.Length == 1) && base.modulesToImport[0].Equals(string.Empty, StringComparison.OrdinalIgnoreCase)))
                        {
                            flag = true;
                            str2 = flag2 ? @"%windir%\system32\windowspowershell\v1.0\Modules\PSWorkflow" : string.Empty;
                        }
                        else
                        {
                            str = PSSessionConfigurationCommandUtilities.GetModulePathAsString(base.modulesToImport).Trim();
                            if (!string.IsNullOrEmpty(str) && flag2)
                            {
                                List<string> list = new List<string>(base.modulesToImport);
                                list.Insert(0, @"%windir%\system32\windowspowershell\v1.0\Modules\PSWorkflow");
                                str = PSSessionConfigurationCommandUtilities.GetModulePathAsString(list.ToArray()).Trim();
                            }
                        }
                    }
                    if (!flag && string.IsNullOrEmpty(str))
                    {
                        str = (data.ModulesToImport == null) ? null : PSSessionConfigurationCommandUtilities.GetModulePathAsString(data.ModulesToImport.ToArray()).Trim();
                    }
                    if (flag || string.IsNullOrEmpty(str))
                    {
                        builder.Append(string.Format(CultureInfo.InvariantCulture, "<Param Name='{0}' Value='{1}' />", new object[] { "modulestoimport", str2 }));
                    }
                    else
                    {
                        builder.Append(string.Format(CultureInfo.InvariantCulture, "<Param Name='{0}' Value='{1}' />", new object[] { "modulestoimport", str }));
                    }
                    string str3 = option.ConstructPrivateData();
                    if (!string.IsNullOrEmpty(str3))
                    {
                        builder.Append(string.Format(CultureInfo.InvariantCulture, "<Param Name='PrivateData'>{0}</Param>", new object[] { str3 }));
                    }
                    if (builder.Length > 0)
                    {
                        string str5 = SecurityElement.Escape(string.Format(CultureInfo.InvariantCulture, "<SessionConfigurationData>{0}</SessionConfigurationData>", new object[] { builder }));
                        ScriptBlock block = ScriptBlock.Create(string.Format(CultureInfo.InvariantCulture, "\r\nfunction Set-SessionConfigurationData([string] $scd) {{\r\n    if (test-path 'WSMan:\\localhost\\Plugin\\{0}\\InitializationParameters\\sessionconfigurationdata')\r\n    {{\r\n        set-item -WarningAction SilentlyContinue -Force 'WSMan:\\localhost\\Plugin\\{0}\\InitializationParameters\\sessionconfigurationdata' -Value $scd\r\n    }}\r\n    else\r\n    {{\r\n        new-item -WarningAction SilentlyContinue -path 'WSMan:\\localhost\\Plugin\\{0}\\InitializationParameters' -paramname sessionconfigurationdata -paramValue $scd -Force\r\n    }}\r\n}}\r\nSet-SessionConfigurationData $args[0]\r\n", new object[] { CommandMetadata.EscapeSingleQuotedString(base.Name) }));
                        block.LanguageMode = 0;
                        block.InvokeUsingCmdlet(this, true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, new object[0], AutomationNull.Value, new object[] { str5 });
                    }
                }
            }
        }
    }
}

