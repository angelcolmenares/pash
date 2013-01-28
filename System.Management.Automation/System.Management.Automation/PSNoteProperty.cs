namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Runspaces;
    using System.Text;

    public class PSNoteProperty : PSPropertyInfo
    {
        internal object noteValue;

        public PSNoteProperty(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            this.noteValue = value;
        }

        public override PSMemberInfo Copy()
        {
            PSNoteProperty destiny = new PSNoteProperty(base.name, this.noteValue);
            base.CloneBaseProperties(destiny);
            return destiny;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.TypeNameOfValue);
            builder.Append(" ");
            builder.Append(base.Name);
            builder.Append("=");
            builder.Append((this.noteValue == null) ? "null" : this.noteValue.ToString());
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
                return base.IsInstance;
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
                object obj2 = this.Value;
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
                return this.noteValue;
            }
            set
            {
                if (!base.IsInstance)
                {
                    throw new SetValueException("ChangeValueOfStaticNote", null, ExtendedTypeSystem.ChangeStaticMember, new object[] { base.Name });
                }
                this.noteValue = value;
            }
        }
    }
}

