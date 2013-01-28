namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;

    [Cmdlet("New", "Module", DefaultParameterSetName="ScriptBlock", HelpUri="http://go.microsoft.com/fwlink/?LinkID=141554"), OutputType(new Type[] { typeof(PSModuleInfo) })]
    public sealed class NewModuleCommand : ModuleCmdletBase
    {
        private object[] _arguments;
        private bool _asCustomObject;
        private string[] _cmdletImportList = new string[0];
        private string[] _functionImportList = new string[0];
        private string _name;
        private bool _returnResult;
        private System.Management.Automation.ScriptBlock _scriptBlock;

        protected override void EndProcessing()
        {
            if (this._scriptBlock != null)
            {
                string path = Guid.NewGuid().ToString();
                if (string.IsNullOrEmpty(this._name))
                {
                    this._name = "__DynamicModule_" + path;
                }
                try
                {
                    base.Context.Modules.IncrementModuleNestingDepth(this, this._name);
                    ArrayList results = null;
                    PSModuleInfo sourceModule = null;
                    try
                    {
                        sourceModule = base.Context.Modules.CreateModule(this._name, path, this._scriptBlock, null, out results, this._arguments);
                        if (!sourceModule.SessionState.Internal.UseExportList)
                        {
                            List<WildcardPattern> cmdletPatterns = (base.BaseCmdletPatterns != null) ? base.BaseCmdletPatterns : base.MatchAll;
                            List<WildcardPattern> functionPatterns = (base.BaseFunctionPatterns != null) ? base.BaseFunctionPatterns : base.MatchAll;
                            ModuleIntrinsics.ExportModuleMembers(this, sourceModule.SessionState.Internal, functionPatterns, cmdletPatterns, base.BaseAliasPatterns, base.BaseVariablePatterns, null);
                        }
                    }
                    catch (RuntimeException exception)
                    {
                        exception.ErrorRecord.PreserveInvocationInfoOnce = true;
                        base.WriteError(exception.ErrorRecord);
                    }
                    if (sourceModule != null)
                    {
                        if (this._returnResult)
                        {
                            base.ImportModuleMembers(sourceModule, string.Empty);
                            base.WriteObject(results, true);
                        }
                        else if (this._asCustomObject)
                        {
                            base.WriteObject(sourceModule.AsCustomObject());
                        }
                        else
                        {
                            base.ImportModuleMembers(sourceModule, string.Empty);
                            base.WriteObject(sourceModule);
                        }
                    }
                }
                finally
                {
                    base.Context.Modules.DecrementModuleNestingCount();
                }
            }
        }

        [Alias(new string[] { "Args" }), Parameter(ValueFromRemainingArguments=true)]
        public object[] ArgumentList
        {
            get
            {
                return this._arguments;
            }
            set
            {
                this._arguments = value;
            }
        }

        [Parameter]
        public SwitchParameter AsCustomObject
        {
            get
            {
                return this._asCustomObject;
            }
            set
            {
                this._asCustomObject = (bool) value;
            }
        }

        [Parameter, ValidateNotNull]
        public string[] Cmdlet
        {
            get
            {
                return this._cmdletImportList;
            }
            set
            {
                if (value != null)
                {
                    this._cmdletImportList = value;
                    base.BaseCmdletPatterns = new List<WildcardPattern>();
                    foreach (string str in this._cmdletImportList)
                    {
                        base.BaseCmdletPatterns.Add(new WildcardPattern(str, WildcardOptions.IgnoreCase));
                    }
                }
            }
        }

        [Parameter, ValidateNotNull]
        public string[] Function
        {
            get
            {
                return this._functionImportList;
            }
            set
            {
                if (value != null)
                {
                    this._functionImportList = value;
                    base.BaseFunctionPatterns = new List<WildcardPattern>();
                    foreach (string str in this._functionImportList)
                    {
                        base.BaseFunctionPatterns.Add(new WildcardPattern(str, WildcardOptions.IgnoreCase));
                    }
                }
            }
        }

        [Parameter(ParameterSetName="Name", Mandatory=true, ValueFromPipeline=true, Position=0)]
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

        [Parameter]
        public SwitchParameter ReturnResult
        {
            get
            {
                return this._returnResult;
            }
            set
            {
                this._returnResult = (bool) value;
            }
        }

        [Parameter(ParameterSetName="Name", Mandatory=true, Position=1), ValidateNotNull, Parameter(ParameterSetName="ScriptBlock", Mandatory=true, Position=0)]
        public System.Management.Automation.ScriptBlock ScriptBlock
        {
            get
            {
                return this._scriptBlock;
            }
            set
            {
                this._scriptBlock = value;
            }
        }
    }
}

