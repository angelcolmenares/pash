namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;
    using System.Text;

    [DebuggerDisplay("CommandName = {_commandName}; Type = {CommandType}")]
    public sealed class CommandMetadata
    {
        private string _commandName;
        private System.Management.Automation.ConfirmImpact _confirmImpact;
        private int _defaultParameterSetFlag;
        private string _defaultParameterSetName;
        private string _helpUri;
        private bool _implementsDynamicParameters;
        private ObsoleteAttribute _obsolete;
        private readonly Collection<Attribute> _otherAttributes;
        private Dictionary<string, ParameterMetadata> _parameters;
        private bool _positionalBinding;
        private System.Management.Automation.RemotingCapability _remotingCapability;
        private ScriptBlock _scriptBlock;
        private bool _shouldGenerateCommonParameters;
        private bool _supportsPaging;
        private bool _supportsShouldProcess;
        private bool _supportsTransactions;
        private bool _wrappedAnyCmdlet;
        private string _wrappedCommand;
        private CommandTypes _wrappedCommandType;
        private static ConcurrentDictionary<string, CommandMetadata> CommandMetadataCache = new ConcurrentDictionary<string, CommandMetadata>(StringComparer.OrdinalIgnoreCase);
        internal const string isSafeNameOrIdentifierRegex = @"^[-._:\\\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Lm}]{1,100}$";
        private readonly MergedCommandParameterMetadata staticCommandParameterMetadata;

        public CommandMetadata(CommandInfo commandInfo) : this(commandInfo, false)
        {
        }

        public CommandMetadata(CommandMetadata other)
        {
            this._commandName = string.Empty;
            this._defaultParameterSetName = "__AllParameterSets";
            this._positionalBinding = true;
            this._helpUri = string.Empty;
            this._remotingCapability = System.Management.Automation.RemotingCapability.PowerShell;
            this._confirmImpact = System.Management.Automation.ConfirmImpact.Medium;
            this._otherAttributes = new Collection<Attribute>();
            if (other == null)
            {
                throw PSTraceSource.NewArgumentNullException("other");
            }
            this._commandName = other._commandName;
            this._confirmImpact = other._confirmImpact;
            this._defaultParameterSetFlag = other._defaultParameterSetFlag;
            this._defaultParameterSetName = other._defaultParameterSetName;
            this._implementsDynamicParameters = other._implementsDynamicParameters;
            this._supportsShouldProcess = other._supportsShouldProcess;
            this._supportsPaging = other._supportsPaging;
            this._supportsTransactions = other._supportsTransactions;
            this.CommandType = other.CommandType;
            this._wrappedAnyCmdlet = other._wrappedAnyCmdlet;
            this._wrappedCommand = other._wrappedCommand;
            this._wrappedCommandType = other._wrappedCommandType;
            this._parameters = new Dictionary<string, ParameterMetadata>(other.Parameters.Count, StringComparer.OrdinalIgnoreCase);
            if (other.Parameters != null)
            {
                foreach (KeyValuePair<string, ParameterMetadata> pair in other.Parameters)
                {
                    this._parameters.Add(pair.Key, new ParameterMetadata(pair.Value));
                }
            }
            if (other._otherAttributes == null)
            {
                this._otherAttributes = null;
            }
            else
            {
                this._otherAttributes = new Collection<Attribute>(new List<Attribute>(other._otherAttributes.Count));
                foreach (Attribute attribute in other._otherAttributes)
                {
                    this._otherAttributes.Add(attribute);
                }
            }
            this.staticCommandParameterMetadata = null;
        }

        public CommandMetadata(string path)
        {
            this._commandName = string.Empty;
            this._defaultParameterSetName = "__AllParameterSets";
            this._positionalBinding = true;
            this._helpUri = string.Empty;
            this._remotingCapability = System.Management.Automation.RemotingCapability.PowerShell;
            this._confirmImpact = System.Management.Automation.ConfirmImpact.Medium;
            this._otherAttributes = new Collection<Attribute>();
            ExternalScriptInfo info = new ExternalScriptInfo(Path.GetFileName(path), path);
            this.Init(info.ScriptBlock, path, false);
            this._wrappedCommandType = CommandTypes.ExternalScript;
        }

        public CommandMetadata(Type commandType)
        {
            this._commandName = string.Empty;
            this._defaultParameterSetName = "__AllParameterSets";
            this._positionalBinding = true;
            this._helpUri = string.Empty;
            this._remotingCapability = System.Management.Automation.RemotingCapability.PowerShell;
            this._confirmImpact = System.Management.Automation.ConfirmImpact.Medium;
            this._otherAttributes = new Collection<Attribute>();
            this.Init(null, commandType, false);
        }

        public CommandMetadata(CommandInfo commandInfo, bool shouldGenerateCommonParameters)
        {
            this._commandName = string.Empty;
            this._defaultParameterSetName = "__AllParameterSets";
            this._positionalBinding = true;
            this._helpUri = string.Empty;
            this._remotingCapability = System.Management.Automation.RemotingCapability.PowerShell;
            this._confirmImpact = System.Management.Automation.ConfirmImpact.Medium;
            this._otherAttributes = new Collection<Attribute>();
            if (commandInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandInfo");
            }
            while (commandInfo is AliasInfo)
            {
                commandInfo = ((AliasInfo) commandInfo).ResolvedCommand;
                if (commandInfo == null)
                {
                    throw PSTraceSource.NewNotSupportedException();
                }
            }
            CmdletInfo info = commandInfo as CmdletInfo;
            if (info != null)
            {
                this.Init(commandInfo.Name, info.ImplementingType, shouldGenerateCommonParameters);
            }
            else
            {
                ExternalScriptInfo info2 = commandInfo as ExternalScriptInfo;
                if (info2 != null)
                {
                    this.Init(info2.ScriptBlock, info2.Path, shouldGenerateCommonParameters);
                    this._wrappedCommandType = CommandTypes.ExternalScript;
                }
                else
                {
                    FunctionInfo info3 = commandInfo as FunctionInfo;
                    if (info3 == null)
                    {
                        throw PSTraceSource.NewNotSupportedException();
                    }
                    this.Init(info3.ScriptBlock, info3.Name, shouldGenerateCommonParameters);
                    this._wrappedCommandType = commandInfo.CommandType;
                }
            }
        }

        internal CommandMetadata(ScriptBlock scriptblock, string commandName, ExecutionContext context)
        {
            this._commandName = string.Empty;
            this._defaultParameterSetName = "__AllParameterSets";
            this._positionalBinding = true;
            this._helpUri = string.Empty;
            this._remotingCapability = System.Management.Automation.RemotingCapability.PowerShell;
            this._confirmImpact = System.Management.Automation.ConfirmImpact.Medium;
            this._otherAttributes = new Collection<Attribute>();
            if (scriptblock == null)
            {
                throw PSTraceSource.NewArgumentException("scriptblock");
            }
            CmdletBindingAttribute cmdletBindingAttribute = scriptblock.CmdletBindingAttribute;
            if (cmdletBindingAttribute != null)
            {
                this.ProcessCmdletAttribute(cmdletBindingAttribute);
            }
            else
            {
                this._defaultParameterSetName = null;
            }
            this._commandName = commandName;
            this.CommandType = typeof(PSScriptCmdlet);
            if (scriptblock.HasDynamicParameters)
            {
                this._implementsDynamicParameters = true;
            }
            InternalParameterMetadata parameterMetadata = InternalParameterMetadata.Get(scriptblock.RuntimeDefinedParameters, false, scriptblock.UsesCmdletBinding);
            this.staticCommandParameterMetadata = this.MergeParameterMetadata(context, parameterMetadata, scriptblock.UsesCmdletBinding);
            this._defaultParameterSetFlag = this.staticCommandParameterMetadata.GenerateParameterSetMappingFromMetadata(this._defaultParameterSetName);
            this.staticCommandParameterMetadata.MakeReadOnly();
        }

        internal CommandMetadata(string commandName, Type cmdletType, ExecutionContext context)
        {
            this._commandName = string.Empty;
            this._defaultParameterSetName = "__AllParameterSets";
            this._positionalBinding = true;
            this._helpUri = string.Empty;
            this._remotingCapability = System.Management.Automation.RemotingCapability.PowerShell;
            this._confirmImpact = System.Management.Automation.ConfirmImpact.Medium;
            this._otherAttributes = new Collection<Attribute>();
            if (string.IsNullOrEmpty(commandName))
            {
                throw PSTraceSource.NewArgumentException("commandName");
            }
            this._commandName = commandName;
            this.CommandType = cmdletType;
            if (cmdletType != null)
            {
                InternalParameterMetadata parameterMetadata = InternalParameterMetadata.Get(cmdletType, context, false);
                this.ConstructCmdletMetadataUsingReflection();
                this.staticCommandParameterMetadata = this.MergeParameterMetadata(context, parameterMetadata, true);
                this._defaultParameterSetFlag = this.staticCommandParameterMetadata.GenerateParameterSetMappingFromMetadata(this._defaultParameterSetName);
                this.staticCommandParameterMetadata.MakeReadOnly();
            }
        }

        internal CommandMetadata(string name, CommandTypes commandType, bool isProxyForCmdlet, string defaultParameterSetName, bool supportsShouldProcess, System.Management.Automation.ConfirmImpact confirmImpact, bool supportsPaging, bool supportsTransactions, bool positionalBinding, Dictionary<string, ParameterMetadata> parameters)
        {
            this._commandName = string.Empty;
            this._defaultParameterSetName = "__AllParameterSets";
            this._positionalBinding = true;
            this._helpUri = string.Empty;
            this._remotingCapability = System.Management.Automation.RemotingCapability.PowerShell;
            this._confirmImpact = System.Management.Automation.ConfirmImpact.Medium;
            this._otherAttributes = new Collection<Attribute>();
            this._commandName = this._wrappedCommand = name;
            this._wrappedCommandType = commandType;
            this._wrappedAnyCmdlet = isProxyForCmdlet;
            this._defaultParameterSetName = defaultParameterSetName;
            this._supportsShouldProcess = supportsShouldProcess;
            this._supportsPaging = supportsPaging;
            this._confirmImpact = confirmImpact;
            this._supportsTransactions = supportsTransactions;
            this._positionalBinding = positionalBinding;
            this.Parameters = parameters;
        }

        private void ConstructCmdletMetadataUsingReflection()
        {
            if (this.CommandType.GetInterface(typeof(IDynamicParameters).Name, true) != null)
            {
                this._implementsDynamicParameters = true;
            }
            foreach (Attribute attribute in this.CommandType.GetCustomAttributes(false))
            {
                CmdletAttribute attribute2 = attribute as CmdletAttribute;
                if (attribute2 != null)
                {
                    this.ProcessCmdletAttribute(attribute2);
                    this.Name = attribute2.VerbName + "-" + attribute2.NounName;
                }
                else if (attribute is ObsoleteAttribute)
                {
                    this._obsolete = (ObsoleteAttribute) attribute;
                }
                else
                {
                    this._otherAttributes.Add(attribute);
                }
            }
        }

        internal static string EscapeBlockComment(string helpContent)
        {
            if (string.IsNullOrEmpty(helpContent))
            {
                return string.Empty;
            }
            return helpContent.Replace("<#", "<`#").Replace("#>", "#`>");
        }

        internal static string EscapeSingleQuotedString(string stringContent)
        {
            if (string.IsNullOrEmpty(stringContent))
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(stringContent.Length);
            foreach (char ch in stringContent)
            {
                builder.Append(ch);
                if (SpecialCharacters.IsSingleQuote(ch))
                {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }

        internal static string EscapeVariableName(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
            {
                return string.Empty;
            }
            return variableName.Replace("`", "``").Replace("}", "`}").Replace("{", "`{");
        }

        internal static CommandMetadata Get(string commandName, Type cmdletType, ExecutionContext context)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                throw PSTraceSource.NewArgumentException("commandName");
            }
            CommandMetadata metadata = null;
            if ((context != null) && (cmdletType != null))
            {
                string assemblyQualifiedName = cmdletType.AssemblyQualifiedName;
                if (CommandMetadataCache.ContainsKey(assemblyQualifiedName))
                {
                    metadata = CommandMetadataCache[assemblyQualifiedName];
                }
            }
            if (metadata == null)
            {
                metadata = new CommandMetadata(commandName, cmdletType, context);
                if ((context != null) && (cmdletType != null))
                {
                    string key = cmdletType.AssemblyQualifiedName;
                    CommandMetadataCache.TryAdd(key, metadata);
                }
            }
            return metadata;
        }

        internal string GetBeginBlock()
        {
            if (string.IsNullOrEmpty(this._wrappedCommand))
            {
                throw new InvalidOperationException(ProxyCommandStrings.CommandMetadataMissingCommandName);
            }
            if (this._wrappedAnyCmdlet)
            {
                return string.Format(CultureInfo.InvariantCulture, "\r\n    try {{\r\n        $outBuffer = $null\r\n        if ($PSBoundParameters.TryGetValue('OutBuffer', [ref]$outBuffer))\r\n        {{\r\n            $PSBoundParameters['OutBuffer'] = 1\r\n        }}\r\n        $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand('{0}', [System.Management.Automation.CommandTypes]::{1})\r\n        $scriptCmd = {{& $wrappedCmd @PSBoundParameters }}\r\n        $steppablePipeline = $scriptCmd.GetSteppablePipeline($myInvocation.CommandOrigin)\r\n        $steppablePipeline.Begin($PSCmdlet)\r\n    }} catch {{\r\n        throw\r\n    }}\r\n", new object[] { EscapeSingleQuotedString(this._wrappedCommand), this._wrappedCommandType });
            }
            return string.Format(CultureInfo.InvariantCulture, "\r\n    try {{\r\n        $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand('{0}', [System.Management.Automation.CommandTypes]::{1})\r\n        $PSBoundParameters.Add('$args', $args)\r\n        $scriptCmd = {{& $wrappedCmd @PSBoundParameters }}\r\n        $steppablePipeline = $scriptCmd.GetSteppablePipeline($myInvocation.CommandOrigin)\r\n        $steppablePipeline.Begin($myInvocation.ExpectingInput, $ExecutionContext)\r\n    }} catch {{\r\n        throw\r\n    }}\r\n", new object[] { EscapeSingleQuotedString(this._wrappedCommand), this._wrappedCommandType });
        }

        internal string GetDecl()
        {
            string str = "";
            string str2 = "";
            if (!this._wrappedAnyCmdlet)
            {
                return str;
            }
            StringBuilder builder = new StringBuilder("[CmdletBinding(");
            if (!string.IsNullOrEmpty(this._defaultParameterSetName))
            {
                builder.Append(str2);
                builder.AppendFormat("DefaultParameterSetName='{0}'", EscapeSingleQuotedString(this._defaultParameterSetName));
                str2 = ", ";
            }
            if (this._supportsShouldProcess)
            {
                builder.Append(str2);
                builder.Append("SupportsShouldProcess=$true");
                str2 = ", ";
                builder.Append(str2);
                builder.AppendFormat("ConfirmImpact='{0}'", this._confirmImpact);
            }
            if (this._supportsPaging)
            {
                builder.Append(str2);
                builder.Append("SupportsPaging=$true");
                str2 = ", ";
            }
            if (this._supportsTransactions)
            {
                builder.Append(str2);
                builder.Append("SupportsTransactions=$true");
                str2 = ", ";
            }
            if (!this.PositionalBinding)
            {
                builder.Append(str2);
                builder.Append("PositionalBinding=$false");
                str2 = ", ";
            }
            if (!string.IsNullOrEmpty(this._helpUri))
            {
                builder.Append(str2);
                builder.AppendFormat("HelpUri='{0}'", EscapeSingleQuotedString(this._helpUri));
                str2 = ", ";
            }
            if (this._remotingCapability != System.Management.Automation.RemotingCapability.PowerShell)
            {
                builder.Append(str2);
                builder.AppendFormat("RemotingCapability='{0}'", this._remotingCapability);
                str2 = ", ";
            }
            builder.Append(")]");
            return builder.ToString();
        }

        internal string GetDynamicParamBlock()
        {
            return "";
        }

        internal string GetEndBlock()
        {
            return "\r\n    try {\r\n        $steppablePipeline.End()\r\n    } catch {\r\n        throw\r\n    }\r\n";
        }

        internal string GetParamBlock()
        {
            if (this.Parameters.Keys.Count <= 0)
            {
                return "";
            }
            StringBuilder builder = new StringBuilder();
            string prefix = string.Format(CultureInfo.InvariantCulture, "{0}    ", new object[] { Environment.NewLine });
            string str2 = "";
            foreach (string str3 in this.Parameters.Keys)
            {
                string str4 = this.Parameters[str3].GetProxyParameterData(prefix, str3, this._wrappedAnyCmdlet);
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", new object[] { str2, str4 });
                str2 = string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { ",", Environment.NewLine });
            }
            return builder.ToString();
        }

        internal string GetProcessBlock()
        {
            return "\r\n    try {\r\n        $steppablePipeline.Process($_)\r\n    } catch {\r\n        throw\r\n    }\r\n";
        }

        internal string GetProxyCommand(string helpComment)
        {
            if (string.IsNullOrEmpty(helpComment))
            {
                helpComment = string.Format(CultureInfo.InvariantCulture, "\r\n.ForwardHelpTargetName {0}\r\n.ForwardHelpCategory {1}\r\n", new object[] { this._wrappedCommand, this._wrappedCommandType });
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}\r\nparam({1})\r\n\r\nbegin\r\n{{{2}}}\r\n\r\nprocess\r\n{{{3}}}\r\n\r\nend\r\n{{{4}}}\r\n<#\r\n{5}\r\n#>\r\n", new object[] { this.GetDecl(), this.GetParamBlock(), this.GetBeginBlock(), this.GetProcessBlock(), this.GetEndBlock(), EscapeBlockComment(helpComment) });
        }

        private static CommandMetadata GetRestrictedCmdlet(string cmdletName, string defaultParameterSet, string helpUri, params ParameterMetadata[] parameters)
        {
            Dictionary<string, ParameterMetadata> dictionary = new Dictionary<string, ParameterMetadata>(StringComparer.OrdinalIgnoreCase);
            foreach (ParameterMetadata metadata in parameters)
            {
                dictionary.Add(metadata.Name, metadata);
            }
            return new CommandMetadata(cmdletName, CommandTypes.Cmdlet, true, defaultParameterSet, false, System.Management.Automation.ConfirmImpact.None, false, false, true, dictionary) { HelpUri = helpUri };
        }

        public static Dictionary<string, CommandMetadata> GetRestrictedCommands(SessionCapabilities sessionCapabilities)
        {
            List<CommandMetadata> list = new List<CommandMetadata>();
            if (SessionCapabilities.RemoteServer == (sessionCapabilities & SessionCapabilities.RemoteServer))
            {
                list.AddRange(GetRestrictedRemotingCommands());
            }
            if (SessionCapabilities.WorkflowServer == (sessionCapabilities & SessionCapabilities.WorkflowServer))
            {
                list.AddRange(GetRestrictedRemotingCommands());
                list.AddRange(GetRestrictedJobCommands());
            }
            Dictionary<string, CommandMetadata> dictionary = new Dictionary<string, CommandMetadata>(StringComparer.OrdinalIgnoreCase);
            foreach (CommandMetadata metadata in list)
            {
                dictionary.Add(metadata.Name, metadata);
            }
            return dictionary;
        }

        private static CommandMetadata GetRestrictedExitPSSession()
        {
            return GetRestrictedCmdlet("Exit-PSSession", null, "http://go.microsoft.com/fwlink/?LinkID=135210", new ParameterMetadata[0]);
        }

        private static CommandMetadata GetRestrictedGetCommand()
        {
            ParameterMetadata metadata = new ParameterMetadata("Name", typeof(string[])) {
                Attributes = { new ValidateLengthAttribute(0, 0x3e8), new ValidateCountAttribute(0, 0x3e8) }
            };
            ParameterMetadata metadata2 = new ParameterMetadata("Module", typeof(string[])) {
                Attributes = { new ValidateLengthAttribute(0, 0x3e8), new ValidateCountAttribute(0, 100) }
            };
            ParameterMetadata metadata3 = new ParameterMetadata("ArgumentList", typeof(object[])) {
                Attributes = { new ValidateCountAttribute(0, 100) }
            };
            ParameterMetadata metadata4 = new ParameterMetadata("CommandType", typeof(CommandTypes));
            ParameterMetadata metadata5 = new ParameterMetadata("ListImported", typeof(SwitchParameter));
            return GetRestrictedCmdlet("Get-Command", null, "http://go.microsoft.com/fwlink/?LinkID=113309", new ParameterMetadata[] { metadata, metadata2, metadata3, metadata4, metadata5 });
        }

        private static CommandMetadata GetRestrictedGetFormatData()
        {
            ParameterMetadata metadata = new ParameterMetadata("TypeName", typeof(string[])) {
                Attributes = { new ValidateLengthAttribute(0, 0x3e8), new ValidateCountAttribute(0, 0x3e8) }
            };
            return GetRestrictedCmdlet("Get-FormatData", null, "http://go.microsoft.com/fwlink/?LinkID=144303", new ParameterMetadata[] { metadata });
        }

        private static CommandMetadata GetRestrictedGetHelp()
        {
            ParameterMetadata metadata = new ParameterMetadata("Name", typeof(string)) {
                Attributes = { new ValidatePatternAttribute(@"^[-._:\\\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Lm}]{1,100}$"), new ValidateLengthAttribute(0, 0x3e8) }
            };
            ParameterMetadata metadata2 = new ParameterMetadata("Category", typeof(string[])) {
                Attributes = { new ValidateSetAttribute(Enum.GetNames(typeof(HelpCategory))), new ValidateCountAttribute(0, 1) }
            };
            return GetRestrictedCmdlet("Get-Help", null, "http://go.microsoft.com/fwlink/?LinkID=113316", new ParameterMetadata[] { metadata, metadata2 });
        }

        private static Collection<CommandMetadata> GetRestrictedJobCommands()
        {
            ParameterSetMetadata metadata = new ParameterSetMetadata(0, ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName, string.Empty);
            ParameterSetMetadata metadata2 = new ParameterSetMetadata(0, ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName, string.Empty);
            ParameterSetMetadata metadata3 = new ParameterSetMetadata(0, ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName, string.Empty);
            ParameterSetMetadata metadata4 = new ParameterSetMetadata(-2147483648, ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName, string.Empty);
            ParameterSetMetadata metadata5 = new ParameterSetMetadata(-2147483648, ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName, string.Empty);
            ParameterSetMetadata metadata6 = new ParameterSetMetadata(0, ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName, string.Empty);
            ParameterSetMetadata metadata7 = new ParameterSetMetadata(0, ParameterSetMetadata.ParameterFlags.Mandatory | ParameterSetMetadata.ParameterFlags.ValueFromPipeline | ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName, string.Empty);
            ParameterSetMetadata metadata8 = new ParameterSetMetadata(0, ParameterSetMetadata.ParameterFlags.Mandatory | ParameterSetMetadata.ParameterFlags.ValueFromPipeline | ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName, string.Empty);
            ParameterSetMetadata metadata9 = new ParameterSetMetadata(0, ParameterSetMetadata.ParameterFlags.Mandatory | ParameterSetMetadata.ParameterFlags.ValueFromPipeline | ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName, string.Empty);
            Dictionary<string, ParameterSetMetadata> parameterSets = new Dictionary<string, ParameterSetMetadata>();
            parameterSets.Add("NameParameterSet", metadata);
            Collection<string> aliases = new Collection<string>();
            ParameterMetadata metadata10 = new ParameterMetadata(aliases, false, "Name", parameterSets, typeof(string[])) {
                Attributes = { new ValidatePatternAttribute(@"^[-._:\\\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Lm}]{1,100}$"), new ValidateLengthAttribute(0, 0x3e8) }
            };
            parameterSets = new Dictionary<string, ParameterSetMetadata>();
            parameterSets.Add("InstanceIdParameterSet", metadata2);
            ParameterMetadata metadata11 = new ParameterMetadata(aliases, false, "InstanceId", parameterSets, typeof(Guid[])) {
                Attributes = { new ValidateNotNullOrEmptyAttribute() }
            };
            parameterSets = new Dictionary<string, ParameterSetMetadata>();
            parameterSets.Add("SessionIdParameterSet", metadata3);
            ParameterMetadata metadata12 = new ParameterMetadata(aliases, false, "Id", parameterSets, typeof(int[])) {
                Attributes = { new ValidateNotNullOrEmptyAttribute() }
            };
            parameterSets = new Dictionary<string, ParameterSetMetadata>();
            parameterSets.Add("StateParameterSet", metadata4);
            ParameterMetadata metadata13 = new ParameterMetadata(aliases, false, "State", parameterSets, typeof(JobState));
            parameterSets = new Dictionary<string, ParameterSetMetadata>();
            parameterSets.Add("CommandParameterSet", metadata5);
            ParameterMetadata metadata14 = new ParameterMetadata(aliases, false, "Command", parameterSets, typeof(string[]));
            parameterSets = new Dictionary<string, ParameterSetMetadata>();
            parameterSets.Add("FilterParameterSet", metadata6);
            ParameterMetadata metadata15 = new ParameterMetadata(aliases, false, "Filter", parameterSets, typeof(Hashtable));
            parameterSets = new Dictionary<string, ParameterSetMetadata>();
            parameterSets.Add("Job", metadata7);
            ParameterMetadata metadata16 = new ParameterMetadata(aliases, false, "Job", parameterSets, typeof(Job[])) {
                Attributes = { new ValidateNotNullOrEmptyAttribute() }
            };
            parameterSets = new Dictionary<string, ParameterSetMetadata>();
            parameterSets.Add("ComputerName", metadata8);
            parameterSets.Add("Location", metadata9);
            ParameterMetadata metadata17 = new ParameterMetadata(aliases, false, "Job", parameterSets, typeof(Job[]));
            Collection<CommandMetadata> collection2 = new Collection<CommandMetadata>();
            ParameterMetadata metadata18 = new ParameterMetadata("PassThru", typeof(SwitchParameter));
            ParameterMetadata metadata19 = new ParameterMetadata("Any", typeof(SwitchParameter));
            CommandMetadata item = GetRestrictedCmdlet("Stop-Job", "SessionIdParameterSet", "http://go.microsoft.com/fwlink/?LinkID=113413", new ParameterMetadata[] { metadata10, metadata11, metadata12, metadata13, metadata15, metadata16, metadata18 });
            collection2.Add(item);
            ParameterMetadata metadata21 = new ParameterMetadata("Timeout", typeof(int)) {
                Attributes = { new ValidateRangeAttribute(-1, 0x7fffffff) }
            };
            CommandMetadata metadata22 = GetRestrictedCmdlet("Wait-Job", "SessionIdParameterSet", "http://go.microsoft.com/fwlink/?LinkID=113422", new ParameterMetadata[] { metadata10, metadata11, metadata12, metadata16, metadata13, metadata15, metadata19, metadata21 });
            collection2.Add(metadata22);
            CommandMetadata metadata23 = GetRestrictedCmdlet("Get-Job", "SessionIdParameterSet", "http://go.microsoft.com/fwlink/?LinkID=113328", new ParameterMetadata[] { metadata10, metadata11, metadata12, metadata13, metadata15, metadata14 });
            collection2.Add(metadata23);
            parameterSets = new Dictionary<string, ParameterSetMetadata>();
            metadata8 = new ParameterSetMetadata(1, ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName, string.Empty);
            parameterSets.Add("ComputerName", metadata8);
            ParameterMetadata metadata24 = new ParameterMetadata(aliases, false, "ComputerName", parameterSets, typeof(string[])) {
                Attributes = { new ValidateLengthAttribute(0, 0x3e8), new ValidateNotNullOrEmptyAttribute() }
            };
            parameterSets = new Dictionary<string, ParameterSetMetadata>();
            metadata9 = new ParameterSetMetadata(1, ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName, string.Empty);
            parameterSets.Add("Location", metadata9);
            ParameterMetadata metadata25 = new ParameterMetadata(aliases, false, "Location", parameterSets, typeof(string[])) {
                Attributes = { new ValidateLengthAttribute(0, 0x3e8), new ValidateNotNullOrEmptyAttribute() }
            };
            ParameterMetadata metadata26 = new ParameterMetadata("NoRecurse", typeof(SwitchParameter));
            ParameterMetadata metadata27 = new ParameterMetadata("Keep", typeof(SwitchParameter));
            ParameterMetadata metadata28 = new ParameterMetadata("Wait", typeof(SwitchParameter));
            ParameterMetadata metadata29 = new ParameterMetadata("WriteEvents", typeof(SwitchParameter));
            ParameterMetadata metadata30 = new ParameterMetadata("WriteJobInResults", typeof(SwitchParameter));
            ParameterMetadata metadata31 = new ParameterMetadata("AutoRemoveJob", typeof(SwitchParameter));
            CommandMetadata metadata32 = GetRestrictedCmdlet("Receive-Job", "Location", "http://go.microsoft.com/fwlink/?LinkID=113372", new ParameterMetadata[] { metadata10, metadata11, metadata12, metadata13, metadata17, metadata24, metadata25, metadata26, metadata27, metadata28, metadata29, metadata30, metadata31 });
            collection2.Add(metadata32);
            ParameterMetadata metadata33 = new ParameterMetadata("Force", typeof(SwitchParameter));
            CommandMetadata metadata34 = GetRestrictedCmdlet("Remove-Job", "SessionIdParameterSet", "http://go.microsoft.com/fwlink/?LinkID=113377", new ParameterMetadata[] { metadata10, metadata11, metadata12, metadata13, metadata15, metadata16, metadata33 });
            collection2.Add(metadata34);
            CommandMetadata metadata35 = GetRestrictedCmdlet("Suspend-Job", "SessionIdParameterSet", "http://go.microsoft.com/fwlink/?LinkID=210613", new ParameterMetadata[] { metadata10, metadata11, metadata12, metadata13, metadata15, metadata16, metadata18 });
            collection2.Add(metadata35);
            CommandMetadata metadata36 = GetRestrictedCmdlet("Resume-Job", "SessionIdParameterSet", "http://go.microsoft.com/fwlink/?LinkID=210611", new ParameterMetadata[] { metadata10, metadata11, metadata12, metadata13, metadata15, metadata16, metadata18 });
            collection2.Add(metadata36);
            return collection2;
        }

        private static CommandMetadata GetRestrictedMeasureObject()
        {
            ParameterMetadata metadata = new ParameterMetadata("InputObject", typeof(object));
            metadata.ParameterSets.Add("__AllParameterSets", new ParameterSetMetadata(-2147483648, ParameterSetMetadata.ParameterFlags.Mandatory | ParameterSetMetadata.ParameterFlags.ValueFromPipeline, null));
            return GetRestrictedCmdlet("Measure-Object", null, "http://go.microsoft.com/fwlink/?LinkID=113349", new ParameterMetadata[] { metadata });
        }

        private static CommandMetadata GetRestrictedOutDefault()
        {
            ParameterMetadata metadata = new ParameterMetadata("InputObject", typeof(object));
            metadata.ParameterSets.Add("__AllParameterSets", new ParameterSetMetadata(-2147483648, ParameterSetMetadata.ParameterFlags.Mandatory | ParameterSetMetadata.ParameterFlags.ValueFromPipeline, null));
            return GetRestrictedCmdlet("Out-Default", null, "http://go.microsoft.com/fwlink/?LinkID=113362", new ParameterMetadata[] { metadata });
        }

        private static Collection<CommandMetadata> GetRestrictedRemotingCommands()
        {
            return new Collection<CommandMetadata> { GetRestrictedGetCommand(), GetRestrictedGetFormatData(), GetRestrictedSelectObject(), GetRestrictedGetHelp(), GetRestrictedMeasureObject(), GetRestrictedExitPSSession(), GetRestrictedOutDefault() };
        }

        private static CommandMetadata GetRestrictedSelectObject()
        {
            string[] validValues = new string[] { "ModuleName", "Namespace", "OutputType", "Count", "HelpUri", "Name", "CommandType", "ResolvedCommandName", "DefaultParameterSet", "CmdletBinding", "Parameters" };
            ParameterMetadata metadata = new ParameterMetadata("Property", typeof(string[])) {
                Attributes = { new ValidateSetAttribute(validValues), new ValidateCountAttribute(1, validValues.Length) }
            };
            ParameterMetadata metadata2 = new ParameterMetadata("InputObject", typeof(object));
            metadata2.ParameterSets.Add("__AllParameterSets", new ParameterSetMetadata(-2147483648, ParameterSetMetadata.ParameterFlags.Mandatory | ParameterSetMetadata.ParameterFlags.ValueFromPipeline, null));
            return GetRestrictedCmdlet("Select-Object", null, "http://go.microsoft.com/fwlink/?LinkID=113387", new ParameterMetadata[] { metadata, metadata2 });
        }

        private void Init(ScriptBlock scriptBlock, string name, bool shouldGenerateCommonParameters)
        {
            if (scriptBlock.UsesCmdletBinding)
            {
                this._wrappedAnyCmdlet = true;
            }
            else
            {
                shouldGenerateCommonParameters = false;
            }
            CmdletBindingAttribute cmdletBindingAttribute = scriptBlock.CmdletBindingAttribute;
            if (cmdletBindingAttribute != null)
            {
                this.ProcessCmdletAttribute(cmdletBindingAttribute);
            }
            else if (scriptBlock.UsesCmdletBinding)
            {
                this._defaultParameterSetName = null;
            }
            this._scriptBlock = scriptBlock;
            this._wrappedCommand = this._commandName = name;
            this._shouldGenerateCommonParameters = shouldGenerateCommonParameters;
        }

        private void Init(string name, Type commandType, bool shouldGenerateCommonParameters)
        {
            this._commandName = name;
            this.CommandType = commandType;
            if (commandType != null)
            {
                this.ConstructCmdletMetadataUsingReflection();
                this._shouldGenerateCommonParameters = shouldGenerateCommonParameters;
            }
            this._wrappedCommand = this._commandName;
            this._wrappedCommandType = CommandTypes.Cmdlet;
            this._wrappedAnyCmdlet = true;
        }

        private MergedCommandParameterMetadata MergeParameterMetadata(ExecutionContext context, InternalParameterMetadata parameterMetadata, bool shouldGenerateCommonParameters)
        {
            MergedCommandParameterMetadata metadata = new MergedCommandParameterMetadata();
            metadata.AddMetadataForBinder(parameterMetadata, ParameterBinderAssociation.DeclaredFormalParameters);
            if (shouldGenerateCommonParameters)
            {
                InternalParameterMetadata metadata2 = InternalParameterMetadata.Get(typeof(CommonParameters), context, false);
                metadata.AddMetadataForBinder(metadata2, ParameterBinderAssociation.CommonParameters);
                if (this.SupportsShouldProcess)
                {
                    InternalParameterMetadata metadata3 = InternalParameterMetadata.Get(typeof(ShouldProcessParameters), context, false);
                    metadata.AddMetadataForBinder(metadata3, ParameterBinderAssociation.ShouldProcessParameters);
                }
                if (this.SupportsPaging)
                {
                    InternalParameterMetadata metadata4 = InternalParameterMetadata.Get(typeof(PagingParameters), context, false);
                    metadata.AddMetadataForBinder(metadata4, ParameterBinderAssociation.PagingParameters);
                }
                if (this.SupportsTransactions)
                {
                    InternalParameterMetadata metadata5 = InternalParameterMetadata.Get(typeof(TransactionParameters), context, false);
                    metadata.AddMetadataForBinder(metadata5, ParameterBinderAssociation.TransactionParameters);
                }
            }
            return metadata;
        }

        private void ProcessCmdletAttribute(CmdletCommonMetadataAttribute attribute)
        {
            if (attribute == null)
            {
                throw PSTraceSource.NewArgumentNullException("attribute");
            }
            this._defaultParameterSetName = attribute.DefaultParameterSetName;
            this._supportsShouldProcess = attribute.SupportsShouldProcess;
            this._confirmImpact = attribute.ConfirmImpact;
            this._supportsPaging = attribute.SupportsPaging;
            this._supportsTransactions = attribute.SupportsTransactions;
            this._helpUri = attribute.HelpUri;
            this._remotingCapability = attribute.RemotingCapability;
            CmdletBindingAttribute attribute2 = attribute as CmdletBindingAttribute;
            if (attribute2 != null)
            {
                this.PositionalBinding = attribute2.PositionalBinding;
            }
        }

        public Type CommandType { get; private set; }

        public System.Management.Automation.ConfirmImpact ConfirmImpact
        {
            get
            {
                return this._confirmImpact;
            }
            set
            {
                this._confirmImpact = value;
            }
        }

        internal int DefaultParameterSetFlag
        {
            get
            {
                return this._defaultParameterSetFlag;
            }
            set
            {
                this._defaultParameterSetFlag = value;
            }
        }

        public string DefaultParameterSetName
        {
            get
            {
                return this._defaultParameterSetName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = "__AllParameterSets";
                }
                this._defaultParameterSetName = value;
            }
        }

        public string HelpUri
        {
            get
            {
                return this._helpUri;
            }
            set
            {
                this._helpUri = value;
            }
        }

        internal bool ImplementsDynamicParameters
        {
            get
            {
                return this._implementsDynamicParameters;
            }
        }

        public string Name
        {
            get
            {
                return this._commandName;
            }
            set
            {
                this._commandName = value;
            }
        }

        internal ObsoleteAttribute Obsolete
        {
            get
            {
                return this._obsolete;
            }
            set
            {
                this._obsolete = value;
            }
        }

        public Dictionary<string, ParameterMetadata> Parameters
        {
            get
            {
                if (this._parameters == null)
                {
                    if (this._scriptBlock != null)
                    {
                        InternalParameterMetadata parameterMetadata = InternalParameterMetadata.Get(this._scriptBlock.RuntimeDefinedParameters, false, this._scriptBlock.UsesCmdletBinding);
                        MergedCommandParameterMetadata cmdParameterMetadata = this.MergeParameterMetadata(null, parameterMetadata, this._shouldGenerateCommonParameters);
                        this._parameters = ParameterMetadata.GetParameterMetadata(cmdParameterMetadata);
                    }
                    else if (this.CommandType != null)
                    {
                        InternalParameterMetadata metadata3 = InternalParameterMetadata.Get(this.CommandType, null, false);
                        MergedCommandParameterMetadata metadata4 = this.MergeParameterMetadata(null, metadata3, this._shouldGenerateCommonParameters);
                        this._parameters = ParameterMetadata.GetParameterMetadata(metadata4);
                    }
                }
                return this._parameters;
            }
            private set
            {
                this._parameters = value;
            }
        }

        public bool PositionalBinding
        {
            get
            {
                return this._positionalBinding;
            }
            set
            {
                this._positionalBinding = value;
            }
        }

        public System.Management.Automation.RemotingCapability RemotingCapability
        {
            get
            {
                if ((this._remotingCapability == System.Management.Automation.RemotingCapability.PowerShell) && this.Parameters.ContainsKey("ComputerName"))
                {
                    this._remotingCapability = System.Management.Automation.RemotingCapability.SupportedByCommand;
                }
                return this._remotingCapability;
            }
            set
            {
                this._remotingCapability = value;
            }
        }

        internal MergedCommandParameterMetadata StaticCommandParameterMetadata
        {
            get
            {
                return this.staticCommandParameterMetadata;
            }
        }

        public bool SupportsPaging
        {
            get
            {
                return this._supportsPaging;
            }
            set
            {
                this._supportsPaging = value;
            }
        }

        public bool SupportsShouldProcess
        {
            get
            {
                return this._supportsShouldProcess;
            }
            set
            {
                this._supportsShouldProcess = value;
            }
        }

        public bool SupportsTransactions
        {
            get
            {
                return this._supportsTransactions;
            }
            set
            {
                this._supportsTransactions = value;
            }
        }

        internal bool WrappedAnyCmdlet
        {
            get
            {
                return this._wrappedAnyCmdlet;
            }
        }

        internal CommandTypes WrappedCommandType
        {
            get
            {
                return this._wrappedCommandType;
            }
        }
    }
}

