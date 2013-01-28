namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class HelpSystem
    {
        private CultureInfo _culture;
        private System.Management.Automation.ExecutionContext _executionContext;
        private System.Management.Automation.HelpErrorTracer _helpErrorTracer;
        private ArrayList _helpProviders = new ArrayList();
        private Collection<ErrorRecord> _lastErrors = new Collection<ErrorRecord>();
        private HelpCategory _lastHelpCategory;
        private Collection<string> _searchPaths;
        private bool _verboseHelpErrors;
        private readonly Lazy<Dictionary<Ast, Token[]>> scriptBlockTokenCache = new Lazy<Dictionary<Ast, Token[]>>(true);

        internal event HelpProgressHandler OnProgress;

        internal HelpSystem(System.Management.Automation.ExecutionContext context)
        {
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("ExecutionContext");
            }
            this._executionContext = context;
            this.Initialize();
        }

        internal void ClearScriptBlockTokenCache()
        {
            if (this.scriptBlockTokenCache.IsValueCreated)
            {
                this.scriptBlockTokenCache.Value.Clear();
            }
        }

        private IEnumerable<HelpInfo> DoGetHelp(HelpRequest helpRequest)
        {
            this._lastErrors.Clear();
            this._searchPaths = null;
            this._lastHelpCategory = helpRequest.HelpCategory;
            if (string.IsNullOrEmpty(helpRequest.Target))
            {
                HelpInfo defaultHelp = this.GetDefaultHelp();
                if (defaultHelp != null)
                {
                    yield return defaultHelp;
                }
                yield return null;
            }
            else
            {
                bool iteratorVariable1 = false;
                if (!WildcardPattern.ContainsWildcardCharacters(helpRequest.Target))
                {
                    foreach (HelpInfo iteratorVariable2 in this.ExactMatchHelp(helpRequest))
                    {
                        iteratorVariable1 = true;
                        yield return iteratorVariable2;
                    }
                }
                if (!iteratorVariable1)
                {
                    foreach (HelpInfo iteratorVariable3 in this.SearchHelp(helpRequest))
                    {
                        iteratorVariable1 = true;
                        yield return iteratorVariable3;
                    }
                    if ((!iteratorVariable1 && !WildcardPattern.ContainsWildcardCharacters(helpRequest.Target)) && (this.LastErrors.Count == 0))
                    {
                        Exception exception = new HelpNotFoundException(helpRequest.Target);
                        ErrorRecord item = new ErrorRecord(exception, "HelpNotFound", ErrorCategory.ResourceUnavailable, null);
                        this.LastErrors.Add(item);
                    }
                }
            }
        }

        internal IEnumerable<HelpInfo> ExactMatchHelp(HelpRequest helpRequest)
        {
            bool iteratorVariable0 = false;
            for (int i = 0; i < this.HelpProviders.Count; i++)
            {
                HelpProvider iteratorVariable2 = (HelpProvider) this.HelpProviders[i];
                if ((iteratorVariable2.HelpCategory & helpRequest.HelpCategory) > HelpCategory.None)
                {
                    foreach (HelpInfo iteratorVariable3 in iteratorVariable2.ExactMatchHelp(helpRequest))
                    {
                        iteratorVariable0 = true;
                        foreach (HelpInfo iteratorVariable4 in this.ForwardHelp(iteratorVariable3, helpRequest))
                        {
                            yield return iteratorVariable4;
                        }
                    }
                }
                if (iteratorVariable0 && !(iteratorVariable2 is ScriptCommandHelpProvider))
                {
                    break;
                }
            }
        }

        private IEnumerable<HelpInfo> ForwardHelp(HelpInfo helpInfo, HelpRequest helpRequest)
        {
            new Collection<HelpInfo>();
            if ((helpInfo.ForwardHelpCategory == HelpCategory.None) && string.IsNullOrEmpty(helpInfo.ForwardTarget))
            {
                yield return helpInfo;
            }
            else
            {
                HelpCategory forwardHelpCategory = helpInfo.ForwardHelpCategory;
                bool iteratorVariable1 = false;
                for (int i = 0; i < this.HelpProviders.Count; i++)
                {
                    HelpProvider iteratorVariable3 = (HelpProvider) this.HelpProviders[i];
                    if ((iteratorVariable3.HelpCategory & forwardHelpCategory) != HelpCategory.None)
                    {
                        iteratorVariable1 = true;
                        foreach (HelpInfo iteratorVariable4 in iteratorVariable3.ProcessForwardedHelp(helpInfo, helpRequest))
                        {
                            foreach (HelpInfo iteratorVariable5 in this.ForwardHelp(iteratorVariable4, helpRequest))
                            {
                                yield return iteratorVariable5;
                            }
                            goto Label_01FB;
                        }
                    }
                }
                if (!iteratorVariable1)
                {
                    yield return helpInfo;
                }
            }
        Label_01FB:
            yield break;
        }

        private HelpInfo GetDefaultHelp()
        {
            HelpRequest helpRequest = new HelpRequest("default", HelpCategory.DefaultHelp);
            using (IEnumerator<HelpInfo> enumerator = this.ExactMatchHelp(helpRequest).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }
            return null;
        }

        internal IEnumerable<HelpInfo> GetHelp(HelpRequest helpRequest)
        {
            if (helpRequest == null)
            {
                return null;
            }
            helpRequest.Validate();
            this.ValidateHelpCulture();
            return this.DoGetHelp(helpRequest);
        }

        internal Collection<string> GetSearchPaths()
        {
            if (this._searchPaths == null)
            {
                this._searchPaths = new Collection<string>();
                RunspaceConfigForSingleShell runspaceConfiguration = this.ExecutionContext.RunspaceConfiguration as RunspaceConfigForSingleShell;
                if (runspaceConfiguration != null)
                {
                    MshConsoleInfo consoleInfo = runspaceConfiguration.ConsoleInfo;
                    if ((consoleInfo == null) || (consoleInfo.ExternalPSSnapIns == null))
                    {
                        return this._searchPaths;
                    }
                    foreach (PSSnapInInfo info2 in consoleInfo.ExternalPSSnapIns)
                    {
                        this._searchPaths.Add(info2.ApplicationBase);
                    }
                }
                if (this.ExecutionContext.Modules != null)
                {
                    foreach (PSModuleInfo info3 in this.ExecutionContext.Modules.ModuleTable.Values)
                    {
                        this._searchPaths.Add(info3.ModuleBase);
                    }
                }
            }
            return this._searchPaths;
        }

        internal void Initialize()
        {
            this._verboseHelpErrors = LanguagePrimitives.IsTrue(this._executionContext.GetVariableValue(SpecialVariables.VerboseHelpErrorsVarPath, false));
            this._helpErrorTracer = new System.Management.Automation.HelpErrorTracer(this);
            this.InitializeHelpProviders();
        }

        private void InitializeHelpProviders()
        {
            HelpProvider provider = null;
            provider = new AliasHelpProvider(this);
            this._helpProviders.Add(provider);
            provider = new ScriptCommandHelpProvider(this);
            this._helpProviders.Add(provider);
            provider = new CommandHelpProvider(this);
            this._helpProviders.Add(provider);
            provider = new ProviderHelpProvider(this);
            this._helpProviders.Add(provider);
            provider = new HelpFileHelpProvider(this);
            this._helpProviders.Add(provider);
            provider = new FaqHelpProvider(this);
            this._helpProviders.Add(provider);
            provider = new GlossaryHelpProvider(this);
            this._helpProviders.Add(provider);
            provider = new GeneralHelpProvider(this);
            this._helpProviders.Add(provider);
            provider = new DefaultHelpProvider(this);
            this._helpProviders.Add(provider);
        }

        internal void ResetHelpProviders()
        {
            if (this._helpProviders != null)
            {
                for (int i = 0; i < this._helpProviders.Count; i++)
                {
                    ((HelpProvider) this._helpProviders[i]).Reset();
                }
            }
        }

        private IEnumerable<HelpInfo> SearchHelp(HelpRequest helpRequest)
        {
            int iteratorVariable0 = 0;
            bool searchOnlyContent = false;
            bool iteratorVariable2 = false;
            HelpProgressInfo arg = new HelpProgressInfo {
                Activity = StringUtil.Format(HelpDisplayStrings.SearchingForHelpContent, helpRequest.Target),
                Completed = false,
                PercentComplete = 0
            };
            this.OnProgress(this, arg);
            do
            {
                if (searchOnlyContent)
                {
                    iteratorVariable2 = true;
                }
                for (int i = 0; i < this.HelpProviders.Count; i++)
                {
                    HelpProvider iteratorVariable5 = (HelpProvider) this.HelpProviders[i];
                    if ((iteratorVariable5.HelpCategory & helpRequest.HelpCategory) > HelpCategory.None)
                    {
                        foreach (HelpInfo iteratorVariable6 in iteratorVariable5.SearchHelp(helpRequest, searchOnlyContent))
                        {
                            if (this._executionContext.CurrentPipelineStopping)
                            {
                                break;
                            }
                            iteratorVariable0++;
                            yield return iteratorVariable6;
                            if ((iteratorVariable0 >= helpRequest.MaxResults) && (helpRequest.MaxResults > 0))
                            {
                                break;
                            }
                        }
                    }
                }
                if (iteratorVariable0 > 0)
                {
                    break;
                }
                searchOnlyContent = true;
                if (this.HelpProviders.Count > 0)
                {
                    arg.PercentComplete += 100 / this.HelpProviders.Count;
                    this.OnProgress(this, arg);
                }
            }
            while (!iteratorVariable2);
        }

        internal IDisposable Trace(string helpFile)
        {
            if (this._helpErrorTracer == null)
            {
                return null;
            }
            return this._helpErrorTracer.Trace(helpFile);
        }

        internal void TraceError(ErrorRecord errorRecord)
        {
            if (this._helpErrorTracer != null)
            {
                this._helpErrorTracer.TraceError(errorRecord);
            }
        }

        internal void TraceErrors(Collection<ErrorRecord> errorRecords)
        {
            if ((this._helpErrorTracer != null) && (errorRecords != null))
            {
                this._helpErrorTracer.TraceErrors(errorRecords);
            }
        }

        private void ValidateHelpCulture()
        {
            CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
            if (this._culture == null)
            {
                this._culture = currentUICulture;
            }
            else if (!this._culture.Equals(currentUICulture))
            {
                this._culture = currentUICulture;
                this.ResetHelpProviders();
            }
        }

        internal System.Management.Automation.ExecutionContext ExecutionContext
        {
            get
            {
                return this._executionContext;
            }
        }

        internal System.Management.Automation.HelpErrorTracer HelpErrorTracer
        {
            get
            {
                return this._helpErrorTracer;
            }
        }

        internal ArrayList HelpProviders
        {
            get
            {
                return this._helpProviders;
            }
        }

        internal Collection<ErrorRecord> LastErrors
        {
            get
            {
                return this._lastErrors;
            }
        }

        internal HelpCategory LastHelpCategory
        {
            get
            {
                return this._lastHelpCategory;
            }
        }

        internal Dictionary<Ast, Token[]> ScriptBlockTokenCache
        {
            get
            {
                return this.scriptBlockTokenCache.Value;
            }
        }

        internal bool VerboseHelpErrors
        {
            get
            {
                return this._verboseHelpErrors;
            }
        }

        

        internal delegate void HelpProgressHandler(object sender, HelpProgressInfo arg);
    }
}

