namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Runtime.CompilerServices;
    using System.Threading;

    [Cmdlet("Get", "Command", DefaultParameterSetName="CmdletSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113309"), OutputType(new Type[] { typeof(AliasInfo), typeof(ApplicationInfo), typeof(FunctionInfo), typeof(CmdletInfo), typeof(ExternalScriptInfo), typeof(FilterInfo), typeof(WorkflowInfo), typeof(string) })]
    public sealed class GetCommandCommand : PSCmdlet
    {
        private HashSet<string> _matchedParameterNames;
        private Collection<WildcardPattern> _modulePatterns;
        private string[] _modules = new string[0];
        private string[] _parameterNames;
        private Collection<WildcardPattern> _parameterNameWildcards;
        private PSTypeName[] _parameterTypes;
        private List<CommandInfo> accumulatedResults = new List<CommandInfo>();
        private bool all;
        private object[] commandArgs;
        private Dictionary<string, CommandInfo> commandsWritten = new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase);
        private CommandTypes commandType = CommandTypes.All;
        private bool isCommandTypeSpecified;
        private bool isModuleSpecified;
        private bool listImported;
        private bool nameContainsWildcard;
        private string[] names;
        private Collection<WildcardPattern> nounPatterns;
        private string[] nouns = new string[0];
        private int totalCount = -1;
        private bool usage;
        private Collection<WildcardPattern> verbPatterns;
        private string[] verbs = new string[0];

        private void AccumulateMatchingCmdlets()
        {
            this.commandType = CommandTypes.Workflow | CommandTypes.Cmdlet | CommandTypes.Filter | CommandTypes.Function | CommandTypes.Alias;
            Collection<string> commandNames = new Collection<string> { "*" };
            this.AccumulateMatchingCommands(commandNames);
        }

        private void AccumulateMatchingCommands()
        {
            Collection<string> commandNames = SessionStateUtilities.ConvertArrayToCollection<string>(this.Name);
            if (commandNames.Count == 0)
            {
                commandNames.Add("*");
            }
            this.AccumulateMatchingCommands(commandNames);
        }

        private void AccumulateMatchingCommands(IEnumerable<string> commandNames)
        {
            SearchResolutionOptions none = SearchResolutionOptions.None;
            if (this.All != false)
            {
                none = SearchResolutionOptions.SearchAllScopes;
            }
            if ((this.CommandType & CommandTypes.Alias) != 0)
            {
                none |= SearchResolutionOptions.ResolveAliasPatterns;
            }
            if ((this.CommandType & (CommandTypes.Workflow | CommandTypes.Filter | CommandTypes.Function)) != 0)
            {
                none |= SearchResolutionOptions.ResolveFunctionPatterns;
            }
            foreach (string str in commandNames)
            {
                try
                {
                    string str2 = null;
                    string pattern = str;
                    bool flag = false;
                    if ((str.IndexOf('\\') > 0) && (str.Split(new char[] { '\\' }).Length == 2))
                    {
                        string[] strArray = str.Split(new char[] { '\\' }, 2);
                        str2 = strArray[0];
                        pattern = strArray[1];
                        flag = true;
                    }
                    if ((this.Module.Length == 1) && !WildcardPattern.ContainsWildcardCharacters(this.Module[0]))
                    {
                        str2 = this.Module[0];
                    }
                    bool isPattern = WildcardPattern.ContainsWildcardCharacters(pattern);
                    if (isPattern)
                    {
                        none |= SearchResolutionOptions.CommandNameIsPattern;
                    }
                    int currentCount = 0;
                    bool flag3 = this.FindCommandForName(none, str, isPattern, true, ref currentCount);
                    if (!flag3 || isPattern)
                    {
                        if (!isPattern || !string.IsNullOrEmpty(str2))
                        {
                            string commandName = str;
                            if (!flag && !string.IsNullOrEmpty(str2))
                            {
                                commandName = str2 + @"\" + str;
                            }
                            try
                            {
                                CommandDiscovery.LookupCommandInfo(commandName, base.MyInvocation.CommandOrigin, base.Context);
                            }
                            catch (CommandNotFoundException)
                            {
                            }
                            flag3 = this.FindCommandForName(none, str, isPattern, false, ref currentCount);
                        }
                        else if ((this.ListImported == false) && ((this.TotalCount < 0) || (currentCount < this.TotalCount)))
                        {
                            foreach (CommandInfo info in ModuleUtils.GetMatchingCommands(pattern, base.Context, base.MyInvocation.CommandOrigin, true))
                            {
                                CommandInfo current = info;
                                if ((this.IsCommandMatch(ref current) && !this.IsCommandInResult(current)) && this.IsParameterMatch(current))
                                {
                                    this.accumulatedResults.Add(current);
                                    currentCount++;
                                    if ((this.TotalCount >= 0) && (currentCount >= this.TotalCount))
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (!flag3 && !isPattern)
                    {
                        CommandNotFoundException replaceParentContainsErrorRecordException = new CommandNotFoundException(str, null, "CommandNotFoundException", DiscoveryExceptions.CommandNotFoundException, new object[0]);
                        base.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
                    }
                }
                catch (CommandNotFoundException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2.ErrorRecord, exception2));
                }
            }
        }

        protected override void EndProcessing()
        {
            if (((this.Name == null) && !this.all) && (this.totalCount == -1))
            {
                CommandTypes commandTypesToIgnore = 0;
                if (((this.CommandType & CommandTypes.Alias) != CommandTypes.Alias) || !this.isCommandTypeSpecified)
                {
                    commandTypesToIgnore |= CommandTypes.Alias;
                }
                if (((this.commandType & CommandTypes.Application) != CommandTypes.Application) || !this.isCommandTypeSpecified)
                {
                    commandTypesToIgnore |= CommandTypes.Application;
                }
                this.accumulatedResults = this.accumulatedResults.Where<CommandInfo>(delegate (CommandInfo commandInfo) {
                    if ((commandInfo.CommandType & commandTypesToIgnore) != 0)
                    {
                        return (commandInfo.Name.IndexOf('-') > 0);
                    }
                    return true;
                }).ToList<CommandInfo>();
            }
            if ((this._matchedParameterNames != null) && (this.ParameterName != null))
            {
                foreach (string str in this.ParameterName)
                {
                    if (!WildcardPattern.ContainsWildcardCharacters(str) && !this._matchedParameterNames.Contains(str))
                    {
                        ArgumentException exception = new ArgumentException(string.Format(CultureInfo.InvariantCulture, DiscoveryExceptions.CommandParameterNotFound, new object[] { str }), str);
                        ErrorRecord errorRecord = new ErrorRecord(exception, "CommandParameterNotFound", ErrorCategory.ObjectNotFound, str);
                        base.WriteError(errorRecord);
                    }
                }
            }
            if ((this.names == null) || this.nameContainsWildcard)
            {
                this.accumulatedResults = this.accumulatedResults.OrderBy<CommandInfo, CommandInfo>(a => a, new CommandInfoComparer()).ToList<CommandInfo>();
            }
            this.OutputResultsHelper(this.accumulatedResults);
            object variableValue = base.Context.GetVariableValue(new VariablePath("PSSenderInfo", VariablePathFlags.None));
            if ((variableValue != null) && (variableValue is PSSenderInfo))
            {
                base.Context.HelpSystem.ResetHelpProviders();
            }
        }

        private bool FindCommandForName(SearchResolutionOptions options, string commandName, bool isPattern, bool emitErrors, ref int currentCount)
        {
            CommandSearcher searcher = new CommandSearcher(commandName, options, this.CommandType, base.Context);
            bool flag = false;
        Label_0016:
            try
            {
                if (!searcher.MoveNext())
                {
                    goto Label_016F;
                }
            }
            catch (ArgumentException exception)
            {
                if (emitErrors)
                {
                    base.WriteError(new ErrorRecord(exception, "GetCommandInvalidArgument", ErrorCategory.SyntaxError, null));
                }
                goto Label_0016;
            }
            catch (PathTooLongException exception2)
            {
                if (emitErrors)
                {
                    base.WriteError(new ErrorRecord(exception2, "GetCommandInvalidArgument", ErrorCategory.SyntaxError, null));
                }
                goto Label_0016;
            }
            catch (FileLoadException exception3)
            {
                if (emitErrors)
                {
                    base.WriteError(new ErrorRecord(exception3, "GetCommandFileLoadError", ErrorCategory.ReadError, null));
                }
                goto Label_0016;
            }
            catch (MetadataException exception4)
            {
                if (emitErrors)
                {
                    base.WriteError(new ErrorRecord(exception4, "GetCommandMetadataError", ErrorCategory.MetadataError, null));
                }
                goto Label_0016;
            }
            catch (FormatException exception5)
            {
                if (emitErrors)
                {
                    base.WriteError(new ErrorRecord(exception5, "GetCommandBadFileFormat", ErrorCategory.InvalidData, null));
                }
                goto Label_0016;
            }
            CommandInfo current = searcher.Current;
            if ((!SessionState.IsVisible(base.MyInvocation.CommandOrigin, current) || !this.IsCommandMatch(ref current)) || this.IsCommandInResult(current))
            {
                goto Label_0016;
            }
            flag = true;
            if (this.IsParameterMatch(current))
            {
                currentCount++;
                if ((this.TotalCount >= 0) && (currentCount > this.TotalCount))
                {
                    goto Label_016F;
                }
                this.accumulatedResults.Add(current);
                if (this.ArgumentList != null)
                {
                    goto Label_016F;
                }
            }
            if (((isPattern || (this.All != false)) || ((this.totalCount != -1) || this.isCommandTypeSpecified)) || this.isModuleSpecified)
            {
                goto Label_0016;
            }
        Label_016F:
            if (this.All != false)
            {
                foreach (CommandInfo info2 in this.GetMatchingCommandsFromModules(commandName))
                {
                    CommandInfo info3 = info2;
                    if (this.IsCommandMatch(ref info3))
                    {
                        flag = true;
                        if (!this.IsCommandInResult(info2) && this.IsParameterMatch(info3))
                        {
                            currentCount++;
                            if ((this.TotalCount >= 0) && (currentCount > this.TotalCount))
                            {
                                return flag;
                            }
                            this.accumulatedResults.Add(info3);
                        }
                    }
                }
            }
            return flag;
        }

        private IEnumerable<CommandInfo> GetMatchingCommandsFromModules(string commandName)
        {
            WildcardPattern iteratorVariable0 = new WildcardPattern(commandName, WildcardOptions.IgnoreCase);
            for (int i = this.Context.EngineSessionState.ModuleTableKeys.Count - 1; i >= 0; i--)
            {
                PSModuleInfo iteratorVariable2 = null;
                if ((this.Context.EngineSessionState.ModuleTable.TryGetValue(this.Context.EngineSessionState.ModuleTableKeys[i], out iteratorVariable2) && SessionStateUtilities.MatchesAnyWildcardPattern(iteratorVariable2.Name, this._modulePatterns, true)) && (iteratorVariable2.SessionState != null))
                {
                    if ((this.CommandType & (CommandTypes.Filter | CommandTypes.Function)) != 0)
                    {
                        IDictionaryEnumerator enumerator = iteratorVariable2.SessionState.Internal.GetFunctionTable().GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            DictionaryEntry current = (DictionaryEntry) enumerator.Current;
                            FunctionInfo iteratorVariable4 = (FunctionInfo) current.Value;
                            if ((iteratorVariable0.IsMatch((string) current.Key) && iteratorVariable4.IsImported) && iteratorVariable4.Module.Path.Equals(iteratorVariable2.Path, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (CommandInfo) current.Value;
                            }
                        }
                    }
                    if ((this.CommandType & CommandTypes.Alias) != 0)
                    {
                        foreach (KeyValuePair<string, AliasInfo> iteratorVariable5 in iteratorVariable2.SessionState.Internal.GetAliasTable())
                        {
                            if ((!iteratorVariable0.IsMatch(iteratorVariable5.Key) || !iteratorVariable5.Value.IsImported) || !iteratorVariable5.Value.Module.Path.Equals(iteratorVariable2.Path, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            yield return iteratorVariable5.Value;
                        }
                    }
                }
            }
        }

        private bool IsCommandInResult(CommandInfo command)
        {
            bool flag2 = command.Module != null;
            foreach (CommandInfo info in this.accumulatedResults)
            {
                if ((((command.CommandType == info.CommandType) && ((string.Compare(command.Name, info.Name, StringComparison.CurrentCultureIgnoreCase) == 0) || (string.Compare(ModuleCmdletBase.RemovePrefixFromCommandName(info.Name, info.Prefix), command.Name, StringComparison.CurrentCultureIgnoreCase) == 0))) && ((info.Module != null) && flag2)) && (((info.IsImported && command.IsImported) && info.Module.Equals(command.Module)) || ((!info.IsImported || !command.IsImported) && info.Module.Path.Equals(command.Module.Path, StringComparison.OrdinalIgnoreCase))))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsCommandMatch(ref CommandInfo current)
        {
            bool flag = false;
            if (!this.IsDuplicate(current))
            {
                if ((current.CommandType & this.CommandType) != 0)
                {
                    flag = true;
                }
                if ((current.CommandType == CommandTypes.Cmdlet) || (((this.verbs.Length > 0) || (this.nouns.Length > 0)) && (((current.CommandType == CommandTypes.Function) || (current.CommandType == CommandTypes.Filter)) || ((current.CommandType == CommandTypes.Workflow) || (current.CommandType == CommandTypes.Alias)))))
                {
                    if (!this.IsNounVerbMatch(current))
                    {
                        flag = false;
                    }
                }
                else if (((this._modulePatterns != null) && (this._modulePatterns.Count > 0)) && !SessionStateUtilities.MatchesAnyWildcardPattern(current.ModuleName, this._modulePatterns, true))
                {
                    flag = false;
                }
                if (flag)
                {
                    if (this.ArgumentList != null)
                    {
                        AliasInfo info = current as AliasInfo;
                        if (info != null)
                        {
                            current = info.ResolvedCommand;
                            if (current == null)
                            {
                                return false;
                            }
                        }
                        else if (!(current is CmdletInfo) && !(current is IScriptCommandInfo))
                        {
                            base.ThrowTerminatingError(new ErrorRecord(PSTraceSource.NewArgumentException("ArgumentList", "DiscoveryExceptions", "CommandArgsOnlyForSingleCmdlet", new object[0]), "CommandArgsOnlyForSingleCmdlet", ErrorCategory.InvalidArgument, current));
                        }
                    }
                    bool implementsDynamicParameters = false;
                    try
                    {
                        implementsDynamicParameters = current.ImplementsDynamicParameters;
                    }
                    catch (PSSecurityException)
                    {
                    }
                    catch (RuntimeException)
                    {
                    }
                    if (!implementsDynamicParameters)
                    {
                        return flag;
                    }
                    try
                    {
                        CommandInfo info2 = current.CreateGetCommandCopy(this.ArgumentList);
                        if (this.ArgumentList != null)
                        {
                            ReadOnlyCollection<CommandParameterSetInfo> parameterSets = info2.ParameterSets;
                        }
                        current = info2;
                    }
                    catch (MetadataException exception)
                    {
                        base.WriteError(new ErrorRecord(exception, "GetCommandMetadataError", ErrorCategory.MetadataError, current));
                    }
                    catch (ParameterBindingException exception2)
                    {
                        if (!exception2.ErrorRecord.FullyQualifiedErrorId.StartsWith("GetDynamicParametersException", StringComparison.Ordinal))
                        {
                            throw;
                        }
                    }
                }
            }
            return flag;
        }

        private bool IsDuplicate(CommandInfo info)
        {
            string key = null;
            ApplicationInfo info2 = info as ApplicationInfo;
            if (info2 != null)
            {
                key = info2.Path;
            }
            else
            {
                CmdletInfo info3 = info as CmdletInfo;
                if (info3 != null)
                {
                    key = info3.FullName;
                }
                else
                {
                    ScriptInfo info4 = info as ScriptInfo;
                    if (info4 != null)
                    {
                        key = info4.Definition;
                    }
                    else
                    {
                        ExternalScriptInfo info5 = info as ExternalScriptInfo;
                        if (info5 != null)
                        {
                            key = info5.Path;
                        }
                    }
                }
            }
            if (key != null)
            {
                if (this.commandsWritten.ContainsKey(key))
                {
                    return true;
                }
                this.commandsWritten.Add(key, info);
            }
            return false;
        }

        private bool IsNounVerbMatch(CommandInfo command)
        {
            string verb;
            string noun;
            bool flag = false;
            if (this.verbPatterns == null)
            {
                this.verbPatterns = SessionStateUtilities.CreateWildcardsFromStrings(this.Verb, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
            }
            if (this.nounPatterns == null)
            {
                this.nounPatterns = SessionStateUtilities.CreateWildcardsFromStrings(this.Noun, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(command.ModuleName))
            {
                if (!SessionStateUtilities.MatchesAnyWildcardPattern(command.ModuleName, this._modulePatterns, true))
                {
                    return flag;
                }
            }
            else if (this._modulePatterns.Count > 0)
            {
                return flag;
            }
            CmdletInfo info = command as CmdletInfo;
            if (info != null)
            {
                verb = info.Verb;
                noun = info.Noun;
            }
            else if (!CmdletInfo.SplitCmdletName(command.Name, out verb, out noun))
            {
                return flag;
            }
            if (SessionStateUtilities.MatchesAnyWildcardPattern(verb, this.verbPatterns, true) && SessionStateUtilities.MatchesAnyWildcardPattern(noun, this.nounPatterns, true))
            {
                flag = true;
            }
            return flag;
        }

        private bool IsParameterMatch(CommandInfo commandInfo)
        {
            if ((this.ParameterName == null) && (this.ParameterType == null))
            {
                return true;
            }
            if (this._matchedParameterNames == null)
            {
                this._matchedParameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            IEnumerable<ParameterMetadata> values = null;
            try
            {
                IDictionary<string, ParameterMetadata> parameters = commandInfo.Parameters;
                if (parameters != null)
                {
                    values = parameters.Values;
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            if (values == null)
            {
                return false;
            }
            bool flag = false;
            foreach (ParameterMetadata metadata in values)
            {
                if (this.IsParameterMatch(metadata))
                {
                    flag = true;
                }
            }
            return flag;
        }

        private bool IsParameterMatch(ParameterMetadata parameterMetadata)
        {
            bool flag4;
            bool flag = SessionStateUtilities.MatchesAnyWildcardPattern(parameterMetadata.Name, this._parameterNameWildcards, true);
            bool flag2 = false;
            foreach (string str in parameterMetadata.Aliases ?? Enumerable.Empty<string>())
            {
                if (SessionStateUtilities.MatchesAnyWildcardPattern(str, this._parameterNameWildcards, true))
                {
                    this._matchedParameterNames.Add(str);
                    flag2 = true;
                }
            }
            bool flag3 = flag || flag2;
            if (flag3)
            {
                this._matchedParameterNames.Add(parameterMetadata.Name);
            }
            if ((this._parameterTypes == null) || (this._parameterTypes.Length == 0))
            {
                flag4 = true;
            }
            else
            {
                flag4 = false;
                if ((this._parameterTypes != null) && (this._parameterTypes.Length > 0))
                {
                    flag4 |= this._parameterTypes.Any<PSTypeName>(new Func<PSTypeName, bool>(parameterMetadata.IsMatchingType));
                }
            }
            return (flag3 && flag4);
        }

        private void OutputResultsHelper(IEnumerable<CommandInfo> results)
        {
            CommandOrigin commandOrigin = base.MyInvocation.CommandOrigin;
            foreach (CommandInfo info in results)
            {
                if (SessionState.IsVisible(commandOrigin, info))
                {
                    if (this.Syntax != 0)
                    {
                        if (!string.IsNullOrEmpty(info.Syntax))
                        {
                            PSObject sendToPipeline = PSObject.AsPSObject(info.Syntax);
                            sendToPipeline.IsHelpObject = true;
                            base.WriteObject(sendToPipeline);
                        }
                    }
                    else
                    {
                        base.WriteObject(info);
                    }
                }
            }
        }

        protected override void ProcessRecord()
        {
            if (this._modulePatterns == null)
            {
                this._modulePatterns = SessionStateUtilities.CreateWildcardsFromStrings(this.Module, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
            }
            string parameterSetName = base.ParameterSetName;
            if (parameterSetName != null)
            {
                if (!(parameterSetName == "CmdletSet"))
                {
                    if (!(parameterSetName == "AllCommandSet"))
                    {
                        return;
                    }
                }
                else
                {
                    this.AccumulateMatchingCmdlets();
                    return;
                }
                this.AccumulateMatchingCommands();
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true)]
        public SwitchParameter All
        {
            get
            {
                return this.all;
            }
            set
            {
                this.all = (bool) value;
            }
        }

        [Alias(new string[] { "Args" }), Parameter(Position=1, ValueFromRemainingArguments=true), AllowNull, AllowEmptyCollection]
        public object[] ArgumentList
        {
            get
            {
                return this.commandArgs;
            }
            set
            {
                this.commandArgs = value;
            }
        }

        [Alias(new string[] { "Type" }), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="AllCommandSet")]
        public CommandTypes CommandType
        {
            get
            {
                return this.commandType;
            }
            set
            {
                this.commandType = value;
                this.isCommandTypeSpecified = true;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true)]
        public SwitchParameter ListImported
        {
            get
            {
                return this.listImported;
            }
            set
            {
                this.listImported = (bool) value;
            }
        }

        [Alias(new string[] { "PSSnapin" }), Parameter(ValueFromPipelineByPropertyName=true)]
        public string[] Module
        {
            get
            {
                return this._modules;
            }
            set
            {
                if (value == null)
                {
                    value = new string[0];
                }
                this._modules = value;
                this._modulePatterns = null;
                this.isModuleSpecified = true;
            }
        }

        [Parameter(Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="AllCommandSet"), ValidateNotNullOrEmpty]
        public string[] Name
        {
            get
            {
                return this.names;
            }
            set
            {
                this.nameContainsWildcard = false;
                this.names = value;
                if (value != null)
                {
                    foreach (string str in value)
                    {
                        if (WildcardPattern.ContainsWildcardCharacters(str))
                        {
                            this.nameContainsWildcard = true;
                            return;
                        }
                    }
                }
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CmdletSet")]
        public string[] Noun
        {
            get
            {
                return this.nouns;
            }
            set
            {
                if (value == null)
                {
                    value = new string[0];
                }
                this.nouns = value;
                this.nounPatterns = null;
            }
        }

        [Parameter, ValidateNotNullOrEmpty]
        public string[] ParameterName
        {
            get
            {
                return this._parameterNames;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._parameterNames = value;
                this._parameterNameWildcards = SessionStateUtilities.CreateWildcardsFromStrings(this._parameterNames, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
            }
        }

        [Parameter, ValidateNotNullOrEmpty]
        public PSTypeName[] ParameterType
        {
            get
            {
                return this._parameterTypes;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                List<PSTypeName> list = new List<PSTypeName>(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    PSTypeName ptn = value[i];
                    if (!value.Any<PSTypeName>(otherPtn => otherPtn.Name.StartsWith(ptn.Name + "#", StringComparison.OrdinalIgnoreCase)) && (((i == 0) || (ptn.Type == null)) || !ptn.Type.Equals(typeof(object))))
                    {
                        list.Add(ptn);
                    }
                }
                this._parameterTypes = list.ToArray();
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true)]
        public SwitchParameter Syntax
        {
            get
            {
                return this.usage;
            }
            set
            {
                this.usage = (bool) value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true)]
        public int TotalCount
        {
            get
            {
                return this.totalCount;
            }
            set
            {
                this.totalCount = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CmdletSet")]
        public string[] Verb
        {
            get
            {
                return this.verbs;
            }
            set
            {
                if (value == null)
                {
                    value = new string[0];
                }
                this.verbs = value;
                this.verbPatterns = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class CommandInfoComparer : IComparer<CommandInfo>
        {
            public CommandInfoComparer()
            {
            }

            public int Compare(CommandInfo x, CommandInfo y)
            {
                if (x.CommandType >= y.CommandType)
                {
                    if (x.CommandType <= y.CommandType)
                    {
                        return string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return -1;
                }
            }
        }
        
    }
}

