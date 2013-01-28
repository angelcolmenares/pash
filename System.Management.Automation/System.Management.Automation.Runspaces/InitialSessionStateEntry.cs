namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public abstract class InitialSessionStateEntry
    {
        private PSModuleInfo _module;
        private string _name;
        private PSSnapInInfo _psSnapIn;

        protected InitialSessionStateEntry(string name)
        {
            this._name = name;
        }

        public abstract InitialSessionStateEntry Clone();
        internal void SetModule(PSModuleInfo module)
        {
            this._module = module;
        }

        internal void SetPSSnapIn(PSSnapInInfo psSnapIn)
        {
            this._psSnapIn = psSnapIn;
        }

        public PSModuleInfo Module
        {
            get
            {
                return this._module;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            internal set
            {
                this._name = value;
            }
        }

        public PSSnapInInfo PSSnapIn
        {
            get
            {
                return this._psSnapIn;
            }
        }
    }
}

