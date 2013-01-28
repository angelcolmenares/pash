namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [OutputType(new Type[] { typeof(PSVariable) }), Cmdlet("Clear", "Variable", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113285")]
    public sealed class ClearVariableCommand : VariableCommandBase
    {
        private bool force;
        private string[] names;
        private bool passThru;

        private PSVariable ClearValue(PSVariable matchingVariable)
        {
            PSVariable variable = matchingVariable;
            if (base.Scope != null)
            {
                matchingVariable.Value = null;
                return variable;
            }
            base.SessionState.PSVariable.Set(matchingVariable.Name, null);
            return base.SessionState.PSVariable.Get(matchingVariable.Name);
        }

        protected override void ProcessRecord()
        {
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
                        string clearVariableAction = VariableCommandStrings.ClearVariableAction;
                        string target = StringUtil.Format(VariableCommandStrings.ClearVariableTarget, variable.Name);
                        if (base.ShouldProcess(target, clearVariableAction))
                        {
                            PSVariable sendToPipeline = variable;
                            try
                            {
                                if (this.force && ((variable.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None))
                                {
                                    variable.SetOptions(variable.Options & ~ScopedItemOptions.ReadOnly, true);
                                    sendToPipeline = this.ClearValue(variable);
                                    variable.SetOptions(variable.Options | ScopedItemOptions.ReadOnly, true);
                                }
                                else
                                {
                                    sendToPipeline = this.ClearValue(variable);
                                }
                            }
                            catch (SessionStateException exception2)
                            {
                                base.WriteError(new ErrorRecord(exception2.ErrorRecord, exception2));
                                continue;
                            }
                            catch (PSArgumentException exception3)
                            {
                                base.WriteError(new ErrorRecord(exception3.ErrorRecord, exception3));
                                continue;
                            }
                            if (this.passThru)
                            {
                                base.WriteObject(sendToPipeline);
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
    }
}

