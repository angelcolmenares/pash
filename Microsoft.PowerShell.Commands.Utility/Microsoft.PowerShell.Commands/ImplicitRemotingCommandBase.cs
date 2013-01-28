namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;

    public class ImplicitRemotingCommandBase : PSCmdlet
    {
        private SwitchParameter allowClobber = new SwitchParameter(false);
        private bool assumeMeasureObjectIsAvailable = true;
        private X509Certificate2 certificate;
        private object[] commandArgs;
        private string[] commandNameParameter;
        private Collection<WildcardPattern> commandNamePatterns;
        private bool commandParameterSpecified;
        private List<string> commandSkipListFromServer;
        private List<string> commandsSkippedBecauseOfShadowing = new List<string>();
        private CommandTypes commandType = (CommandTypes.Workflow | CommandTypes.Cmdlet | CommandTypes.Filter | CommandTypes.Function | CommandTypes.Alias);
        private static readonly string[] commonParameterNames = new string[] { "Verbose", "Debug", "ErrorAction", "WarningAction", "ErrorVariable", "WarningVariable", "OutVariable", "OutBuffer" };
        private Dictionary<string, object> existingCommands;
        private string[] formatTypeNameParameter;
        private Collection<WildcardPattern> formatTypeNamePatterns;
        private bool formatTypeNamesSpecified;
        internal const string ImplicitRemotingCommandsToSkipKey = "CommandsToSkip";
        internal const string ImplicitRemotingHashKey = "Hash";
        internal const string ImplicitRemotingKey = "ImplicitRemoting";
        private DateTime lastTimeProgressWasWritten = DateTime.UtcNow;
        private Guid moduleGuid = Guid.NewGuid();
        private string prefix = string.Empty;
        private string[] PSSnapins = new string[0];
        private PSSession remoteRunspaceInfo;

        internal ImplicitRemotingCommandBase()
        {
            this.CommandName = new string[] { "*" };
            this.commandParameterSpecified = false;
            this.FormatTypeName = new string[] { "*" };
            this.formatTypeNamesSpecified = false;
        }

        private void AddRemoteCommandMetadata(Dictionary<string, CommandMetadata> name2commandMetadata, Dictionary<string, string> alias2resolvedCommandName, PSObject remoteCommandInfo)
        {
            string str;
            CommandMetadata commandMetadata = this.RehydrateCommandMetadata(remoteCommandInfo, out str);
            if (this.IsSafeCommandMetadata(commandMetadata))
            {
                if ((str != null) && !IsSafeNameOrIdentifier(commandMetadata.Name))
                {
                    base.WriteError(this.GetErrorSkippedUnsafeCommandName(str));
                }
                else if (!this.IsCommandSkippedByServerDeclaration(commandMetadata.Name) && this.IsCommandNameAllowedForImport(commandMetadata.Name))
                {
                    CommandMetadata metadata2;
                    if (name2commandMetadata.TryGetValue(commandMetadata.Name, out metadata2))
                    {
                        int commandTypePriority = this.GetCommandTypePriority(metadata2.WrappedCommandType);
                        int num2 = this.GetCommandTypePriority(commandMetadata.WrappedCommandType);
                        if (commandTypePriority < num2)
                        {
                            return;
                        }
                    }
                    if (str != null)
                    {
                        alias2resolvedCommandName[commandMetadata.Name] = str;
                        commandMetadata.Name = str;
                    }
                    name2commandMetadata[commandMetadata.Name] = commandMetadata;
                }
            }
        }

        private void AddRemoteTypeDefinition(IList<ExtendedTypeDefinition> listOfTypeDefinitions, PSObject remoteTypeDefinition)
        {
            ExtendedTypeDefinition typeDefinition = this.ConvertTo<ExtendedTypeDefinition>("Get-FormatData", remoteTypeDefinition);
            if (this.IsSafeTypeDefinition(typeDefinition))
            {
                listOfTypeDefinitions.Add(typeDefinition);
            }
        }

        private PowerShell BuildPowerShellForGetCommand()
        {
            PowerShell shell = PowerShell.Create();
            shell.AddCommand("Get-Command");
            shell.AddParameter("CommandType", this.CommandType);
            if (this.CommandName != null)
            {
                shell.AddParameter("Name", this.CommandName);
            }
            shell.AddParameter("Module", this.Module);
            shell.AddParameter("ArgumentList", this.ArgumentList);
            shell.Runspace = this.remoteRunspaceInfo.Runspace;
            shell.RemotePowerShell.HostCallReceived += new EventHandler<RemoteDataEventArgs<RemoteHostCall>>(this.HandleHostCallReceived);
            return shell;
        }

        private PowerShell BuildPowerShellForGetFormatData()
        {
            PowerShell shell = PowerShell.Create();
            shell.AddCommand("Get-FormatData");
            shell.AddParameter("TypeName", this.FormatTypeName);
            shell.Runspace = this.remoteRunspaceInfo.Runspace;
            return shell;
        }

        private T ConvertTo<T>(string commandName, object value)
        {
            return this.ConvertTo<T>(commandName, value, false);
        }

        private T ConvertTo<T>(string commandName, object value, bool nullOk)
        {
            T local;
            if (value == null)
            {
                if (nullOk)
                {
                    return default(T);
                }
                base.ThrowTerminatingError(this.GetErrorMalformedDataFromRemoteCommand(commandName));
            }
            if (!LanguagePrimitives.TryConvertTo<T>(value, out local))
            {
                base.ThrowTerminatingError(this.GetErrorMalformedDataFromRemoteCommand(commandName));
            }
            return local;
        }

        private int CountRemoteObjects(PowerShell powerShell)
        {
            if (!this.assumeMeasureObjectIsAvailable)
            {
                return -1;
            }
            try
            {
                Collection<PSObject> collection;
                int num;
                powerShell.AddCommand("Measure-Object");
                using (new PowerShellStopper(base.Context, powerShell))
                {
                    collection = powerShell.Invoke();
                }
                if ((collection == null) || (collection.Count != 1))
                {
                    this.assumeMeasureObjectIsAvailable = false;
                    return -1;
                }
                PSPropertyInfo info = collection[0].Properties["Count"];
                if (info == null)
                {
                    this.assumeMeasureObjectIsAvailable = false;
                    return -1;
                }
                if (LanguagePrimitives.TryConvertTo<int>(info.Value, out num))
                {
                    return num;
                }
                this.assumeMeasureObjectIsAvailable = false;
                return -1;
            }
            catch (RuntimeException)
            {
                this.assumeMeasureObjectIsAvailable = false;
                return -1;
            }
        }

        internal void DuplicatePowerShellStreams(PowerShell powerShell)
        {
            foreach (ErrorRecord record in powerShell.Streams.Error.ReadAll())
            {
                base.WriteError(record);
            }
            foreach (WarningRecord record2 in powerShell.Streams.Warning.ReadAll())
            {
                base.WriteWarning(record2.Message);
            }
            foreach (VerboseRecord record3 in powerShell.Streams.Verbose.ReadAll())
            {
                base.WriteVerbose(record3.Message);
            }
            foreach (DebugRecord record4 in powerShell.Streams.Debug.ReadAll())
            {
                base.WriteDebug(record4.Message);
            }
        }

        internal List<string> GenerateProxyModule(DirectoryInfo moduleRootDirectory, string moduleNamePrefix, Encoding encoding, bool force, List<CommandMetadata> listOfCommandMetadata, Dictionary<string, string> alias2resolvedCommandName, List<ExtendedTypeDefinition> listOfFormatData)
        {
            if (this.commandsSkippedBecauseOfShadowing.Count != 0)
            {
                this.ReportSkippedCommands();
                if (listOfCommandMetadata.Count == 0)
                {
                    ErrorRecord errorNoCommandsImportedBecauseOfSkipping = this.GetErrorNoCommandsImportedBecauseOfSkipping();
                    base.ThrowTerminatingError(errorNoCommandsImportedBecauseOfSkipping);
                }
            }
            List<string> list = new ImplicitRemotingCodeGenerator(this.Session, this.ModuleGuid, base.MyInvocation).GenerateProxyModule(moduleRootDirectory, moduleNamePrefix, encoding, force, listOfCommandMetadata, alias2resolvedCommandName, listOfFormatData, this.Certificate);
            this.WriteProgress(StringUtil.Format(ImplicitRemotingStrings.ProgressStatusCompleted, new object[0]), 100, 0);
            return list;
        }

        private int GetCommandTypePriority(CommandTypes commandType)
        {
            CommandTypes types = commandType;
            if (types <= CommandTypes.ExternalScript)
            {
                switch (types)
                {
                    case CommandTypes.Alias:
                        return 10;

                    case CommandTypes.Function:
                    case CommandTypes.Filter:
                        goto Label_0041;

                    case CommandTypes.Cmdlet:
                        return 30;

                    case CommandTypes.ExternalScript:
                        goto Label_0047;
                }
                goto Label_004A;
            }
            if (types == CommandTypes.Application)
            {
                goto Label_0047;
            }
            if ((types != CommandTypes.Script) && (types != CommandTypes.Workflow))
            {
                goto Label_004A;
            }
        Label_0041:
            return 20;
        Label_0047:
            return 40;
        Label_004A:
            return 50;
        }

        private ErrorRecord GetErrorCommandSkippedBecauseOfShadowing(string commandNames)
        {
            if (string.IsNullOrEmpty(commandNames))
            {
                throw PSTraceSource.NewArgumentNullException("commandNames");
            }
            string errorId = "ErrorCommandSkippedBecauseOfShadowing";
            ErrorDetails errorDetails = this.GetErrorDetails(errorId, new object[] { commandNames });
            return new ErrorRecord(new InvalidOperationException(errorDetails.Message), errorId, ErrorCategory.InvalidData, null) { ErrorDetails = errorDetails };
        }

        private ErrorRecord GetErrorCouldntResolvedAlias(string aliasName)
        {
            if (string.IsNullOrEmpty(aliasName))
            {
                throw PSTraceSource.NewArgumentNullException("aliasName");
            }
            string errorId = "ErrorCouldntResolveAlias";
            ErrorDetails errorDetails = this.GetErrorDetails(errorId, new object[] { aliasName });
            return new ErrorRecord(new ArgumentException(errorDetails.Message), errorId, ErrorCategory.OperationTimeout, null) { ErrorDetails = errorDetails };
        }

        internal ErrorDetails GetErrorDetails(string errorId, params object[] args)
        {
            if (string.IsNullOrEmpty(errorId))
            {
                throw PSTraceSource.NewArgumentNullException("errorId");
            }
            return new ErrorDetails(base.GetType().Assembly, "ImplicitRemotingStrings", errorId, args);
        }

        private ErrorRecord GetErrorFromRemoteCommand(string commandName, RuntimeException runtimeException)
        {
            string str;
            ErrorDetails errorDetails;
            if (string.IsNullOrEmpty(commandName))
            {
                throw PSTraceSource.NewArgumentNullException("commandName");
            }
            if (runtimeException == null)
            {
                throw PSTraceSource.NewArgumentNullException("runtimeException");
            }
            RemoteException exception = runtimeException as RemoteException;
            if (((exception != null) && (exception.SerializedRemoteException != null)) && Deserializer.IsInstanceOfType(exception.SerializedRemoteException, typeof(CommandNotFoundException)))
            {
                str = "ErrorRequiredRemoteCommandNotFound";
                errorDetails = this.GetErrorDetails(str, new object[] { base.MyInvocation.MyCommand.Name });
                return new ErrorRecord(new RuntimeException(errorDetails.Message, runtimeException), str, ErrorCategory.ObjectNotFound, null) { ErrorDetails = errorDetails };
            }
            str = "ErrorFromRemoteCommand";
            errorDetails = this.GetErrorDetails(str, new object[] { "Get-Command", runtimeException.Message });
            return new ErrorRecord(new RuntimeException(errorDetails.Message, runtimeException), str, ErrorCategory.InvalidResult, null) { ErrorDetails = errorDetails };
        }

        private ErrorRecord GetErrorMalformedDataFromRemoteCommand(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                throw PSTraceSource.NewArgumentNullException("commandName");
            }
            string errorId = "ErrorMalformedDataFromRemoteCommand";
            ErrorDetails errorDetails = this.GetErrorDetails(errorId, new object[] { commandName });
            return new ErrorRecord(new ArgumentException(errorDetails.Message), errorId, ErrorCategory.InvalidResult, null) { ErrorDetails = errorDetails };
        }

        private ErrorRecord GetErrorNoCommandsImportedBecauseOfSkipping()
        {
            string errorId = "ErrorNoCommandsImportedBecauseOfSkipping";
            ErrorDetails errorDetails = this.GetErrorDetails(errorId, new object[0]);
            return new ErrorRecord(new ArgumentException(errorDetails.Message), errorId, ErrorCategory.InvalidResult, null) { ErrorDetails = errorDetails };
        }

        private ErrorRecord GetErrorNoResultsFromRemoteEnd(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                throw PSTraceSource.NewArgumentNullException("commandName");
            }
            string errorId = "ErrorNoResultsFromRemoteEnd";
            ErrorDetails errorDetails = this.GetErrorDetails(errorId, new object[] { commandName });
            return new ErrorRecord(new ArgumentException(errorDetails.Message), errorId, ErrorCategory.InvalidResult, null) { ErrorDetails = errorDetails };
        }

        private ErrorRecord GetErrorSkippedNonRequestedCommand(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                throw PSTraceSource.NewArgumentNullException("commandName");
            }
            string errorId = "ErrorSkippedNonRequestedCommand";
            ErrorDetails errorDetails = this.GetErrorDetails(errorId, new object[] { commandName });
            return new ErrorRecord(new InvalidOperationException(errorDetails.Message), errorId, ErrorCategory.ResourceExists, null) { ErrorDetails = errorDetails };
        }

        private ErrorRecord GetErrorSkippedNonRequestedTypeDefinition(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw PSTraceSource.NewArgumentNullException("typeName");
            }
            string errorId = "ErrorSkippedNonRequestedTypeDefinition";
            ErrorDetails errorDetails = this.GetErrorDetails(errorId, new object[] { typeName });
            return new ErrorRecord(new InvalidOperationException(errorDetails.Message), errorId, ErrorCategory.ResourceExists, null) { ErrorDetails = errorDetails };
        }

        private ErrorRecord GetErrorSkippedUnsafeCommandName(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                throw PSTraceSource.NewArgumentNullException("commandName");
            }
            string errorId = "ErrorSkippedUnsafeCommandName";
            ErrorDetails errorDetails = this.GetErrorDetails(errorId, new object[] { commandName });
            return new ErrorRecord(new InvalidOperationException(errorDetails.Message), errorId, ErrorCategory.InvalidData, null) { ErrorDetails = errorDetails };
        }

        private ErrorRecord GetErrorSkippedUnsafeNameInMetadata(string commandName, string nameType, string name)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                throw PSTraceSource.NewArgumentNullException("commandName");
            }
            if (string.IsNullOrEmpty(nameType))
            {
                throw PSTraceSource.NewArgumentNullException("nameType");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            string errorId = "ErrorSkippedUnsafe" + nameType + "Name";
            ErrorDetails errorDetails = this.GetErrorDetails(errorId, new object[] { commandName, name });
            return new ErrorRecord(new InvalidOperationException(errorDetails.Message), errorId, ErrorCategory.InvalidData, null) { ErrorDetails = errorDetails };
        }

        private T GetPropertyValue<T>(string commandName, PSObject pso, string propertyName)
        {
            return this.GetPropertyValue<T>(commandName, pso, propertyName, false);
        }

        private T GetPropertyValue<T>(string commandName, PSObject pso, string propertyName, bool nullOk)
        {
            PSPropertyInfo info = pso.Properties[propertyName];
            if (info == null)
            {
                base.ThrowTerminatingError(this.GetErrorMalformedDataFromRemoteCommand(commandName));
            }
            return this.ConvertTo<T>(commandName, info.Value, nullOk);
        }

        internal List<CommandMetadata> GetRemoteCommandMetadata(out Dictionary<string, string> alias2resolvedCommandName)
        {
            bool flag = this.Session.Runspace.GetRemoteProtocolVersion() == RemotingConstants.ProtocolVersionWin7RC;
            alias2resolvedCommandName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (((this.CommandName == null) || (this.CommandName.Length == 0)) || (!this.commandParameterSpecified && this.formatTypeNamesSpecified))
            {
                return new List<CommandMetadata>();
            }
            this.WriteProgress(StringUtil.Format(ImplicitRemotingStrings.ProgressStatusGetCommandStart, new object[0]), null, null);
            using (PowerShell shell = this.BuildPowerShellForGetCommand())
            {
                shell.AddCommand("Select-Object");
                shell.AddParameter("Property", new string[] { "Name", "CommandType", "ResolvedCommandName", "DefaultParameterSet", "CmdletBinding", "Parameters" });
                shell.IsGetCommandMetadataSpecialPipeline = !flag;
                IAsyncResult asyncResult = null;
                try
                {
                    int expectedCount = -1;
                    if (flag)
                    {
                        using (PowerShell shell2 = this.BuildPowerShellForGetCommand())
                        {
                            expectedCount = this.CountRemoteObjects(shell2);
                        }
                    }
                    Dictionary<string, CommandMetadata> dictionary = new Dictionary<string, CommandMetadata>(StringComparer.OrdinalIgnoreCase);
                    using (new PowerShellStopper(base.Context, shell))
                    {
                        DateTime utcNow = DateTime.UtcNow;
                        PSDataCollection<PSObject> output = new PSDataCollection<PSObject>();
                        asyncResult = shell.BeginInvoke<PSObject, PSObject>(null, output);
                        int num2 = 0;
                        foreach (PSObject obj2 in output)
                        {
                            if (!flag && (expectedCount == -1))
                            {
                                expectedCount = RemotingDecoder.GetPropertyValue<int>(obj2, "Count");
                            }
                            else
                            {
                                this.AddRemoteCommandMetadata(dictionary, alias2resolvedCommandName, obj2);
                                this.DuplicatePowerShellStreams(shell);
                                this.WriteProgress(utcNow, ++num2, expectedCount, ImplicitRemotingStrings.ProgressStatusGetCommandProgress);
                            }
                        }
                        this.DuplicatePowerShellStreams(shell);
                        shell.EndInvoke(asyncResult);
                        if ((num2 == 0) && this.commandParameterSpecified)
                        {
                            base.ThrowTerminatingError(this.GetErrorNoResultsFromRemoteEnd("Get-Command"));
                        }
                        return new List<CommandMetadata>(dictionary.Values);
                    }
                }
                catch (RuntimeException exception)
                {
                    base.ThrowTerminatingError(this.GetErrorFromRemoteCommand("Get-Command", exception));
                }
            }
            return null;
        }

        internal List<ExtendedTypeDefinition> GetRemoteFormatData()
        {
            if (((this.FormatTypeName == null) || (this.FormatTypeName.Length == 0)) || (this.commandParameterSpecified && !this.formatTypeNamesSpecified))
            {
                return new List<ExtendedTypeDefinition>();
            }
            this.WriteProgress(StringUtil.Format(ImplicitRemotingStrings.ProgressStatusGetFormatDataStart, new object[0]), null, null);
            using (PowerShell shell = this.BuildPowerShellForGetFormatData())
            {
                IAsyncResult asyncResult = null;
                try
                {
                    int expectedCount = -1;
                    using (PowerShell shell2 = this.BuildPowerShellForGetFormatData())
                    {
                        expectedCount = this.CountRemoteObjects(shell2);
                    }
                    using (new PowerShellStopper(base.Context, shell))
                    {
                        DateTime utcNow = DateTime.UtcNow;
                        PSDataCollection<PSObject> output = new PSDataCollection<PSObject>();
                        asyncResult = shell.BeginInvoke<PSObject, PSObject>(null, output);
                        int num2 = 0;
                        List<ExtendedTypeDefinition> listOfTypeDefinitions = new List<ExtendedTypeDefinition>();
                        foreach (PSObject obj2 in output)
                        {
                            this.AddRemoteTypeDefinition(listOfTypeDefinitions, obj2);
                            this.DuplicatePowerShellStreams(shell);
                            this.WriteProgress(utcNow, ++num2, expectedCount, ImplicitRemotingStrings.ProgressStatusGetFormatDataProgress);
                        }
                        this.DuplicatePowerShellStreams(shell);
                        shell.EndInvoke(asyncResult);
                        if ((num2 == 0) && this.formatTypeNamesSpecified)
                        {
                            base.ThrowTerminatingError(this.GetErrorNoResultsFromRemoteEnd("Get-FormatData"));
                        }
                        return listOfTypeDefinitions;
                    }
                }
                catch (RuntimeException exception)
                {
                    base.ThrowTerminatingError(this.GetErrorFromRemoteCommand("Get-FormatData", exception));
                }
            }
            return null;
        }

        private void HandleHostCallReceived(object sender, RemoteDataEventArgs<RemoteHostCall> eventArgs)
        {
            ClientRemotePowerShell.ExitHandler(sender, eventArgs);
        }

        private bool IsCommandNameAllowedForImport(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                throw PSTraceSource.NewArgumentNullException("commandName");
            }
            if (!this.AllowClobber.IsPresent && this.IsShadowingExistingCommands(commandName))
            {
                this.commandsSkippedBecauseOfShadowing.Add(commandName);
                return false;
            }
            return true;
        }

        private bool IsCommandNameMatchingParameters(string commandName)
        {
            if (SessionStateUtilities.MatchesAnyWildcardPattern(commandName, this.commandNamePatterns, false))
            {
                return true;
            }
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(commandName);
            return (!fileNameWithoutExtension.Equals(commandName, StringComparison.OrdinalIgnoreCase) && SessionStateUtilities.MatchesAnyWildcardPattern(fileNameWithoutExtension, this.commandNamePatterns, false));
        }

        private bool IsCommandSkippedByServerDeclaration(string commandName)
        {
            foreach (string str in this.CommandSkipListFromServer)
            {
                if (str.Equals(commandName, StringComparison.OrdinalIgnoreCase))
                {
                    if (this.CommandName != null)
                    {
                        foreach (string str2 in this.CommandName)
                        {
                            if (commandName.Equals(str2, StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private bool IsProxyForCmdlet(Dictionary<string, ParameterMetadata> parameters)
        {
            foreach (string str in commonParameterNames)
            {
                if (!parameters.ContainsKey(str))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsSafeCommandMetadata(CommandMetadata commandMetadata)
        {
            if (!this.IsCommandNameMatchingParameters(commandMetadata.Name))
            {
                base.WriteError(this.GetErrorSkippedNonRequestedCommand(commandMetadata.Name));
                return false;
            }
            if (!IsSafeNameOrIdentifier(commandMetadata.Name))
            {
                base.WriteError(this.GetErrorSkippedUnsafeCommandName(commandMetadata.Name));
                return false;
            }
            if ((commandMetadata.DefaultParameterSetName != null) && !IsSafeNameOrIdentifier(commandMetadata.DefaultParameterSetName))
            {
                base.WriteError(this.GetErrorSkippedUnsafeNameInMetadata(commandMetadata.Name, "ParameterSet", commandMetadata.DefaultParameterSetName));
                return false;
            }
            if (commandMetadata.Parameters != null)
            {
                foreach (ParameterMetadata metadata in commandMetadata.Parameters.Values)
                {
                    if (!IsSafeTypeConstraint(metadata.ParameterType))
                    {
                        metadata.ParameterType = null;
                    }
                    if (!IsSafeParameterName(metadata.Name))
                    {
                        base.WriteError(this.GetErrorSkippedUnsafeNameInMetadata(commandMetadata.Name, "Parameter", metadata.Name));
                        return false;
                    }
                    if (metadata.Aliases != null)
                    {
                        foreach (string str in metadata.Aliases)
                        {
                            if (!IsSafeNameOrIdentifier(str))
                            {
                                base.WriteError(this.GetErrorSkippedUnsafeNameInMetadata(commandMetadata.Name, "Alias", str));
                                return false;
                            }
                        }
                    }
                    if (metadata.ParameterSets != null)
                    {
                        foreach (KeyValuePair<string, ParameterSetMetadata> pair in metadata.ParameterSets)
                        {
                            if (!IsSafeNameOrIdentifier(pair.Key))
                            {
                                base.WriteError(this.GetErrorSkippedUnsafeNameInMetadata(commandMetadata.Name, "ParameterSet", pair.Key));
                                return false;
                            }
                            ParameterSetMetadata local1 = pair.Value;
                        }
                    }
                }
            }
            return true;
        }

        private static bool IsSafeNameOrIdentifier(string name)
        {
            return (!string.IsNullOrEmpty(name) && Regex.IsMatch(name, @"^[-._:\\\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Lm}]{1,100}$", RegexOptions.CultureInvariant | RegexOptions.Singleline));
        }

        private static bool IsSafeParameterName(string parameterName)
        {
            return (IsSafeNameOrIdentifier(parameterName) && !parameterName.Contains(":"));
        }

        private static bool IsSafeTypeConstraint(Type type)
        {
            if (type == null)
            {
                return true;
            }
            if (type.IsArray)
            {
                return IsSafeTypeConstraint(type.GetElementType());
            }
            return (type.Equals(typeof(Hashtable)) || (type.Equals(typeof(SwitchParameter)) || (type.Equals(typeof(PSCredential)) || (type.Equals(typeof(SecureString)) || (KnownTypes.GetTypeSerializationInfo(type) != null)))));
        }

        private bool IsSafeTypeDefinition(ExtendedTypeDefinition typeDefinition)
        {
            if (!this.IsTypeNameMatchingParameters(typeDefinition.TypeName))
            {
                base.WriteError(this.GetErrorSkippedNonRequestedTypeDefinition(typeDefinition.TypeName));
                return false;
            }
            return true;
        }

        private bool IsShadowingExistingCommands(string commandName)
        {
            commandName = ModuleCmdletBase.AddPrefixToCommandName(commandName, this.Prefix);
            CommandSearcher searcher = new CommandSearcher(commandName, SearchResolutionOptions.None, CommandTypes.All, base.Context);
            foreach (string str in searcher.ConstructSearchPatternsFromName(commandName))
            {
                if (this.ExistingCommands.ContainsKey(str))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsTypeNameMatchingParameters(string name)
        {
            return SessionStateUtilities.MatchesAnyWildcardPattern(name, this.formatTypeNamePatterns, false);
        }

        private CommandMetadata RehydrateCommandMetadata(PSObject deserializedCommandInfo, out string resolvedCommandName)
        {
            if (deserializedCommandInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("deserializedCommandInfo");
            }
            string aliasName = this.GetPropertyValue<string>("Get-Command", deserializedCommandInfo, "Name");
            CommandTypes commandType = this.GetPropertyValue<CommandTypes>("Get-Command", deserializedCommandInfo, "CommandType");
            if (commandType == CommandTypes.Alias)
            {
                resolvedCommandName = this.GetPropertyValue<string>("Get-Command", deserializedCommandInfo, "ResolvedCommandName", true);
                if (string.IsNullOrEmpty(resolvedCommandName))
                {
                    base.WriteError(this.GetErrorCouldntResolvedAlias(aliasName));
                }
            }
            else
            {
                resolvedCommandName = null;
            }
            Dictionary<string, ParameterMetadata> parameters = this.RehydrateDictionary<string, ParameterMetadata>("Get-Command", deserializedCommandInfo, "Parameters", new Converter<PSObject, ParameterMetadata>(this.RehydrateParameterMetadata));
            parameters.Remove("AsJob");
            ParameterMetadata metadata = new ParameterMetadata("AsJob", typeof(SwitchParameter));
            parameters.Add(metadata.Name, metadata);
            return new CommandMetadata(aliasName, commandType, this.IsProxyForCmdlet(parameters), "__AllParameterSets", false, ConfirmImpact.None, false, false, true, parameters);
        }

        private Dictionary<K, V> RehydrateDictionary<K, V>(string commandName, PSObject deserializedObject, string propertyName, Converter<PSObject, V> valueRehydrator)
        {
            Converter<PSObject, V> converter = null;
            if (valueRehydrator == null)
            {
                if (converter == null)
                {
                    converter = pso => this.ConvertTo<V>(commandName, pso);
                }
                valueRehydrator = converter;
            }
            Dictionary<K, V> dictionary = new Dictionary<K, V>();
            PSPropertyInfo info = deserializedObject.Properties[propertyName];
            if (info != null)
            {
                Hashtable hashtable = this.ConvertTo<Hashtable>(commandName, info.Value, true);
                if (hashtable == null)
                {
                    return dictionary;
                }
                foreach (DictionaryEntry entry in hashtable)
                {
                    K key = this.ConvertTo<K>(commandName, entry.Key);
                    PSObject input = this.ConvertTo<PSObject>(commandName, entry.Value);
                    V local2 = valueRehydrator(input);
                    dictionary.Add(key, local2);
                }
            }
            return dictionary;
        }

        private List<T> RehydrateList<T>(string commandName, object deserializedList, Converter<PSObject, T> itemRehydrator)
        {
            Converter<PSObject, T> converter = null;
            if (itemRehydrator == null)
            {
                if (converter == null)
                {
                    converter = pso => this.ConvertTo<T>(commandName, pso);
                }
                itemRehydrator = converter;
            }
            List<T> list = null;
            ArrayList list2 = this.ConvertTo<ArrayList>(commandName, deserializedList, true);
            if (list2 != null)
            {
                list = new List<T>();
                foreach (object obj2 in list2)
                {
                    PSObject input = this.ConvertTo<PSObject>(commandName, obj2);
                    T item = itemRehydrator(input);
                    list.Add(item);
                }
            }
            return list;
        }

        private List<T> RehydrateList<T>(string commandName, PSObject deserializedObject, string propertyName, Converter<PSObject, T> itemRehydrator)
        {
            List<T> list = null;
            PSPropertyInfo info = deserializedObject.Properties[propertyName];
            if (info != null)
            {
                list = this.RehydrateList<T>(commandName, info.Value, itemRehydrator);
            }
            return list;
        }

        private ParameterMetadata RehydrateParameterMetadata(PSObject deserializedParameterMetadata)
        {
            if (deserializedParameterMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("deserializedParameterMetadata");
            }
            string name = this.GetPropertyValue<string>("Get-Command", deserializedParameterMetadata, "Name");
            bool isDynamic = this.GetPropertyValue<bool>("Get-Command", deserializedParameterMetadata, "IsDynamic");
            Type parameterType = this.RehydrateParameterType(deserializedParameterMetadata);
            List<string> list = this.RehydrateList<string>("Get-Command", deserializedParameterMetadata, "Aliases", null);
            ParameterSetMetadata metadata = new ParameterSetMetadata(-2147483648, 0, null);
            Dictionary<string, ParameterSetMetadata> parameterSets = new Dictionary<string, ParameterSetMetadata>(StringComparer.OrdinalIgnoreCase);
            parameterSets.Add("__AllParameterSets", metadata);
            return new ParameterMetadata((list == null) ? new Collection<string>() : new Collection<string>(list), isDynamic, name, parameterSets, parameterType);
        }

        private Type RehydrateParameterType(PSObject deserializedParameterMetadata)
        {
            if (this.GetPropertyValue<bool>("Get-Command", deserializedParameterMetadata, "SwitchParameter"))
            {
                return typeof(SwitchParameter);
            }
            return typeof(object);
        }

        private void ReportSkippedCommands()
        {
            if (this.commandsSkippedBecauseOfShadowing.Count != 0)
            {
                ErrorRecord errorCommandSkippedBecauseOfShadowing = this.GetErrorCommandSkippedBecauseOfShadowing(string.Join(", ", this.commandsSkippedBecauseOfShadowing.ToArray()).ToString());
                base.WriteWarning(errorCommandSkippedBecauseOfShadowing.ErrorDetails.Message);
            }
        }

        private void WriteProgress(string statusDescription, int? percentComplete, int? secondsRemaining)
        {
            ProgressRecordType completed;
            if ((secondsRemaining.HasValue && (secondsRemaining.Value == 0)) && (percentComplete.HasValue && (percentComplete.Value == 100)))
            {
                completed = ProgressRecordType.Completed;
            }
            else
            {
                completed = ProgressRecordType.Processing;
            }
            if (completed == ProgressRecordType.Processing)
            {
                TimeSpan span = (TimeSpan) (DateTime.UtcNow - this.lastTimeProgressWasWritten);
                if (span < TimeSpan.FromMilliseconds(200.0))
                {
                    return;
                }
            }
            this.lastTimeProgressWasWritten = DateTime.UtcNow;
            string activity = StringUtil.Format(ImplicitRemotingStrings.ProgressActivity, new object[0]);
            ProgressRecord progressRecord = new ProgressRecord(0x71914cd7, activity, statusDescription);
            if (percentComplete.HasValue)
            {
                progressRecord.PercentComplete = percentComplete.Value;
            }
            if (secondsRemaining.HasValue)
            {
                progressRecord.SecondsRemaining = secondsRemaining.Value;
            }
            progressRecord.RecordType = completed;
            base.WriteProgress(progressRecord);
        }

        private void WriteProgress(DateTime startTime, int currentCount, int expectedCount, string resourceId)
        {
            string statusDescription = StringUtil.Format(resourceId, currentCount);
            if (expectedCount <= 0)
            {
                this.WriteProgress(statusDescription, null, null);
            }
            else
            {
                double percentageComplete = ((double) currentCount) / ((double) expectedCount);
                int? secondsRemaining = ProgressRecord.GetSecondsRemaining(startTime, percentageComplete);
                this.WriteProgress(statusDescription, new int?((int) (100.0 * percentageComplete)), secondsRemaining);
            }
        }

        [Parameter]
        public SwitchParameter AllowClobber
        {
            get
            {
                return this.allowClobber;
            }
            set
            {
                this.allowClobber = value;
            }
        }

        [Parameter, AllowNull, AllowEmptyCollection, Alias(new string[] { "Args" })]
        public object[] ArgumentList
        {
            get
            {
                return this.commandArgs;
            }
            set
            {
                this.commandArgs = value;
                this.commandParameterSpecified = true;
            }
        }

        [Parameter]
        public X509Certificate2 Certificate
        {
            get
            {
                return this.certificate;
            }
            set
            {
                this.certificate = value;
            }
        }

        [Alias(new string[] { "Name" }), Parameter(Position=2)]
        public string[] CommandName
        {
            get
            {
                return this.commandNameParameter;
            }
            set
            {
                this.commandNameParameter = value;
                this.commandParameterSpecified = true;
                this.commandNamePatterns = SessionStateUtilities.CreateWildcardsFromStrings(this.commandNameParameter, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
            }
        }

        private List<string> CommandSkipListFromServer
        {
            get
            {
                string[] strArray;
                if ((this.commandSkipListFromServer == null) && PSPrimitiveDictionary.TryPathGet<string[]>(this.Session.ApplicationPrivateData, out strArray, new string[] { "ImplicitRemoting", "CommandsToSkip" }))
                {
                    this.commandSkipListFromServer = new List<string>();
                    if (strArray != null)
                    {
                        this.commandSkipListFromServer.AddRange(strArray);
                    }
                }
                if (this.commandSkipListFromServer == null)
                {
                    this.commandSkipListFromServer = new List<string>();
                    this.commandSkipListFromServer.Add("Get-Command");
                    this.commandSkipListFromServer.Add("Get-FormatData");
                    this.commandSkipListFromServer.Add("Get-Help");
                    this.commandSkipListFromServer.Add("Select-Object");
                    this.commandSkipListFromServer.Add("Measure-Object");
                    this.commandSkipListFromServer.Add("Exit-PSSession");
                    this.commandSkipListFromServer.Add("Out-Default");
                }
                return this.commandSkipListFromServer;
            }
        }

        [Alias(new string[] { "Type" }), Parameter]
        public CommandTypes CommandType
        {
            get
            {
                return this.commandType;
            }
            set
            {
                this.commandType = value;
                this.commandParameterSpecified = true;
            }
        }

        private Dictionary<string, object> ExistingCommands
        {
            get
            {
                if (this.existingCommands == null)
                {
                    this.existingCommands = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    CommandSearcher searcher = new CommandSearcher("*", SearchResolutionOptions.CommandNameIsPattern | SearchResolutionOptions.ResolveFunctionPatterns | SearchResolutionOptions.ResolveAliasPatterns, CommandTypes.All, base.Context);
                    foreach (CommandInfo info in (IEnumerable<CommandInfo>) searcher)
                    {
                        this.existingCommands[info.Name] = null;
                    }
                }
                return this.existingCommands;
            }
        }

        [Parameter(Position=3)]
        public string[] FormatTypeName
        {
            get
            {
                return this.formatTypeNameParameter;
            }
            set
            {
                this.formatTypeNameParameter = value;
                this.formatTypeNamesSpecified = true;
                this.formatTypeNamePatterns = SessionStateUtilities.CreateWildcardsFromStrings(this.formatTypeNameParameter, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
            }
        }

        [Alias(new string[] { "PSSnapin" }), ValidateNotNull, Parameter]
        public string[] Module
        {
            get
            {
                return this.PSSnapins;
            }
            set
            {
                if (value == null)
                {
                    value = new string[0];
                }
                this.PSSnapins = value;
                this.commandParameterSpecified = true;
            }
        }

        internal Guid ModuleGuid
        {
            get
            {
                return this.moduleGuid;
            }
        }

        internal string Prefix
        {
            get
            {
                return this.prefix;
            }
            set
            {
                this.prefix = value;
            }
        }

        [ValidateNotNull, Parameter(Mandatory=true, Position=0)]
        public PSSession Session
        {
            get
            {
                return this.remoteRunspaceInfo;
            }
            set
            {
                this.remoteRunspaceInfo = value;
            }
        }
    }
}

