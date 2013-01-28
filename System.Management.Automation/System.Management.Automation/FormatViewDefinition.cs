namespace System.Management.Automation
{
    using System;
    using System.Globalization;

    public sealed class FormatViewDefinition
    {
        private PSControl _control;
        private Guid _instanceId;
        private string _name;

        public FormatViewDefinition(string name, PSControl control)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            if (control == null)
            {
                throw PSTraceSource.NewArgumentNullException("control");
            }
            this._name = name;
            this._control = control;
        }

        internal FormatViewDefinition(string name, PSControl control, Guid instanceid)
        {
            this._name = name;
            this._control = control;
            this._instanceId = instanceid;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} , {1}", new object[] { this._name, this._control.ToString() });
        }

        public PSControl Control
        {
            get
            {
                return this._control;
            }
        }

        internal Guid InstanceId
        {
            get
            {
                return this._instanceId;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}

