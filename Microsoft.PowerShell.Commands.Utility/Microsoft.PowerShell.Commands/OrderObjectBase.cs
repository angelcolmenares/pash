namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    public class OrderObjectBase : ObjectCmdletBase
    {
        private PSObject _inputObject = AutomationNull.Value;
        private bool ascending = true;
        private object[] expr;
        private List<PSObject> inputObjects = new List<PSObject>();

        protected override void ProcessRecord()
        {
            if ((this.InputObject != null) && (this.InputObject != AutomationNull.Value))
            {
                this.inputObjects.Add(this.InputObject);
            }
        }

        internal CultureInfo ConvertedCulture
        {
            get
            {
                return base._cultureInfo;
            }
        }

        internal SwitchParameter DescendingOrder
        {
            get
            {
                return !this.ascending;
            }
            set
            {
                this.ascending = value == 0;
            }
        }

        [Parameter(ValueFromPipeline=true)]
        public PSObject InputObject
        {
            get
            {
                return this._inputObject;
            }
            set
            {
                this._inputObject = value;
            }
        }

        internal List<PSObject> InputObjects
        {
            get
            {
                return this.inputObjects;
            }
        }

        [Parameter(Position=0)]
        public object[] Property
        {
            get
            {
                return this.expr;
            }
            set
            {
                this.expr = value;
            }
        }
    }
}

