namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Reflection;

    public sealed class CodeMethodData : TypeMemberData
    {
        private MethodInfo _codeReference;

        public CodeMethodData(string name, MethodInfo methodToCall) : base(name)
        {
            this._codeReference = methodToCall;
        }

        internal override TypeMemberData Copy()
        {
            return new CodeMethodData(base.Name, this.CodeReference);
        }

        public MethodInfo CodeReference
        {
            get
            {
                return this._codeReference;
            }
            set
            {
                this._codeReference = value;
            }
        }
    }
}

