namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    public sealed class TypeData
    {
        private string _defaultDisplayProperty;
        private PropertySetData _defaultDisplayPropertySet;
        private PropertySetData _defaultKeyPropertySet;
        private bool _inheritPropertySerializationSet;
        private bool _isOverride;
        private Dictionary<string, TypeMemberData> _members;
        private PropertySetData _propertySerializationSet;
        private int _serializationDepth;
        private string _serializationMethod;
        private Dictionary<string, TypeMemberData> _standardMembers;
        private string _stringSerializationSource;
        private Type _targetTypeForDeserialization;
        private Type _typeAdapter;
        private Type _typeConverter;
        private string _typeName;
        internal const string AliasProperty = "AliasProperty";
        internal const string CodeMethod = "CodeMethod";
        internal const string CodeProperty = "CodeProperty";
        internal const string MemberSet = "MemberSet";
        internal const string NoteProperty = "NoteProperty";
        internal const string PropertySet = "PropertySet";
        internal const string ScriptMethod = "ScriptMethod";
        internal const string ScriptProperty = "ScriptProperty";

        public TypeData(string typeName)
        {
            this._members = new Dictionary<string, TypeMemberData>(StringComparer.OrdinalIgnoreCase);
            this._standardMembers = new Dictionary<string, TypeMemberData>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(typeName.Trim()))
            {
                throw PSTraceSource.NewArgumentNullException("typeName");
            }
            this._typeName = typeName;
        }

        public TypeData(Type type)
        {
            this._members = new Dictionary<string, TypeMemberData>(StringComparer.OrdinalIgnoreCase);
            this._standardMembers = new Dictionary<string, TypeMemberData>(StringComparer.OrdinalIgnoreCase);
            if (type == null)
            {
                throw PSTraceSource.NewArgumentNullException("type");
            }
            this._typeName = type.FullName;
        }

        public TypeData Copy()
        {
            TypeData data = new TypeData(this.TypeName);
            foreach (KeyValuePair<string, TypeMemberData> pair in this.Members)
            {
                data.Members.Add(pair.Key, pair.Value.Copy());
            }
            data.TypeConverter = this.TypeConverter;
            data.TypeAdapter = this.TypeAdapter;
            data.IsOverride = this.IsOverride;
            foreach (KeyValuePair<string, TypeMemberData> pair2 in this.StandardMembers)
            {
                switch (pair2.Key)
                {
                    case "SerializationMethod":
                        data.SerializationMethod = this.SerializationMethod;
                        break;

                    case "TargetTypeForDeserialization":
                        data.TargetTypeForDeserialization = this.TargetTypeForDeserialization;
                        break;

                    case "SerializationDepth":
                        data.SerializationDepth = this.SerializationDepth;
                        break;

                    case "DefaultDisplayProperty":
                        data.DefaultDisplayProperty = this.DefaultDisplayProperty;
                        break;

                    case "InheritPropertySerializationSet":
                        data.InheritPropertySerializationSet = this.InheritPropertySerializationSet;
                        break;

                    case "StringSerializationSource":
                        data.StringSerializationSource = this.StringSerializationSource;
                        break;
                }
            }
            data.DefaultDisplayPropertySet = (this.DefaultDisplayPropertySet == null) ? null : this.DefaultDisplayPropertySet.Copy();
            data.DefaultKeyPropertySet = (this.DefaultKeyPropertySet == null) ? null : this.DefaultKeyPropertySet.Copy();
            data.PropertySerializationSet = (this.PropertySerializationSet == null) ? null : this.PropertySerializationSet.Copy();
            return data;
        }

        public string DefaultDisplayProperty
        {
            get
            {
                return this._defaultDisplayProperty;
            }
            set
            {
                this._defaultDisplayProperty = value;
                if (this._defaultDisplayProperty == null)
                {
                    if (this._standardMembers.ContainsKey("DefaultDisplayProperty"))
                    {
                        this._standardMembers.Remove("DefaultDisplayProperty");
                    }
                }
                else if (this._standardMembers.ContainsKey("DefaultDisplayProperty"))
                {
                    NotePropertyData data = this._standardMembers["DefaultDisplayProperty"] as NotePropertyData;
                    data.Value = this._defaultDisplayProperty;
                }
                else
                {
                    NotePropertyData data2 = new NotePropertyData("DefaultDisplayProperty", this._defaultDisplayProperty);
                    this._standardMembers.Add("DefaultDisplayProperty", data2);
                }
            }
        }

        public PropertySetData DefaultDisplayPropertySet
        {
            get
            {
                return this._defaultDisplayPropertySet;
            }
            set
            {
                this._defaultDisplayPropertySet = value;
                if (this._defaultDisplayPropertySet != null)
                {
                    this._defaultDisplayPropertySet.Name = "DefaultDisplayPropertySet";
                }
            }
        }

        public PropertySetData DefaultKeyPropertySet
        {
            get
            {
                return this._defaultKeyPropertySet;
            }
            set
            {
                this._defaultKeyPropertySet = value;
                if (this._defaultKeyPropertySet != null)
                {
                    this._defaultKeyPropertySet.Name = "DefaultKeyPropertySet";
                }
            }
        }

        public bool InheritPropertySerializationSet
        {
            get
            {
                return this._inheritPropertySerializationSet;
            }
            set
            {
                this._inheritPropertySerializationSet = value;
                if (this._standardMembers.ContainsKey("InheritPropertySerializationSet"))
                {
                    NotePropertyData data = this._standardMembers["InheritPropertySerializationSet"] as NotePropertyData;
                    data.Value = this._inheritPropertySerializationSet;
                }
                else
                {
                    NotePropertyData data2 = new NotePropertyData("InheritPropertySerializationSet", this._inheritPropertySerializationSet);
                    this._standardMembers.Add("InheritPropertySerializationSet", data2);
                }
            }
        }

        public bool IsOverride
        {
            get
            {
                return this._isOverride;
            }
            set
            {
                this._isOverride = value;
            }
        }

        public Dictionary<string, TypeMemberData> Members
        {
            get
            {
                return this._members;
            }
        }

        public PropertySetData PropertySerializationSet
        {
            get
            {
                return this._propertySerializationSet;
            }
            set
            {
                this._propertySerializationSet = value;
                if (this._propertySerializationSet != null)
                {
                    this._propertySerializationSet.Name = "PropertySerializationSet";
                }
            }
        }

        public int SerializationDepth
        {
            get
            {
                return this._serializationDepth;
            }
            set
            {
                this._serializationDepth = value;
                if (this._standardMembers.ContainsKey("SerializationDepth"))
                {
                    NotePropertyData data = this._standardMembers["SerializationDepth"] as NotePropertyData;
                    data.Value = this._serializationDepth;
                }
                else
                {
                    NotePropertyData data2 = new NotePropertyData("SerializationDepth", this._serializationDepth);
                    this._standardMembers.Add("SerializationDepth", data2);
                }
            }
        }

        public string SerializationMethod
        {
            get
            {
                return this._serializationMethod;
            }
            set
            {
                this._serializationMethod = value;
                if (this._serializationMethod == null)
                {
                    if (this._standardMembers.ContainsKey("SerializationMethod"))
                    {
                        this._standardMembers.Remove("SerializationMethod");
                    }
                }
                else if (this._standardMembers.ContainsKey("SerializationMethod"))
                {
                    NotePropertyData data = this._standardMembers["SerializationMethod"] as NotePropertyData;
                    data.Value = this._serializationMethod;
                }
                else
                {
                    NotePropertyData data2 = new NotePropertyData("SerializationMethod", this._serializationMethod);
                    this._standardMembers.Add("SerializationMethod", data2);
                }
            }
        }

        internal Dictionary<string, TypeMemberData> StandardMembers
        {
            get
            {
                return this._standardMembers;
            }
        }

        public string StringSerializationSource
        {
            get
            {
                return this._stringSerializationSource;
            }
            set
            {
                this._stringSerializationSource = value;
                if (this._stringSerializationSource == null)
                {
                    if (this._standardMembers.ContainsKey("StringSerializationSource"))
                    {
                        this._standardMembers.Remove("StringSerializationSource");
                    }
                }
                else if (this._standardMembers.ContainsKey("StringSerializationSource"))
                {
                    AliasPropertyData data = this._standardMembers["StringSerializationSource"] as AliasPropertyData;
                    data.ReferencedMemberName = this._stringSerializationSource;
                }
                else
                {
                    AliasPropertyData data2 = new AliasPropertyData("StringSerializationSource", this._stringSerializationSource);
                    this._standardMembers.Add("StringSerializationSource", data2);
                }
            }
        }

        public Type TargetTypeForDeserialization
        {
            get
            {
                return this._targetTypeForDeserialization;
            }
            set
            {
                this._targetTypeForDeserialization = value;
                if (this._targetTypeForDeserialization == null)
                {
                    if (this._standardMembers.ContainsKey("TargetTypeForDeserialization"))
                    {
                        this._standardMembers.Remove("TargetTypeForDeserialization");
                    }
                }
                else if (this._standardMembers.ContainsKey("TargetTypeForDeserialization"))
                {
                    NotePropertyData data = this._standardMembers["TargetTypeForDeserialization"] as NotePropertyData;
                    data.Value = this._targetTypeForDeserialization;
                }
                else
                {
                    NotePropertyData data2 = new NotePropertyData("TargetTypeForDeserialization", this._targetTypeForDeserialization);
                    this._standardMembers.Add("TargetTypeForDeserialization", data2);
                }
            }
        }

        public Type TypeAdapter
        {
            get
            {
                return this._typeAdapter;
            }
            set
            {
                this._typeAdapter = value;
            }
        }

        public Type TypeConverter
        {
            get
            {
                return this._typeConverter;
            }
            set
            {
                this._typeConverter = value;
            }
        }

        public string TypeName
        {
            get
            {
                return this._typeName;
            }
        }
    }
}

