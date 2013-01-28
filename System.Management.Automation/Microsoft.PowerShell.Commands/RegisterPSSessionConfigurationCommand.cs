namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
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

    [Cmdlet("Register", "PSSessionConfiguration", DefaultParameterSetName="NameParameterSet", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High, HelpUri="http://go.microsoft.com/fwlink/?LinkID=144306")]
    public sealed class RegisterPSSessionConfigurationCommand : PSSessionConfigurationCommandBase
    {
        private const string allowRemoteShellAccessFormat = "\r\n\tEnabled='{0}'";
        private string architecture;
        private const string architectureAttribFormat = "\r\n\tArchitecture='{0}'";
        private const string initParamFormat = "\r\n<Param Name='{0}' Value='{1}' />{2}";
        private bool isErrorReported;
        private static readonly ScriptBlock newPluginSb;
        private const string newPluginSbFormat = "\r\nfunction Register-PSSessionConfiguration\r\n{{\r\n    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\n    param(  \r\n      [string]$filepath,\r\n      [string]$pluginName,\r\n      [bool]$shouldShowUI,\r\n      [bool]$force,\r\n      [bool]$noServiceRestart,\r\n      [string]$restartWSManTarget,\r\n      [string]$restartWSManAction,\r\n      [string]$restartWSManRequired,\r\n\t  [string]$runAsUserName,\r\n\t  [system.security.securestring]$runAsPassword,\r\n      [System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]$accessMode,\r\n      [bool]$isSddlSpecified\r\n    )\r\n\r\n    begin\r\n    {{\r\n        ## Construct SID for network users\r\n        [system.security.principal.wellknownsidtype]$evst = \"NetworkSid\"\r\n        $networkSID = new-object system.security.principal.securityidentifier $evst,$null\r\n\r\n        ## If all session configurations have Network Access disabled,\r\n        ## then we create this endpoint as Local as well.\r\n        $newSDDL = $null\r\n        $foundRemoteEndpoint = $false;\r\n        Get-PSSessionConfiguration | Foreach-Object {{\r\n            if ($_.Enabled)\r\n            {{\r\n                $sddl = $null\r\n                if ($_.psobject.members[\"SecurityDescriptorSddl\"])\r\n                {{\r\n                    $sddl = $_.psobject.members[\"SecurityDescriptorSddl\"].Value\r\n                }}\r\n        \r\n                if($sddl)\r\n                {{\r\n                    # See if it has 'Disable Network Access'\r\n                    $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddl\r\n                    $disableNetworkExists = $false\r\n                    $sd.DiscretionaryAcl | % {{\r\n                        if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $networkSID) -and ($_.AccessMask -eq 268435456))\r\n                        {{\r\n                            $disableNetworkExists = $true              \r\n                        }}\r\n                    }}\r\n\r\n                    if(-not $disableNetworkExists) {{ $foundRemoteEndpoint = $true }}\r\n                }}\r\n            }}\r\n        }}\r\n\r\n        if(-not $foundRemoteEndpoint)\r\n        {{\r\n            $newSDDL = \"{1}\"\r\n        }}\r\n    }}\r\n\r\n    process\r\n    {{\r\n        if ($force)\r\n        {{\r\n            if (Test-Path WSMan:\\localhost\\Plugin\\\"$pluginName\")\r\n            {{\r\n                Unregister-PSSessionConfiguration -name \"$pluginName\" -force\r\n            }}\r\n        }}\r\n\r\n        new-item -path WSMan:\\localhost\\Plugin -file \"$filepath\" -name \"$pluginName\"\r\n        # $? is to make sure the last operation is succeeded\r\n\r\n\t\tif ($? -and $runAsUserName) \r\n\t\t{{\r\n\t\t\ttry {{\r\n\t\t\t\t$runAsCredential = new-object system.management.automation.PSCredential($runAsUserName, $runAsPassword)\r\n\t\t\t\tset-item -WarningAction SilentlyContinue WSMan:\\localhost\\Plugin\\\"$pluginName\"\\RunAsUser $runAsCredential -confirm:$false\r\n\t\t\t}} catch {{\r\n\t\t\t\tremove-item WSMan:\\localhost\\Plugin\\\"$pluginName\" -recurse -force\r\n\t\t\t\twrite-error $_\r\n                # Do not add anymore clean up code after Write-Error, because if EA=Stop is set by user\r\n                # any code at this point will not execute.\r\n\r\n\t\t\t\treturn\r\n\t\t\t}}\r\n\t\t}}\r\n\r\n        if ($? -and $shouldShowUI)\r\n        {{\r\n           if ($noServiceRestart)\r\n           {{\r\n               write-error $restartWSManRequired\r\n               return\r\n           }}\r\n\r\n           if ($force -or $pscmdlet.shouldprocess($restartWSManTarget, $restartWSManAction))\r\n           {{\r\n               restart-service winrm -force\r\n               $null = winrm configsddl \"{0}$pluginName\"\r\n           }}\r\n           else\r\n           {{\r\n               write-error $restartWSManRequired\r\n               return \r\n           }}\r\n\r\n           # if AccessMode is Disabled OR the winrm configsddl failed, we just return\r\n           if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Disabled.Equals($accessMode) -or !$?)\r\n           {{\r\n               return\r\n           }}\r\n        }} # end of if ($shouldShowUI)\r\n\r\n        if ($? -and ($shouldShowUI -or $isSddlSpecified))\r\n        {{\r\n           # if AccessMode is Local or Remote, we need to check the SDDL the user set in the UI or passed in to the cmdlet.\r\n           $newSDDL = $null\r\n           $curPlugin = Get-PSSessionConfiguration -Name $pluginName\r\n           $curSDDL = $curPlugin.SecurityDescriptorSddl\r\n           if (!$curSDDL)\r\n           {{\r\n               if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode))\r\n               {{\r\n                    $newSDDL = \"{1}\"\r\n               }}\r\n           }}\r\n           else\r\n           {{\r\n               # Construct SID for network users\r\n               [system.security.principal.wellknownsidtype]$evst = \"NetworkSid\"\r\n               $networkSID = new-object system.security.principal.securityidentifier $evst,$null\r\n                \r\n               $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$curSDDL\r\n               $haveDisableACE = $false\r\n               $securityIdentifierToPurge = $null\r\n               $sd.DiscretionaryAcl | % {{\r\n                    if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $networkSID) -and ($_.AccessMask -eq 268435456))\r\n                    {{\r\n                        $haveDisableACE = $true\r\n                        $securityIdentifierToPurge = $_.securityidentifier\r\n                    }}\r\n               }}\r\n               if (([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode) -or\r\n                    ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Remote.Equals($accessMode)-and $disableNetworkExists)) -and\r\n                   !$haveDisableACE)\r\n               {{\r\n                    # Add network deny ACE for local access or remote access with PSRemoting disabled ($disableNetworkExists)\r\n                    $sd.DiscretionaryAcl.AddAccess(\"deny\", $networkSID, 268435456, \"None\", \"None\")\r\n                    $newSDDL = $sd.GetSddlForm(\"all\")\r\n               }}\r\n               if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Remote.Equals($accessMode) -and -not $disableNetworkExists -and $haveDisableACE)\r\n               {{\r\n                    # Remove the specific ACE\r\n                    $sd.discretionaryacl.RemoveAccessSpecific('Deny', $securityIdentifierToPurge, 268435456, 'none', 'none')\r\n\r\n                    # if there is no discretionaryacl..add Builtin Administrators and Remote Management Users\r\n                    # to the DACL group as this is the default WSMan behavior\r\n                    if ($sd.discretionaryacl.count -eq 0)\r\n                    {{\r\n                        [system.security.principal.wellknownsidtype]$bast = \"BuiltinAdministratorsSid\"\r\n                        $basid = new-object system.security.principal.securityidentifier $bast,$null\r\n                        $sd.DiscretionaryAcl.AddAccess('Allow',$basid, 268435456, 'none', 'none')\r\n\r\n                        # Remote Management Users, Win8+ only\r\n                        if ([System.Environment]::OSVersion.Version.Major -ge 6 -and [System.Environment]::OSVersion.Version.Minor -ge 2)\r\n                        {{\r\n                            $rmSidId = new-object system.security.principal.securityidentifier \"{2}\"\r\n                            $sd.DiscretionaryAcl.AddAccess('Allow', $rmSidId, 268435456, 'none', 'none')\r\n                        }}\r\n                    }}\r\n\r\n                    $newSDDL = $sd.GetSddlForm(\"all\")\r\n               }}\r\n           }} # end of if(!$curSDDL)\r\n        }} # end of if ($shouldShowUI -or $isSddlSpecified)\r\n\r\n        if ($? -and $newSDDL)\r\n        {{\r\n            try {{\r\n                if ($runAsUserName)\r\n                {{\r\n                    $runAsCredential = new-object system.management.automation.PSCredential($runAsUserName, $runAsPassword)\r\n                    $null = Set-PSSessionConfiguration -Name $pluginName -SecurityDescriptorSddl $newSDDL -NoServiceRestart -force -WarningAction 0 -RunAsCredential $runAsCredential\r\n                }}\r\n                else\r\n                {{\r\n                    $null = Set-PSSessionConfiguration -Name $pluginName -SecurityDescriptorSddl $newSDDL -NoServiceRestart -force -WarningAction 0\r\n                }}\r\n            }} catch {{\r\n\t\t\t\tremove-item WSMan:\\localhost\\Plugin\\\"$pluginName\" -recurse -force\r\n\t\t\t\twrite-error $_\r\n                # Do not add anymore clean up code after Write-Error, because if EA=Stop is set by user\r\n                # any code at this point will not execute.\r\n\r\n\t\t\t\treturn\r\n\t\t\t}}\r\n        }}\r\n    }}\r\n}}\r\n\r\nRegister-PSSessionConfiguration -filepath $args[0] -pluginName $args[1] -shouldShowUI $args[2] -force $args[3] -noServiceRestart $args[4] -whatif:$args[5] -confirm:$args[6] -restartWSManTarget $args[7] -restartWSManAction $args[8] -restartWSManRequired $args[9] -runAsUserName $args[10] -runAsPassword $args[11] -accessMode $args[12] -isSddlSpecified $args[13]\r\n";
        private const string pluginXmlFormat = "\r\n<PlugInConfiguration xmlns='http://schemas.microsoft.com/wbem/wsman/1/config/PluginConfiguration'\r\n    Name='{0}'\r\n    Filename='%windir%\\system32\\{1}'\r\n    SDKVersion='{10}'\r\n    XmlRenderingType='text' {2} {6} {7} {8}>\r\n  <InitializationParameters>    \r\n{3}\r\n  </InitializationParameters> \r\n  <Resources>\r\n    <Resource ResourceUri='{4}' SupportsOptions='true' ExactMatch='true'>\r\n{5}\r\n      <Capability Type='Shell' />\r\n    </Resource>\r\n  </Resources>\r\n  {9}\r\n</PlugInConfiguration>\r\n";
        private const string privateDataFormat = "<Param Name='PrivateData'>{0}</Param>";
        private const string securityElementFormat = "<Security Uri='{0}' ExactMatch='true' Sddl='{1}' />";
        private const string SessionConfigDataFormat = "<SessionConfigurationData>{0}</SessionConfigurationData>";
        internal PSSessionType sessionType;
        private const string sharedHostAttribFormat = "\r\n\tUseSharedProcess='{0}'";

        static RegisterPSSessionConfigurationCommand()
        {
            string localSddl = PSSessionConfigurationCommandBase.GetLocalSddl();
            newPluginSb = ScriptBlock.Create(string.Format(CultureInfo.InvariantCulture, "\r\nfunction Register-PSSessionConfiguration\r\n{{\r\n    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\n    param(  \r\n      [string]$filepath,\r\n      [string]$pluginName,\r\n      [bool]$shouldShowUI,\r\n      [bool]$force,\r\n      [bool]$noServiceRestart,\r\n      [string]$restartWSManTarget,\r\n      [string]$restartWSManAction,\r\n      [string]$restartWSManRequired,\r\n\t  [string]$runAsUserName,\r\n\t  [system.security.securestring]$runAsPassword,\r\n      [System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]$accessMode,\r\n      [bool]$isSddlSpecified\r\n    )\r\n\r\n    begin\r\n    {{\r\n        ## Construct SID for network users\r\n        [system.security.principal.wellknownsidtype]$evst = \"NetworkSid\"\r\n        $networkSID = new-object system.security.principal.securityidentifier $evst,$null\r\n\r\n        ## If all session configurations have Network Access disabled,\r\n        ## then we create this endpoint as Local as well.\r\n        $newSDDL = $null\r\n        $foundRemoteEndpoint = $false;\r\n        Get-PSSessionConfiguration | Foreach-Object {{\r\n            if ($_.Enabled)\r\n            {{\r\n                $sddl = $null\r\n                if ($_.psobject.members[\"SecurityDescriptorSddl\"])\r\n                {{\r\n                    $sddl = $_.psobject.members[\"SecurityDescriptorSddl\"].Value\r\n                }}\r\n        \r\n                if($sddl)\r\n                {{\r\n                    # See if it has 'Disable Network Access'\r\n                    $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddl\r\n                    $disableNetworkExists = $false\r\n                    $sd.DiscretionaryAcl | % {{\r\n                        if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $networkSID) -and ($_.AccessMask -eq 268435456))\r\n                        {{\r\n                            $disableNetworkExists = $true              \r\n                        }}\r\n                    }}\r\n\r\n                    if(-not $disableNetworkExists) {{ $foundRemoteEndpoint = $true }}\r\n                }}\r\n            }}\r\n        }}\r\n\r\n        if(-not $foundRemoteEndpoint)\r\n        {{\r\n            $newSDDL = \"{1}\"\r\n        }}\r\n    }}\r\n\r\n    process\r\n    {{\r\n        if ($force)\r\n        {{\r\n            if (Test-Path WSMan:\\localhost\\Plugin\\\"$pluginName\")\r\n            {{\r\n                Unregister-PSSessionConfiguration -name \"$pluginName\" -force\r\n            }}\r\n        }}\r\n\r\n        new-item -path WSMan:\\localhost\\Plugin -file \"$filepath\" -name \"$pluginName\"\r\n        # $? is to make sure the last operation is succeeded\r\n\r\n\t\tif ($? -and $runAsUserName) \r\n\t\t{{\r\n\t\t\ttry {{\r\n\t\t\t\t$runAsCredential = new-object system.management.automation.PSCredential($runAsUserName, $runAsPassword)\r\n\t\t\t\tset-item -WarningAction SilentlyContinue WSMan:\\localhost\\Plugin\\\"$pluginName\"\\RunAsUser $runAsCredential -confirm:$false\r\n\t\t\t}} catch {{\r\n\t\t\t\tremove-item WSMan:\\localhost\\Plugin\\\"$pluginName\" -recurse -force\r\n\t\t\t\twrite-error $_\r\n                # Do not add anymore clean up code after Write-Error, because if EA=Stop is set by user\r\n                # any code at this point will not execute.\r\n\r\n\t\t\t\treturn\r\n\t\t\t}}\r\n\t\t}}\r\n\r\n        if ($? -and $shouldShowUI)\r\n        {{\r\n           if ($noServiceRestart)\r\n           {{\r\n               write-error $restartWSManRequired\r\n               return\r\n           }}\r\n\r\n           if ($force -or $pscmdlet.shouldprocess($restartWSManTarget, $restartWSManAction))\r\n           {{\r\n               restart-service winrm -force\r\n               $null = winrm configsddl \"{0}$pluginName\"\r\n           }}\r\n           else\r\n           {{\r\n               write-error $restartWSManRequired\r\n               return \r\n           }}\r\n\r\n           # if AccessMode is Disabled OR the winrm configsddl failed, we just return\r\n           if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Disabled.Equals($accessMode) -or !$?)\r\n           {{\r\n               return\r\n           }}\r\n        }} # end of if ($shouldShowUI)\r\n\r\n        if ($? -and ($shouldShowUI -or $isSddlSpecified))\r\n        {{\r\n           # if AccessMode is Local or Remote, we need to check the SDDL the user set in the UI or passed in to the cmdlet.\r\n           $newSDDL = $null\r\n           $curPlugin = Get-PSSessionConfiguration -Name $pluginName\r\n           $curSDDL = $curPlugin.SecurityDescriptorSddl\r\n           if (!$curSDDL)\r\n           {{\r\n               if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode))\r\n               {{\r\n                    $newSDDL = \"{1}\"\r\n               }}\r\n           }}\r\n           else\r\n           {{\r\n               # Construct SID for network users\r\n               [system.security.principal.wellknownsidtype]$evst = \"NetworkSid\"\r\n               $networkSID = new-object system.security.principal.securityidentifier $evst,$null\r\n                \r\n               $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$curSDDL\r\n               $haveDisableACE = $false\r\n               $securityIdentifierToPurge = $null\r\n               $sd.DiscretionaryAcl | % {{\r\n                    if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $networkSID) -and ($_.AccessMask -eq 268435456))\r\n                    {{\r\n                        $haveDisableACE = $true\r\n                        $securityIdentifierToPurge = $_.securityidentifier\r\n                    }}\r\n               }}\r\n               if (([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode) -or\r\n                    ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Remote.Equals($accessMode)-and $disableNetworkExists)) -and\r\n                   !$haveDisableACE)\r\n               {{\r\n                    # Add network deny ACE for local access or remote access with PSRemoting disabled ($disableNetworkExists)\r\n                    $sd.DiscretionaryAcl.AddAccess(\"deny\", $networkSID, 268435456, \"None\", \"None\")\r\n                    $newSDDL = $sd.GetSddlForm(\"all\")\r\n               }}\r\n               if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Remote.Equals($accessMode) -and -not $disableNetworkExists -and $haveDisableACE)\r\n               {{\r\n                    # Remove the specific ACE\r\n                    $sd.discretionaryacl.RemoveAccessSpecific('Deny', $securityIdentifierToPurge, 268435456, 'none', 'none')\r\n\r\n                    # if there is no discretionaryacl..add Builtin Administrators and Remote Management Users\r\n                    # to the DACL group as this is the default WSMan behavior\r\n                    if ($sd.discretionaryacl.count -eq 0)\r\n                    {{\r\n                        [system.security.principal.wellknownsidtype]$bast = \"BuiltinAdministratorsSid\"\r\n                        $basid = new-object system.security.principal.securityidentifier $bast,$null\r\n                        $sd.DiscretionaryAcl.AddAccess('Allow',$basid, 268435456, 'none', 'none')\r\n\r\n                        # Remote Management Users, Win8+ only\r\n                        if ([System.Environment]::OSVersion.Version.Major -ge 6 -and [System.Environment]::OSVersion.Version.Minor -ge 2)\r\n                        {{\r\n                            $rmSidId = new-object system.security.principal.securityidentifier \"{2}\"\r\n                            $sd.DiscretionaryAcl.AddAccess('Allow', $rmSidId, 268435456, 'none', 'none')\r\n                        }}\r\n                    }}\r\n\r\n                    $newSDDL = $sd.GetSddlForm(\"all\")\r\n               }}\r\n           }} # end of if(!$curSDDL)\r\n        }} # end of if ($shouldShowUI -or $isSddlSpecified)\r\n\r\n        if ($? -and $newSDDL)\r\n        {{\r\n            try {{\r\n                if ($runAsUserName)\r\n                {{\r\n                    $runAsCredential = new-object system.management.automation.PSCredential($runAsUserName, $runAsPassword)\r\n                    $null = Set-PSSessionConfiguration -Name $pluginName -SecurityDescriptorSddl $newSDDL -NoServiceRestart -force -WarningAction 0 -RunAsCredential $runAsCredential\r\n                }}\r\n                else\r\n                {{\r\n                    $null = Set-PSSessionConfiguration -Name $pluginName -SecurityDescriptorSddl $newSDDL -NoServiceRestart -force -WarningAction 0\r\n                }}\r\n            }} catch {{\r\n\t\t\t\tremove-item WSMan:\\localhost\\Plugin\\\"$pluginName\" -recurse -force\r\n\t\t\t\twrite-error $_\r\n                # Do not add anymore clean up code after Write-Error, because if EA=Stop is set by user\r\n                # any code at this point will not execute.\r\n\r\n\t\t\t\treturn\r\n\t\t\t}}\r\n        }}\r\n    }}\r\n}}\r\n\r\nRegister-PSSessionConfiguration -filepath $args[0] -pluginName $args[1] -shouldShowUI $args[2] -force $args[3] -noServiceRestart $args[4] -whatif:$args[5] -confirm:$args[6] -restartWSManTarget $args[7] -restartWSManAction $args[8] -restartWSManRequired $args[9] -runAsUserName $args[10] -runAsPassword $args[11] -accessMode $args[12] -isSddlSpecified $args[13]\r\n", new object[] { "http://schemas.microsoft.com/powershell/", localSddl, "S-1-5-32-580" }));
            newPluginSb.LanguageMode = 0;
        }

        protected override void BeginProcessing()
        {
            if (base.isSddlSpecified && base.showUISpecified)
            {
                throw new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.ShowUIAndSDDLCannotExist, "SecurityDescriptorSddl", "ShowSecurityDescriptorUI"));
            }
            if (base.isRunAsCredentialSpecified)
            {
                base.WriteWarning(RemotingErrorIdStrings.RunAsSessionConfigurationSecurityWarning);
            }
            if (base.isSddlSpecified)
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
            if (!base.isSddlSpecified && !base.showUISpecified)
            {
                if (base.AccessMode.Equals(PSSessionConfigurationAccessMode.Local))
                {
                    base.sddl = PSSessionConfigurationCommandBase.GetLocalSddl();
                    base.isSddlSpecified = true;
                }
                else if (base.AccessMode.Equals(PSSessionConfigurationAccessMode.Remote))
                {
                    base.sddl = PSSessionConfigurationCommandBase.GetRemoteSddl();
                    base.isSddlSpecified = true;
                }
            }
            RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
            PSSessionConfigurationCommandUtilities.ThrowIfNotAdministrator();
            WSManConfigurationOption transportOption = base.transportOption as WSManConfigurationOption;
            if (((transportOption != null) && transportOption.ProcessIdleTimeoutSec.HasValue) && !base.isUseSharedProcessSpecified)
            {
                PSInvalidOperationException exception = new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.InvalidConfigurationXMLAttribute, "ProcessIdleTimeoutSec", "UseSharedProcess"));
                base.ThrowTerminatingError(exception.ErrorRecord);
            }
        }

        private string ConstructPluginContent()
        {
            object[] objArray;
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            if (this.sessionType == PSSessionType.Workflow)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "sessiontype", this.sessionType, Environment.NewLine }));
                builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "assemblyname", "Microsoft.PowerShell.Workflow.ServiceCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL", Environment.NewLine }));
                builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "pssessionconfigurationtypename", "Microsoft.PowerShell.Workflow.PSWorkflowSessionConfiguration", Environment.NewLine }));
                flag = true;
            }
            if (base.Path != null)
            {
                PSDriveInfo info2;
                ProviderInfo provider = null;
                string o = base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(base.Path, out provider, out info2);
                if (!provider.NameEquals(base.Context.ProviderNames.FileSystem) || !o.EndsWith(".pssc", StringComparison.OrdinalIgnoreCase))
                {
                    InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.InvalidPSSessionConfigurationFilePath, o));
                    ErrorRecord errorRecord = new ErrorRecord(exception, "InvalidPSSessionConfigurationFilePath", ErrorCategory.InvalidArgument, base.Path);
                    base.ThrowTerminatingError(errorRecord);
                }
                Guid empty = Guid.Empty;
                ExternalScriptInfo scriptInfo = null;
                Hashtable table = null;
                try
                {
                    string str3;
                    scriptInfo = DISCUtils.GetScriptInfoForFile(base.Context, o, out str3);
                    table = DISCUtils.LoadConfigFile(base.Context, scriptInfo);
                }
                catch (RuntimeException exception2)
                {
                    InvalidOperationException exception3 = new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.InvalidPSSessionConfigurationFileErrorProcessing, o, exception2.Message), exception2);
                    ErrorRecord record2 = new ErrorRecord(exception3, "InvalidPSSessionConfigurationFilePath", ErrorCategory.InvalidArgument, base.Path);
                    base.ThrowTerminatingError(record2);
                }
                if (table == null)
                {
                    InvalidOperationException exception4 = new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.InvalidPSSessionConfigurationFile, o));
                    ErrorRecord record3 = new ErrorRecord(exception4, "InvalidPSSessionConfigurationFile", ErrorCategory.InvalidArgument, base.Path);
                    base.ThrowTerminatingError(record3);
                }
                else
                {
                    if (table.ContainsKey(ConfigFileContants.Guid))
                    {
                        try
                        {
                            if (table[ConfigFileContants.Guid] != null)
                            {
                                empty = Guid.Parse(table[ConfigFileContants.Guid].ToString());
                            }
                            else
                            {
                                InvalidOperationException exception5 = new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.ErrorParsingTheKeyInPSSessionConfigurationFile, ConfigFileContants.Guid, o));
                                base.ThrowTerminatingError(new ErrorRecord(exception5, "InvalidGuidInPSSessionConfigurationFile", ErrorCategory.InvalidOperation, null));
                            }
                        }
                        catch (FormatException exception6)
                        {
                            base.ThrowTerminatingError(new ErrorRecord(exception6, "InvalidGuidInPSSessionConfigurationFile", ErrorCategory.InvalidOperation, null));
                        }
                    }
                    if (table.ContainsKey(ConfigFileContants.PowerShellVersion) && !base.isPSVersionSpecified)
                    {
                        try
                        {
                            base.PSVersion = new Version(table[ConfigFileContants.PowerShellVersion].ToString());
                        }
                        catch (ArgumentException exception7)
                        {
                            base.ThrowTerminatingError(new ErrorRecord(exception7, "InvalidPowerShellVersion", ErrorCategory.InvalidOperation, null));
                        }
                        catch (FormatException exception8)
                        {
                            base.ThrowTerminatingError(new ErrorRecord(exception8, "InvalidPowerShellVersion", ErrorCategory.InvalidOperation, null));
                        }
                        catch (OverflowException exception9)
                        {
                            base.ThrowTerminatingError(new ErrorRecord(exception9, "InvalidPowerShellVersion", ErrorCategory.InvalidOperation, null));
                        }
                    }
                    try
                    {
                        DISCUtils.ValidateAbsolutePaths(base.SessionState, table, base.Path);
                    }
                    catch (InvalidOperationException exception10)
                    {
                        base.ThrowTerminatingError(new ErrorRecord(exception10, "RelativePathsNotSupported", ErrorCategory.InvalidOperation, null));
                    }
                    try
                    {
                        DISCUtils.ValidateExtensions(table, base.Path);
                    }
                    catch (InvalidOperationException exception11)
                    {
                        base.ThrowTerminatingError(new ErrorRecord(exception11, "FileExtensionNotSupported", ErrorCategory.InvalidOperation, null));
                    }
                }
                string destFileName = System.IO.Path.Combine(Utils.GetApplicationBase(Utils.DefaultPowerShellShellID), "SessionConfig", base.shellName + "_" + empty.ToString() + ".pssc");
                if (string.Equals(this.ProcessorArchitecture, "x86", StringComparison.OrdinalIgnoreCase))
                {
                    string environmentVariable = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                    if (string.Equals(environmentVariable, "amd64", StringComparison.OrdinalIgnoreCase) || string.Equals(environmentVariable, "ia64", StringComparison.OrdinalIgnoreCase))
                    {
                        destFileName = destFileName.ToLowerInvariant().Replace(@"\system32\", @"\syswow64\");
                    }
                }
                File.Copy(o, destFileName, true);
                builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "ConfigFilePath", destFileName, Environment.NewLine }));
            }
            if (!flag)
            {
                if (!string.IsNullOrEmpty(base.configurationTypeName))
                {
                    builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "pssessionconfigurationtypename", base.configurationTypeName, Environment.NewLine }));
                }
                if (!string.IsNullOrEmpty(base.assemblyName))
                {
                    builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "assemblyname", base.assemblyName, Environment.NewLine }));
                }
            }
            if (!string.IsNullOrEmpty(base.applicationBase))
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "applicationbase", base.applicationBase, Environment.NewLine }));
            }
            if (!string.IsNullOrEmpty(base.configurationScript))
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "startupscript", base.configurationScript, Environment.NewLine }));
            }
            if (this.maxCommandSizeMB.HasValue)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "psmaximumreceiveddatasizepercommandmb", this.maxCommandSizeMB.Value, Environment.NewLine }));
            }
            if (this.maxObjectSizeMB.HasValue)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "psmaximumreceivedobjectsizemb", this.maxObjectSizeMB.Value, Environment.NewLine }));
            }
            if (this.threadAptState.HasValue)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "pssessionthreadapartmentstate", this.threadAptState.Value, Environment.NewLine }));
            }
            if (this.threadOptions.HasValue)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "pssessionthreadoptions", this.threadOptions.Value, Environment.NewLine }));
            }
            if (!base.isPSVersionSpecified)
            {
                base.psVersion = PSVersionInfo.PSVersion;
            }
            if (base.psVersion != null)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "PSVersion", PSSessionConfigurationCommandUtilities.ConstructVersionFormatForConfigXml(base.psVersion), Environment.NewLine }));
                base.MaxPSVersion = PSSessionConfigurationCommandUtilities.CalculateMaxPSVersion(base.psVersion);
                if (base.MaxPSVersion != null)
                {
                    builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "MaxPSVersion", PSSessionConfigurationCommandUtilities.ConstructVersionFormatForConfigXml(base.MaxPSVersion), Environment.NewLine }));
                }
            }
            string str8 = "";
            if (!string.IsNullOrEmpty(base.sddl))
            {
                str8 = string.Format(CultureInfo.InvariantCulture, "<Security Uri='{0}' ExactMatch='true' Sddl='{1}' />", new object[] { "http://schemas.microsoft.com/powershell/" + base.shellName, base.sddl });
            }
            string str9 = string.Empty;
            if (!string.IsNullOrEmpty(this.architecture))
            {
                string str10 = "32";
                string str18 = this.architecture.ToLowerInvariant();
                if (str18 != null)
                {
                    if (!(str18 == "x86"))
                    {
                        if (str18 == "amd64")
                        {
                            str10 = "64";
                        }
                    }
                    else
                    {
                        str10 = "32";
                    }
                }
                str9 = string.Format(CultureInfo.InvariantCulture, "\r\n\tArchitecture='{0}'", new object[] { str10 });
            }
            if ((this.sessionType == PSSessionType.Workflow) && !base.isUseSharedProcessSpecified)
            {
                base.UseSharedProcess = true;
            }
            string str11 = string.Empty;
            if (base.isUseSharedProcessSpecified)
            {
                str11 = string.Format(CultureInfo.InvariantCulture, "\r\n\tUseSharedProcess='{0}'", new object[] { base.UseSharedProcess.ToString() });
            }
            string str12 = string.Empty;
            switch (base.AccessMode)
            {
                case PSSessionConfigurationAccessMode.Disabled:
                    objArray = new object[] { false.ToString() };
                    str12 = string.Format(CultureInfo.InvariantCulture, "\r\n\tEnabled='{0}'", objArray);
                    break;

                case PSSessionConfigurationAccessMode.Local:
                case PSSessionConfigurationAccessMode.Remote:
                    objArray = new object[] { true.ToString() };
                    str12 = string.Format(CultureInfo.InvariantCulture, "\r\n\tEnabled='{0}'", objArray);
                    break;
            }
            StringBuilder builder2 = new StringBuilder();
            if (this.sessionType == PSSessionType.Workflow)
            {
                List<string> list = new List<string>(base.modulesToImport ?? new string[0]);
                list.Insert(0, @"%windir%\system32\windowspowershell\v1.0\Modules\PSWorkflow");
                base.modulesToImport = list.ToArray();
            }
            if ((base.modulesToImport != null) && (base.modulesToImport.Length > 0))
            {
                builder2.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "modulestoimport", PSSessionConfigurationCommandUtilities.GetModulePathAsString(base.modulesToImport), string.Empty }));
            }
            if (base.sessionTypeOption != null)
            {
                string str13 = base.sessionTypeOption.ConstructPrivateData();
                if (!string.IsNullOrEmpty(str13))
                {
                    builder2.Append(string.Format(CultureInfo.InvariantCulture, "<Param Name='PrivateData'>{0}</Param>", new object[] { str13 }));
                }
            }
            if (builder2.Length > 0)
            {
                string str15 = SecurityElement.Escape(string.Format(CultureInfo.InvariantCulture, "<SessionConfigurationData>{0}</SessionConfigurationData>", new object[] { builder2 }));
                builder.Append(string.Format(CultureInfo.InvariantCulture, "\r\n<Param Name='{0}' Value='{1}' />{2}", new object[] { "sessionconfigurationdata", str15, string.Empty }));
            }
            if (base.transportOption == null)
            {
                base.transportOption = new WSManConfigurationOption();
            }
            else
            {
                int num;
                string str16;
                Hashtable hashtable2 = base.transportOption.ConstructQuotasAsHashtable();
                if (hashtable2.ContainsKey("IdleTimeoutms") && LanguagePrimitives.TryConvertTo<int>(hashtable2["IdleTimeoutms"], out num))
                {
                    PSSQMAPI.NoteSessionConfigurationIdleTimeout(num);
                }
                Hashtable hashtable3 = base.transportOption.ConstructOptionsAsHashtable();
                if (hashtable3.ContainsKey("OutputBufferingMode") && LanguagePrimitives.TryConvertTo<string>(hashtable3["OutputBufferingMode"], out str16))
                {
                    PSSQMAPI.NoteSessionConfigurationOutputBufferingMode(str16);
                }
            }
            base.transportOption = base.transportOption.Clone() as PSTransportOption;
            base.transportOption.LoadFromDefaults(this.sessionType, true);
            if (base.isUseSharedProcessSpecified && (base.UseSharedProcess == false))
            {
                (base.transportOption as WSManConfigurationOption).ProcessIdleTimeoutSec = 0;
            }
            return string.Format(CultureInfo.InvariantCulture, "\r\n<PlugInConfiguration xmlns='http://schemas.microsoft.com/wbem/wsman/1/config/PluginConfiguration'\r\n    Name='{0}'\r\n    Filename='%windir%\\system32\\{1}'\r\n    SDKVersion='{10}'\r\n    XmlRenderingType='text' {2} {6} {7} {8}>\r\n  <InitializationParameters>    \r\n{3}\r\n  </InitializationParameters> \r\n  <Resources>\r\n    <Resource ResourceUri='{4}' SupportsOptions='true' ExactMatch='true'>\r\n{5}\r\n      <Capability Type='Shell' />\r\n    </Resource>\r\n  </Resources>\r\n  {9}\r\n</PlugInConfiguration>\r\n", new object[] { base.shellName, "pwrshplugin.dll", str9, builder.ToString(), "http://schemas.microsoft.com/powershell/" + base.shellName, str8, str11, str12, base.transportOption.ConstructOptionsAsXmlAttributes(), base.transportOption.ConstructQuotas(), (base.psVersion.Major < 3) ? 1 : 2 });
        }

        private string ConstructTemporaryFile(string pluginContent)
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName()) + "psshell.xml";
            Exception exception = null;
            if (File.Exists(path))
            {
                FileInfo info = new FileInfo(path);
                if (info != null)
                {
                    try
                    {
                        info.Attributes &= ~(System.IO.FileAttributes.Hidden | System.IO.FileAttributes.ReadOnly);
                        info.Delete();
                    }
                    catch (FileNotFoundException exception2)
                    {
                        exception = exception2;
                    }
                    catch (DirectoryNotFoundException exception3)
                    {
                        exception = exception3;
                    }
                    catch (UnauthorizedAccessException exception4)
                    {
                        exception = exception4;
                    }
                    catch (SecurityException exception5)
                    {
                        exception = exception5;
                    }
                    catch (ArgumentNullException exception6)
                    {
                        exception = exception6;
                    }
                    catch (ArgumentException exception7)
                    {
                        exception = exception7;
                    }
                    catch (PathTooLongException exception8)
                    {
                        exception = exception8;
                    }
                    catch (NotSupportedException exception9)
                    {
                        exception = exception9;
                    }
                    catch (IOException exception10)
                    {
                        exception = exception10;
                    }
                    if (exception != null)
                    {
                        throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "NcsCannotDeleteFile", new object[] { path, exception.Message });
                    }
                }
            }
            try
            {
                StreamWriter writer = File.CreateText(path);
                writer.Write(pluginContent);
                writer.Flush();
                writer.Close();
            }
            catch (UnauthorizedAccessException exception11)
            {
                exception = exception11;
            }
            catch (ArgumentException exception12)
            {
                exception = exception12;
            }
            catch (PathTooLongException exception13)
            {
                exception = exception13;
            }
            catch (DirectoryNotFoundException exception14)
            {
                exception = exception14;
            }
            if (exception != null)
            {
                throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "NcsCannotWritePluginContent", new object[] { path, exception.Message });
            }
            return path;
        }

        private void DeleteFile(string tmpFileName)
        {
            Exception exception = null;
            try
            {
                File.Delete(tmpFileName);
            }
            catch (UnauthorizedAccessException exception2)
            {
                exception = exception2;
            }
            catch (ArgumentException exception3)
            {
                exception = exception3;
            }
            catch (PathTooLongException exception4)
            {
                exception = exception4;
            }
            catch (DirectoryNotFoundException exception5)
            {
                exception = exception5;
            }
            catch (IOException exception6)
            {
                exception = exception6;
            }
            catch (NotSupportedException exception7)
            {
                exception = exception7;
            }
            if (exception != null)
            {
                throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "NcsCannotDeleteFileAfterInstall", new object[] { tmpFileName, exception.Message });
            }
        }

        protected override void EndProcessing()
        {
            if (base.ShowSecurityDescriptorUI == 0)
            {
                PSSessionConfigurationCommandUtilities.RestartWinRMService(this, this.isErrorReported, (bool) base.Force, base.noRestart);
            }
            if (!this.isErrorReported && base.noRestart)
            {
                string o = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, base.CommandInfo.Name);
                base.WriteWarning(StringUtil.Format(RemotingErrorIdStrings.WinRMRequiresRestart, o));
            }
            new Tracer().EndpointRegistered(base.Name, this.sessionType.ToString(), WindowsIdentity.GetCurrent().Name);
        }

        protected override void ProcessRecord()
        {
            base.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.NcsScriptMessageV, "\r\nfunction Register-PSSessionConfiguration\r\n{{\r\n    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\n    param(  \r\n      [string]$filepath,\r\n      [string]$pluginName,\r\n      [bool]$shouldShowUI,\r\n      [bool]$force,\r\n      [bool]$noServiceRestart,\r\n      [string]$restartWSManTarget,\r\n      [string]$restartWSManAction,\r\n      [string]$restartWSManRequired,\r\n\t  [string]$runAsUserName,\r\n\t  [system.security.securestring]$runAsPassword,\r\n      [System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]$accessMode,\r\n      [bool]$isSddlSpecified\r\n    )\r\n\r\n    begin\r\n    {{\r\n        ## Construct SID for network users\r\n        [system.security.principal.wellknownsidtype]$evst = \"NetworkSid\"\r\n        $networkSID = new-object system.security.principal.securityidentifier $evst,$null\r\n\r\n        ## If all session configurations have Network Access disabled,\r\n        ## then we create this endpoint as Local as well.\r\n        $newSDDL = $null\r\n        $foundRemoteEndpoint = $false;\r\n        Get-PSSessionConfiguration | Foreach-Object {{\r\n            if ($_.Enabled)\r\n            {{\r\n                $sddl = $null\r\n                if ($_.psobject.members[\"SecurityDescriptorSddl\"])\r\n                {{\r\n                    $sddl = $_.psobject.members[\"SecurityDescriptorSddl\"].Value\r\n                }}\r\n        \r\n                if($sddl)\r\n                {{\r\n                    # See if it has 'Disable Network Access'\r\n                    $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddl\r\n                    $disableNetworkExists = $false\r\n                    $sd.DiscretionaryAcl | % {{\r\n                        if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $networkSID) -and ($_.AccessMask -eq 268435456))\r\n                        {{\r\n                            $disableNetworkExists = $true              \r\n                        }}\r\n                    }}\r\n\r\n                    if(-not $disableNetworkExists) {{ $foundRemoteEndpoint = $true }}\r\n                }}\r\n            }}\r\n        }}\r\n\r\n        if(-not $foundRemoteEndpoint)\r\n        {{\r\n            $newSDDL = \"{1}\"\r\n        }}\r\n    }}\r\n\r\n    process\r\n    {{\r\n        if ($force)\r\n        {{\r\n            if (Test-Path WSMan:\\localhost\\Plugin\\\"$pluginName\")\r\n            {{\r\n                Unregister-PSSessionConfiguration -name \"$pluginName\" -force\r\n            }}\r\n        }}\r\n\r\n        new-item -path WSMan:\\localhost\\Plugin -file \"$filepath\" -name \"$pluginName\"\r\n        # $? is to make sure the last operation is succeeded\r\n\r\n\t\tif ($? -and $runAsUserName) \r\n\t\t{{\r\n\t\t\ttry {{\r\n\t\t\t\t$runAsCredential = new-object system.management.automation.PSCredential($runAsUserName, $runAsPassword)\r\n\t\t\t\tset-item -WarningAction SilentlyContinue WSMan:\\localhost\\Plugin\\\"$pluginName\"\\RunAsUser $runAsCredential -confirm:$false\r\n\t\t\t}} catch {{\r\n\t\t\t\tremove-item WSMan:\\localhost\\Plugin\\\"$pluginName\" -recurse -force\r\n\t\t\t\twrite-error $_\r\n                # Do not add anymore clean up code after Write-Error, because if EA=Stop is set by user\r\n                # any code at this point will not execute.\r\n\r\n\t\t\t\treturn\r\n\t\t\t}}\r\n\t\t}}\r\n\r\n        if ($? -and $shouldShowUI)\r\n        {{\r\n           if ($noServiceRestart)\r\n           {{\r\n               write-error $restartWSManRequired\r\n               return\r\n           }}\r\n\r\n           if ($force -or $pscmdlet.shouldprocess($restartWSManTarget, $restartWSManAction))\r\n           {{\r\n               restart-service winrm -force\r\n               $null = winrm configsddl \"{0}$pluginName\"\r\n           }}\r\n           else\r\n           {{\r\n               write-error $restartWSManRequired\r\n               return \r\n           }}\r\n\r\n           # if AccessMode is Disabled OR the winrm configsddl failed, we just return\r\n           if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Disabled.Equals($accessMode) -or !$?)\r\n           {{\r\n               return\r\n           }}\r\n        }} # end of if ($shouldShowUI)\r\n\r\n        if ($? -and ($shouldShowUI -or $isSddlSpecified))\r\n        {{\r\n           # if AccessMode is Local or Remote, we need to check the SDDL the user set in the UI or passed in to the cmdlet.\r\n           $newSDDL = $null\r\n           $curPlugin = Get-PSSessionConfiguration -Name $pluginName\r\n           $curSDDL = $curPlugin.SecurityDescriptorSddl\r\n           if (!$curSDDL)\r\n           {{\r\n               if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode))\r\n               {{\r\n                    $newSDDL = \"{1}\"\r\n               }}\r\n           }}\r\n           else\r\n           {{\r\n               # Construct SID for network users\r\n               [system.security.principal.wellknownsidtype]$evst = \"NetworkSid\"\r\n               $networkSID = new-object system.security.principal.securityidentifier $evst,$null\r\n                \r\n               $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$curSDDL\r\n               $haveDisableACE = $false\r\n               $securityIdentifierToPurge = $null\r\n               $sd.DiscretionaryAcl | % {{\r\n                    if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $networkSID) -and ($_.AccessMask -eq 268435456))\r\n                    {{\r\n                        $haveDisableACE = $true\r\n                        $securityIdentifierToPurge = $_.securityidentifier\r\n                    }}\r\n               }}\r\n               if (([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode) -or\r\n                    ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Remote.Equals($accessMode)-and $disableNetworkExists)) -and\r\n                   !$haveDisableACE)\r\n               {{\r\n                    # Add network deny ACE for local access or remote access with PSRemoting disabled ($disableNetworkExists)\r\n                    $sd.DiscretionaryAcl.AddAccess(\"deny\", $networkSID, 268435456, \"None\", \"None\")\r\n                    $newSDDL = $sd.GetSddlForm(\"all\")\r\n               }}\r\n               if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Remote.Equals($accessMode) -and -not $disableNetworkExists -and $haveDisableACE)\r\n               {{\r\n                    # Remove the specific ACE\r\n                    $sd.discretionaryacl.RemoveAccessSpecific('Deny', $securityIdentifierToPurge, 268435456, 'none', 'none')\r\n\r\n                    # if there is no discretionaryacl..add Builtin Administrators and Remote Management Users\r\n                    # to the DACL group as this is the default WSMan behavior\r\n                    if ($sd.discretionaryacl.count -eq 0)\r\n                    {{\r\n                        [system.security.principal.wellknownsidtype]$bast = \"BuiltinAdministratorsSid\"\r\n                        $basid = new-object system.security.principal.securityidentifier $bast,$null\r\n                        $sd.DiscretionaryAcl.AddAccess('Allow',$basid, 268435456, 'none', 'none')\r\n\r\n                        # Remote Management Users, Win8+ only\r\n                        if ([System.Environment]::OSVersion.Version.Major -ge 6 -and [System.Environment]::OSVersion.Version.Minor -ge 2)\r\n                        {{\r\n                            $rmSidId = new-object system.security.principal.securityidentifier \"{2}\"\r\n                            $sd.DiscretionaryAcl.AddAccess('Allow', $rmSidId, 268435456, 'none', 'none')\r\n                        }}\r\n                    }}\r\n\r\n                    $newSDDL = $sd.GetSddlForm(\"all\")\r\n               }}\r\n           }} # end of if(!$curSDDL)\r\n        }} # end of if ($shouldShowUI -or $isSddlSpecified)\r\n\r\n        if ($? -and $newSDDL)\r\n        {{\r\n            try {{\r\n                if ($runAsUserName)\r\n                {{\r\n                    $runAsCredential = new-object system.management.automation.PSCredential($runAsUserName, $runAsPassword)\r\n                    $null = Set-PSSessionConfiguration -Name $pluginName -SecurityDescriptorSddl $newSDDL -NoServiceRestart -force -WarningAction 0 -RunAsCredential $runAsCredential\r\n                }}\r\n                else\r\n                {{\r\n                    $null = Set-PSSessionConfiguration -Name $pluginName -SecurityDescriptorSddl $newSDDL -NoServiceRestart -force -WarningAction 0\r\n                }}\r\n            }} catch {{\r\n\t\t\t\tremove-item WSMan:\\localhost\\Plugin\\\"$pluginName\" -recurse -force\r\n\t\t\t\twrite-error $_\r\n                # Do not add anymore clean up code after Write-Error, because if EA=Stop is set by user\r\n                # any code at this point will not execute.\r\n\r\n\t\t\t\treturn\r\n\t\t\t}}\r\n        }}\r\n    }}\r\n}}\r\n\r\nRegister-PSSessionConfiguration -filepath $args[0] -pluginName $args[1] -shouldShowUI $args[2] -force $args[3] -noServiceRestart $args[4] -whatif:$args[5] -confirm:$args[6] -restartWSManTarget $args[7] -restartWSManAction $args[8] -restartWSManRequired $args[9] -runAsUserName $args[10] -runAsPassword $args[11] -accessMode $args[12] -isSddlSpecified $args[13]\r\n"));
            if (!base.force)
            {
                string str2;
                string action = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, base.CommandInfo.Name);
                if (base.isSddlSpecified)
                {
                    str2 = StringUtil.Format(RemotingErrorIdStrings.NcsShouldProcessTargetSDDL, base.Name, base.sddl);
                }
                else
                {
                    str2 = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessTargetAdminEnable, base.Name);
                }
                if (!base.noRestart)
                {
                    string o = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, base.CommandInfo.Name);
                    base.WriteWarning(StringUtil.Format(RemotingErrorIdStrings.WinRMRestartWarning, o));
                }
                if (!base.ShouldProcess(str2, action))
                {
                    return;
                }
            }
            string pluginContent = this.ConstructPluginContent();
            string tmpFileName = this.ConstructTemporaryFile(pluginContent);
            try
            {
                string restartWSManServiceAction = RemotingErrorIdStrings.RestartWSManServiceAction;
                string str7 = StringUtil.Format(RemotingErrorIdStrings.RestartWSManServiceTarget, "WinRM");
                string str8 = StringUtil.Format(RemotingErrorIdStrings.RestartWSManRequiredShowUI, string.Format(CultureInfo.InvariantCulture, "Set-PSSessionConfiguration {0} -ShowSecurityDescriptorUI", new object[] { base.shellName }));
                bool whatIf = false;
                bool confirm = true;
                PSSessionConfigurationCommandUtilities.CollectShouldProcessParameters(this, out whatIf, out confirm);
                ArrayList dollarErrorVariable = (ArrayList) base.Context.DollarErrorVariable;
                int count = dollarErrorVariable.Count;
                newPluginSb.InvokeUsingCmdlet(this, true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, new object[0], AutomationNull.Value, new object[] { tmpFileName, base.shellName, base.ShowSecurityDescriptorUI.ToBool(), base.force, base.NoServiceRestart, whatIf, confirm, str7, restartWSManServiceAction, str8, (base.runAsCredential != null) ? base.runAsCredential.UserName : null, (base.runAsCredential != null) ? base.runAsCredential.Password : null, base.AccessMode, base.isSddlSpecified });
                dollarErrorVariable = (ArrayList) base.Context.DollarErrorVariable;
                this.isErrorReported = dollarErrorVariable.Count > count;
            }
            finally
            {
                this.DeleteFile(tmpFileName);
            }
        }

        [Parameter, ValidateNotNullOrEmpty, Alias(new string[] { "PA" }), ValidateSet(new string[] { "x86", "amd64" })]
        public string ProcessorArchitecture
        {
            get
            {
                return this.architecture;
            }
            set
            {
                this.architecture = value;
            }
        }

        [Parameter(ParameterSetName="NameParameterSet")]
        public PSSessionType SessionType
        {
            get
            {
                return this.sessionType;
            }
            set
            {
                this.sessionType = value;
            }
        }
    }
}

