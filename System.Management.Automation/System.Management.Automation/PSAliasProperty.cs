namespace System.Management.Automation
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    public class PSAliasProperty : PSPropertyInfo
    {
        private Type conversionType;
        private string referencedMemberName;

        public PSAliasProperty(string name, string referencedMemberName)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            if (string.IsNullOrEmpty(referencedMemberName))
            {
                throw PSTraceSource.NewArgumentException("referencedMemberName");
            }
            this.referencedMemberName = referencedMemberName;
        }

        public PSAliasProperty(string name, string referencedMemberName, Type conversionType)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            if (string.IsNullOrEmpty(referencedMemberName))
            {
                throw PSTraceSource.NewArgumentException("referencedMemberName");
            }
            this.referencedMemberName = referencedMemberName;
            this.conversionType = conversionType;
        }

        public override PSMemberInfo Copy()
        {
            PSAliasProperty destiny = new PSAliasProperty(base.name, this.referencedMemberName) {
                conversionType = this.conversionType
            };
            base.CloneBaseProperties(destiny);
            return destiny;
        }

        private PSMemberInfo LookupMember(string name)
        {
            bool flag;
            PSMemberInfo info;
            this.LookupMember(name, new HybridDictionary(), out info, out flag);
            if (flag)
            {
                throw new ExtendedTypeSystemException("CycleInAliasLookup", null, ExtendedTypeSystem.CycleInAlias, new object[] { base.Name });
            }
            return info;
        }

        private void LookupMember(string name, HybridDictionary visitedAliases, out PSMemberInfo returnedMember, out bool hasCycle)
        {
            returnedMember = null;
            if (base.instance == null)
            {
                throw new ExtendedTypeSystemException("AliasLookupMemberOutsidePSObject", null, ExtendedTypeSystem.AccessMemberOutsidePSObject, new object[] { name });
            }
            PSMemberInfo info = PSObject.AsPSObject(base.instance).Properties[name];
            if (info == null)
            {
                throw new ExtendedTypeSystemException("AliasLookupMemberNotPresent", null, ExtendedTypeSystem.MemberNotPresent, new object[] { name });
            }
            PSAliasProperty property = info as PSAliasProperty;
            if (property == null)
            {
                hasCycle = false;
                returnedMember = info;
            }
            else if (visitedAliases.Contains(name))
            {
                hasCycle = true;
            }
            else
            {
                visitedAliases.Add(name, name);
                this.LookupMember(property.ReferencedMemberName, visitedAliases, out returnedMember, out hasCycle);
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(base.Name);
            builder.Append(" = ");
            if (this.conversionType != null)
            {
                builder.Append("(");
                builder.Append(this.conversionType);
                builder.Append(")");
            }
            builder.Append(this.referencedMemberName);
            return builder.ToString();
        }

        public Type ConversionType
        {
            get
            {
                return this.conversionType;
            }
        }

        public override bool IsGettable
        {
            get
            {
                PSPropertyInfo referencedMember = this.ReferencedMember as PSPropertyInfo;
                return ((referencedMember != null) && referencedMember.IsGettable);
            }
        }

        public override bool IsSettable
        {
            get
            {
                PSPropertyInfo referencedMember = this.ReferencedMember as PSPropertyInfo;
                return ((referencedMember != null) && referencedMember.IsSettable);
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.AliasProperty;
            }
        }

        internal PSMemberInfo ReferencedMember
        {
            get
            {
                return this.LookupMember(this.referencedMemberName);
            }
        }

        public string ReferencedMemberName
        {
            get
            {
                return this.referencedMemberName;
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                if (this.conversionType != null)
                {
                    return this.conversionType.FullName;
                }
                return this.ReferencedMember.TypeNameOfValue;
            }
        }

        public override object Value
        {
            get
            {
                object valueToConvert = this.ReferencedMember.Value;
                if (this.conversionType != null)
                {
                    valueToConvert = LanguagePrimitives.ConvertTo(valueToConvert, this.conversionType, CultureInfo.InvariantCulture);
                }
                return valueToConvert;
            }
            set
            {
                this.ReferencedMember.Value = value;
            }
        }
    }
}

