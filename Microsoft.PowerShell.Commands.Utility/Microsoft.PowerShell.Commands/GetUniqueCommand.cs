namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [Cmdlet("Get", "Unique", DefaultParameterSetName="AsString", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113335", RemotingCapability=RemotingCapability.None)]
    public sealed class GetUniqueCommand : PSCmdlet
    {
        private bool _asString;
        private ObjectCommandComparer _comparer;
        private PSObject _inputObject = AutomationNull.Value;
        private PSObject _lastObject;
        private string _lastObjectAsString;
        private bool _onType;

        protected override void ProcessRecord()
        {
            bool flag = true;
            if (this._lastObject == null)
            {
                if (AutomationNull.Value == this.InputObject)
                {
                    return;
                }
            }
            else if (this.OnType != 0)
            {
                flag = this.InputObject.InternalTypeNames[0] != this._lastObject.InternalTypeNames[0];
            }
            else if (this.AsString != 0)
            {
                string strA = this.InputObject.ToString();
                if (this._lastObjectAsString == null)
                {
                    this._lastObjectAsString = this._lastObject.ToString();
                }
                if (string.Compare(strA, this._lastObjectAsString, StringComparison.CurrentCulture) == 0)
                {
                    flag = false;
                }
                else
                {
                    this._lastObjectAsString = strA;
                }
            }
            else
            {
                if (this._comparer == null)
                {
                    this._comparer = new ObjectCommandComparer(true, CultureInfo.CurrentCulture, true);
                }
                flag = 0 != this._comparer.Compare(this.InputObject, this._lastObject);
            }
            if (flag)
            {
                base.WriteObject(this.InputObject);
                this._lastObject = this.InputObject;
            }
        }

        [Parameter(ParameterSetName="AsString")]
        public SwitchParameter AsString
        {
            get
            {
                return this._asString;
            }
            set
            {
                this._asString = (bool) value;
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

        [Parameter(ParameterSetName="UniqueByType")]
        public SwitchParameter OnType
        {
            get
            {
                return this._onType;
            }
            set
            {
                this._onType = (bool) value;
            }
        }
    }
}

