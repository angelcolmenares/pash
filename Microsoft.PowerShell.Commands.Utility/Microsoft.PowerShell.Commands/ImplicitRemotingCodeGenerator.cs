namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell;
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Xml;

    internal class ImplicitRemotingCodeGenerator
    {
        private const string AuthenticationMechanismParameterTemplate = "-Authentication {0}";
        private const string CertificateThumbprintParameterTemplate = "-CertificateThumbprint '{0}'";
        private const string CommandProxyTemplate = "\r\n& $script:SetItem 'function:script:{0}' `\r\n{{\r\n    param(\r\n    {3})\r\n\r\n    Begin {{\r\n        try {{\r\n            $positionalArguments = & $script:NewObject collections.arraylist\r\n            foreach ($parameterName in $PSBoundParameters.BoundPositionally)\r\n            {{\r\n                $null = $positionalArguments.Add( $PSBoundParameters[$parameterName] )\r\n                $null = $PSBoundParameters.Remove($parameterName)\r\n            }}\r\n            $positionalArguments.AddRange($args)\r\n\r\n            $clientSideParameters = Get-PSImplicitRemotingClientSideParameters $PSBoundParameters ${8}\r\n\r\n            $scriptCmd = {{ & $script:InvokeCommand `\r\n                            @clientSideParameters `\r\n                            -HideComputerName `\r\n                            -Session (Get-PSImplicitRemotingSession -CommandName '{0}') `\r\n                            -Arg ('{0}', $PSBoundParameters, $positionalArguments) `\r\n                            -Script {{ param($name, $boundParams, $unboundParams) & $name @boundParams @unboundParams }} `\r\n                         }}\r\n\r\n            $steppablePipeline = $scriptCmd.GetSteppablePipeline($myInvocation.CommandOrigin)\r\n            $steppablePipeline.Begin($myInvocation.ExpectingInput, $ExecutionContext)\r\n        }} catch {{\r\n            throw\r\n        }}\r\n    }}\r\n    Process {{ {6} }}\r\n    End {{ {7} }}\r\n\r\n    # .ForwardHelpTargetName {1}\r\n    # .ForwardHelpCategory {5}\r\n    # .RemoteHelpRunspace PSSession\r\n}}\r\n        ";
        private const string ComputerNameParameterTemplate = "-ComputerName '{0}' `\r\n                    -ApplicationName '{1}' {2} {3} ";
        private const string CredentialParameterTemplate = "-Credential ( $host.UI.PromptForCredential( '{0}', '{1}', '{2}', '{3}' ) )";
        private const string ExportAliasesTemplate = "\r\n& $script:ExportModuleMember -Alias {0}\r\n        ";
        private const string ExportFunctionsTemplate = "\r\n& $script:ExportModuleMember -Function {0}\r\n        ";
        private const string HeaderTemplate = "\r\nparam(\r\n    <# {0} #>    \r\n    [System.Management.Automation.Runspaces.PSSession] $PSSessionOverride,\r\n    [System.Management.Automation.Remoting.PSSessionOption] $PSSessionOptionOverride\r\n)\r\n\r\n$script:__psImplicitRemoting_versionOfScriptGenerator = {1}\r\nif ($script:__psImplicitRemoting_versionOfScriptGenerator.Major -ne {2})\r\n{{\r\n    throw '{3}'\r\n}}\r\n\r\n\r\n$script:WriteHost = $executionContext.InvokeCommand.GetCommand('Write-Host', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:WriteWarning = $executionContext.InvokeCommand.GetCommand('Write-Warning', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:GetPSSession = $executionContext.InvokeCommand.GetCommand('Get-PSSession', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:NewPSSession = $executionContext.InvokeCommand.GetCommand('New-PSSession', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:ConnectPSSession = $executionContext.InvokeCommand.GetCommand('Connect-PSSession', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:NewObject = $executionContext.InvokeCommand.GetCommand('New-Object', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:RemovePSSession = $executionContext.InvokeCommand.GetCommand('Remove-PSSession', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:InvokeCommand = $executionContext.InvokeCommand.GetCommand('Invoke-Command', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:SetItem = $executionContext.InvokeCommand.GetCommand('Set-Item', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:ImportCliXml = $executionContext.InvokeCommand.GetCommand('Import-CliXml', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:NewPSSessionOption = $executionContext.InvokeCommand.GetCommand('New-PSSessionOption', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:JoinPath = $executionContext.InvokeCommand.GetCommand('Join-Path', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:ExportModuleMember = $executionContext.InvokeCommand.GetCommand('Export-ModuleMember', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:SetAlias = $executionContext.InvokeCommand.GetCommand('Set-Alias', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n\r\n$script:MyModule = $MyInvocation.MyCommand.ScriptBlock.Module\r\n        ";
        private const string HelperFunctionsGetImplicitRunspaceTemplate = "\r\nfunction Get-PSImplicitRemotingSession\r\n{{\r\n    param(\r\n        [Parameter(Mandatory = $true, Position = 0)]\r\n        [string] \r\n        $commandName\r\n    )\r\n\r\n    $savedImplicitRemotingHash = '{4}'\r\n\r\n    if (($script:PSSession -eq $null) -or ($script:PSSession.Runspace.RunspaceStateInfo.State -ne 'Opened'))\r\n    {{\r\n        Set-PSImplicitRemotingSession `\r\n            (& $script:GetPSSession `\r\n                -InstanceId {0} `\r\n                -ErrorAction SilentlyContinue )\r\n    }}\r\n    if (($script:PSSession -ne $null) -and ($script:PSSession.Runspace.RunspaceStateInfo.State -eq 'Disconnected'))\r\n    {{\r\n        # If we are handed a disconnected session, try re-connecting it before creating a new session.\r\n        Set-PSImplicitRemotingSession `\r\n            (& $script:ConnectPSSession `\r\n                -Session $script:PSSession `\r\n                -ErrorAction SilentlyContinue)\r\n    }}\r\n    if (($script:PSSession -eq $null) -or ($script:PSSession.Runspace.RunspaceStateInfo.State -ne 'Opened'))\r\n    {{\r\n        Write-PSImplicitRemotingMessage ('{1}' -f $commandName)\r\n\r\n        Set-PSImplicitRemotingSession `\r\n            -CreatedByModule $true `\r\n            -PSSession ( {2} )\r\n\r\n        if ($savedImplicitRemotingHash -ne '')\r\n        {{\r\n            $newImplicitRemotingHash = [string]($script:PSSession.ApplicationPrivateData.{6}.{7})\r\n            if ($newImplicitRemotingHash -ne $savedImplicitRemotingHash)\r\n            {{\r\n                & $script:WriteWarning -Message '{5}'\r\n            }}\r\n        }}\r\n\r\n        {8}\r\n    }}\r\n    if (($script:PSSession -eq $null) -or ($script:PSSession.Runspace.RunspaceStateInfo.State -ne 'Opened'))\r\n    {{\r\n        throw '{3}'\r\n    }}\r\n    return [Management.Automation.Runspaces.PSSession]$script:PSSession\r\n}}\r\n";
        private const string HelperFunctionsGetSessionOptionTemplate = "\r\nfunction Get-PSImplicitRemotingSessionOption\r\n{{\r\n    if ($PSSessionOptionOverride -ne $null)\r\n    {{\r\n        return $PSSessionOptionOverride\r\n    }}\r\n    else\r\n    {{\r\n        return $({0})\r\n    }}\r\n}}\r\n";
        private const string HelperFunctionsModifyParameters = "\r\nfunction Modify-PSImplicitRemotingParameters\r\n{\r\n    param(\r\n        [Parameter(Mandatory = $true, Position = 0)]\r\n        [hashtable]\r\n        $clientSideParameters,\r\n\r\n        [Parameter(Mandatory = $true, Position = 1)]\r\n        $PSBoundParameters,\r\n\r\n        [Parameter(Mandatory = $true, Position = 2)]\r\n        [string]\r\n        $parameterName,\r\n\r\n        [Parameter()]\r\n        [switch]\r\n        $leaveAsRemoteParameter)\r\n        \r\n    if ($PSBoundParameters.ContainsKey($parameterName))\r\n    {\r\n        $clientSideParameters.Add($parameterName, $PSBoundParameters[$parameterName])\r\n        if (-not $leaveAsRemoteParameter) { \r\n            $null = $PSBoundParameters.Remove($parameterName) \r\n        }\r\n    }\r\n}\r\n\r\nfunction Get-PSImplicitRemotingClientSideParameters\r\n{\r\n    param(\r\n        [Parameter(Mandatory = $true, Position = 1)]\r\n        $PSBoundParameters,\r\n\r\n        [Parameter(Mandatory = $true, Position = 2)]\r\n        $proxyForCmdlet)\r\n\r\n    $clientSideParameters = @{}\r\n    Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'AsJob'\r\n    if ($proxyForCmdlet)\r\n    {\r\n        Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'OutVariable'\r\n        Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'OutBuffer'\r\n        Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'ErrorAction' -LeaveAsRemoteParameter\r\n        Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'ErrorVariable'\r\n        Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'WarningAction' -LeaveAsRemoteParameter\r\n        Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'WarningVariable'\r\n    }\r\n\r\n    return $clientSideParameters\r\n}\r\n";
        private const string HelperFunctionsSetImplicitRunspaceTemplate = "\r\n$script:PSSession = $null\r\n\r\nfunction Get-PSImplicitRemotingModuleName {{ $myInvocation.MyCommand.ScriptBlock.File }}\r\n\r\nfunction Set-PSImplicitRemotingSession\r\n{{\r\n    param(\r\n        [Parameter(Mandatory = $true, Position = 0)]\r\n        [AllowNull()]\r\n        [Management.Automation.Runspaces.PSSession] \r\n        $PSSession, \r\n\r\n        [Parameter(Mandatory = $false, Position = 1)]\r\n        [bool] $createdByModule = $false)\r\n\r\n    if ($PSSession -ne $null)\r\n    {{\r\n        $script:PSSession = $PSSession\r\n\r\n        if ($createdByModule -and ($script:PSSession -ne $null))\r\n        {{\r\n            $moduleName = Get-PSImplicitRemotingModuleName \r\n            $script:PSSession.Name = '{0}' -f $moduleName\r\n            \r\n            $oldCleanUpScript = $script:MyModule.OnRemove\r\n            $removePSSessionCommand = $script:RemovePSSession\r\n            $script:MyModule.OnRemove = {{ \r\n                & $removePSSessionCommand -Session $PSSession -ErrorAction SilentlyContinue\r\n                if ($oldCleanUpScript)\r\n                {{\r\n                    & $oldCleanUpScript $args\r\n                }}\r\n            }}.GetNewClosure()\r\n        }}\r\n    }}\r\n}}\r\n\r\nif ($PSSessionOverride) {{ Set-PSImplicitRemotingSession $PSSessionOverride }}\r\n";
        private const string HelperFunctionsWriteMessage = "\r\nfunction Write-PSImplicitRemotingMessage\r\n{\r\n    param(\r\n        [Parameter(Mandatory = $true, Position = 0)]\r\n        [string]\r\n        $message)\r\n        \r\n    try { & $script:WriteHost -Object $message -ErrorAction SilentlyContinue } catch { }\r\n}\r\n";
        private InvocationInfo invocationInfo;
        private const string ManifestTemplate = "\r\n@{{\r\n    GUID = '{0}'\r\n    Description = '{1}'\r\n    ModuleToProcess = @('{2}')\r\n    FormatsToProcess = @('{3}')\r\n\r\n    ModuleVersion = '1.0'\r\n\r\n    PrivateData = @{{\r\n        ImplicitRemoting = $true\r\n    }}\r\n}}\r\n        ";
        private Guid moduleGuid;
        private const string NewRunspaceTemplate = "\r\n            $( \r\n                & $script:NewPSSession `\r\n                    {0} -ConfigurationName '{1}' `\r\n                    -SessionOption (Get-PSImplicitRemotingSessionOption) `\r\n                    {2} `\r\n                    {3} `\r\n                    {4} `\r\n                    {5} `\r\n            )\r\n";
        private const string ProxyCredentialParameterTemplate = "-ProxyCredential ( $host.UI.PromptForCredential( '{0}', '{1}', '{2}', '{3}' ) ) ";
        private const string ReimportTemplate = "\r\n            try {{\r\n                & $script:InvokeCommand -Session $script:PSSession -ScriptBlock {{ \r\n                    Get-Module -ListAvailable -Name '{0}' | Import-Module \r\n                }} -ErrorAction SilentlyContinue\r\n            }} catch {{ }}\r\n";
        private PSSession remoteRunspaceInfo;
        private const string SectionSeparator = "\r\n##############################################################################\r\n";
        private const string SetAliasTemplate = "\r\n& $script:SetAlias -Name '{0}' -Value '{1}' -Force -Scope script\r\n        ";
        private const string TopCommentTemplate = "\r\n<#\r\n # {0}\r\n # {1}\r\n # {2}\r\n # {3}\r\n #>\r\n        ";
        internal static readonly Version VersionOfScriptWriter = new Version(1, 0);

        internal ImplicitRemotingCodeGenerator(PSSession remoteRunspaceInfo, Guid moduleGuid, InvocationInfo invocationInfo)
        {
            this.remoteRunspaceInfo = remoteRunspaceInfo;
            this.moduleGuid = moduleGuid;
            this.invocationInfo = invocationInfo;
        }

        private string EscapeFunctionNameForRemoteHelp(string name)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            StringBuilder builder = new StringBuilder(name.Length);
            foreach (char ch in name)
            {
                if ((("\"'`$".IndexOf(ch) == -1) && !char.IsControl(ch)) && !char.IsWhiteSpace(ch))
                {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }

        private void GenerateAliases(TextWriter writer, Dictionary<string, string> alias2resolvedCommandName)
        {
            this.GenerateSectionSeparator(writer);
            foreach (KeyValuePair<string, string> pair in alias2resolvedCommandName)
            {
                string key = pair.Key;
                string stringContent = pair.Value;
                writer.Write("\r\n& $script:SetAlias -Name '{0}' -Value '{1}' -Force -Scope script\r\n        ", CommandMetadata.EscapeSingleQuotedString(key), CommandMetadata.EscapeSingleQuotedString(stringContent));
            }
            string str3 = this.GenerateArrayString(alias2resolvedCommandName.Keys);
            writer.Write("\r\n& $script:ExportModuleMember -Alias {0}\r\n        ", str3);
        }

        private string GenerateAllowRedirectionParameter()
        {
            WSManConnectionInfo connectionInfo = this.remoteRunspaceInfo.Runspace.ConnectionInfo as WSManConnectionInfo;
            if ((connectionInfo != null) && (connectionInfo.MaximumConnectionRedirectionCount != 0))
            {
                return "-AllowRedirection";
            }
            return string.Empty;
        }

        private string GenerateArrayString(IEnumerable<string> listOfStrings)
        {
            if (listOfStrings == null)
            {
                throw PSTraceSource.NewArgumentNullException("listOfStrings");
            }
            StringBuilder builder = new StringBuilder();
            foreach (string str in listOfStrings)
            {
                if (builder.Length != 0)
                {
                    builder.Append(", ");
                }
                builder.Append('\'');
                builder.Append(CommandMetadata.EscapeSingleQuotedString(str));
                builder.Append('\'');
            }
            builder.Insert(0, "@(");
            builder.Append(")");
            return builder.ToString();
        }

        private string GenerateAuthenticationMechanismParameter()
        {
            if (this.remoteRunspaceInfo.Runspace.ConnectionInfo.CertificateThumbprint != null)
            {
                return string.Empty;
            }
            WSManConnectionInfo connectionInfo = this.remoteRunspaceInfo.Runspace.ConnectionInfo as WSManConnectionInfo;
            if (connectionInfo == null)
            {
                return string.Empty;
            }
            return string.Format(CultureInfo.InvariantCulture, "-Authentication {0}", new object[] { connectionInfo.AuthenticationMechanism.ToString() });
        }

        private string GenerateCertificateThumbprintParameter()
        {
            if (this.remoteRunspaceInfo.Runspace.ConnectionInfo.CertificateThumbprint == null)
            {
                return string.Empty;
            }
            return string.Format(CultureInfo.InvariantCulture, "-CertificateThumbprint '{0}'", new object[] { CommandMetadata.EscapeSingleQuotedString(this.remoteRunspaceInfo.Runspace.ConnectionInfo.CertificateThumbprint) });
        }

        private void GenerateCommandProxy(TextWriter writer, IEnumerable<CommandMetadata> listOfCommandMetadata)
        {
            if (writer == null)
            {
                throw PSTraceSource.NewArgumentNullException("writer");
            }
            if (listOfCommandMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("listOfCommandMetadata");
            }
            this.GenerateSectionSeparator(writer);
            foreach (CommandMetadata metadata in listOfCommandMetadata)
            {
                this.GenerateCommandProxy(writer, metadata);
            }
        }

        private void GenerateCommandProxy(TextWriter writer, CommandMetadata commandMetadata)
        {
            if (writer == null)
            {
                throw PSTraceSource.NewArgumentNullException("writer");
            }
            string str = CommandMetadata.EscapeSingleQuotedString(commandMetadata.Name);
            string str2 = this.EscapeFunctionNameForRemoteHelp(commandMetadata.Name);
            object[] arg = new object[9];
            arg[0] = str;
            arg[1] = str2;
            arg[2] = commandMetadata.GetDecl();
            arg[3] = commandMetadata.GetParamBlock();
            arg[5] = commandMetadata.WrappedCommandType;
            arg[6] = ProxyCommand.GetProcess(commandMetadata);
            arg[7] = ProxyCommand.GetEnd(commandMetadata);
            arg[8] = commandMetadata.WrappedAnyCmdlet;
            writer.Write("\r\n& $script:SetItem 'function:script:{0}' `\r\n{{\r\n    param(\r\n    {3})\r\n\r\n    Begin {{\r\n        try {{\r\n            $positionalArguments = & $script:NewObject collections.arraylist\r\n            foreach ($parameterName in $PSBoundParameters.BoundPositionally)\r\n            {{\r\n                $null = $positionalArguments.Add( $PSBoundParameters[$parameterName] )\r\n                $null = $PSBoundParameters.Remove($parameterName)\r\n            }}\r\n            $positionalArguments.AddRange($args)\r\n\r\n            $clientSideParameters = Get-PSImplicitRemotingClientSideParameters $PSBoundParameters ${8}\r\n\r\n            $scriptCmd = {{ & $script:InvokeCommand `\r\n                            @clientSideParameters `\r\n                            -HideComputerName `\r\n                            -Session (Get-PSImplicitRemotingSession -CommandName '{0}') `\r\n                            -Arg ('{0}', $PSBoundParameters, $positionalArguments) `\r\n                            -Script {{ param($name, $boundParams, $unboundParams) & $name @boundParams @unboundParams }} `\r\n                         }}\r\n\r\n            $steppablePipeline = $scriptCmd.GetSteppablePipeline($myInvocation.CommandOrigin)\r\n            $steppablePipeline.Begin($myInvocation.ExpectingInput, $ExecutionContext)\r\n        }} catch {{\r\n            throw\r\n        }}\r\n    }}\r\n    Process {{ {6} }}\r\n    End {{ {7} }}\r\n\r\n    # .ForwardHelpTargetName {1}\r\n    # .ForwardHelpCategory {5}\r\n    # .RemoteHelpRunspace PSSession\r\n}}\r\n        ", arg);
        }

        private string GenerateConnectionStringForNewRunspace()
        {
            string str = null;
            WSManConnectionInfo connectionInfo = this.remoteRunspaceInfo.Runspace.ConnectionInfo as WSManConnectionInfo;
            if (connectionInfo == null)
            {
                return str;
            }
            if (connectionInfo.UseDefaultWSManPort)
            {
                bool flag;
                WSManConnectionInfo.GetConnectionString(connectionInfo.ConnectionUri, out flag);
                return string.Format(CultureInfo.InvariantCulture, "-ComputerName '{0}' `\r\n                    -ApplicationName '{1}' {2} {3} ", new object[] { CommandMetadata.EscapeSingleQuotedString(connectionInfo.ComputerName), CommandMetadata.EscapeSingleQuotedString(connectionInfo.AppName), connectionInfo.UseDefaultWSManPort ? string.Empty : string.Format(CultureInfo.InvariantCulture, "-Port {0} ", new object[] { connectionInfo.Port }), flag ? "-useSSL" : string.Empty });
            }
            return string.Format(CultureInfo.InvariantCulture, "-connectionUri '{0}'", new object[] { CommandMetadata.EscapeSingleQuotedString(this.GetConnectionString()) });
        }

        private string GenerateCredentialParameter()
        {
            if (this.remoteRunspaceInfo.Runspace.ConnectionInfo.Credential == null)
            {
                return string.Empty;
            }
            return string.Format(CultureInfo.InvariantCulture, "-Credential ( $host.UI.PromptForCredential( '{0}', '{1}', '{2}', '{3}' ) )", new object[] { CommandMetadata.EscapeSingleQuotedString(StringUtil.Format(ImplicitRemotingStrings.CredentialRequestTitle, new object[0])), CommandMetadata.EscapeSingleQuotedString(StringUtil.Format(ImplicitRemotingStrings.CredentialRequestBody, this.GetConnectionString())), CommandMetadata.EscapeSingleQuotedString(this.remoteRunspaceInfo.Runspace.ConnectionInfo.Credential.UserName), CommandMetadata.EscapeSingleQuotedString(this.remoteRunspaceInfo.ComputerName) });
        }

        private void GenerateExportDeclaration(TextWriter writer, IEnumerable<CommandMetadata> listOfCommandMetadata)
        {
            if (writer == null)
            {
                throw PSTraceSource.NewArgumentNullException("writer");
            }
            if (listOfCommandMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("listOfCommandMetadata");
            }
            this.GenerateSectionSeparator(writer);
            List<string> listOfCommandNames = this.GetListOfCommandNames(listOfCommandMetadata);
            string str = this.GenerateArrayString(listOfCommandNames);
            writer.Write("\r\n& $script:ExportModuleMember -Function {0}\r\n        ", str);
        }

        private void GenerateFormatFile(TextWriter writer, List<ExtendedTypeDefinition> listOfFormatData)
        {
            if (writer == null)
            {
                throw PSTraceSource.NewArgumentNullException("writer");
            }
            if (listOfFormatData == null)
            {
                throw PSTraceSource.NewArgumentNullException("listOfFormatData");
            }
            XmlWriterSettings settings = new XmlWriterSettings {
                CloseOutput = false,
                ConformanceLevel = ConformanceLevel.Document,
                Encoding = writer.Encoding,
                Indent = true
            };
            using (XmlWriter writer2 = XmlWriter.Create(writer, settings))
            {
                FormatXMLHelper.WriteToXML(writer2, listOfFormatData, false);
            }
        }

        private void GenerateHelperFunctions(TextWriter writer)
        {
            this.GenerateSectionSeparator(writer);
            this.GenerateHelperFunctionsWriteMessage(writer);
            this.GenerateHelperFunctionsGetSessionOption(writer);
            this.GenerateHelperFunctionsSetImplicitRunspace(writer);
            this.GenerateHelperFunctionsGetImplicitRunspace(writer);
            this.GenerateHelperFunctionsClientSideParameters(writer);
        }

        private void GenerateHelperFunctionsClientSideParameters(TextWriter writer)
        {
            if (writer == null)
            {
                throw PSTraceSource.NewArgumentNullException("writer");
            }
            writer.Write("\r\nfunction Modify-PSImplicitRemotingParameters\r\n{\r\n    param(\r\n        [Parameter(Mandatory = $true, Position = 0)]\r\n        [hashtable]\r\n        $clientSideParameters,\r\n\r\n        [Parameter(Mandatory = $true, Position = 1)]\r\n        $PSBoundParameters,\r\n\r\n        [Parameter(Mandatory = $true, Position = 2)]\r\n        [string]\r\n        $parameterName,\r\n\r\n        [Parameter()]\r\n        [switch]\r\n        $leaveAsRemoteParameter)\r\n        \r\n    if ($PSBoundParameters.ContainsKey($parameterName))\r\n    {\r\n        $clientSideParameters.Add($parameterName, $PSBoundParameters[$parameterName])\r\n        if (-not $leaveAsRemoteParameter) { \r\n            $null = $PSBoundParameters.Remove($parameterName) \r\n        }\r\n    }\r\n}\r\n\r\nfunction Get-PSImplicitRemotingClientSideParameters\r\n{\r\n    param(\r\n        [Parameter(Mandatory = $true, Position = 1)]\r\n        $PSBoundParameters,\r\n\r\n        [Parameter(Mandatory = $true, Position = 2)]\r\n        $proxyForCmdlet)\r\n\r\n    $clientSideParameters = @{}\r\n    Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'AsJob'\r\n    if ($proxyForCmdlet)\r\n    {\r\n        Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'OutVariable'\r\n        Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'OutBuffer'\r\n        Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'ErrorAction' -LeaveAsRemoteParameter\r\n        Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'ErrorVariable'\r\n        Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'WarningAction' -LeaveAsRemoteParameter\r\n        Modify-PSImplicitRemotingParameters $clientSideParameters $PSBoundParameters 'WarningVariable'\r\n    }\r\n\r\n    return $clientSideParameters\r\n}\r\n");
        }

        private void GenerateHelperFunctionsGetImplicitRunspace(TextWriter writer)
        {
            string str;
            if (writer == null)
            {
                throw PSTraceSource.NewArgumentNullException("writer");
            }
            PSPrimitiveDictionary.TryPathGet<string>(this.remoteRunspaceInfo.ApplicationPrivateData, out str, new string[] { "ImplicitRemoting", "Hash" });
            str = str ?? string.Empty;
            writer.Write("\r\nfunction Get-PSImplicitRemotingSession\r\n{{\r\n    param(\r\n        [Parameter(Mandatory = $true, Position = 0)]\r\n        [string] \r\n        $commandName\r\n    )\r\n\r\n    $savedImplicitRemotingHash = '{4}'\r\n\r\n    if (($script:PSSession -eq $null) -or ($script:PSSession.Runspace.RunspaceStateInfo.State -ne 'Opened'))\r\n    {{\r\n        Set-PSImplicitRemotingSession `\r\n            (& $script:GetPSSession `\r\n                -InstanceId {0} `\r\n                -ErrorAction SilentlyContinue )\r\n    }}\r\n    if (($script:PSSession -ne $null) -and ($script:PSSession.Runspace.RunspaceStateInfo.State -eq 'Disconnected'))\r\n    {{\r\n        # If we are handed a disconnected session, try re-connecting it before creating a new session.\r\n        Set-PSImplicitRemotingSession `\r\n            (& $script:ConnectPSSession `\r\n                -Session $script:PSSession `\r\n                -ErrorAction SilentlyContinue)\r\n    }}\r\n    if (($script:PSSession -eq $null) -or ($script:PSSession.Runspace.RunspaceStateInfo.State -ne 'Opened'))\r\n    {{\r\n        Write-PSImplicitRemotingMessage ('{1}' -f $commandName)\r\n\r\n        Set-PSImplicitRemotingSession `\r\n            -CreatedByModule $true `\r\n            -PSSession ( {2} )\r\n\r\n        if ($savedImplicitRemotingHash -ne '')\r\n        {{\r\n            $newImplicitRemotingHash = [string]($script:PSSession.ApplicationPrivateData.{6}.{7})\r\n            if ($newImplicitRemotingHash -ne $savedImplicitRemotingHash)\r\n            {{\r\n                & $script:WriteWarning -Message '{5}'\r\n            }}\r\n        }}\r\n\r\n        {8}\r\n    }}\r\n    if (($script:PSSession -eq $null) -or ($script:PSSession.Runspace.RunspaceStateInfo.State -ne 'Opened'))\r\n    {{\r\n        throw '{3}'\r\n    }}\r\n    return [Management.Automation.Runspaces.PSSession]$script:PSSession\r\n}}\r\n", new object[] { this.remoteRunspaceInfo.InstanceId, CommandMetadata.EscapeSingleQuotedString(StringUtil.Format(ImplicitRemotingStrings.CreateNewRunspaceMessageTemplate, new object[0])), this.GenerateNewRunspaceExpression(), CommandMetadata.EscapeSingleQuotedString(StringUtil.Format(ImplicitRemotingStrings.ErrorNoRunspaceForThisModule, new object[0])), CommandMetadata.EscapeSingleQuotedString(str), CommandMetadata.EscapeSingleQuotedString(StringUtil.Format(ImplicitRemotingStrings.WarningMismatchedImplicitRemotingHash, new object[0])), "ImplicitRemoting", "Hash", this.GenerateReimportingOfModules() });
        }

        private void GenerateHelperFunctionsGetSessionOption(TextWriter writer)
        {
            if (writer == null)
            {
                throw PSTraceSource.NewArgumentNullException("writer");
            }
            writer.Write("\r\nfunction Get-PSImplicitRemotingSessionOption\r\n{{\r\n    if ($PSSessionOptionOverride -ne $null)\r\n    {{\r\n        return $PSSessionOptionOverride\r\n    }}\r\n    else\r\n    {{\r\n        return $({0})\r\n    }}\r\n}}\r\n", this.GenerateNewPSSessionOption());
        }

        private void GenerateHelperFunctionsSetImplicitRunspace(TextWriter writer)
        {
            if (writer == null)
            {
                throw PSTraceSource.NewArgumentNullException("writer");
            }
            string stringContent = StringUtil.Format(ImplicitRemotingStrings.ProxyRunspaceNameTemplate, new object[0]);
            writer.Write("\r\n$script:PSSession = $null\r\n\r\nfunction Get-PSImplicitRemotingModuleName {{ $myInvocation.MyCommand.ScriptBlock.File }}\r\n\r\nfunction Set-PSImplicitRemotingSession\r\n{{\r\n    param(\r\n        [Parameter(Mandatory = $true, Position = 0)]\r\n        [AllowNull()]\r\n        [Management.Automation.Runspaces.PSSession] \r\n        $PSSession, \r\n\r\n        [Parameter(Mandatory = $false, Position = 1)]\r\n        [bool] $createdByModule = $false)\r\n\r\n    if ($PSSession -ne $null)\r\n    {{\r\n        $script:PSSession = $PSSession\r\n\r\n        if ($createdByModule -and ($script:PSSession -ne $null))\r\n        {{\r\n            $moduleName = Get-PSImplicitRemotingModuleName \r\n            $script:PSSession.Name = '{0}' -f $moduleName\r\n            \r\n            $oldCleanUpScript = $script:MyModule.OnRemove\r\n            $removePSSessionCommand = $script:RemovePSSession\r\n            $script:MyModule.OnRemove = {{ \r\n                & $removePSSessionCommand -Session $PSSession -ErrorAction SilentlyContinue\r\n                if ($oldCleanUpScript)\r\n                {{\r\n                    & $oldCleanUpScript $args\r\n                }}\r\n            }}.GetNewClosure()\r\n        }}\r\n    }}\r\n}}\r\n\r\nif ($PSSessionOverride) {{ Set-PSImplicitRemotingSession $PSSessionOverride }}\r\n", CommandMetadata.EscapeSingleQuotedString(stringContent));
        }

        private void GenerateHelperFunctionsWriteMessage(TextWriter writer)
        {
            if (writer == null)
            {
                throw PSTraceSource.NewArgumentNullException("writer");
            }
            writer.Write("\r\nfunction Write-PSImplicitRemotingMessage\r\n{\r\n    param(\r\n        [Parameter(Mandatory = $true, Position = 0)]\r\n        [string]\r\n        $message)\r\n        \r\n    try { & $script:WriteHost -Object $message -ErrorAction SilentlyContinue } catch { }\r\n}\r\n");
        }

        private void GenerateManifest(TextWriter writer, string psm1fileName, string formatPs1xmlFileName)
        {
            if (writer == null)
            {
                throw PSTraceSource.NewArgumentNullException("writer");
            }
            this.GenerateTopComment(writer);
            writer.Write("\r\n@{{\r\n    GUID = '{0}'\r\n    Description = '{1}'\r\n    ModuleToProcess = @('{2}')\r\n    FormatsToProcess = @('{3}')\r\n\r\n    ModuleVersion = '1.0'\r\n\r\n    PrivateData = @{{\r\n        ImplicitRemoting = $true\r\n    }}\r\n}}\r\n        ", new object[] { CommandMetadata.EscapeSingleQuotedString(this.moduleGuid.ToString()), CommandMetadata.EscapeSingleQuotedString(StringUtil.Format(ImplicitRemotingStrings.ProxyModuleDescription, this.GetConnectionString())), CommandMetadata.EscapeSingleQuotedString(Path.GetFileName(psm1fileName)), CommandMetadata.EscapeSingleQuotedString(Path.GetFileName(formatPs1xmlFileName)) });
        }

        private void GenerateModuleHeader(TextWriter writer)
        {
            if (writer == null)
            {
                throw PSTraceSource.NewArgumentNullException("writer");
            }
            string str = "[" + typeof(ExportPSSessionCommand).AssemblyQualifiedName + "]::VersionOfScriptGenerator";
            this.GenerateTopComment(writer);
            writer.Write("\r\nparam(\r\n    <# {0} #>    \r\n    [System.Management.Automation.Runspaces.PSSession] $PSSessionOverride,\r\n    [System.Management.Automation.Remoting.PSSessionOption] $PSSessionOptionOverride\r\n)\r\n\r\n$script:__psImplicitRemoting_versionOfScriptGenerator = {1}\r\nif ($script:__psImplicitRemoting_versionOfScriptGenerator.Major -ne {2})\r\n{{\r\n    throw '{3}'\r\n}}\r\n\r\n\r\n$script:WriteHost = $executionContext.InvokeCommand.GetCommand('Write-Host', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:WriteWarning = $executionContext.InvokeCommand.GetCommand('Write-Warning', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:GetPSSession = $executionContext.InvokeCommand.GetCommand('Get-PSSession', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:NewPSSession = $executionContext.InvokeCommand.GetCommand('New-PSSession', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:ConnectPSSession = $executionContext.InvokeCommand.GetCommand('Connect-PSSession', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:NewObject = $executionContext.InvokeCommand.GetCommand('New-Object', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:RemovePSSession = $executionContext.InvokeCommand.GetCommand('Remove-PSSession', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:InvokeCommand = $executionContext.InvokeCommand.GetCommand('Invoke-Command', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:SetItem = $executionContext.InvokeCommand.GetCommand('Set-Item', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:ImportCliXml = $executionContext.InvokeCommand.GetCommand('Import-CliXml', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:NewPSSessionOption = $executionContext.InvokeCommand.GetCommand('New-PSSessionOption', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:JoinPath = $executionContext.InvokeCommand.GetCommand('Join-Path', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:ExportModuleMember = $executionContext.InvokeCommand.GetCommand('Export-ModuleMember', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n$script:SetAlias = $executionContext.InvokeCommand.GetCommand('Set-Alias', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n\r\n$script:MyModule = $MyInvocation.MyCommand.ScriptBlock.Module\r\n        ", new object[] { CommandMetadata.EscapeBlockComment(StringUtil.Format(ImplicitRemotingStrings.ModuleHeaderRunspaceOverrideParameter, new object[0])), str, VersionOfScriptWriter, CommandMetadata.EscapeSingleQuotedString(string.Format(null, PathUtilsStrings.ExportPSSession_ScriptGeneratorVersionMismatch, new object[] { "Export-PSSession" })) });
        }

        private string GenerateNewPSSessionOption()
        {
            StringBuilder builder = new StringBuilder("& $script:NewPSSessionOption ");
            RunspaceConnectionInfo connectionInfo = this.remoteRunspaceInfo.Runspace.ConnectionInfo;
            if (connectionInfo != null)
            {
                builder.AppendFormat(null, "-Culture '{0}' ", new object[] { CommandMetadata.EscapeSingleQuotedString(connectionInfo.Culture.ToString()) });
                builder.AppendFormat(null, "-UICulture '{0}' ", new object[] { CommandMetadata.EscapeSingleQuotedString(connectionInfo.UICulture.ToString()) });
                builder.AppendFormat(null, "-CancelTimeOut {0} ", new object[] { connectionInfo.CancelTimeout });
                builder.AppendFormat(null, "-IdleTimeOut {0} ", new object[] { connectionInfo.IdleTimeout });
                builder.AppendFormat(null, "-OpenTimeOut {0} ", new object[] { connectionInfo.OpenTimeout });
                builder.AppendFormat(null, "-OperationTimeOut {0} ", new object[] { connectionInfo.OperationTimeout });
            }
            WSManConnectionInfo wsmanConnectionInfo = this.remoteRunspaceInfo.Runspace.ConnectionInfo as WSManConnectionInfo;
            if (wsmanConnectionInfo != null)
            {
                if (!wsmanConnectionInfo.UseCompression)
                {
                    builder.Append("-NoCompression ");
                }
                if (wsmanConnectionInfo.NoEncryption)
                {
                    builder.Append("-NoEncryption ");
                }
                if (wsmanConnectionInfo.NoMachineProfile)
                {
                    builder.Append("-NoMachineProfile ");
                }
                if (wsmanConnectionInfo.UseUTF16)
                {
                    builder.Append("-UseUTF16 ");
                }
                if (wsmanConnectionInfo.SkipCACheck)
                {
                    builder.Append("-SkipCACheck ");
                }
                if (wsmanConnectionInfo.SkipCNCheck)
                {
                    builder.Append("-SkipCNCheck ");
                }
                if (wsmanConnectionInfo.SkipRevocationCheck)
                {
                    builder.Append("-SkipRevocationCheck ");
                }
                if (wsmanConnectionInfo.MaximumReceivedDataSizePerCommand.HasValue)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, "-MaximumReceivedDataSizePerCommand {0} ", new object[] { wsmanConnectionInfo.MaximumReceivedDataSizePerCommand.Value });
                }
                if (wsmanConnectionInfo.MaximumReceivedObjectSize.HasValue)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, "-MaximumReceivedObjectSize {0} ", new object[] { wsmanConnectionInfo.MaximumReceivedObjectSize.Value });
                }
                builder.AppendFormat(CultureInfo.InvariantCulture, "-MaximumRedirection {0} ", new object[] { wsmanConnectionInfo.MaximumConnectionRedirectionCount });
                builder.AppendFormat(CultureInfo.InvariantCulture, "-ProxyAccessType {0} ", new object[] { wsmanConnectionInfo.ProxyAccessType.ToString() });
                builder.AppendFormat(CultureInfo.InvariantCulture, "-ProxyAuthentication {0} ", new object[] { wsmanConnectionInfo.ProxyAuthentication.ToString() });
                builder.Append(this.GenerateProxyCredentialParameter(wsmanConnectionInfo));
            }
            if (this.GetApplicationArguments() != null)
            {
                builder.Append("-ApplicationArguments $(");
                builder.Append("& $script:ImportCliXml -Path $(");
                builder.Append("& $script:JoinPath -Path $PSScriptRoot -ChildPath ApplicationArguments.xml");
                builder.Append(")");
                builder.Append(") ");
            }
            return builder.ToString();
        }

        private string GenerateNewRunspaceExpression()
        {
            return string.Format(CultureInfo.InvariantCulture, "\r\n            $( \r\n                & $script:NewPSSession `\r\n                    {0} -ConfigurationName '{1}' `\r\n                    -SessionOption (Get-PSImplicitRemotingSessionOption) `\r\n                    {2} `\r\n                    {3} `\r\n                    {4} `\r\n                    {5} `\r\n            )\r\n", new object[] { this.GenerateConnectionStringForNewRunspace(), CommandMetadata.EscapeSingleQuotedString(this.remoteRunspaceInfo.ConfigurationName), this.GenerateCredentialParameter(), this.GenerateCertificateThumbprintParameter(), this.GenerateAuthenticationMechanismParameter(), this.GenerateAllowRedirectionParameter() });
        }

        private string GenerateProxyCredentialParameter(WSManConnectionInfo wsmanConnectionInfo)
        {
            if ((wsmanConnectionInfo == null) || (wsmanConnectionInfo.ProxyCredential == null))
            {
                return string.Empty;
            }
            return string.Format(CultureInfo.InvariantCulture, "-ProxyCredential ( $host.UI.PromptForCredential( '{0}', '{1}', '{2}', '{3}' ) ) ", new object[] { CommandMetadata.EscapeSingleQuotedString(StringUtil.Format(ImplicitRemotingStrings.CredentialRequestTitle, new object[0])), CommandMetadata.EscapeSingleQuotedString(StringUtil.Format(ImplicitRemotingStrings.ProxyCredentialRequestBody, this.GetConnectionString())), CommandMetadata.EscapeSingleQuotedString(wsmanConnectionInfo.ProxyCredential.UserName), CommandMetadata.EscapeSingleQuotedString(this.remoteRunspaceInfo.ComputerName + @"\httpproxy") });
        }

        internal List<string> GenerateProxyModule(DirectoryInfo moduleRootDirectory, string fileNamePrefix, Encoding encoding, bool force, List<CommandMetadata> listOfCommandMetadata, Dictionary<string, string> alias2resolvedCommandName, List<ExtendedTypeDefinition> listOfFormatData, X509Certificate2 certificate)
        {
            List<string> list = new List<string>();
            string str = Path.Combine(moduleRootDirectory.FullName, fileNamePrefix);
            FileMode mode = force ? FileMode.OpenOrCreate : FileMode.CreateNew;
            list.Add(str + ".psm1");
            FileStream stream = new FileStream(str + ".psm1", mode, FileAccess.Write, FileShare.None);
            using (TextWriter writer = new StreamWriter(stream, encoding))
            {
                if (listOfCommandMetadata == null)
                {
                    listOfCommandMetadata = new List<CommandMetadata>();
                }
                this.GenerateModuleHeader(writer);
                this.GenerateHelperFunctions(writer);
                this.GenerateCommandProxy(writer, listOfCommandMetadata);
                this.GenerateExportDeclaration(writer, listOfCommandMetadata);
                this.GenerateAliases(writer, alias2resolvedCommandName);
                stream.SetLength(stream.Position);
            }
            list.Add(str + ".format.ps1xml");
            FileStream stream2 = new FileStream(str + ".format.ps1xml", mode, FileAccess.Write, FileShare.None);
            using (TextWriter writer2 = new StreamWriter(stream2, encoding))
            {
                if (listOfFormatData == null)
                {
                    listOfFormatData = new List<ExtendedTypeDefinition>();
                }
                this.GenerateFormatFile(writer2, listOfFormatData);
                stream2.SetLength(stream2.Position);
            }
            switch (SecuritySupport.GetExecutionPolicy(Utils.DefaultPowerShellShellID))
            {
                case ExecutionPolicy.Restricted:
                case ExecutionPolicy.AllSigned:
                {
                    if (certificate == null)
                    {
                        throw new PSInvalidOperationException(ImplicitRemotingStrings.CertificateNeeded);
                    }
                    string fileName = str + ".psm1";
                    try
                    {
                        SignatureHelper.SignFile(SigningOption.AddFullCertificateChainExceptRoot, fileName, certificate, string.Empty, null);
                        fileName = str + ".format.ps1xml";
                        SignatureHelper.SignFile(SigningOption.AddFullCertificateChainExceptRoot, fileName, certificate, string.Empty, null);
                    }
                    catch (Exception exception)
                    {
                        throw new PSInvalidOperationException(StringUtil.Format(ImplicitRemotingStrings.InvalidSigningOperation, fileName), exception);
                    }
                    break;
                }
            }
            list.Add(str + ".psd1");
            FileInfo info = new FileInfo(str + ".psd1");
            FileStream stream3 = new FileStream(info.FullName, mode, FileAccess.Write, FileShare.None);
            using (TextWriter writer3 = new StreamWriter(stream3, encoding))
            {
                this.GenerateManifest(writer3, str + ".psm1", str + ".format.ps1xml");
                stream3.SetLength(stream3.Position);
            }
            PSPrimitiveDictionary applicationArguments = this.GetApplicationArguments();
            if (applicationArguments != null)
            {
                string item = Path.Combine(moduleRootDirectory.FullName, "ApplicationArguments.xml");
                list.Add(item);
                using (XmlWriter writer4 = XmlWriter.Create(item))
                {
                    Serializer serializer = new Serializer(writer4);
                    serializer.Serialize(applicationArguments);
                    serializer.Done();
                }
            }
            return list;
        }

        private string GenerateReimportingOfModules()
        {
            StringBuilder builder = new StringBuilder();
            if (this.invocationInfo.BoundParameters.ContainsKey("Module"))
            {
                string[] strArray = (string[]) this.invocationInfo.BoundParameters["Module"];
                foreach (string str in strArray)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, "\r\n            try {{\r\n                & $script:InvokeCommand -Session $script:PSSession -ScriptBlock {{ \r\n                    Get-Module -ListAvailable -Name '{0}' | Import-Module \r\n                }} -ErrorAction SilentlyContinue\r\n            }} catch {{ }}\r\n", new object[] { CommandMetadata.EscapeSingleQuotedString(str) });
                }
            }
            return builder.ToString();
        }

        private void GenerateSectionSeparator(TextWriter writer)
        {
            writer.Write("\r\n##############################################################################\r\n");
        }

        private void GenerateTopComment(TextWriter writer)
        {
            writer.Write("\r\n<#\r\n # {0}\r\n # {1}\r\n # {2}\r\n # {3}\r\n #>\r\n        ", new object[] { CommandMetadata.EscapeBlockComment(StringUtil.Format(ImplicitRemotingStrings.ModuleHeaderTitle, new object[0])), CommandMetadata.EscapeBlockComment(StringUtil.Format(ImplicitRemotingStrings.ModuleHeaderDate, DateTime.Now.ToString(CultureInfo.CurrentCulture))), CommandMetadata.EscapeBlockComment(StringUtil.Format(ImplicitRemotingStrings.ModuleHeaderCommand, this.invocationInfo.MyCommand.Name)), CommandMetadata.EscapeBlockComment(StringUtil.Format(ImplicitRemotingStrings.ModuleHeaderCommandLine, this.invocationInfo.Line)) });
        }

        private PSPrimitiveDictionary GetApplicationArguments()
        {
            RemoteRunspace runspace = this.remoteRunspaceInfo.Runspace as RemoteRunspace;
            return runspace.RunspacePool.RemoteRunspacePoolInternal.ApplicationArguments;
        }

        private string GetConnectionString()
        {
            string str = null;
            WSManConnectionInfo connectionInfo = this.remoteRunspaceInfo.Runspace.ConnectionInfo as WSManConnectionInfo;
            if (connectionInfo != null)
            {
                str = connectionInfo.ConnectionUri.ToString();
            }
            return str;
        }

        private List<string> GetListOfCommandNames(IEnumerable<CommandMetadata> listOfCommandMetadata)
        {
            if (listOfCommandMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("listOfCommandMetadata");
            }
            List<string> list = new List<string>();
            foreach (CommandMetadata metadata in listOfCommandMetadata)
            {
                list.Add(metadata.Name);
            }
            return list;
        }
    }
}

