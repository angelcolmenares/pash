namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Runspaces;
    using System.Text;

    public class PSVariableProperty : PSNoteProperty
    {
        internal PSVariable _variable;

        public PSVariableProperty(PSVariable variable) : base((variable != null) ? variable.Name : null, null)
        {
            if (variable == null)
            {
                throw PSTraceSource.NewArgumentException("variable");
            }
            this._variable = variable;
        }

        public override PSMemberInfo Copy()
        {
            PSNoteProperty destiny = new PSVariableProperty(this._variable);
            base.CloneBaseProperties(destiny);
            return destiny;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.TypeNameOfValue);
            builder.Append(" ");
            builder.Append(this._variable.Name);
            builder.Append("=");
            builder.Append((this._variable.Value == null) ? "null" : this._variable.Value);
            return builder.ToString();
        }

        public override bool IsGettable
        {
            get
            {
                return true;
            }
        }

        public override bool IsSettable
        {
            get
            {
                return ((this._variable.Options & (ScopedItemOptions.Constant | ScopedItemOptions.ReadOnly)) == ScopedItemOptions.None);
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.NoteProperty;
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                object obj2 = this._variable.Value;
                if (obj2 == null)
                {
                    return string.Empty;
                }
                PSObject obj3 = obj2 as PSObject;
                if (obj3 != null)
                {
                    ConsolidatedString internalTypeNames = obj3.InternalTypeNames;
                    if ((internalTypeNames != null) && (internalTypeNames.Count >= 1))
                    {
                        return internalTypeNames[0];
                    }
                }
                return obj2.GetType().FullName;
            }
        }

        public override object Value
        {
            get
            {
                return this._variable.Value;
            }
            set
            {
                if (!base.IsInstance)
                {
                    throw new SetValueException("ChangeValueOfStaticNote", null, ExtendedTypeSystem.ChangeStaticMember, new object[] { base.Name });
                }
                this._variable.Value = value;
            }
        }
    }
}

