namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    [Cmdlet("Get", "PSBreakpoint", DefaultParameterSetName="Script", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113325"), OutputType(new System.Type[] { typeof(Breakpoint) })]
    public class GetPSBreakpointCommand : PSCmdlet
    {
        private string[] _command;
        private int[] _id;
        private string[] _script;
        private BreakpointType[] _type;
        private string[] _variable;

        private List<Breakpoint> Filter<T>(List<Breakpoint> input, T[] filter, FilterSelector<T> selector)
        {
            List<Breakpoint> list = new List<Breakpoint>();
            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < filter.Length; j++)
                {
                    if (selector(input[i], filter[j]))
                    {
                        list.Add(input[i]);
                        break;
                    }
                }
            }
            return list;
        }

        protected override void ProcessRecord()
        {
            FilterSelector<string> selector = null;
            List<Breakpoint> breakpoints = base.Context.Debugger.GetBreakpoints();
            if (!base.ParameterSetName.Equals("Script", StringComparison.OrdinalIgnoreCase))
            {
                if (base.ParameterSetName.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    breakpoints = this.Filter<int>(breakpoints, this._id, (breakpoint, id) => breakpoint.Id == id);
                }
                else if (base.ParameterSetName.Equals("Command", StringComparison.OrdinalIgnoreCase))
                {
                    breakpoints = this.Filter<string>(breakpoints, this._command, delegate (Breakpoint breakpoint, string command) {
                        CommandBreakpoint breakpoint2 = breakpoint as CommandBreakpoint;
                        if (breakpoint2 == null)
                        {
                            return false;
                        }
                        return breakpoint2.Command.Equals(command, StringComparison.OrdinalIgnoreCase);
                    });
                }
                else if (base.ParameterSetName.Equals("Variable", StringComparison.OrdinalIgnoreCase))
                {
                    breakpoints = this.Filter<string>(breakpoints, this._variable, delegate (Breakpoint breakpoint, string variable) {
                        VariableBreakpoint breakpoint2 = breakpoint as VariableBreakpoint;
                        if (breakpoint2 == null)
                        {
                            return false;
                        }
                        return breakpoint2.Variable.Equals(variable, StringComparison.OrdinalIgnoreCase);
                    });
                }
                else if (base.ParameterSetName.Equals("Type", StringComparison.OrdinalIgnoreCase))
                {
                    breakpoints = this.Filter<BreakpointType>(breakpoints, this._type, delegate (Breakpoint breakpoint, BreakpointType type) {
                        switch (type)
                        {
                            case BreakpointType.Line:
                                if (breakpoint is LineBreakpoint)
                                {
                                    return true;
                                }
                                break;

                            case BreakpointType.Variable:
                                if (breakpoint is VariableBreakpoint)
                                {
                                    return true;
                                }
                                break;

                            case BreakpointType.Command:
                                if (breakpoint is CommandBreakpoint)
                                {
                                    return true;
                                }
                                break;
                        }
                        return false;
                    });
                }
            }
            if (this._script != null)
            {
                if (selector == null)
                {
                    selector = delegate (Breakpoint breakpoint, string script) {
                        if (breakpoint.Script == null)
                        {
                            return false;
                        }
                        return string.Compare(base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(breakpoint.Script), base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(script), StringComparison.OrdinalIgnoreCase) == 0;
                    };
                }
                breakpoints = this.Filter<string>(breakpoints, this._script, selector);
            }
            foreach (Breakpoint breakpoint in breakpoints)
            {
                base.WriteObject(breakpoint);
            }
        }

        [Parameter(ParameterSetName="Command", Mandatory=true), ValidateNotNull]
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

        [Parameter(ParameterSetName="Id", Mandatory=true, Position=0, ValueFromPipeline=true), ValidateNotNull]
        public int[] Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        [ValidateNotNull, Parameter(ParameterSetName="Type"), Parameter(ParameterSetName="Command"), Parameter(ParameterSetName="Script", Position=0, ValueFromPipeline=true), Parameter(ParameterSetName="Variable")]
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

        [ValidateNotNull, Parameter(ParameterSetName="Type", Mandatory=true, Position=0, ValueFromPipeline=true)]
        public BreakpointType[] Type
        {
            get
            {
                return this._type;
            }
            set
            {
                this._type = value;
            }
        }

        [ValidateNotNull, Parameter(ParameterSetName="Variable", Mandatory=true)]
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

        private delegate bool FilterSelector<T>(Breakpoint breakpoint, T target);
    }
}

