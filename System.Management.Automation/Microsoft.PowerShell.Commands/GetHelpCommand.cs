namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Help;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;

    [Cmdlet("Get", "Help", DefaultParameterSetName="AllUsersView", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113316")]
    public sealed class GetHelpCommand : PSCmdlet
    {
        private string[] _category;
        private string[] _component;
        private string[] _functionality;
        private string _name = "";
        private string _parameter;
        private string _path;
        private string _provider = "";
        private string[] _role;
        private HelpView _viewTokenToAdd;
        private GraphicalHostReflectionWrapper graphicalHostReflectionWrapper;
        internal const string resBaseName = "HelpErrors";
        private bool showOnlineHelp;
        private bool showWindow;
        [TraceSource("GetHelpCommand ", "GetHelpCommand ")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("GetHelpCommand ", "GetHelpCommand ");

        protected override void BeginProcessing()
        {
            if ((!this.Online.IsPresent && UpdatableHelpSystem.ShouldPromptToUpdateHelp()) && (HostUtilities.IsProcessInteractive(base.MyInvocation) && this.HasInternetConnection()))
            {
                if (base.ShouldContinue(HelpDisplayStrings.UpdateHelpPromptBody, HelpDisplayStrings.UpdateHelpPromptTitle))
                {
                    PowerShell.Create(RunspaceMode.CurrentRunspace).AddCommand("Update-Help").Invoke();
                }
                UpdatableHelpSystem.SetDisablePromptToUpdateHelp();
            }
        }

        private void GetAndWriteParameterInfo(HelpInfo helpInfo)
        {
            tracer.WriteLine("Searching parameters for {0}", new object[] { helpInfo.Name });
            PSObject[] parameter = helpInfo.GetParameter(this._parameter);
            if ((parameter == null) || (parameter.Length == 0))
            {
                Exception exception = PSTraceSource.NewArgumentException("Parameter", "HelpErrors", "NoParmsFound", new object[] { this._parameter });
                base.WriteError(new ErrorRecord(exception, "NoParmsFound", ErrorCategory.InvalidArgument, helpInfo));
            }
            else
            {
                foreach (PSObject obj2 in parameter)
                {
                    base.WriteObject(obj2);
                }
            }
        }

        private bool HasInternetConnection()
        {
            int num;
			if (OSHelper.IsUnix) return true; /* Assume Internet for Unix */
            return InternetGetConnectedState(out num, 0);
        }

        private void HelpSystem_OnProgress(object sender, HelpProgressInfo arg)
        {
            ProgressRecord progressRecord = new ProgressRecord(0, base.CommandInfo.Name, arg.Activity) {
                PercentComplete = arg.PercentComplete
            };
            base.WriteProgress(progressRecord);
        }

		/* Windows Only TODO: Isolate */
        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int desc, int reserved);

        private void LaunchOnlineHelp(Uri uriToLaunch)
        {
            if (!uriToLaunch.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) && !uriToLaunch.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                throw PSTraceSource.NewInvalidOperationException("HelpErrors", "ProtocolNotSupported", new object[] { uriToLaunch.ToString(), "http", "https" });
            }
            Exception innerException = null;
            try
            {
                new Process { StartInfo = { UseShellExecute = true, FileName = uriToLaunch.OriginalString } }.Start();
            }
            catch (InvalidOperationException exception2)
            {
                innerException = exception2;
            }
            catch (Win32Exception exception3)
            {
                innerException = exception3;
            }
            if (innerException != null)
            {
                throw PSTraceSource.NewInvalidOperationException(innerException, "HelpErrors", "CannotLaunchURI", new object[] { uriToLaunch.OriginalString });
            }
        }

        protected override void ProcessRecord()
        {
            try
            {
                if (this.ShowWindow != 0)
                {
                    this.graphicalHostReflectionWrapper = GraphicalHostReflectionWrapper.GetGraphicalHostReflectionWrapper(this, "Microsoft.PowerShell.Commands.Internal.HelpWindowHelper");
                }
                base.Context.HelpSystem.OnProgress += new HelpSystem.HelpProgressHandler(this.HelpSystem_OnProgress);
                bool failed = false;
                HelpCategory cat = this.ToHelpCategory(this._category, ref failed);
                if (!failed)
                {
                    this.ValidateAndThrowIfError(cat);
                    HelpRequest helpRequest = new HelpRequest(this.Name, cat) {
                        Provider = this._provider,
                        Component = this._component,
                        Role = this._role,
                        Functionality = this._functionality,
                        ProviderContext = new ProviderContext(this.Path, base.Context.Engine.Context, base.SessionState.Path),
                        CommandOrigin = base.MyInvocation.CommandOrigin
                    };
                    IEnumerable<HelpInfo> help = base.Context.HelpSystem.GetHelp(helpRequest);
                    HelpInfo helpInfo = null;
                    int num = 0;
                    foreach (HelpInfo info2 in help)
                    {
                        if (base.IsStopping)
                        {
                            return;
                        }
                        if (num == 0)
                        {
                            helpInfo = info2;
                        }
                        else
                        {
                            if (helpInfo != null)
                            {
                                this.WriteObjectsOrShowOnlineHelp(helpInfo, false);
                                helpInfo = null;
                            }
                            this.WriteObjectsOrShowOnlineHelp(info2, false);
                        }
                        num++;
                    }
                    if (1 == num)
                    {
                        this.WriteObjectsOrShowOnlineHelp(helpInfo, true);
                    }
                    else if (this.showOnlineHelp && (num > 1))
                    {
                        throw PSTraceSource.NewInvalidOperationException("HelpErrors", "MultipleOnlineTopicsNotSupported", new object[] { "Online" });
                    }
                    if ((((num == 0) && !WildcardPattern.ContainsWildcardCharacters(helpRequest.Target)) || base.Context.HelpSystem.VerboseHelpErrors) && (base.Context.HelpSystem.LastErrors.Count > 0))
                    {
                        foreach (ErrorRecord record in base.Context.HelpSystem.LastErrors)
                        {
                            base.WriteError(record);
                        }
                    }
                }
            }
            finally
            {
                base.Context.HelpSystem.OnProgress -= new HelpSystem.HelpProgressHandler(this.HelpSystem_OnProgress);
                base.Context.HelpSystem.ClearScriptBlockTokenCache();
            }
        }

        private HelpCategory ToHelpCategory(string[] category, ref bool failed)
        {
            if ((category == null) || (category.Length == 0))
            {
                return HelpCategory.None;
            }
            HelpCategory none = HelpCategory.None;
            failed = false;
            for (int i = 0; i < category.Length; i++)
            {
                try
                {
                    HelpCategory category3 = (HelpCategory) Enum.Parse(typeof(HelpCategory), category[i], true);
                    none |= category3;
                }
                catch (ArgumentException exception)
                {
                    Exception exception2 = new HelpCategoryInvalidException(category[i], exception);
                    ErrorRecord errorRecord = new ErrorRecord(exception2, "InvalidHelpCategory", ErrorCategory.InvalidArgument, null);
                    base.WriteError(errorRecord);
                    failed = true;
                }
            }
            return none;
        }

        private PSObject TransformView(PSObject originalHelpObject)
        {
            if (this._viewTokenToAdd == HelpView.Default)
            {
                tracer.WriteLine("Detailed, Full, Examples are not selected. Constructing default view.", new object[0]);
                return originalHelpObject;
            }
            string str = this._viewTokenToAdd.ToString();
            PSObject obj2 = originalHelpObject.Copy();
            obj2.TypeNames.Clear();
            if (originalHelpObject.TypeNames.Count == 0)
            {
                string item = string.Format(CultureInfo.InvariantCulture, "HelpInfo#{0}", new object[] { str });
                obj2.TypeNames.Add(item);
                return obj2;
            }
            foreach (string str3 in originalHelpObject.TypeNames)
            {
                if (!str3.ToLower(CultureInfo.InvariantCulture).Equals("system.string") && !str3.ToLower(CultureInfo.InvariantCulture).Equals("system.object"))
                {
                    string str4 = string.Format(CultureInfo.InvariantCulture, "{0}#{1}", new object[] { str3, str });
                    tracer.WriteLine("Adding type {0}", new object[] { str4 });
                    obj2.TypeNames.Add(str4);
                }
            }
            foreach (string str5 in originalHelpObject.TypeNames)
            {
                tracer.WriteLine("Adding type {0}", new object[] { str5 });
                obj2.TypeNames.Add(str5);
            }
            return obj2;
        }

        private void ValidateAndThrowIfError(HelpCategory cat)
        {
            if (cat != HelpCategory.None)
            {
                HelpCategory category = HelpCategory.Workflow | HelpCategory.ExternalScript | HelpCategory.Filter | HelpCategory.Function | HelpCategory.ScriptCommand | HelpCategory.Cmdlet | HelpCategory.Alias;
                if ((cat & category) == HelpCategory.None)
                {
                    if (!string.IsNullOrEmpty(this._parameter))
                    {
                        throw PSTraceSource.NewArgumentException("Parameter", "HelpErrors", "ParamNotSupported", new object[] { "-Parameter" });
                    }
                    if (this._component != null)
                    {
                        throw PSTraceSource.NewArgumentException("Component", "HelpErrors", "ParamNotSupported", new object[] { "-Component" });
                    }
                    if (this._role != null)
                    {
                        throw PSTraceSource.NewArgumentException("Role", "HelpErrors", "ParamNotSupported", new object[] { "-Role" });
                    }
                    if (this._functionality != null)
                    {
                        throw PSTraceSource.NewArgumentException("Functionality", "HelpErrors", "ParamNotSupported", new object[] { "-Functionality" });
                    }
                }
            }
        }

        internal static void VerifyParameterForbiddenInRemoteRunspace(Cmdlet cmdlet, string parameterName)
        {
            if (NativeCommandProcessor.IsServerSide)
            {
                Exception exception = new InvalidOperationException(StringUtil.Format(CommandBaseStrings.ParameterNotValidInRemoteRunspace, cmdlet.MyInvocation.InvocationName, parameterName));
                ErrorRecord errorRecord = new ErrorRecord(exception, "ParameterNotValidInRemoteRunspace", ErrorCategory.InvalidArgument, null);
                cmdlet.ThrowTerminatingError(errorRecord);
            }
        }

        private void WriteObjectsOrShowOnlineHelp(HelpInfo helpInfo, bool showFullHelp)
        {
            if (helpInfo != null)
            {
                if (showFullHelp && this.showOnlineHelp)
                {
                    bool flag = false;
                    tracer.WriteLine("Preparing to show help online.", new object[0]);
                    Uri uriForOnlineHelp = helpInfo.GetUriForOnlineHelp();
                    if (null != uriForOnlineHelp)
                    {
                        flag = true;
                        this.LaunchOnlineHelp(uriForOnlineHelp);
                    }
                    else if (!flag)
                    {
                        throw PSTraceSource.NewInvalidOperationException("HelpErrors", "NoURIFound", new object[0]);
                    }
                }
                else if (showFullHelp && (this.ShowWindow != 0))
                {
                    this.graphicalHostReflectionWrapper.CallStaticMethod("ShowHelpWindow", new object[] { helpInfo.FullHelp, this });
                }
                else if (showFullHelp)
                {
                    if (!string.IsNullOrEmpty(this._parameter))
                    {
                        this.GetAndWriteParameterInfo(helpInfo);
                    }
                    else
                    {
                        PSObject sendToPipeline = this.TransformView(helpInfo.FullHelp);
                        sendToPipeline.IsHelpObject = true;
                        base.WriteObject(sendToPipeline);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(this._parameter))
                    {
                        PSObject[] parameter = helpInfo.GetParameter(this._parameter);
                        if ((parameter == null) || (parameter.Length == 0))
                        {
                            return;
                        }
                    }
                    base.WriteObject(helpInfo.ShortHelp);
                }
            }
        }

        [Parameter, ValidateSet(new string[] { "Alias", "Cmdlet", "Provider", "General", "FAQ", "Glossary", "HelpFile", "ScriptCommand", "Function", "Filter", "ExternalScript", "All", "DefaultHelp", "Workflow" }, IgnoreCase=true)]
        public string[] Category
        {
            get
            {
                return this._category;
            }
            set
            {
                this._category = value;
            }
        }

        [Parameter]
        public string[] Component
        {
            get
            {
                return this._component;
            }
            set
            {
                this._component = value;
            }
        }

        [Parameter(ParameterSetName="DetailedView", Mandatory=true)]
        public SwitchParameter Detailed
        {
            set
            {
                if (value.ToBool())
                {
                    this._viewTokenToAdd = HelpView.DetailedView;
                }
            }
        }

        [Parameter(ParameterSetName="Examples", Mandatory=true)]
        public SwitchParameter Examples
        {
            set
            {
                if (value.ToBool())
                {
                    this._viewTokenToAdd = HelpView.ExamplesView;
                }
            }
        }

        [Parameter(ParameterSetName="AllUsersView")]
        public SwitchParameter Full
        {
            set
            {
                if (value.ToBool())
                {
                    this._viewTokenToAdd = HelpView.FullView;
                }
            }
        }

        [Parameter]
        public string[] Functionality
        {
            get
            {
                return this._functionality;
            }
            set
            {
                this._functionality = value;
            }
        }

        [Parameter(Position=0, ValueFromPipelineByPropertyName=true)]
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        [Parameter(ParameterSetName="Online", Mandatory=true)]
        public SwitchParameter Online
        {
            get
            {
                return this.showOnlineHelp;
            }
            set
            {
                this.showOnlineHelp = (bool) value;
                if (this.showOnlineHelp)
                {
                    VerifyParameterForbiddenInRemoteRunspace(this, "Online");
                }
            }
        }

        [Parameter(ParameterSetName="Parameters", Mandatory=true)]
        public string Parameter
        {
            get
            {
                return this._parameter;
            }
            set
            {
                this._parameter = value;
            }
        }

        [Parameter]
        public string Path
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
            }
        }

        [Parameter]
        public string[] Role
        {
            get
            {
                return this._role;
            }
            set
            {
                this._role = value;
            }
        }

        [Parameter(ParameterSetName="ShowWindow", Mandatory=true)]
        public SwitchParameter ShowWindow
        {
            get
            {
                return this.showWindow;
            }
            set
            {
                this.showWindow = (bool) value;
                if (this.showWindow)
                {
                    VerifyParameterForbiddenInRemoteRunspace(this, "ShowWindow");
                }
            }
        }

        internal enum HelpView
        {
            Default,
            DetailedView,
            FullView,
            ExamplesView
        }
    }
}

