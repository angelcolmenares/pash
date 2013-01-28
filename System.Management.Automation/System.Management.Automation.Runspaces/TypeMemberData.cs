namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public abstract class TypeMemberData
    {
        private string _name;

        internal TypeMemberData(string name)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name.Trim()))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            this._name = name;
        }

        internal abstract TypeMemberData Copy();

        public string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}

