namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Help;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Tracing;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class UpdatableHelpCommandBase : PSCmdlet
    {
        internal UpdatableHelpCommandType _commandType;
        internal PSCredential _credential;
        internal bool _force;
        internal UpdatableHelpSystem _helpSystem;
        internal string[] _language;
        internal bool _stopping;
        internal bool _useDefaultCredentials;
        internal int activityId;
        private Dictionary<string, UpdatableHelpExceptionContext> exceptions;
        internal const string LiteralPathParameterSetName = "LiteralPath";
        private static Dictionary<string, string> metadataCache = new Dictionary<string, string>();
        internal const string PathParameterSetName = "Path";

        static UpdatableHelpCommandBase()
        {
            metadataCache.Add("Microsoft.PowerShell.Diagnostics", "http://go.microsoft.com/fwlink/?LinkID=210596");
            metadataCache.Add("Microsoft.PowerShell.Core", "http://go.microsoft.com/fwlink/?LinkID=210598");
            metadataCache.Add("Microsoft.PowerShell.Utility", "http://go.microsoft.com/fwlink/?LinkID=210599");
            metadataCache.Add("Microsoft.PowerShell.Host", "http://go.microsoft.com/fwlink/?LinkID=210600");
            metadataCache.Add("Microsoft.PowerShell.Management", "http://go.microsoft.com/fwlink/?LinkID=210601");
            metadataCache.Add("Microsoft.PowerShell.Security", "http://go.microsoft.com/fwlink/?LinkID=210602");
            metadataCache.Add("Microsoft.WSMan.Management", "http://go.microsoft.com/fwlink/?LinkID=210597");
        }

        internal UpdatableHelpCommandBase(UpdatableHelpCommandType commandType)
        {
            this._commandType = commandType;
            this._helpSystem = new UpdatableHelpSystem(this, this._useDefaultCredentials);
            this.exceptions = new Dictionary<string, UpdatableHelpExceptionContext>();
            this._helpSystem.OnProgressChanged += new EventHandler<UpdatableHelpProgressEventArgs>(this.HandleProgressChanged);
            this.activityId = new Random().Next();
        }

        internal bool CheckOncePerDayPerModule(string moduleName, string path, string filename, DateTime time, bool force)
        {
            if (force)
            {
                return true;
            }
            string str = base.SessionState.Path.Combine(path, filename);
            if (!System.IO.File.Exists(str))
            {
                return true;
            }
            DateTime lastWriteTimeUtc = System.IO.File.GetLastWriteTimeUtc(str);
            TimeSpan span = (TimeSpan) (time - lastWriteTimeUtc);
            if (span.Days >= 1)
            {
                return true;
            }
            if (this._commandType == UpdatableHelpCommandType.UpdateHelpCommand)
            {
                base.WriteVerbose(StringUtil.Format(HelpDisplayStrings.UseForceToUpdateHelp, moduleName));
            }
            else if (this._commandType == UpdatableHelpCommandType.SaveHelpCommand)
            {
                base.WriteVerbose(StringUtil.Format(HelpDisplayStrings.UseForceToSaveHelp, moduleName));
            }
            return false;
        }

        protected override void EndProcessing()
        {
            foreach (UpdatableHelpExceptionContext context in this.exceptions.Values)
            {
                UpdatableHelpExceptionContext context2 = context;
                if ((context.Exception.FullyQualifiedErrorId == "HelpCultureNotSupported") && (((context.Cultures != null) && (context.Cultures.Count > 1)) || ((context.Modules != null) && (context.Modules.Count > 1))))
                {
                    context2 = new UpdatableHelpExceptionContext(new UpdatableHelpSystemException("HelpCultureNotSupported", StringUtil.Format(HelpDisplayStrings.CannotMatchUICulturePattern, string.Join(", ", context.Cultures)), ErrorCategory.InvalidArgument, context.Cultures, null)) {
                        Modules = context.Modules,
                        Cultures = context.Cultures
                    };
                }
                base.WriteError(context2.CreateErrorRecord(this._commandType));
                LogContext logContext = MshLog.GetLogContext(base.Context, base.MyInvocation);
                logContext.Severity = "Error";
                PSEtwLog.LogOperationalError(PSEventId.Pipeline_Detail, PSOpcode.Exception, PSTask.ExecutePipeline, logContext, context2.GetExceptionMessage(this._commandType));
            }
        }

        internal Dictionary<string, UpdatableHelpModuleInfo> GetModuleInfo(string pattern, bool loaded, bool noErrors)
        {
            Dictionary<string, UpdatableHelpModuleInfo> dictionary = this.GetModuleInfo(base.Context, pattern, loaded, noErrors);
            if (((dictionary.Count == 0) && (this.exceptions.Count == 0)) && !noErrors)
            {
                ErrorRecord errorRecord = new ErrorRecord(new Exception(StringUtil.Format(HelpDisplayStrings.CannotMatchModulePattern, pattern)), "ModuleNotFound", ErrorCategory.InvalidArgument, pattern);
                base.WriteError(errorRecord);
            }
            return dictionary;
        }

        private Dictionary<string, UpdatableHelpModuleInfo> GetModuleInfo(System.Management.Automation.ExecutionContext context, string pattern, bool loaded, bool noErrors)
        {
            List<PSModuleInfo> modules = context.Modules.GetModules(new string[] { pattern }, false);
            Dictionary<string, UpdatableHelpModuleInfo> dictionary = new Dictionary<string, UpdatableHelpModuleInfo>();
            if (modules.Count != 0)
            {
                base.WriteDebug(StringUtil.Format("Found {0} loaded modules.", modules.Count));
                foreach (PSModuleInfo info in modules)
                {
                    if (InitialSessionState.IsEngineModule(info.Name) && !InitialSessionState.IsNestedEngineModule(info.Name))
                    {
                        base.WriteDebug(StringUtil.Format("Found engine module: {0}, {1}.", info.Name, info.Guid));
                        if (!dictionary.ContainsKey(info.Name))
                        {
                            dictionary.Add(info.Name, new UpdatableHelpModuleInfo(info.Name, info.Guid, Utils.GetApplicationBase(context.ShellID), metadataCache[info.Name]));
                        }
                    }
                    else if (!InitialSessionState.IsNestedEngineModule(info.Name))
                    {
                        if (string.IsNullOrEmpty(info.HelpInfoUri))
                        {
                            if (!noErrors)
                            {
                                this.ProcessException(info.Name, null, new UpdatableHelpSystemException("HelpInfoUriNotFound", StringUtil.Format(HelpDisplayStrings.HelpInfoUriNotFound, new object[0]), ErrorCategory.NotSpecified, new Uri("HelpInfoUri", UriKind.Relative), null));
                            }
                        }
                        else if (!info.HelpInfoUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!noErrors)
                            {
                                this.ProcessException(info.Name, null, new UpdatableHelpSystemException("InvalidHelpInfoUriFormat", StringUtil.Format(HelpDisplayStrings.InvalidHelpInfoUriFormat, info.HelpInfoUri), ErrorCategory.NotSpecified, new Uri("HelpInfoUri", UriKind.Relative), null));
                            }
                        }
                        else if (!dictionary.ContainsKey(info.Name))
                        {
                            dictionary.Add(info.Name, new UpdatableHelpModuleInfo(info.Name, info.Guid, info.ModuleBase, info.HelpInfoUri));
                        }
                    }
                }
            }
            WildcardOptions options = WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase;
            IEnumerable<WildcardPattern> patterns = SessionStateUtilities.CreateWildcardsFromStrings(new string[] { pattern }, options);
            if (!loaded)
            {
                Collection<PSObject> collection = base.InvokeCommand.InvokeScript("Get-Module -ListAvailable");
                Collection<PSModuleInfo> collection2 = new Collection<PSModuleInfo>();
                if (collection != null)
                {
                    foreach (PSObject obj2 in collection)
                    {
                        try
                        {
                            collection2.Add((PSModuleInfo) LanguagePrimitives.ConvertTo(obj2, typeof(PSModuleInfo), CultureInfo.InvariantCulture));
                        }
                        catch (PSInvalidCastException)
                        {
                        }
                    }
                }
                base.WriteDebug(StringUtil.Format("Found {0} available (Get-Module -ListAvailable) modules.", collection2.Count));
                foreach (PSModuleInfo info2 in collection2)
                {
                    if (SessionStateUtilities.MatchesAnyWildcardPattern(info2.Name, patterns, true))
                    {
                        if (InitialSessionState.IsEngineModule(info2.Name) && !InitialSessionState.IsNestedEngineModule(info2.Name))
                        {
                            base.WriteDebug(StringUtil.Format("Found engine module: {0}, {1}.", info2.Name, info2.Guid));
                            if (!dictionary.ContainsKey(info2.Name))
                            {
                                dictionary.Add(info2.Name, new UpdatableHelpModuleInfo(info2.Name, info2.Guid, Utils.GetApplicationBase(context.ShellID), metadataCache[info2.Name]));
                            }
                        }
                        else if (!InitialSessionState.IsNestedEngineModule(info2.Name))
                        {
                            if (string.IsNullOrEmpty(info2.HelpInfoUri))
                            {
                                if (!noErrors)
                                {
                                    this.ProcessException(info2.Name, null, new UpdatableHelpSystemException("HelpInfoUriNotFound", StringUtil.Format(HelpDisplayStrings.HelpInfoUriNotFound, new object[0]), ErrorCategory.NotSpecified, new Uri("HelpInfoUri", UriKind.Relative), null));
                                }
                            }
                            else if (!info2.HelpInfoUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!noErrors)
                                {
                                    this.ProcessException(info2.Name, null, new UpdatableHelpSystemException("InvalidHelpInfoUriFormat", StringUtil.Format(HelpDisplayStrings.InvalidHelpInfoUriFormat, info2.HelpInfoUri), ErrorCategory.NotSpecified, new Uri("HelpInfoUri", UriKind.Relative), null));
                                }
                            }
                            else if (!dictionary.ContainsKey(info2.Name))
                            {
                                dictionary.Add(info2.Name, new UpdatableHelpModuleInfo(info2.Name, info2.Guid, info2.ModuleBase, info2.HelpInfoUri));
                            }
                        }
                    }
                }
            }
            foreach (KeyValuePair<string, string> pair in metadataCache)
            {
                if (SessionStateUtilities.MatchesAnyWildcardPattern(pair.Key, patterns, true))
                {
                    if (!pair.Key.Equals(InitialSessionState.CoreSnapin, StringComparison.OrdinalIgnoreCase))
                    {
                        Collection<PSObject> collection3 = base.InvokeCommand.InvokeScript(StringUtil.Format("Get-Module {0} -ListAvailable", pair.Key));
                        Collection<PSModuleInfo> collection4 = new Collection<PSModuleInfo>();
                        if (collection3 != null)
                        {
                            foreach (PSObject obj3 in collection3)
                            {
                                try
                                {
                                    collection4.Add((PSModuleInfo) LanguagePrimitives.ConvertTo(obj3, typeof(PSModuleInfo), CultureInfo.InvariantCulture));
                                }
                                catch (PSInvalidCastException)
                                {
                                }
                            }
                        }
                        foreach (PSModuleInfo info3 in collection4)
                        {
                            if (!dictionary.ContainsKey(info3.Name))
                            {
                                base.WriteDebug(StringUtil.Format("Found engine module: {0}, {1}.", info3.Name, info3.Guid));
                                dictionary.Add(pair.Key, new UpdatableHelpModuleInfo(info3.Name, info3.Guid, Utils.GetApplicationBase(context.ShellID), metadataCache[info3.Name]));
                            }
                        }
                    }
                    else if (!dictionary.ContainsKey(pair.Key))
                    {
                        dictionary.Add(pair.Key, new UpdatableHelpModuleInfo(pair.Key, Guid.Empty, Utils.GetApplicationBase(context.ShellID), pair.Value));
                    }
                }
            }
            return dictionary;
        }

        private void HandleProgressChanged(object sender, UpdatableHelpProgressEventArgs e)
        {
            string formatSpec = (e.CommandType == UpdatableHelpCommandType.UpdateHelpCommand) ? HelpDisplayStrings.UpdateProgressActivityForModule : HelpDisplayStrings.SaveProgressActivityForModule;
            ProgressRecord progressRecord = new ProgressRecord(this.activityId, StringUtil.Format(formatSpec, e.ModuleName), e.ProgressStatus) {
                PercentComplete = e.ProgressPercent
            };
            base.WriteProgress(progressRecord);
        }

        internal static bool IsSystemModule(string module)
        {
            return metadataCache.ContainsKey(module);
        }

        internal bool IsUpdateNecessary(UpdatableHelpModuleInfo module, UpdatableHelpInfo currentHelpInfo, UpdatableHelpInfo newHelpInfo, CultureInfo culture, bool force)
        {
            if (newHelpInfo == null)
            {
                throw new UpdatableHelpSystemException("UnableToRetrieveHelpInfoXml", StringUtil.Format(HelpDisplayStrings.UnableToRetrieveHelpInfoXml, culture.Name), ErrorCategory.ResourceUnavailable, null, null);
            }
            if (!newHelpInfo.IsCultureSupported(culture))
            {
                throw new UpdatableHelpSystemException("HelpCultureNotSupported", StringUtil.Format(HelpDisplayStrings.HelpCultureNotSupported, culture.Name, newHelpInfo.GetSupportedCultures()), ErrorCategory.InvalidOperation, null, null);
            }
            if ((!force && (currentHelpInfo != null)) && !currentHelpInfo.IsNewerVersion(newHelpInfo, culture))
            {
                return false;
            }
            return true;
        }

        internal void LogMessage(string message)
        {
            List<string> pipelineExecutionDetail = new List<string> {
                message
            };
            PSEtwLog.LogPipelineExecutionDetailEvent(MshLog.GetLogContext(base.Context, base.Context.CurrentCommandProcessor.Command.MyInvocation), pipelineExecutionDetail);
        }

        internal void Process(string[] modules)
        {
            this._helpSystem.WebClient.UseDefaultCredentials = this._useDefaultCredentials;
            if (modules != null)
            {
                foreach (string str in modules)
                {
                    if (this._stopping)
                    {
                        return;
                    }
                    this.ProcessModuleWithGlobbing(str);
                }
            }
            else
            {
                foreach (KeyValuePair<string, UpdatableHelpModuleInfo> pair in this.GetModuleInfo("*", false, true))
                {
                    if (this._stopping)
                    {
                        break;
                    }
                    this.ProcessModule(pair.Value);
                }
            }
        }

        internal void ProcessException(string moduleName, string culture, Exception e)
        {
            UpdatableHelpSystemException exception = null;
            if (e is UpdatableHelpSystemException)
            {
                exception = (UpdatableHelpSystemException) e;
            }
            else if (e is WebException)
            {
                exception = new UpdatableHelpSystemException("UnableToConnect", StringUtil.Format(HelpDisplayStrings.UnableToConnect, new object[0]), ErrorCategory.InvalidOperation, null, e);
            }
            else if (e is PSArgumentException)
            {
                exception = new UpdatableHelpSystemException("InvalidArgument", e.Message, ErrorCategory.InvalidArgument, null, e);
            }
            else
            {
                exception = new UpdatableHelpSystemException("UnknownErrorId", e.Message, ErrorCategory.InvalidOperation, null, e);
            }
            if (!this.exceptions.ContainsKey(exception.FullyQualifiedErrorId))
            {
                this.exceptions.Add(exception.FullyQualifiedErrorId, new UpdatableHelpExceptionContext(exception));
            }
            this.exceptions[exception.FullyQualifiedErrorId].Modules.Add(moduleName);
            if (culture != null)
            {
                this.exceptions[exception.FullyQualifiedErrorId].Cultures.Add(culture);
            }
        }

        private void ProcessModule(UpdatableHelpModuleInfo module)
        {
            this._helpSystem.CurrentModule = module.ModuleName;
            if (!Directory.Exists(module.ModuleBase))
            {
                this.ProcessException(module.ModuleName, null, new UpdatableHelpSystemException("ModuleBaseMustExist", StringUtil.Format(HelpDisplayStrings.ModuleBaseMustExist, new object[0]), ErrorCategory.InvalidOperation, null, null));
            }
            else
            {
                IEnumerable<string> currentUICulture;
                if (this._language == null)
                {
                    currentUICulture = this._helpSystem.GetCurrentUICulture();
                }
                else
                {
                    currentUICulture = this._language;
                }
                foreach (string str in currentUICulture)
                {
                    bool flag = true;
                    if (this._stopping)
                    {
                        break;
                    }
                    try
                    {
                        this.ProcessModuleWithCulture(module, str);
                    }
                    catch (IOException exception)
                    {
                        this.ProcessException(module.ModuleName, str, new UpdatableHelpSystemException("FailedToCopyFile", exception.Message, ErrorCategory.InvalidOperation, null, exception));
                    }
                    catch (UnauthorizedAccessException exception2)
                    {
                        this.ProcessException(module.ModuleName, str, new UpdatableHelpSystemException("AccessIsDenied", exception2.Message, ErrorCategory.PermissionDenied, null, exception2));
                    }
                    catch (WebException exception3)
                    {
                        if ((exception3.InnerException != null) && (exception3.InnerException is UnauthorizedAccessException))
                        {
                            this.ProcessException(module.ModuleName, str, new UpdatableHelpSystemException("AccessIsDenied", exception3.InnerException.Message, ErrorCategory.PermissionDenied, null, exception3));
                        }
                        else
                        {
                            this.ProcessException(module.ModuleName, str, exception3);
                        }
                    }
                    catch (UpdatableHelpSystemException exception4)
                    {
                        if (exception4.FullyQualifiedErrorId == "HelpCultureNotSupported")
                        {
                            flag = false;
                            if (this._language != null)
                            {
                                this.ProcessException(module.ModuleName, str, exception4);
                            }
                        }
                        else
                        {
                            this.ProcessException(module.ModuleName, str, exception4);
                        }
                    }
                    catch (Exception exception5)
                    {
                        this.ProcessException(module.ModuleName, str, exception5);
                    }
                    finally
                    {
                        if (this._helpSystem.Errors.Count != 0)
                        {
                            foreach (Exception exception6 in this._helpSystem.Errors)
                            {
                                this.ProcessException(module.ModuleName, str, exception6);
                            }
                            this._helpSystem.Errors.Clear();
                        }
                    }
                    if ((this._language == null) && flag)
                    {
                        break;
                    }
                }
            }
        }

        internal virtual bool ProcessModuleWithCulture(UpdatableHelpModuleInfo module, string culture)
        {
            return false;
        }

        private void ProcessModuleWithGlobbing(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                PSArgumentException exception = new PSArgumentException(StringUtil.Format(HelpDisplayStrings.ModuleNameNullOrEmpty, new object[0]));
                base.WriteError(exception.ErrorRecord);
            }
            else
            {
                foreach (KeyValuePair<string, UpdatableHelpModuleInfo> pair in this.GetModuleInfo(name, false, false))
                {
                    this.ProcessModule(pair.Value);
                }
            }
        }

        private IEnumerable<string> RecursiveResolvePathHelper(string path)
        {
            if (Directory.Exists(path))
            {
                yield return path;
                foreach (string iteratorVariable0 in Directory.GetDirectories(path))
                {
                    foreach (string iteratorVariable1 in this.RecursiveResolvePathHelper(iteratorVariable0))
                    {
                        yield return iteratorVariable1;
                    }
                }
            }
        }

        internal IEnumerable<string> ResolvePath(string path, bool recurse, bool isLiteralPath)
        {
            List<string> iteratorVariable0 = new List<string>();
            if (isLiteralPath)
            {
                string unresolvedProviderPathFromPSPath = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);
                if (!Directory.Exists(unresolvedProviderPathFromPSPath))
                {
                    throw new UpdatableHelpSystemException("PathMustBeValidContainers", StringUtil.Format(HelpDisplayStrings.PathMustBeValidContainers, path), ErrorCategory.InvalidArgument, null, new ItemNotFoundException());
                }
                iteratorVariable0.Add(unresolvedProviderPathFromPSPath);
            }
            else
            {
                foreach (PathInfo info in this.SessionState.Path.GetResolvedPSPathFromPSPath(path))
                {
                    this.ValidatePathProvider(info);
                    iteratorVariable0.Add(info.ProviderPath);
                }
            }
            List<string>.Enumerator enumerator = iteratorVariable0.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string current = enumerator.Current;
                if (recurse)
                {
                    foreach (string iteratorVariable2 in this.RecursiveResolvePathHelper(current))
                    {
                        yield return iteratorVariable2;
                    }
                    continue;
                }
                CmdletProviderContext context = new CmdletProviderContext(this.Context) {
                    SuppressWildcardExpansion = true
                };
                if (isLiteralPath || this.InvokeProvider.Item.IsContainer(current, context))
                {
                    yield return current;
                }
            }
        }

        protected override void StopProcessing()
        {
            this._stopping = true;
            this._helpSystem.CancelDownload();
        }

        internal void ValidatePathProvider(PathInfo path)
        {
            if ((path.Provider == null) || (path.Provider.Name != "FileSystem"))
            {
                throw new PSArgumentException(StringUtil.Format(HelpDisplayStrings.ProviderIsNotFileSystem, path.Path));
            }
        }

        [Parameter, Credential]
        public PSCredential Credential
        {
            get
            {
                return this._credential;
            }
            set
            {
                this._credential = value;
            }
        }

        [Parameter]
        public SwitchParameter Force
        {
            get
            {
                return this._force;
            }
            set
            {
                this._force = (bool) value;
            }
        }

        [Parameter(Position=2), ValidateNotNull]
        public CultureInfo[] UICulture
        {
            get
            {
                CultureInfo[] infoArray = null;
                if (this._language != null)
                {
                    infoArray = new CultureInfo[this._language.Length];
                    for (int i = 0; i < this._language.Length; i++)
                    {
                        infoArray[i] = new CultureInfo(this._language[i]);
                    }
                }
                return infoArray;
            }
            set
            {
                if (value != null)
                {
                    this._language = new string[value.Length];
                    for (int i = 0; i < value.Length; i++)
                    {
                        this._language[i] = value[i].Name;
                    }
                }
            }
        }

        [Parameter]
        public SwitchParameter UseDefaultCredentials
        {
            get
            {
                return this._useDefaultCredentials;
            }
            set
            {
                this._useDefaultCredentials = (bool) value;
            }
        }

        
    }
}

