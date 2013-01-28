namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public abstract class RunspaceConfigurationEntry
    {
        internal UpdateAction _action;
        internal bool _builtIn;
        private string _name;
        private PSSnapInInfo _PSSnapin;

        protected RunspaceConfigurationEntry(string name)
        {
            this._action = UpdateAction.None;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name.Trim()))
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            this._name = name.Trim();
        }

        internal RunspaceConfigurationEntry(string name, PSSnapInInfo psSnapin)
        {
            this._action = UpdateAction.None;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name.Trim()))
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            this._name = name.Trim();
            if (psSnapin == null)
            {
                throw PSTraceSource.NewArgumentException("psSnapin");
            }
            this._PSSnapin = psSnapin;
        }

        internal UpdateAction Action
        {
            get
            {
                return this._action;
            }
        }

        public bool BuiltIn
        {
            get
            {
                return this._builtIn;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public PSSnapInInfo PSSnapIn
        {
            get
            {
                return this._PSSnapin;
            }
        }
    }
}

