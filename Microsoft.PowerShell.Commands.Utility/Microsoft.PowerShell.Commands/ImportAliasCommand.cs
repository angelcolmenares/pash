namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;
    using System.Security;

    [OutputType(new Type[] { typeof(AliasInfo) }), Cmdlet("Import", "Alias", SupportsShouldProcess=true, DefaultParameterSetName="ByPath", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113339")]
    public class ImportAliasCommand : PSCmdlet
    {
        private Dictionary<string, CommandTypes> existingCommands;
        private bool force;
        private const string LiteralPathParameterSetName = "ByLiteralPath";
        private bool passThru;
        private string path;
        private string scope;

        private Collection<AliasInfo> GetAliasesFromFile(bool isLiteralPath)
        {
            Collection<AliasInfo> collection = new Collection<AliasInfo>();
            string filePath = null;
            using (StreamReader reader = this.OpenFile(out filePath, isLiteralPath))
            {
                CSVHelper helper = new CSVHelper(',');
                long num = 0L;
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    num += 1L;
                    if (((line.Length != 0) && !OnlyContainsWhitespace(line)) && (line[0] != '#'))
                    {
                        Collection<string> collection2 = helper.ParseCsv(line);
                        if (collection2.Count != 4)
                        {
                            string message = StringUtil.Format(AliasCommandStrings.ImportAliasFileInvalidFormat, filePath, num);
                            FormatException exception = new FormatException(message);
                            ErrorRecord errorRecord = new ErrorRecord(exception, "ImportAliasFileFormatError", ErrorCategory.ReadError, filePath) {
                                ErrorDetails = new ErrorDetails(message)
                            };
                            base.ThrowTerminatingError(errorRecord);
                        }
                        ScopedItemOptions none = ScopedItemOptions.None;
                        try
                        {
                            none = (ScopedItemOptions) Enum.Parse(typeof(ScopedItemOptions), collection2[3], true);
                        }
                        catch (ArgumentException exception2)
                        {
                            string str4 = StringUtil.Format(AliasCommandStrings.ImportAliasOptionsError, filePath, num);
                            ErrorRecord record2 = new ErrorRecord(exception2, "ImportAliasOptionsError", ErrorCategory.ReadError, filePath) {
                                ErrorDetails = new ErrorDetails(str4)
                            };
                            base.WriteError(record2);
                            continue;
                        }
                        AliasInfo item = new AliasInfo(collection2[0], collection2[1], base.Context, none);
                        if (!string.IsNullOrEmpty(collection2[2]))
                        {
                            item.Description = collection2[2];
                        }
                        collection.Add(item);
                    }
                }
                reader.Close();
            }
            return collection;
        }

        private static bool OnlyContainsWhitespace(string line)
        {
            foreach (char ch in line)
            {
                if ((!char.IsWhiteSpace(ch) || (ch == '\n')) || (ch == '\r'))
                {
                    return false;
                }
            }
            return true;
        }

        private StreamReader OpenFile(out string filePath, bool isLiteralPath)
        {
            StreamReader reader = null;
            filePath = null;
            ProviderInfo provider = null;
            Collection<string> resolvedProviderPathFromPSPath = null;
            if (isLiteralPath)
            {
                PSDriveInfo info2;
                resolvedProviderPathFromPSPath = new Collection<string> {
                    base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(this.Path, out provider, out info2)
                };
            }
            else
            {
                resolvedProviderPathFromPSPath = base.SessionState.Path.GetResolvedProviderPathFromPSPath(this.Path, out provider);
            }
            if (!provider.NameEquals(base.Context.ProviderNames.FileSystem))
            {
                throw PSTraceSource.NewNotSupportedException("AliasCommandStrings", "ImportAliasFromFileSystemOnly", new object[] { this.Path, provider.FullName });
            }
            if (resolvedProviderPathFromPSPath.Count != 1)
            {
                throw PSTraceSource.NewNotSupportedException("AliasCommandStrings", "ImportAliasPathResolvedToMultiple", new object[] { this.Path });
            }
            filePath = resolvedProviderPathFromPSPath[0];
            try
            {
                FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                reader = new StreamReader(stream);
            }
            catch (IOException exception)
            {
                this.ThrowFileOpenError(exception, filePath);
            }
            catch (SecurityException exception2)
            {
                this.ThrowFileOpenError(exception2, filePath);
            }
            catch (UnauthorizedAccessException exception3)
            {
                this.ThrowFileOpenError(exception3, filePath);
            }
            return reader;
        }

        protected override void ProcessRecord()
        {
            Collection<AliasInfo> aliasesFromFile = this.GetAliasesFromFile(base.ParameterSetName.Equals("ByLiteralPath", StringComparison.OrdinalIgnoreCase));
            CommandOrigin commandOrigin = base.MyInvocation.CommandOrigin;
            foreach (AliasInfo info in aliasesFromFile)
            {
                string importAliasAction = AliasCommandStrings.ImportAliasAction;
                string target = StringUtil.Format(AliasCommandStrings.ImportAliasTarget, info.Name, info.Definition);
                if (base.ShouldProcess(target, importAliasAction))
                {
                    if (this.Force == 0)
                    {
                        AliasInfo valueToCheck = null;
                        if (string.IsNullOrEmpty(this.Scope))
                        {
                            valueToCheck = base.SessionState.Internal.GetAlias(info.Name);
                        }
                        else
                        {
                            valueToCheck = base.SessionState.Internal.GetAliasAtScope(info.Name, this.Scope);
                        }
                        if (valueToCheck != null)
                        {
                            try
                            {
                                SessionState.ThrowIfNotVisible(commandOrigin, valueToCheck);
                            }
                            catch (SessionStateException exception)
                            {
                                base.WriteError(new ErrorRecord(exception.ErrorRecord, exception));
                                continue;
                            }
                            SessionStateException replaceParentContainsErrorRecordException = new SessionStateException(info.Name, SessionStateCategory.Alias, "AliasAlreadyExists", SessionStateStrings.AliasAlreadyExists, ErrorCategory.ResourceExists, new object[0]);
                            base.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
                            continue;
                        }
                        if (this.VerifyShadowingExistingCommandsAndWriteError(info.Name))
                        {
                            continue;
                        }
                    }
                    AliasInfo sendToPipeline = null;
                    try
                    {
                        if (string.IsNullOrEmpty(this.Scope))
                        {
                            sendToPipeline = base.SessionState.Internal.SetAliasItem(info, (bool) this.Force, base.MyInvocation.CommandOrigin);
                        }
                        else
                        {
                            sendToPipeline = base.SessionState.Internal.SetAliasItemAtScope(info, this.Scope, (bool) this.Force, base.MyInvocation.CommandOrigin);
                        }
                    }
                    catch (SessionStateException exception3)
                    {
                        base.WriteError(new ErrorRecord(exception3.ErrorRecord, exception3));
                        continue;
                    }
                    catch (PSArgumentOutOfRangeException exception4)
                    {
                        base.WriteError(new ErrorRecord(exception4.ErrorRecord, exception4));
                        continue;
                    }
                    catch (PSArgumentException exception5)
                    {
                        base.WriteError(new ErrorRecord(exception5.ErrorRecord, exception5));
                        continue;
                    }
                    if ((this.PassThru != 0) && (sendToPipeline != null))
                    {
                        base.WriteObject(sendToPipeline);
                    }
                }
            }
        }

        private void ThrowFileOpenError(Exception e, string pathWithError)
        {
            string message = StringUtil.Format(AliasCommandStrings.ImportAliasFileOpenFailed, pathWithError, e.Message);
            ErrorRecord errorRecord = new ErrorRecord(e, "FileOpenFailure", ErrorCategory.OpenError, pathWithError) {
                ErrorDetails = new ErrorDetails(message)
            };
            base.ThrowTerminatingError(errorRecord);
        }

        private bool VerifyShadowingExistingCommandsAndWriteError(string aliasName)
        {
            CommandSearcher searcher = new CommandSearcher(aliasName, SearchResolutionOptions.None, CommandTypes.Workflow | CommandTypes.Script | CommandTypes.Application | CommandTypes.ExternalScript | CommandTypes.Cmdlet | CommandTypes.Filter | CommandTypes.Function, base.Context);
            foreach (string str in searcher.ConstructSearchPatternsFromName(aliasName))
            {
                CommandTypes types;
                if (this.ExistingCommands.TryGetValue(str, out types))
                {
                    SessionStateException replaceParentContainsErrorRecordException = new SessionStateException(aliasName, SessionStateCategory.Alias, "AliasAlreadyExists", SessionStateStrings.AliasWithCommandNameAlreadyExists, ErrorCategory.ResourceExists, new object[] { types });
                    base.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
                    return true;
                }
            }
            return false;
        }

        private Dictionary<string, CommandTypes> ExistingCommands
        {
            get
            {
                if (this.existingCommands == null)
                {
                    this.existingCommands = new Dictionary<string, CommandTypes>(StringComparer.OrdinalIgnoreCase);
                    CommandSearcher searcher = new CommandSearcher("*", SearchResolutionOptions.CommandNameIsPattern | SearchResolutionOptions.ResolveFunctionPatterns | SearchResolutionOptions.ResolveAliasPatterns, CommandTypes.Workflow | CommandTypes.Script | CommandTypes.Application | CommandTypes.ExternalScript | CommandTypes.Cmdlet | CommandTypes.Filter | CommandTypes.Function, base.Context);
                    foreach (CommandInfo info in (IEnumerable<CommandInfo>) searcher)
                    {
                        this.existingCommands[info.Name] = info.CommandType;
                    }
                    foreach (CommandInfo info2 in ModuleUtils.GetMatchingCommands("*", base.Context, base.MyInvocation.CommandOrigin, false))
                    {
                        if (!this.existingCommands.ContainsKey(info2.Name))
                        {
                            this.existingCommands[info2.Name] = info2.CommandType;
                        }
                    }
                }
                return this.existingCommands;
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

        [Parameter(Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByLiteralPath"), Alias(new string[] { "PSPath" })]
        public string LiteralPath
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
            }
        }

        [Parameter]
        public SwitchParameter PassThru
        {
            get
            {
                return this.passThru;
            }
            set
            {
                this.passThru = (bool) value;
            }
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByPath")]
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter]
        public string Scope
        {
            get
            {
                return this.scope;
            }
            set
            {
                this.scope = value;
            }
        }
    }
}

