namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    [Cmdlet("Get", "Variable", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113336"), OutputType(new Type[] { typeof(PSVariable) })]
    public class GetVariableCommand : VariableCommandBase
    {
        private string[] name = new string[] { "*" };
        private bool valueOnly;

        protected override void ProcessRecord()
        {
            foreach (string str in this.name)
            {
                bool wasFiltered = false;
                List<PSVariable> list = base.GetMatchingVariables(str, base.Scope, out wasFiltered, false);
                list.Sort((Comparison<PSVariable>) ((left, right) => StringComparer.CurrentCultureIgnoreCase.Compare(left.Name, right.Name)));
                bool flag2 = false;
                foreach (PSVariable variable in list)
                {
                    flag2 = true;
                    if (this.valueOnly)
                    {
                        base.WriteObject(variable.Value);
                    }
                    else
                    {
                        base.WriteObject(variable);
                    }
                }
                if (!flag2 && !wasFiltered)
                {
                    ItemNotFoundException replaceParentContainsErrorRecordException = new ItemNotFoundException(str, "VariableNotFound", SessionStateStrings.VariableNotFound);
                    base.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
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

        [ValidateNotNull, Parameter(Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
        public string[] Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value == null)
                {
                    value = new string[] { "*" };
                }
                this.name = value;
            }
        }

        [Parameter]
        public SwitchParameter ValueOnly
        {
            get
            {
                return this.valueOnly;
            }
            set
            {
                this.valueOnly = (bool) value;
            }
        }
    }
}

