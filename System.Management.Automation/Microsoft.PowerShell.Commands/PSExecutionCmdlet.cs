namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public abstract class PSExecutionCmdlet : PSRemotingBaseCmdlet
    {
        private PowerShell _powershellv2;
        private PowerShell _powershellv3;
        private object[] args;
        private SwitchParameter enableNetworkAccess;
        private string filePath;
        protected const string FilePathComputerNameParameterSet = "FilePathComputerName";
        protected const string FilePathSessionParameterSet = "FilePathRunspace";
        protected const string FilePathUriParameterSet = "FilePathUri";
        private PSObject inputObject = AutomationNull.Value;
        private bool invokeAndDisconnect;
        protected const string LiteralFilePathComputerNameParameterSet = "LiteralFilePathComputerName";
        private List<IThrottleOperation> operations = new List<IThrottleOperation>();
        private System.Management.Automation.ScriptBlock scriptBlock;
        private string[] sessionName;

        protected PSExecutionCmdlet()
        {
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (this.filePath != null)
            {
                this.scriptBlock = this.GetScriptBlockFromFile(this.filePath, this.IsLiteralPath);
            }
            try
            {
                this._powershellv3 = this._powershellv2 = this.scriptBlock.GetPowerShell(this.args);
            }
            catch (ScriptBlockToPowerShellNotSupportedException)
            {
            }
            switch (base.ParameterSetName)
            {
                case "FilePathComputerName":
                case "LiteralFilePathComputerName":
                case "ComputerName":
                {
                    string[] resolvedComputerNames = null;
                    base.ResolveComputerNames(this.ComputerName, out resolvedComputerNames);
                    base.ResolvedComputerNames = resolvedComputerNames;
                    this.CreateHelpersForSpecifiedComputerNames();
                    return;
                }
                case "FilePathRunspace":
                case "Session":
                    base.ValidateRemoteRunspacesSpecified();
                    this.CreateHelpersForSpecifiedRunspaces();
                    return;

                case "FilePathUri":
                case "Uri":
                    this.CreateHelpersForSpecifiedUris();
                    return;
            }
        }

        protected void CloseAllInputStreams()
        {
            foreach (IThrottleOperation operation in this.Operations)
            {
                ExecutionCmdletHelper helper = (ExecutionCmdletHelper) operation;
                helper.Pipeline.Input.Close();
            }
        }

        protected virtual void CreateHelpersForSpecifiedComputerNames()
        {
            base.ValidateComputerName(base.ResolvedComputerNames);
            RemoteRunspace remoteRunspace = null;
            string str = this.UseSSL.IsPresent ? "https" : "http";
            for (int i = 0; i < base.ResolvedComputerNames.Length; i++)
            {
                try
                {
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo {
                        Scheme = str,
                        ComputerName = base.ResolvedComputerNames[i],
                        Port = this.Port,
                        AppName = this.ApplicationName,
                        ShellUri = this.ConfigurationName
                    };
                    if (this.CertificateThumbprint != null)
                    {
                        connectionInfo.CertificateThumbprint = this.CertificateThumbprint;
                    }
                    else
                    {
                        connectionInfo.Credential = this.Credential;
                    }
                    connectionInfo.AuthenticationMechanism = this.Authentication;
                    base.UpdateConnectionInfo(connectionInfo);
                    connectionInfo.EnableNetworkAccess = (bool) this.EnableNetworkAccess;
                    int id = PSSession.GenerateRunspaceId();
                    string name = ((this.DisconnectedSessionName != null) && (this.DisconnectedSessionName.Length > i)) ? this.DisconnectedSessionName[i] : PSSession.ComposeRunspaceName(id);
                    remoteRunspace = new RemoteRunspace(Utils.GetTypeTableFromExecutionContextTLS(), connectionInfo, base.Host, this.SessionOption.ApplicationArguments, name, id);
                    remoteRunspace.Events.ReceivedEvents.PSEventReceived += new PSEventReceivedEventHandler(this.OnRunspacePSEventReceived);
                }
                catch (UriFormatException exception)
                {
                    ErrorRecord errorRecord = new ErrorRecord(exception, "CreateRemoteRunspaceFailed", ErrorCategory.InvalidArgument, base.ResolvedComputerNames[i]);
                    base.WriteError(errorRecord);
                    continue;
                }
                Pipeline pipeline = this.CreatePipeline(remoteRunspace);
                IThrottleOperation item = new ExecutionCmdletHelperComputerName(remoteRunspace, pipeline, this.invokeAndDisconnect);
                this.Operations.Add(item);
            }
        }

        protected void CreateHelpersForSpecifiedRunspaces()
        {
            int length = this.Session.Length;
            RemoteRunspace[] runspaceArray = new RemoteRunspace[length];
            for (int i = 0; i < length; i++)
            {
                runspaceArray[i] = (RemoteRunspace) this.Session[i].Runspace;
            }
            Pipeline[] pipelineArray = new Pipeline[length];
            for (int j = 0; j < length; j++)
            {
                pipelineArray[j] = this.CreatePipeline(runspaceArray[j]);
                IThrottleOperation item = new ExecutionCmdletHelperRunspace(pipelineArray[j]);
                this.Operations.Add(item);
            }
        }

        protected void CreateHelpersForSpecifiedUris()
        {
            RemoteRunspace remoteRunspace = null;
            for (int i = 0; i < this.ConnectionUri.Length; i++)
            {
                try
                {
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo {
                        ConnectionUri = this.ConnectionUri[i],
                        ShellUri = this.ConfigurationName
                    };
                    if (this.CertificateThumbprint != null)
                    {
                        connectionInfo.CertificateThumbprint = this.CertificateThumbprint;
                    }
                    else
                    {
                        connectionInfo.Credential = this.Credential;
                    }
                    connectionInfo.AuthenticationMechanism = this.Authentication;
                    base.UpdateConnectionInfo(connectionInfo);
                    connectionInfo.EnableNetworkAccess = (bool) this.EnableNetworkAccess;
                    remoteRunspace = (RemoteRunspace) RunspaceFactory.CreateRunspace(connectionInfo, base.Host, Utils.GetTypeTableFromExecutionContextTLS(), this.SessionOption.ApplicationArguments);
                    remoteRunspace.Events.ReceivedEvents.PSEventReceived += new PSEventReceivedEventHandler(this.OnRunspacePSEventReceived);
                }
                catch (UriFormatException exception)
                {
                    this.WriteErrorCreateRemoteRunspaceFailed(exception, this.ConnectionUri[i]);
                    continue;
                }
                catch (InvalidOperationException exception2)
                {
                    this.WriteErrorCreateRemoteRunspaceFailed(exception2, this.ConnectionUri[i]);
                    continue;
                }
                catch (ArgumentException exception3)
                {
                    this.WriteErrorCreateRemoteRunspaceFailed(exception3, this.ConnectionUri[i]);
                    continue;
                }
                Pipeline pipeline = this.CreatePipeline(remoteRunspace);
                IThrottleOperation item = new ExecutionCmdletHelperComputerName(remoteRunspace, pipeline, this.invokeAndDisconnect);
                this.Operations.Add(item);
            }
        }

        internal Pipeline CreatePipeline(RemoteRunspace remoteRunspace)
        {
            PowerShell shell = remoteRunspace.GetCapabilities().Equals(RunspaceCapability.Default) ? this.GetPowerShellForPSv2() : this.GetPowerShellForPSv3();
            Pipeline pipeline = remoteRunspace.CreatePipeline(shell.Commands.Commands[0].CommandText, true);
            pipeline.Commands.Clear();
            foreach (Command command in shell.Commands.Commands)
            {
                pipeline.Commands.Add(command);
            }
            pipeline.RedirectShellErrorOutputPipe = true;
            return pipeline;
        }

        private string GetConvertedScript(out List<string> newParameterNames, out List<object> newParameterValues)
        {
            newParameterNames = null;
            newParameterValues = null;
            string str = null;
            List<VariableExpressionAst> usingVariables = this.GetUsingVariables(this.scriptBlock);
            if ((usingVariables == null) || (usingVariables.Count == 0))
            {
                return (base.MyInvocation.ExpectingInput ? this.scriptBlock.GetWithInputHandlingForInvokeCommand() : this.scriptBlock.ToString());
            }
            newParameterNames = new List<string>();
            List<string> values = new List<string>();
            List<VariableExpressionAst> paramUsingVars = new List<VariableExpressionAst>();
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ScriptBlockAst ast = this.scriptBlock.Ast as ScriptBlockAst;
            foreach (VariableExpressionAst ast2 in usingVariables)
            {
                string userPath = ast2.VariablePath.UserPath;
                string item = "__using_" + userPath;
                string str4 = "$" + item;
                if (!set.Contains(str4))
                {
                    newParameterNames.Add(item);
                    values.Add(str4);
                    paramUsingVars.Add(ast2);
                    set.Add(str4);
                }
            }
            newParameterValues = this.GetUsingVariableValues(paramUsingVars);
            string str5 = string.Join(", ", values);
            str = base.MyInvocation.ExpectingInput ? ast.GetWithInputHandlingForInvokeCommandImpl(Tuple.Create<List<VariableExpressionAst>, string>(usingVariables, str5)) : ast.ToStringForSerialization(Tuple.Create<List<VariableExpressionAst>, string>(usingVariables, str5), ast.Extent.StartOffset, ast.Extent.EndOffset);
            if (ast.ParamBlock == null)
            {
                str = "param(" + str5 + ")\n" + str;
            }
            return str;
        }

        private PowerShell GetPowerShellForPSv2()
        {
            if (this._powershellv2 == null)
            {
                List<string> list;
                List<object> list2;
                string convertedScript = this.GetConvertedScript(out list, out list2);
                this._powershellv2 = PowerShell.Create().AddScript(convertedScript);
                if (this.args != null)
                {
                    foreach (object obj2 in this.args)
                    {
                        this._powershellv2.AddArgument(obj2);
                    }
                }
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        this._powershellv2.AddParameter(list[i], list2[i]);
                    }
                }
            }
            return this._powershellv2;
        }

        private PowerShell GetPowerShellForPSv3()
        {
            if (this._powershellv3 == null)
            {
                object[] objArray = ScriptBlockToPowerShellConverter.GetUsingValues(this.scriptBlock, base.Context, null);
                string script = base.MyInvocation.ExpectingInput ? this.scriptBlock.GetWithInputHandlingForInvokeCommand() : this.scriptBlock.ToString();
                this._powershellv3 = PowerShell.Create().AddScript(script);
                if (this.args != null)
                {
                    foreach (object obj2 in this.args)
                    {
                        this._powershellv3.AddArgument(obj2);
                    }
                }
                if ((objArray != null) && (objArray.Length > 0))
                {
                    this._powershellv3.AddParameter("--%", objArray);
                }
            }
            return this._powershellv3;
        }

        protected System.Management.Automation.ScriptBlock GetScriptBlockFromFile(string filePath, bool isLiteralPath)
        {
            if (!isLiteralPath && WildcardPattern.ContainsWildcardCharacters(filePath))
            {
                throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.WildCardErrorFilePathParameter, new object[0]), "filePath");
            }
            if (!filePath.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.FilePathShouldPS1Extension, new object[0]), "filePath");
            }
            string path = new PathResolver().ResolveProviderAndPath(filePath, isLiteralPath, this, false, "RemotingErrorIdStrings", PSRemotingErrorId.FilePathNotFromFileSystemProvider.ToString());
            ExternalScriptInfo commandInfo = new ExternalScriptInfo(filePath, path, base.Context);
            if (!filePath.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
            {
                base.Context.AuthorizationManager.ShouldRunInternal(commandInfo, CommandOrigin.Internal, base.Context.EngineHostInterface);
            }
            return commandInfo.ScriptBlock;
        }

        private List<VariableExpressionAst> GetUsingVariables(System.Management.Automation.ScriptBlock localScriptBlock)
        {
            List<UsingExpressionAst> source = new List<UsingExpressionAst>();
            foreach (Ast ast in UsingExpressionAstSearcher.FindAllUsingExpressionExceptForWorkflow(localScriptBlock.Ast).ToList<Ast>())
            {
                UsingExpressionAst usingExpr = ast as UsingExpressionAst;
                if (ScriptBlockToPowerShellConverter.IsUsingExpressionInFunction(usingExpr, localScriptBlock.Ast))
                {
                    ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(AutomationExceptions.UsingVariableNotSupportedInFunctionOrFilter), "UsingVariableNotSupportedInFunctionOrFilter", ErrorCategory.InvalidOperation, ast);
                    base.ThrowTerminatingError(errorRecord);
                }
                else
                {
                    source.Add(usingExpr);
                }
            }
            return source.Select<UsingExpressionAst, VariableExpressionAst>(new Func<UsingExpressionAst, VariableExpressionAst>(UsingExpressionAst.ExtractUsingVariable)).ToList<VariableExpressionAst>();
        }

        private List<object> GetUsingVariableValues(List<VariableExpressionAst> paramUsingVars)
        {
            List<object> list = new List<object>(paramUsingVars.Count);
            VariableExpressionAst ast = null;
            Version strictModeVersion = base.Context.EngineSessionState.CurrentScope.StrictModeVersion;
            try
            {
                base.Context.EngineSessionState.CurrentScope.StrictModeVersion = PSVersionInfo.PSVersion;
                foreach (VariableExpressionAst ast2 in paramUsingVars)
                {
                    ast = ast2;
                    object item = Compiler.GetExpressionValue(ast2, base.Context, (System.Collections.IList)null);
                    list.Add(item);
                }
                return list;
            }
            catch (RuntimeException exception)
            {
                if (exception.ErrorRecord.FullyQualifiedErrorId.Equals("VariableIsUndefined", StringComparison.Ordinal))
                {
                    throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), ast.Extent, "UsingVariableIsUndefined", AutomationExceptions.UsingVariableIsUndefined, new object[] { exception.ErrorRecord.TargetObject });
                }
            }
            finally
            {
                base.Context.EngineSessionState.CurrentScope.StrictModeVersion = strictModeVersion;
            }
            return list;
        }

        internal void OnRunspacePSEventReceived(object sender, PSEventArgs e)
        {
            if (base.Events != null)
            {
                base.Events.AddForwardedEvent(e);
            }
        }

        private void WriteErrorCreateRemoteRunspaceFailed(Exception e, Uri uri)
        {
            ErrorRecord errorRecord = new ErrorRecord(e, "CreateRemoteRunspaceFailed", ErrorCategory.InvalidArgument, uri);
            base.WriteError(errorRecord);
        }

        [Alias(new string[] { "Args" }), Parameter]
        public virtual object[] ArgumentList
        {
            get
            {
                return this.args;
            }
            set
            {
                this.args = value;
            }
        }

        protected string[] DisconnectedSessionName
        {
            get
            {
                return this.sessionName;
            }
            set
            {
                this.sessionName = value;
            }
        }

        public virtual SwitchParameter EnableNetworkAccess
        {
            get
            {
                return this.enableNetworkAccess;
            }
            set
            {
                this.enableNetworkAccess = value;
            }
        }

        [Parameter(Position=1, Mandatory=true, ParameterSetName="FilePathRunspace"), Parameter(Position=1, Mandatory=true, ParameterSetName="FilePathComputerName"), Parameter(Position=1, Mandatory=true, ParameterSetName="FilePathUri"), ValidateNotNull]
        public virtual string FilePath
        {
            get
            {
                return this.filePath;
            }
            set
            {
                this.filePath = value;
            }
        }

        [Parameter(ValueFromPipeline=true)]
        public virtual PSObject InputObject
        {
            get
            {
                return this.inputObject;
            }
            set
            {
                this.inputObject = value;
            }
        }

        protected bool InvokeAndDisconnect
        {
            get
            {
                return this.invokeAndDisconnect;
            }
            set
            {
                this.invokeAndDisconnect = value;
            }
        }

        protected bool IsLiteralPath { get; set; }

        internal List<IThrottleOperation> Operations
        {
            get
            {
                return this.operations;
            }
        }

        public virtual System.Management.Automation.ScriptBlock ScriptBlock
        {
            get
            {
                return this.scriptBlock;
            }
            set
            {
                this.scriptBlock = value;
            }
        }
    }
}

