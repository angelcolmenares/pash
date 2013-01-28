namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [Cmdlet("New", "Variable", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113361")]
    public sealed class NewVariableCommand : VariableCommandBase
    {
        private object _value;
        private SessionStateEntryVisibility? _visibility;
        private string description;
        private bool force;
        private string name;
        private ScopedItemOptions options;
        private bool passThru;

        protected override void ProcessRecord()
        {
            if (this.Force == 0)
            {
                PSVariable atScope = null;
                if (string.IsNullOrEmpty(base.Scope))
                {
                    atScope = base.SessionState.PSVariable.GetAtScope(this.name, "local");
                }
                else
                {
                    atScope = base.SessionState.PSVariable.GetAtScope(this.name, base.Scope);
                }
                if (atScope != null)
                {
                    SessionStateException replaceParentContainsErrorRecordException = new SessionStateException(this.name, SessionStateCategory.Variable, "VariableAlreadyExists", SessionStateStrings.VariableAlreadyExists, ErrorCategory.ResourceExists, new object[0]);
                    base.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
                    return;
                }
            }
            string newVariableAction = VariableCommandStrings.NewVariableAction;
            string target = StringUtil.Format(VariableCommandStrings.NewVariableTarget, this.Name, this.Value);
            if (base.ShouldProcess(target, newVariableAction))
            {
                PSVariable variable = new PSVariable(this.name, this._value, this.options);
                if (this._visibility.HasValue)
                {
                    variable.Visibility = this._visibility.Value;
                }
                if (this.description != null)
                {
                    variable.Description = this.description;
                }
                try
                {
                    if (string.IsNullOrEmpty(base.Scope))
                    {
                        base.SessionState.Internal.NewVariable(variable, (bool) this.Force);
                    }
                    else
                    {
                        base.SessionState.Internal.NewVariableAtScope(variable, base.Scope, (bool) this.Force);
                    }
                }
                catch (SessionStateException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2.ErrorRecord, exception2));
                    return;
                }
                catch (PSArgumentException exception3)
                {
                    base.WriteError(new ErrorRecord(exception3.ErrorRecord, exception3));
                    return;
                }
                if (this.passThru)
                {
                    base.WriteObject(variable);
                }
            }
        }

        [Parameter]
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
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

        [Parameter(Position=0, ValueFromPipelineByPropertyName=true, Mandatory=true)]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        [Parameter]
        public ScopedItemOptions Option
        {
            get
            {
                return this.options;
            }
            set
            {
                this.options = value;
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

        [Parameter(Position=1, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
        public object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
            }
        }

        [Parameter]
        public SessionStateEntryVisibility Visibility
        {
            get
            {
                return this._visibility.Value;
            }
            set
            {
                this._visibility = new SessionStateEntryVisibility?(value);
            }
        }
    }
}

