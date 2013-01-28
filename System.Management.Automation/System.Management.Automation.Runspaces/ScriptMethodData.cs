namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class ScriptMethodData : TypeMemberData
    {
        private ScriptBlock _script;

        public ScriptMethodData(string name, ScriptBlock scriptToInvoke) : base(name)
        {
            this._script = scriptToInvoke;
        }

        internal override TypeMemberData Copy()
        {
            return new ScriptMethodData(base.Name, this.Script);
        }

        public ScriptBlock Script
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
    }
}

