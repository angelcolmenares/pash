namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Security;

    [Cmdlet("Set", "PSBreakpoint", DefaultParameterSetName="Line", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113449"), OutputType(new Type[] { typeof(VariableBreakpoint), typeof(CommandBreakpoint), typeof(LineBreakpoint) })]
    public class SetPSBreakpointCommand : PSCmdlet
    {
        private VariableAccessMode _accessMode = VariableAccessMode.Write;
        private ScriptBlock _action;
        private int? _column = null;
        private string[] _command;
        private int[] _line;
        private string[] _script;
        private string[] _variable;

        protected override void BeginProcessing()
        {
            if (base.Context.InternalHost.ExternalHost is ServerRemoteHost)
            {
                base.ThrowTerminatingError(new ErrorRecord(new PSNotSupportedException(UtilityDebuggerStrings.RemoteDebuggerNotSupported), "SetPSBreakpoint:RemoteDebuggerNotSupported", ErrorCategory.NotImplemented, null));
            }
            if ((base.Context.LanguageMode == PSLanguageMode.ConstrainedLanguage) && (SystemPolicy.GetSystemLockdownPolicy() != SystemEnforcementMode.Enforce))
            {
                base.ThrowTerminatingError(new ErrorRecord(new PSNotSupportedException(UtilityDebuggerStrings.RemoteDebuggerNotSupported), "CannotSetBreakpointInconsistentLanguageMode", ErrorCategory.PermissionDenied, base.Context.LanguageMode));
            }
        }

        protected override void ProcessRecord()
        {
            Collection<string> collection = new Collection<string>();
            if (this._script != null)
            {
                foreach (string str in this._script)
                {
                    Collection<PathInfo> resolvedPSPathFromPSPath = base.SessionState.Path.GetResolvedPSPathFromPSPath(str);
                    for (int i = 0; i < resolvedPSPathFromPSPath.Count; i++)
                    {
                        string providerPath = resolvedPSPathFromPSPath[i].ProviderPath;
                        if (!File.Exists(providerPath))
                        {
                            base.WriteError(new ErrorRecord(new ArgumentException(StringUtil.Format(UtilityDebuggerStrings.FileDoesNotExist, providerPath)), "SetPSBreakpoint:FileDoesNotExist", ErrorCategory.InvalidArgument, null));
                        }
                        else
                        {
                            string extension = Path.GetExtension(providerPath);
                            if (!extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".psm1", StringComparison.OrdinalIgnoreCase))
                            {
                                base.WriteError(new ErrorRecord(new ArgumentException(StringUtil.Format(UtilityDebuggerStrings.WrongExtension, providerPath)), "SetPSBreakpoint:WrongExtension", ErrorCategory.InvalidArgument, null));
                            }
                            else
                            {
                                collection.Add(providerPath);
                            }
                        }
                    }
                }
            }
            if (base.ParameterSetName.Equals("Command", StringComparison.OrdinalIgnoreCase))
            {
                for (int j = 0; j < this.Command.Length; j++)
                {
                    if (collection.Count > 0)
                    {
                        foreach (string str4 in collection)
                        {
                            base.WriteObject(base.Context.Debugger.NewCommandBreakpoint(str4.ToString(), this.Command[j], this.Action));
                        }
                    }
                    else
                    {
                        base.WriteObject(base.Context.Debugger.NewCommandBreakpoint(this.Command[j], this.Action));
                    }
                }
            }
            else if (base.ParameterSetName.Equals("Variable", StringComparison.OrdinalIgnoreCase))
            {
                for (int k = 0; k < this.Variable.Length; k++)
                {
                    if (collection.Count > 0)
                    {
                        foreach (string str5 in collection)
                        {
                            base.WriteObject(base.Context.Debugger.NewVariableBreakpoint(str5.ToString(), this.Variable[k], this.Mode, this.Action));
                        }
                    }
                    else
                    {
                        base.WriteObject(base.Context.Debugger.NewVariableBreakpoint(this.Variable[k], this.Mode, this.Action));
                    }
                }
            }
            else
            {
                for (int m = 0; m < this.Line.Length; m++)
                {
                    if (this.Line[m] < 1)
                    {
                        base.WriteError(new ErrorRecord(new ArgumentException(UtilityDebuggerStrings.LineLessThanOne), "SetPSBreakpoint:LineLessThanOne", ErrorCategory.InvalidArgument, null));
                    }
                    else
                    {
                        foreach (string str6 in collection)
                        {
                            if (this._column.HasValue)
                            {
                                base.WriteObject(base.Context.Debugger.NewStatementBreakpoint(str6.ToString(), this.Line[m], this.Column, this.Action));
                            }
                            else
                            {
                                base.WriteObject(base.Context.Debugger.NewLineBreakpoint(str6.ToString(), this.Line[m], this.Action));
                            }
                        }
                    }
                }
            }
        }

        [Parameter(ParameterSetName="Variable"), Parameter(ParameterSetName="Command"), Parameter(ParameterSetName="Line")]
        public ScriptBlock Action
        {
            get
            {
                return this._action;
            }
            set
            {
                this._action = value;
            }
        }

        [ValidateRange(1, 0x7fffffff), Parameter(Position=2, ParameterSetName="Line")]
        public int Column
        {
            get
            {
                int? nullable = this._column;
                if (!nullable.HasValue)
                {
                    return 0;
                }
                return nullable.GetValueOrDefault();
            }
            set
            {
                this._column = new int?(value);
            }
        }

        [ValidateNotNull, Alias(new string[] { "C" }), Parameter(ParameterSetName="Command", Mandatory=true)]
        public string[] Command
        {
            get
            {
                return this._command;
            }
            set
            {
                this._command = value;
            }
        }

        [Parameter(Position=1, ParameterSetName="Line", Mandatory=true), ValidateNotNull]
        public int[] Line
        {
            get
            {
                return this._line;
            }
            set
            {
                this._line = value;
            }
        }

        [Parameter(ParameterSetName="Variable")]
        public VariableAccessMode Mode
        {
            get
            {
                return this._accessMode;
            }
            set
            {
                this._accessMode = value;
            }
        }

        [Parameter(ParameterSetName="Line", Mandatory=true, Position=0), Parameter(ParameterSetName="Variable", Position=0), ValidateNotNull, Parameter(ParameterSetName="Command", Position=0)]
        public string[] Script
        {
            get
            {
                return this._script;
            }
            set
            {
                this._script = value;
            }
        }

        [ValidateNotNull, Parameter(ParameterSetName="Variable", Mandatory=true), Alias(new string[] { "V" })]
        public string[] Variable
        {
            get
            {
                return this._variable;
            }
            set
            {
                this._variable = value;
            }
        }
    }
}

