namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [Cmdlet("Set", "Variable", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113401"), OutputType(new Type[] { typeof(PSVariable) })]
    public sealed class SetVariableCommand : VariableCommandBase
    {
        private object _value = AutomationNull.Value;
        private SessionStateEntryVisibility? _visibility;
        private string description;
        private bool force;
        private bool nameIsFormalParameter;
        private string[] names;
        private ScopedItemOptions? options;
        private bool passThru;
        private bool valueIsFormalParameter;
        private ArrayList valueList;

        protected override void BeginProcessing()
        {
            if ((this.names != null) && (this.names.Length > 0))
            {
                this.nameIsFormalParameter = true;
            }
            if (this._value != AutomationNull.Value)
            {
                this.valueIsFormalParameter = true;
            }
        }

        protected override void EndProcessing()
        {
            if (this.nameIsFormalParameter)
            {
                if (this.valueIsFormalParameter)
                {
                    this.SetVariable(this.names, this._value);
                }
                else if (this.valueList != null)
                {
                    if (this.valueList.Count == 1)
                    {
                        this.SetVariable(this.names, this.valueList[0]);
                    }
                    else if (this.valueList.Count == 0)
                    {
                        this.SetVariable(this.names, AutomationNull.Value);
                    }
                    else
                    {
                        this.SetVariable(this.names, this.valueList.ToArray());
                    }
                }
                else
                {
                    this.SetVariable(this.names, AutomationNull.Value);
                }
            }
        }

        protected override void ProcessRecord()
        {
            if (!this.nameIsFormalParameter || !this.valueIsFormalParameter)
            {
                if (this.nameIsFormalParameter && !this.valueIsFormalParameter)
                {
                    if (this._value != AutomationNull.Value)
                    {
                        if (this.valueList == null)
                        {
                            this.valueList = new ArrayList();
                        }
                        this.valueList.Add(this._value);
                    }
                }
                else
                {
                    this.SetVariable(this.names, this._value);
                }
            }
        }

        private void SetVariable(string[] varNames, object varValue)
        {
            CommandOrigin commandOrigin = base.MyInvocation.CommandOrigin;
            foreach (string str in varNames)
            {
                List<PSVariable> list = new List<PSVariable>();
                bool wasFiltered = false;
                if (!string.IsNullOrEmpty(base.Scope))
                {
                    list = base.GetMatchingVariables(str, base.Scope, out wasFiltered, false);
                }
                else
                {
                    list = base.GetMatchingVariables(str, "LOCAL", out wasFiltered, false);
                }
                if ((list.Count == 0) && !wasFiltered)
                {
                    try
                    {
                        ScopedItemOptions none = ScopedItemOptions.None;
                        if (!string.IsNullOrEmpty(base.Scope) && string.Equals("private", base.Scope, StringComparison.OrdinalIgnoreCase))
                        {
                            none = ScopedItemOptions.Private;
                        }
                        if (this.options.HasValue)
                        {
                            none |= (ScopedItemOptions) this.options.Value;
                        }
                        object obj2 = varValue;
                        if (obj2 == AutomationNull.Value)
                        {
                            obj2 = null;
                        }
                        PSVariable variable = new PSVariable(str, obj2, none);
                        if (this.description == null)
                        {
                            this.description = string.Empty;
                        }
                        variable.Description = this.Description;
                        if (this._visibility.HasValue)
                        {
                            variable.Visibility = this.Visibility;
                        }
                        string setVariableAction = VariableCommandStrings.SetVariableAction;
                        string target = StringUtil.Format(VariableCommandStrings.SetVariableTarget, str, obj2);
                        if (base.ShouldProcess(target, setVariableAction))
                        {
                            object sendToPipeline = null;
                            if (string.IsNullOrEmpty(base.Scope))
                            {
                                sendToPipeline = base.SessionState.Internal.SetVariable(variable, (bool) this.Force, commandOrigin);
                            }
                            else
                            {
                                sendToPipeline = base.SessionState.Internal.SetVariableAtScope(variable, base.Scope, (bool) this.Force, commandOrigin);
                            }
                            if (this.passThru && (sendToPipeline != null))
                            {
                                base.WriteObject(sendToPipeline);
                            }
                        }
                    }
                    catch (SessionStateException exception)
                    {
                        base.WriteError(new ErrorRecord(exception.ErrorRecord, exception));
                    }
                    catch (PSArgumentException exception2)
                    {
                        base.WriteError(new ErrorRecord(exception2.ErrorRecord, exception2));
                    }
                }
                else
                {
                    foreach (PSVariable variable2 in list)
                    {
                        string action = VariableCommandStrings.SetVariableAction;
                        string str5 = StringUtil.Format(VariableCommandStrings.SetVariableTarget, variable2.Name, varValue);
                        if (base.ShouldProcess(str5, action))
                        {
                            object obj4 = null;
                            try
                            {
                                bool flag2 = false;
                                if ((this.Force != 0) && ((variable2.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None))
                                {
                                    variable2.SetOptions(variable2.Options & ~ScopedItemOptions.ReadOnly, true);
                                    flag2 = true;
                                }
                                if (varValue != AutomationNull.Value)
                                {
                                    variable2.Value = varValue;
                                }
                                if (this.description != null)
                                {
                                    variable2.Description = this.description;
                                }
                                if (this.options.HasValue)
                                {
                                    variable2.Options = this.options.Value;
                                }
                                else if (flag2)
                                {
                                    variable2.SetOptions(variable2.Options | ScopedItemOptions.ReadOnly, true);
                                }
                                if (this._visibility.HasValue)
                                {
                                    variable2.Visibility = this.Visibility;
                                }
                                obj4 = variable2;
                            }
                            catch (SessionStateException exception3)
                            {
                                base.WriteError(new ErrorRecord(exception3.ErrorRecord, exception3));
                                continue;
                            }
                            catch (PSArgumentException exception4)
                            {
                                base.WriteError(new ErrorRecord(exception4.ErrorRecord, exception4));
                                continue;
                            }
                            if (this.passThru && (obj4 != null))
                            {
                                base.WriteObject(obj4);
                            }
                        }
                    }
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
        public ScopedItemOptions Option
        {
            get
            {
                return this.options.Value;
            }
            set
            {
                this.options = new ScopedItemOptions?(value);
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

