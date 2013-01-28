namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [Cmdlet("Remove", "Variable", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113380")]
    public sealed class RemoveVariableCommand : VariableCommandBase
    {
        private bool force;
        private string[] names;

        protected override void ProcessRecord()
        {
            if (base.Scope == null)
            {
                base.Scope = "local";
            }
            foreach (string str in this.names)
            {
                bool wasFiltered = false;
                List<PSVariable> list = base.GetMatchingVariables(str, base.Scope, out wasFiltered, false);
                if ((list.Count == 0) && !wasFiltered)
                {
                    ItemNotFoundException replaceParentContainsErrorRecordException = new ItemNotFoundException(str, "VariableNotFound", SessionStateStrings.VariableNotFound);
                    base.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
                }
                else
                {
                    foreach (PSVariable variable in list)
                    {
                        string removeVariableAction = VariableCommandStrings.RemoveVariableAction;
                        string target = StringUtil.Format(VariableCommandStrings.RemoveVariableTarget, variable.Name);
                        if (base.ShouldProcess(target, removeVariableAction))
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(base.Scope))
                                {
                                    base.SessionState.Internal.RemoveVariable(variable, this.force);
                                }
                                else
                                {
                                    base.SessionState.Internal.RemoveVariableAtScope(variable, base.Scope, this.force);
                                }
                            }
                            catch (SessionStateException exception2)
                            {
                                base.WriteError(new ErrorRecord(exception2.ErrorRecord, exception2));
                            }
                            catch (PSArgumentException exception3)
                            {
                                base.WriteError(new ErrorRecord(exception3.ErrorRecord, exception3));
                            }
                        }
                    }
                }
            }
        }

        [Parameter]
        public string[] Exclude
        {
            get
            {
                return base.ExcludeFilters;
            }
            set
            {
                base.ExcludeFilters = value;
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

        [Parameter]
        public string[] Include
        {
            get
            {
                return base.IncludeFilters;
            }
            set
            {
                base.IncludeFilters = value;
            }
        }

        [Parameter(Position=0, ValueFromPipelineByPropertyName=true, Mandatory=true)]
        public string[] Name
        {
            get
            {
                return this.names;
            }
            set
            {
                this.names = value;
            }
        }
    }
}

