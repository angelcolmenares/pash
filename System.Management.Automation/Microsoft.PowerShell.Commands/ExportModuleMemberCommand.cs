namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [Cmdlet("Export", "ModuleMember", HelpUri="http://go.microsoft.com/fwlink/?LinkID=141551")]
    public sealed class ExportModuleMemberCommand : PSCmdlet
    {
        private string[] _aliasExportList;
        private List<WildcardPattern> _aliasPatterns;
        private string[] _cmdletList;
        private List<WildcardPattern> _cmdletPatterns;
        private string[] _functionList;
        private List<WildcardPattern> _functionPatterns;
        private string[] _variableExportList;
        private List<WildcardPattern> _variablePatterns;

        protected override void ProcessRecord()
        {
            if (base.Context.EngineSessionState == base.Context.TopLevelSessionState)
            {
                InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(Modules.CanOnlyBeUsedFromWithinAModule, new object[0]));
                ErrorRecord errorRecord = new ErrorRecord(exception, "Modules_CanOnlyExecuteExportModuleMemberInsideAModule", ErrorCategory.PermissionDenied, null);
                base.ThrowTerminatingError(errorRecord);
            }
            ModuleIntrinsics.ExportModuleMembers(this, base.Context.EngineSessionState, this._functionPatterns, this._cmdletPatterns, this._aliasPatterns, this._variablePatterns, null);
        }

        [ValidateNotNull, Parameter(ValueFromPipelineByPropertyName=true)]
        public string[] Alias
        {
            get
            {
                return this._aliasExportList;
            }
            set
            {
                this._aliasExportList = value;
                this._aliasPatterns = new List<WildcardPattern>();
                if (this._aliasExportList != null)
                {
                    foreach (string str in this._aliasExportList)
                    {
                        this._aliasPatterns.Add(new WildcardPattern(str, WildcardOptions.IgnoreCase));
                    }
                }
            }
        }

        [AllowEmptyCollection, Parameter(ValueFromPipelineByPropertyName=true)]
        public string[] Cmdlet
        {
            get
            {
                return this._cmdletList;
            }
            set
            {
                this._cmdletList = value;
                this._cmdletPatterns = new List<WildcardPattern>();
                if (this._cmdletList != null)
                {
                    foreach (string str in this._cmdletList)
                    {
                        this._cmdletPatterns.Add(new WildcardPattern(str, WildcardOptions.IgnoreCase));
                    }
                }
            }
        }

        [AllowEmptyCollection, Parameter(ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, Position=0)]
        public string[] Function
        {
            get
            {
                return this._functionList;
            }
            set
            {
                this._functionList = value;
                this._functionPatterns = new List<WildcardPattern>();
                if (this._functionList != null)
                {
                    foreach (string str in this._functionList)
                    {
                        this._functionPatterns.Add(new WildcardPattern(str, WildcardOptions.IgnoreCase));
                    }
                }
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true), ValidateNotNull]
        public string[] Variable
        {
            get
            {
                return this._variableExportList;
            }
            set
            {
                this._variableExportList = value;
                this._variablePatterns = new List<WildcardPattern>();
                if (this._variableExportList != null)
                {
                    foreach (string str in this._variableExportList)
                    {
                        this._variablePatterns.Add(new WildcardPattern(str, WildcardOptions.IgnoreCase));
                    }
                }
            }
        }
    }
}

