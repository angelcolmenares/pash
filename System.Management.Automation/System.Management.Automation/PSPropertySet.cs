namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;

    public class PSPropertySet : PSMemberInfo
    {
        private Collection<string> referencedPropertyNames;

        public PSPropertySet(string name, IEnumerable<string> referencedPropertyNames)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            if (referencedPropertyNames == null)
            {
                throw PSTraceSource.NewArgumentNullException("referencedPropertyNames");
            }
            this.referencedPropertyNames = new Collection<string>();
            foreach (string str in referencedPropertyNames)
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw PSTraceSource.NewArgumentException("referencedPropertyNames");
                }
                this.referencedPropertyNames.Add(str);
            }
        }

        public override PSMemberInfo Copy()
        {
            PSPropertySet destiny = new PSPropertySet(base.name, this.referencedPropertyNames);
            base.CloneBaseProperties(destiny);
            return destiny;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(base.Name);
            builder.Append(" {");
            if (this.referencedPropertyNames.Count != 0)
            {
                foreach (string str in this.referencedPropertyNames)
                {
                    builder.Append(str);
                    builder.Append(", ");
                }
                builder.Remove(builder.Length - 2, 2);
            }
            builder.Append("}");
            return builder.ToString();
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.PropertySet;
            }
        }

        public Collection<string> ReferencedPropertyNames
        {
            get
            {
                return this.referencedPropertyNames;
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                return typeof(PSPropertySet).FullName;
            }
        }

        public override object Value
        {
            get
            {
                return this;
            }
            set
            {
                throw new ExtendedTypeSystemException("CannotChangePSPropertySetValue", null, ExtendedTypeSystem.CannotSetValueForMemberType, new object[] { base.GetType().FullName });
            }
        }
    }
}

