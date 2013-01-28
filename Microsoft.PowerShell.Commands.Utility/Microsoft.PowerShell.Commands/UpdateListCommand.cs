namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Management.Automation;

    [Cmdlet("Update", "List", DefaultParameterSetName="AddRemoveSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113447", RemotingCapability=RemotingCapability.None)]
    public class UpdateListCommand : PSCmdlet
    {
        private object[] _add;
        private PSObject _inputobject;
        private string _property;
        private object[] _remove;
        private object[] _replace;
        private PSListModifier listModifier;

        private Hashtable CreateHashtable()
        {
            Hashtable hashtable = new Hashtable(2);
            if (this.Add != null)
            {
                hashtable.Add("Add", this.Add);
            }
            if (this.Remove != null)
            {
                hashtable.Add("Remove", this.Remove);
            }
            if (this.Replace != null)
            {
                hashtable.Add("Replace", this.Replace);
            }
            return hashtable;
        }

        private PSListModifier CreatePSListModifier()
        {
            PSListModifier modifier = new PSListModifier();
            if (this.Add != null)
            {
                foreach (object obj2 in this.Add)
                {
                    modifier.Add.Add(obj2);
                }
            }
            if (this.Remove != null)
            {
                foreach (object obj3 in this.Remove)
                {
                    modifier.Remove.Add(obj3);
                }
            }
            if (this.Replace != null)
            {
                foreach (object obj4 in this.Replace)
                {
                    modifier.Replace.Add(obj4);
                }
            }
            return modifier;
        }

        protected override void EndProcessing()
        {
            if (this.Property == null)
            {
                if (this.InputObject != null)
                {
                    base.ThrowTerminatingError(this.NewError("MissingPropertyParameter", "MissingPropertyParameter", null, new object[0]));
                }
                else
                {
                    base.WriteObject(this.CreateHashtable());
                }
            }
        }

        private ErrorRecord NewError(string errorId, string resourceId, object targetObject, params object[] args)
        {
            ErrorDetails details = new ErrorDetails(base.GetType().Assembly, "UpdateListStrings", resourceId, args);
            return new ErrorRecord(new InvalidOperationException(details.Message), errorId, ErrorCategory.InvalidOperation, targetObject);
        }

        protected override void ProcessRecord()
        {
            if (this.Property != null)
            {
                if (this.InputObject == null)
                {
                    base.WriteError(this.NewError("MissingInputObjectParameter", "MissingInputObjectParameter", null, new object[0]));
                }
                else
                {
                    if (this.listModifier == null)
                    {
                        this.listModifier = this.CreatePSListModifier();
                    }
                    PSMemberInfo info = this.InputObject.Members[this.Property];
                    if (info != null)
                    {
                        try
                        {
                            this.listModifier.ApplyTo(info.Value);
                            base.WriteObject(this.InputObject);
                        }
                        catch (PSInvalidOperationException exception)
                        {
                            base.WriteError(new ErrorRecord(exception, "ApplyFailed", ErrorCategory.InvalidOperation, null));
                        }
                    }
                    else
                    {
                        base.WriteError(this.NewError("MemberDoesntExist", "MemberDoesntExist", this.InputObject, new object[] { this.Property }));
                    }
                }
            }
        }

        [Parameter(ParameterSetName="AddRemoveSet"), ValidateNotNullOrEmpty]
        public object[] Add
        {
            get
            {
                return this._add;
            }
            set
            {
                this._add = value;
            }
        }

        [Parameter(ValueFromPipeline=true), ValidateNotNullOrEmpty]
        public PSObject InputObject
        {
            get
            {
                return this._inputobject;
            }
            set
            {
                this._inputobject = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Position=0)]
        public string Property
        {
            get
            {
                return this._property;
            }
            set
            {
                this._property = value;
            }
        }

        [Parameter(ParameterSetName="AddRemoveSet"), ValidateNotNullOrEmpty]
        public object[] Remove
        {
            get
            {
                return this._remove;
            }
            set
            {
                this._remove = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Mandatory=true, ParameterSetName="ReplaceSet")]
        public object[] Replace
        {
            get
            {
                return this._replace;
            }
            set
            {
                this._replace = value;
            }
        }
    }
}

