namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Tracing;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Text;

    [Cmdlet("Enable", "PSSessionConfiguration", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High, HelpUri="http://go.microsoft.com/fwlink/?LinkID=144301")]
    public sealed class EnablePSSessionConfigurationCommand : PSCmdlet
    {
        private static ScriptBlock enablePluginSb = ScriptBlock.Create(string.Format(CultureInfo.InvariantCulture, "\r\nfunction Enable-PSSessionConfiguration\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter(Position=0, ValueFromPipeline=$true)]\r\n    [System.String]\r\n    $Name,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $sddl,\r\n    \r\n    [Parameter()]\r\n    [bool]\r\n    $isSDDLSpecified,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $queryForSet,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForSet,\r\n        \r\n    [Parameter()]\r\n    [string]\r\n    $queryForQC,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForQC,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $shouldProcessDescForQC,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $setEnabledTarget,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $setEnabledAction,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $skipNetworkProfileCheck\r\n    )\r\n     \r\n    begin\r\n    {{\r\n        $needWinRMRestart = $false\r\n        if ($force -or $pscmdlet.ShouldProcess($shouldProcessDescForQC, $queryForQC, $captionForQC))\r\n        {{\r\n            # get the status of winrm before Quick Config. if it is already\r\n            # running..restart the service after Quick Config.\r\n            $svc = get-service winrm\r\n            if ($skipNetworkProfileCheck)\r\n            {{\r\n                {0} -force -SkipNetworkProfileCheck\r\n            }}\r\n            else\r\n            {{\r\n                {0} -force\r\n            }}\r\n            if ($svc.Status -match \"Running\")\r\n            {{\r\n               Restart-Service winrm -force -confirm:$false\r\n            }}\r\n        }}\r\n    }} #end of Begin block   \r\n        \r\n    process\r\n    {{\r\n       Get-PSSessionConfiguration $name | % {{\r\n\r\n          if ($_.Enabled -eq $false -and ($force -or $pscmdlet.ShouldProcess($setEnabledTarget, $setEnabledAction)))\r\n          {{\r\n             Set-Item -WarningAction SilentlyContinue -Path \"WSMan:\\localhost\\Plugin\\$name\\Enabled\" -Value $true -confirm:$false\r\n             $needWinRMRestart = $true\r\n          }}\r\n\r\n          if (!$isSDDLSpecified)\r\n          {{\r\n             $sddlTemp = $null\r\n             if ($_.psobject.members[\"SecurityDescriptorSddl\"])\r\n             {{\r\n                 $sddlTemp = $_.psobject.members[\"SecurityDescriptorSddl\"].Value\r\n             }}\r\n\r\n             $securityIdentifierToPurge = $null\r\n             # strip out Disable-Everyone DACL from the SDDL\r\n             if ($sddlTemp)\r\n             {{\r\n                # construct SID for \"EveryOne\"\r\n                [system.security.principal.wellknownsidtype]$evst = \"worldsid\"\r\n                $everyOneSID = new-object system.security.principal.securityidentifier $evst,$null\r\n                                \r\n                $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddlTemp                \r\n                $sd.DiscretionaryAcl | % {{\r\n                    if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $everyOneSID))\r\n                    {{\r\n                       $securityIdentifierToPurge = $_.securityidentifier\r\n                    }}\r\n                }}\r\n             \r\n                if ($securityIdentifierToPurge)\r\n                {{\r\n                   $sd.discretionaryacl.purge($securityIdentifierToPurge)\r\n\r\n                   # if there is no discretionaryacl..add Builtin Administrators and Remote Management Users\r\n                   # to the DACL group as this is the default WSMan behavior\r\n                   if ($sd.discretionaryacl.count -eq 0)\r\n                   {{\r\n                      # Built-in administrators\r\n                      [system.security.principal.wellknownsidtype]$bast = \"BuiltinAdministratorsSid\"\r\n                      $basid = new-object system.security.principal.securityidentifier $bast,$null\r\n                      $sd.DiscretionaryAcl.AddAccess('Allow',$basid, 268435456, 'none', 'none')\r\n\r\n                      # Remote Management Users, Win8+ only\r\n                      if ([System.Environment]::OSVersion.Version.Major -ge 6 -and [System.Environment]::OSVersion.Version.Minor -ge 2)\r\n                      {{\r\n                          $rmSidId = new-object system.security.principal.securityidentifier \"{1}\"\r\n                          $sd.DiscretionaryAcl.AddAccess('Allow', $rmSidId, 268435456, 'none', 'none')\r\n                      }}\r\n                   }}\r\n\r\n                   $sddl = $sd.GetSddlForm(\"all\")\r\n                }}\r\n             }} # if ($sddlTemp)\r\n          }} # if (!$isSDDLSpecified) \r\n          \r\n          $qMessage = $queryForSet -f $_.name,$sddl\r\n          if (($sddl -or $isSDDLSpecified) -and ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForSet)))\r\n          {{\r\n              $null = Set-PSSessionConfiguration -Name $_.Name -SecurityDescriptorSddl $sddl -NoServiceRestart -force -WarningAction 0\r\n          }}\r\n       }} #end of Get-PSSessionConfiguration | foreach\r\n    }} # end of Process block\r\n\r\n    # restart the winrm to make the config change takes effect immediately\r\n    End\r\n    {{\r\n        if ($needWinRMRestart)\r\n        {{\r\n            Restart-Service winrm -force -confirm:$false\r\n        }}\r\n    }}\r\n}}\r\n\r\n$_ | Enable-PSSessionConfiguration -force $args[0] -sddl $args[1] -isSDDLSpecified $args[2] -queryForSet $args[3] -captionForSet $args[4] -queryForQC $args[5] -captionForQC $args[6] -whatif:$args[7] -confirm:$args[8] -shouldProcessDescForQC $args[9] -setEnabledTarget $args[10] -setEnabledAction $args[11] -skipNetworkProfileCheck $args[12]\r\n", new object[] { "Set-WSManQuickConfig", "S-1-5-32-580" }));
        private const string enablePluginSbFormat = "\r\nfunction Enable-PSSessionConfiguration\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter(Position=0, ValueFromPipeline=$true)]\r\n    [System.String]\r\n    $Name,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $sddl,\r\n    \r\n    [Parameter()]\r\n    [bool]\r\n    $isSDDLSpecified,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $queryForSet,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForSet,\r\n        \r\n    [Parameter()]\r\n    [string]\r\n    $queryForQC,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForQC,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $shouldProcessDescForQC,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $setEnabledTarget,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $setEnabledAction,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $skipNetworkProfileCheck\r\n    )\r\n     \r\n    begin\r\n    {{\r\n        $needWinRMRestart = $false\r\n        if ($force -or $pscmdlet.ShouldProcess($shouldProcessDescForQC, $queryForQC, $captionForQC))\r\n        {{\r\n            # get the status of winrm before Quick Config. if it is already\r\n            # running..restart the service after Quick Config.\r\n            $svc = get-service winrm\r\n            if ($skipNetworkProfileCheck)\r\n            {{\r\n                {0} -force -SkipNetworkProfileCheck\r\n            }}\r\n            else\r\n            {{\r\n                {0} -force\r\n            }}\r\n            if ($svc.Status -match \"Running\")\r\n            {{\r\n               Restart-Service winrm -force -confirm:$false\r\n            }}\r\n        }}\r\n    }} #end of Begin block   \r\n        \r\n    process\r\n    {{\r\n       Get-PSSessionConfiguration $name | % {{\r\n\r\n          if ($_.Enabled -eq $false -and ($force -or $pscmdlet.ShouldProcess($setEnabledTarget, $setEnabledAction)))\r\n          {{\r\n             Set-Item -WarningAction SilentlyContinue -Path \"WSMan:\\localhost\\Plugin\\$name\\Enabled\" -Value $true -confirm:$false\r\n             $needWinRMRestart = $true\r\n          }}\r\n\r\n          if (!$isSDDLSpecified)\r\n          {{\r\n             $sddlTemp = $null\r\n             if ($_.psobject.members[\"SecurityDescriptorSddl\"])\r\n             {{\r\n                 $sddlTemp = $_.psobject.members[\"SecurityDescriptorSddl\"].Value\r\n             }}\r\n\r\n             $securityIdentifierToPurge = $null\r\n             # strip out Disable-Everyone DACL from the SDDL\r\n             if ($sddlTemp)\r\n             {{\r\n                # construct SID for \"EveryOne\"\r\n                [system.security.principal.wellknownsidtype]$evst = \"worldsid\"\r\n                $everyOneSID = new-object system.security.principal.securityidentifier $evst,$null\r\n                                \r\n                $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddlTemp                \r\n                $sd.DiscretionaryAcl | % {{\r\n                    if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $everyOneSID))\r\n                    {{\r\n                       $securityIdentifierToPurge = $_.securityidentifier\r\n                    }}\r\n                }}\r\n             \r\n                if ($securityIdentifierToPurge)\r\n                {{\r\n                   $sd.discretionaryacl.purge($securityIdentifierToPurge)\r\n\r\n                   # if there is no discretionaryacl..add Builtin Administrators and Remote Management Users\r\n                   # to the DACL group as this is the default WSMan behavior\r\n                   if ($sd.discretionaryacl.count -eq 0)\r\n                   {{\r\n                      # Built-in administrators\r\n                      [system.security.principal.wellknownsidtype]$bast = \"BuiltinAdministratorsSid\"\r\n                      $basid = new-object system.security.principal.securityidentifier $bast,$null\r\n                      $sd.DiscretionaryAcl.AddAccess('Allow',$basid, 268435456, 'none', 'none')\r\n\r\n                      # Remote Management Users, Win8+ only\r\n                      if ([System.Environment]::OSVersion.Version.Major -ge 6 -and [System.Environment]::OSVersion.Version.Minor -ge 2)\r\n                      {{\r\n                          $rmSidId = new-object system.security.principal.securityidentifier \"{1}\"\r\n                          $sd.DiscretionaryAcl.AddAccess('Allow', $rmSidId, 268435456, 'none', 'none')\r\n                      }}\r\n                   }}\r\n\r\n                   $sddl = $sd.GetSddlForm(\"all\")\r\n                }}\r\n             }} # if ($sddlTemp)\r\n          }} # if (!$isSDDLSpecified) \r\n          \r\n          $qMessage = $queryForSet -f $_.name,$sddl\r\n          if (($sddl -or $isSDDLSpecified) -and ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForSet)))\r\n          {{\r\n              $null = Set-PSSessionConfiguration -Name $_.Name -SecurityDescriptorSddl $sddl -NoServiceRestart -force -WarningAction 0\r\n          }}\r\n       }} #end of Get-PSSessionConfiguration | foreach\r\n    }} # end of Process block\r\n\r\n    # restart the winrm to make the config change takes effect immediately\r\n    End\r\n    {{\r\n        if ($needWinRMRestart)\r\n        {{\r\n            Restart-Service winrm -force -confirm:$false\r\n        }}\r\n    }}\r\n}}\r\n\r\n$_ | Enable-PSSessionConfiguration -force $args[0] -sddl $args[1] -isSDDLSpecified $args[2] -queryForSet $args[3] -captionForSet $args[4] -queryForQC $args[5] -captionForQC $args[6] -whatif:$args[7] -confirm:$args[8] -shouldProcessDescForQC $args[9] -setEnabledTarget $args[10] -setEnabledAction $args[11] -skipNetworkProfileCheck $args[12]\r\n";
        private bool force;
        internal bool isSddlSpecified;
        internal string sddl;
        private const string setWSManConfigCommand = "Set-WSManQuickConfig";
        private string[] shellName;
        private Collection<string> shellsToEnable = new Collection<string>();
        private bool skipNetworkProfileCheck;

        static EnablePSSessionConfigurationCommand()
        {
            enablePluginSb.LanguageMode = 0;
        }

        protected override void BeginProcessing()
        {
            RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
            PSSessionConfigurationCommandUtilities.ThrowIfNotAdministrator();
        }

        protected override void EndProcessing()
        {
            if (this.shellsToEnable.Count == 0)
            {
                this.shellsToEnable.Add("Microsoft.PowerShell");
            }
            base.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.EcsScriptMessageV, "\r\nfunction Enable-PSSessionConfiguration\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter(Position=0, ValueFromPipeline=$true)]\r\n    [System.String]\r\n    $Name,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $sddl,\r\n    \r\n    [Parameter()]\r\n    [bool]\r\n    $isSDDLSpecified,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $queryForSet,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForSet,\r\n        \r\n    [Parameter()]\r\n    [string]\r\n    $queryForQC,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForQC,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $shouldProcessDescForQC,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $setEnabledTarget,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $setEnabledAction,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $skipNetworkProfileCheck\r\n    )\r\n     \r\n    begin\r\n    {{\r\n        $needWinRMRestart = $false\r\n        if ($force -or $pscmdlet.ShouldProcess($shouldProcessDescForQC, $queryForQC, $captionForQC))\r\n        {{\r\n            # get the status of winrm before Quick Config. if it is already\r\n            # running..restart the service after Quick Config.\r\n            $svc = get-service winrm\r\n            if ($skipNetworkProfileCheck)\r\n            {{\r\n                {0} -force -SkipNetworkProfileCheck\r\n            }}\r\n            else\r\n            {{\r\n                {0} -force\r\n            }}\r\n            if ($svc.Status -match \"Running\")\r\n            {{\r\n               Restart-Service winrm -force -confirm:$false\r\n            }}\r\n        }}\r\n    }} #end of Begin block   \r\n        \r\n    process\r\n    {{\r\n       Get-PSSessionConfiguration $name | % {{\r\n\r\n          if ($_.Enabled -eq $false -and ($force -or $pscmdlet.ShouldProcess($setEnabledTarget, $setEnabledAction)))\r\n          {{\r\n             Set-Item -WarningAction SilentlyContinue -Path \"WSMan:\\localhost\\Plugin\\$name\\Enabled\" -Value $true -confirm:$false\r\n             $needWinRMRestart = $true\r\n          }}\r\n\r\n          if (!$isSDDLSpecified)\r\n          {{\r\n             $sddlTemp = $null\r\n             if ($_.psobject.members[\"SecurityDescriptorSddl\"])\r\n             {{\r\n                 $sddlTemp = $_.psobject.members[\"SecurityDescriptorSddl\"].Value\r\n             }}\r\n\r\n             $securityIdentifierToPurge = $null\r\n             # strip out Disable-Everyone DACL from the SDDL\r\n             if ($sddlTemp)\r\n             {{\r\n                # construct SID for \"EveryOne\"\r\n                [system.security.principal.wellknownsidtype]$evst = \"worldsid\"\r\n                $everyOneSID = new-object system.security.principal.securityidentifier $evst,$null\r\n                                \r\n                $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddlTemp                \r\n                $sd.DiscretionaryAcl | % {{\r\n                    if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $everyOneSID))\r\n                    {{\r\n                       $securityIdentifierToPurge = $_.securityidentifier\r\n                    }}\r\n                }}\r\n             \r\n                if ($securityIdentifierToPurge)\r\n                {{\r\n                   $sd.discretionaryacl.purge($securityIdentifierToPurge)\r\n\r\n                   # if there is no discretionaryacl..add Builtin Administrators and Remote Management Users\r\n                   # to the DACL group as this is the default WSMan behavior\r\n                   if ($sd.discretionaryacl.count -eq 0)\r\n                   {{\r\n                      # Built-in administrators\r\n                      [system.security.principal.wellknownsidtype]$bast = \"BuiltinAdministratorsSid\"\r\n                      $basid = new-object system.security.principal.securityidentifier $bast,$null\r\n                      $sd.DiscretionaryAcl.AddAccess('Allow',$basid, 268435456, 'none', 'none')\r\n\r\n                      # Remote Management Users, Win8+ only\r\n                      if ([System.Environment]::OSVersion.Version.Major -ge 6 -and [System.Environment]::OSVersion.Version.Minor -ge 2)\r\n                      {{\r\n                          $rmSidId = new-object system.security.principal.securityidentifier \"{1}\"\r\n                          $sd.DiscretionaryAcl.AddAccess('Allow', $rmSidId, 268435456, 'none', 'none')\r\n                      }}\r\n                   }}\r\n\r\n                   $sddl = $sd.GetSddlForm(\"all\")\r\n                }}\r\n             }} # if ($sddlTemp)\r\n          }} # if (!$isSDDLSpecified) \r\n          \r\n          $qMessage = $queryForSet -f $_.name,$sddl\r\n          if (($sddl -or $isSDDLSpecified) -and ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForSet)))\r\n          {{\r\n              $null = Set-PSSessionConfiguration -Name $_.Name -SecurityDescriptorSddl $sddl -NoServiceRestart -force -WarningAction 0\r\n          }}\r\n       }} #end of Get-PSSessionConfiguration | foreach\r\n    }} # end of Process block\r\n\r\n    # restart the winrm to make the config change takes effect immediately\r\n    End\r\n    {{\r\n        if ($needWinRMRestart)\r\n        {{\r\n            Restart-Service winrm -force -confirm:$false\r\n        }}\r\n    }}\r\n}}\r\n\r\n$_ | Enable-PSSessionConfiguration -force $args[0] -sddl $args[1] -isSDDLSpecified $args[2] -queryForSet $args[3] -captionForSet $args[4] -queryForQC $args[5] -captionForQC $args[6] -whatif:$args[7] -confirm:$args[8] -shouldProcessDescForQC $args[9] -setEnabledTarget $args[10] -setEnabledAction $args[11] -skipNetworkProfileCheck $args[12]\r\n"));
            bool whatIf = false;
            bool confirm = true;
            PSSessionConfigurationCommandUtilities.CollectShouldProcessParameters(this, out whatIf, out confirm);
            string str = StringUtil.Format(RemotingErrorIdStrings.EcsWSManQCCaption, new object[0]);
            string str2 = StringUtil.Format(RemotingErrorIdStrings.EcsWSManQCQuery, "Set-WSManQuickConfig");
            string str3 = StringUtil.Format(RemotingErrorIdStrings.EcsWSManShouldProcessDesc, "Set-WSManQuickConfig");
            string str4 = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, "Set-PSSessionConfiguration");
            string ecsShouldProcessTarget = RemotingErrorIdStrings.EcsShouldProcessTarget;
            string str6 = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, "Set-Item");
            string setEnabledTrueTarget = RemotingErrorIdStrings.SetEnabledTrueTarget;
            enablePluginSb.InvokeUsingCmdlet(this, true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, this.shellsToEnable, new object[0], AutomationNull.Value, new object[] { this.force, this.sddl, this.isSddlSpecified, ecsShouldProcessTarget, str4, str2, str, whatIf, confirm, str3, setEnabledTrueTarget, str6, this.skipNetworkProfileCheck });
            Tracer tracer = new Tracer();
            StringBuilder builder = new StringBuilder();
            foreach (string str8 in this.Name ?? new string[0])
            {
                builder.Append(str8);
                builder.Append(", ");
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 2, 2);
            }
            tracer.EndpointEnabled(builder.ToString(), WindowsIdentity.GetCurrent().Name);
        }

        protected override void ProcessRecord()
        {
            if (this.shellName != null)
            {
                foreach (string str in this.shellName)
                {
                    this.shellsToEnable.Add(str);
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

        [Parameter(Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true), ValidateNotNullOrEmpty]
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

        [Parameter]
        public string SecurityDescriptorSddl
        {
            get
            {
                return this.sddl;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && (new CommonSecurityDescriptor(false, false, value) == null))
                {
                    throw new NotSupportedException();
                }
                this.sddl = value;
                this.isSddlSpecified = true;
            }
        }

        [Parameter]
        public SwitchParameter SkipNetworkProfileCheck
        {
            get
            {
                return this.skipNetworkProfileCheck;
            }
            set
            {
                this.skipNetworkProfileCheck = (bool) value;
            }
        }
    }
}

