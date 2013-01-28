namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Reflection;

    [Cmdlet("Update", "TypeData", SupportsShouldProcess=true, DefaultParameterSetName="FileSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113421")]
    public class UpdateTypeDataCommand : UpdateData
    {
        private string _defaultDisplayProperty;
        private string[] _defaultDisplayPropertySet;
        private string[] _defaultKeyPropertySet;
        private bool? _inheritPropertySerializationSet;
        private string[] _propertySerializationSet;
        private int _serializationDepth = -2147483648;
        private string _serializationMethod;
        private string _stringSerializationSource;
        private Type _targetTypeForDeserialization;
        private System.Management.Automation.Runspaces.TypeData[] _typeData;
        private string _typeName;
        private const string DynamicTypeSet = "DynamicTypeSet";
        private bool force;
        private bool isMemberTypeSet;
        private string memberName;
        private PSMemberTypes memberType;
        private static object notSpecified = new object();
        private Type typeAdapter;
        private Type typeConverter;
        private const string TypeDataSet = "TypeDataSet";
        private object value1 = notSpecified;
        private object value2;

        protected override void BeginProcessing()
        {
            if (base.Context.TypeTable.isShared)
            {
                InvalidOperationException exception = new InvalidOperationException(TypesXmlStrings.SharedTypeTableCannotBeUpdated);
                base.ThrowTerminatingError(new ErrorRecord(exception, "CannotUpdateSharedTypeTable", ErrorCategory.InvalidOperation, null));
            }
        }

        private void EnsureMemberNameHasBeenSpecified()
        {
            if (string.IsNullOrEmpty(this.memberName))
            {
                base.ThrowTerminatingError(this.NewError("MemberNameShouldBeSpecified", "ShouldBeSpecified", null, new object[] { "MemberName", this.memberType }));
            }
        }

        private bool EnsureTypeDataIsNotEmpty(System.Management.Automation.Runspaces.TypeData typeData)
        {
            if ((((typeData.Members.Count == 0) && (typeData.StandardMembers.Count == 0)) && ((typeData.TypeConverter == null) && (typeData.TypeAdapter == null))) && (((typeData.DefaultDisplayPropertySet == null) && (typeData.DefaultKeyPropertySet == null)) && (typeData.PropertySerializationSet == null)))
            {
                base.WriteError(this.NewError("TypeDataEmpty", "TypeDataEmpty", null, new object[] { typeData.TypeName }));
                return false;
            }
            return true;
        }

        private void EnsureValue1AndValue2AreNotBothNull()
        {
            if ((this.value1 == null) && (this.value2 == null))
            {
                base.ThrowTerminatingError(this.NewError("ValueAndSecondValueAreNotBothNull", "Value1AndValue2AreNotBothNull", null, new object[] { this.memberType }));
            }
        }

        private void EnsureValue1HasBeenSpecified()
        {
            if (!HasBeenSpecified(this.value1))
            {
                base.ThrowTerminatingError(this.NewError("ValueShouldBeSpecified", "ShouldBeSpecified", null, new object[] { "Value", this.memberType }));
            }
        }

        private void EnsureValue1NotNullOrEmpty()
        {
            if (this.value1 is string)
            {
                if (string.IsNullOrEmpty((string) this.value1))
                {
                    base.ThrowTerminatingError(this.NewError("ValueShouldBeSpecified", "ShouldNotBeNull", null, new object[] { "Value", this.memberType }));
                }
            }
            else if (this.value1 == null)
            {
                base.ThrowTerminatingError(this.NewError("ValueShouldBeSpecified", "ShouldNotBeNull", null, new object[] { "Value", this.memberType }));
            }
        }

        private void EnsureValue2HasNotBeenSpecified()
        {
            if (this.value2 != null)
            {
                base.ThrowTerminatingError(this.NewError("SecondValueShouldNotBeSpecified", "ShouldNotBeSpecified", null, new object[] { "SecondValue", this.memberType }));
            }
        }

        private AliasPropertyData GetAliasProperty()
        {
            this.EnsureMemberNameHasBeenSpecified();
            this.EnsureValue1HasBeenSpecified();
            this.EnsureValue1NotNullOrEmpty();
            string parameterType = this.GetParameterType<string>(this.value1);
            if (this.value2 != null)
            {
                return new AliasPropertyData(this.memberName, parameterType, this.GetParameterType<Type>(this.value2));
            }
            return new AliasPropertyData(this.memberName, parameterType);
        }

        private CodeMethodData GetCodeMethod()
        {
            this.EnsureMemberNameHasBeenSpecified();
            this.EnsureValue1HasBeenSpecified();
            this.EnsureValue2HasNotBeenSpecified();
            return new CodeMethodData(this.memberName, this.GetParameterType<MethodInfo>(this.value1));
        }

        private CodePropertyData GetCodeProperty()
        {
            this.EnsureMemberNameHasBeenSpecified();
            this.EnsureValue1HasBeenSpecified();
            this.EnsureValue1AndValue2AreNotBothNull();
            MethodInfo getMethod = null;
            if (this.value1 != null)
            {
                getMethod = this.GetParameterType<MethodInfo>(this.value1);
            }
            MethodInfo setMethod = null;
            if (this.value2 != null)
            {
                setMethod = this.GetParameterType<MethodInfo>(this.value2);
            }
            return new CodePropertyData(this.memberName, getMethod, setMethod);
        }

        private void GetMembers(Dictionary<string, TypeMemberData> members)
        {
            if (!this.isMemberTypeSet)
            {
                if (((this.memberName != null) || HasBeenSpecified(this.value1)) || (this.value2 != null))
                {
                    base.ThrowTerminatingError(this.NewError("MemberTypeIsMissing", "MemberTypeIsMissing", null, new object[0]));
                }
            }
            else
            {
                switch (this.MemberType)
                {
                    case PSMemberTypes.ScriptProperty:
                    {
                        ScriptPropertyData scriptProperty = this.GetScriptProperty();
                        members.Add(scriptProperty.Name, scriptProperty);
                        return;
                    }
                    case PSMemberTypes.CodeMethod:
                    {
                        CodeMethodData codeMethod = this.GetCodeMethod();
                        members.Add(codeMethod.Name, codeMethod);
                        return;
                    }
                    case PSMemberTypes.ScriptMethod:
                    {
                        ScriptMethodData scriptMethod = this.GetScriptMethod();
                        members.Add(scriptMethod.Name, scriptMethod);
                        return;
                    }
                    case PSMemberTypes.AliasProperty:
                    {
                        AliasPropertyData aliasProperty = this.GetAliasProperty();
                        members.Add(aliasProperty.Name, aliasProperty);
                        return;
                    }
                    case PSMemberTypes.CodeProperty:
                    {
                        CodePropertyData codeProperty = this.GetCodeProperty();
                        members.Add(codeProperty.Name, codeProperty);
                        return;
                    }
                    case PSMemberTypes.NoteProperty:
                    {
                        NotePropertyData noteProperty = this.GetNoteProperty();
                        members.Add(noteProperty.Name, noteProperty);
                        return;
                    }
                }
                base.ThrowTerminatingError(this.NewError("CannotUpdateMemberType", "CannotUpdateMemberType", null, new object[] { this.memberType.ToString() }));
            }
        }

        private NotePropertyData GetNoteProperty()
        {
            this.EnsureMemberNameHasBeenSpecified();
            this.EnsureValue1HasBeenSpecified();
            this.EnsureValue2HasNotBeenSpecified();
            return new NotePropertyData(this.memberName, this.value1);
        }

        private T GetParameterType<T>(object sourceValue)
        {
            return (T) LanguagePrimitives.ConvertTo(sourceValue, typeof(T), CultureInfo.InvariantCulture);
        }

        private ScriptMethodData GetScriptMethod()
        {
            this.EnsureMemberNameHasBeenSpecified();
            this.EnsureValue1HasBeenSpecified();
            this.EnsureValue2HasNotBeenSpecified();
            return new ScriptMethodData(this.memberName, this.GetParameterType<ScriptBlock>(this.value1));
        }

        private ScriptPropertyData GetScriptProperty()
        {
            this.EnsureMemberNameHasBeenSpecified();
            this.EnsureValue1HasBeenSpecified();
            this.EnsureValue1AndValue2AreNotBothNull();
            ScriptBlock getScriptBlock = null;
            if (this.value1 != null)
            {
                getScriptBlock = this.GetParameterType<ScriptBlock>(this.value1);
            }
            ScriptBlock setScriptBlock = null;
            if (this.value2 != null)
            {
                setScriptBlock = this.GetParameterType<ScriptBlock>(this.value2);
            }
            return new ScriptPropertyData(this.memberName, getScriptBlock, setScriptBlock);
        }

        private static bool HasBeenSpecified(object obj)
        {
            return !object.ReferenceEquals(obj, notSpecified);
        }

        private ErrorRecord NewError(string errorId, string resourceId, object targetObject, params object[] args)
        {
            ErrorDetails details = new ErrorDetails(base.GetType().Assembly, "UpdateDataStrings", resourceId, args);
            return new ErrorRecord(new InvalidOperationException(details.Message), errorId, ErrorCategory.InvalidOperation, targetObject);
        }

        private void ProcessDynamicType()
        {
            if (string.IsNullOrWhiteSpace(this._typeName))
            {
                base.ThrowTerminatingError(this.NewError("TargetTypeNameEmpty", "TargetTypeNameEmpty", this._typeName, new object[0]));
            }
            System.Management.Automation.Runspaces.TypeData typeData = new System.Management.Automation.Runspaces.TypeData(this._typeName) {
                IsOverride = this.force
            };
            this.GetMembers(typeData.Members);
            if (this.typeConverter != null)
            {
                typeData.TypeConverter = this.typeConverter;
            }
            if (this.typeAdapter != null)
            {
                typeData.TypeAdapter = this.typeAdapter;
            }
            if (this._serializationMethod != null)
            {
                typeData.SerializationMethod = this._serializationMethod;
            }
            if (this._targetTypeForDeserialization != null)
            {
                typeData.TargetTypeForDeserialization = this._targetTypeForDeserialization;
            }
            if (this._serializationDepth != -2147483648)
            {
                typeData.SerializationDepth = this._serializationDepth;
            }
            if (this._defaultDisplayProperty != null)
            {
                typeData.DefaultDisplayProperty = this._defaultDisplayProperty;
            }
            if (this._inheritPropertySerializationSet.HasValue)
            {
                typeData.InheritPropertySerializationSet = this._inheritPropertySerializationSet.Value;
            }
            if (this._stringSerializationSource != null)
            {
                typeData.StringSerializationSource = this._stringSerializationSource;
            }
            if (this._defaultDisplayPropertySet != null)
            {
                PropertySetData data2 = new PropertySetData(this._defaultDisplayPropertySet);
                typeData.DefaultDisplayPropertySet = data2;
            }
            if (this._defaultKeyPropertySet != null)
            {
                PropertySetData data3 = new PropertySetData(this._defaultKeyPropertySet);
                typeData.DefaultKeyPropertySet = data3;
            }
            if (this._propertySerializationSet != null)
            {
                PropertySetData data4 = new PropertySetData(this._propertySerializationSet);
                typeData.PropertySerializationSet = data4;
            }
            if (this.EnsureTypeDataIsNotEmpty(typeData))
            {
                string updateTypeDataAction = UpdateDataStrings.UpdateTypeDataAction;
                string updateTypeDataTarget = UpdateDataStrings.UpdateTypeDataTarget;
                string target = string.Format(CultureInfo.InvariantCulture, updateTypeDataTarget, new object[] { this._typeName });
                if (base.ShouldProcess(target, updateTypeDataAction))
                {
                    try
                    {
                        Collection<string> errors = new Collection<string>();
                        base.Context.TypeTable.Update(typeData, errors, false, false);
                        if (errors.Count > 0)
                        {
                            foreach (string str4 in errors)
                            {
                                RuntimeException exception = new RuntimeException(str4);
                                base.WriteError(new ErrorRecord(exception, "TypesDynamicUpdateException", ErrorCategory.InvalidOperation, null));
                            }
                        }
                        else if (base.Context.RunspaceConfiguration != null)
                        {
                            base.Context.RunspaceConfiguration.Types.Append(new TypeConfigurationEntry(typeData, false));
                        }
                        else if (base.Context.InitialSessionState != null)
                        {
                            base.Context.InitialSessionState.Types.Add(new SessionStateTypeEntry(typeData, false));
                        }
                    }
                    catch (RuntimeException exception2)
                    {
                        base.WriteError(new ErrorRecord(exception2, "TypesDynamicUpdateException", ErrorCategory.InvalidOperation, null));
                    }
                }
            }
        }

        protected override void ProcessRecord()
        {
            string parameterSetName = base.ParameterSetName;
            if (parameterSetName != null)
            {
                if (!(parameterSetName == "FileSet"))
                {
                    if (!(parameterSetName == "DynamicTypeSet"))
                    {
                        if (parameterSetName == "TypeDataSet")
                        {
                            this.ProcessStrongTypeData();
                        }
                        return;
                    }
                }
                else
                {
                    this.ProcessTypeFiles();
                    return;
                }
                this.ProcessDynamicType();
            }
        }

        private void ProcessStrongTypeData()
        {
            string updateTypeDataAction = UpdateDataStrings.UpdateTypeDataAction;
            string updateTypeDataTarget = UpdateDataStrings.UpdateTypeDataTarget;
            foreach (System.Management.Automation.Runspaces.TypeData data in this._typeData)
            {
                if (this.EnsureTypeDataIsNotEmpty(data))
                {
                    System.Management.Automation.Runspaces.TypeData type = data.Copy();
                    if (this.force)
                    {
                        type.IsOverride = true;
                    }
                    string target = string.Format(CultureInfo.InvariantCulture, updateTypeDataTarget, new object[] { type.TypeName });
                    if (base.ShouldProcess(target, updateTypeDataAction))
                    {
                        try
                        {
                            Collection<string> errors = new Collection<string>();
                            base.Context.TypeTable.Update(type, errors, false, false);
                            if (errors.Count > 0)
                            {
                                foreach (string str4 in errors)
                                {
                                    RuntimeException exception = new RuntimeException(str4);
                                    base.WriteError(new ErrorRecord(exception, "TypesDynamicUpdateException", ErrorCategory.InvalidOperation, null));
                                }
                            }
                            else if (base.Context.RunspaceConfiguration != null)
                            {
                                base.Context.RunspaceConfiguration.Types.Append(new TypeConfigurationEntry(type, false));
                            }
                            else if (base.Context.InitialSessionState != null)
                            {
                                base.Context.InitialSessionState.Types.Add(new SessionStateTypeEntry(type, false));
                            }
                        }
                        catch (RuntimeException exception2)
                        {
                            base.WriteError(new ErrorRecord(exception2, "TypesDynamicUpdateException", ErrorCategory.InvalidOperation, null));
                        }
                    }
                }
            }
        }

        private void ProcessTypeFiles()
        {
            Collection<string> collection = UpdateData.Glob(base.PrependPath, "TypesPrependPathException", this);
            Collection<string> collection2 = UpdateData.Glob(base.AppendPath, "TypesAppendPathException", this);
            if (((base.PrependPath.Length <= 0) && (base.AppendPath.Length <= 0)) || ((collection.Count != 0) || (collection2.Count != 0)))
            {
                string updateTypeDataAction = UpdateDataStrings.UpdateTypeDataAction;
                string updateTarget = UpdateDataStrings.UpdateTarget;
                if (base.Context.RunspaceConfiguration != null)
                {
                    for (int i = collection.Count - 1; i >= 0; i--)
                    {
                        string target = string.Format(CultureInfo.InvariantCulture, updateTarget, new object[] { collection[i] });
                        if (base.ShouldProcess(target, updateTypeDataAction))
                        {
                            base.Context.RunspaceConfiguration.Types.Prepend(new TypeConfigurationEntry(collection[i]));
                        }
                    }
                    foreach (string str4 in collection2)
                    {
                        string str5 = string.Format(CultureInfo.InvariantCulture, updateTarget, new object[] { str4 });
                        if (base.ShouldProcess(str5, updateTypeDataAction))
                        {
                            base.Context.RunspaceConfiguration.Types.Append(new TypeConfigurationEntry(str4));
                        }
                    }
                    try
                    {
                        base.Context.CurrentRunspace.RunspaceConfiguration.Types.Update(true);
                        return;
                    }
                    catch (RuntimeException exception)
                    {
                        base.WriteError(new ErrorRecord(exception, "TypesXmlUpdateException", ErrorCategory.InvalidOperation, null));
                        return;
                    }
                }
                if (base.Context.InitialSessionState != null)
                {
                    HashSet<string> set = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
                    Collection<SessionStateTypeEntry> collection3 = new Collection<SessionStateTypeEntry>();
                    for (int j = collection.Count - 1; j >= 0; j--)
                    {
                        string str6 = string.Format(CultureInfo.InvariantCulture, updateTarget, new object[] { collection[j] });
                        string item = ModuleCmdletBase.ResolveRootedFilePath(collection[j], base.Context) ?? collection[j];
                        if (base.ShouldProcess(str6, updateTypeDataAction) && !set.Contains(item))
                        {
                            set.Add(item);
                            collection3.Add(new SessionStateTypeEntry(collection[j]));
                        }
                    }
                    foreach (SessionStateTypeEntry entry in base.Context.InitialSessionState.Types)
                    {
                        if (entry.FileName != null)
                        {
                            string str8 = ModuleCmdletBase.ResolveRootedFilePath(entry.FileName, base.Context) ?? entry.FileName;
                            if (!set.Contains(str8))
                            {
                                set.Add(str8);
                                collection3.Add(entry);
                            }
                        }
                        else
                        {
                            collection3.Add(entry);
                        }
                    }
                    foreach (string str9 in collection2)
                    {
                        string str10 = string.Format(CultureInfo.InvariantCulture, updateTarget, new object[] { str9 });
                        string str11 = ModuleCmdletBase.ResolveRootedFilePath(str9, base.Context) ?? str9;
                        if (base.ShouldProcess(str10, updateTypeDataAction) && !set.Contains(str11))
                        {
                            set.Add(str11);
                            collection3.Add(new SessionStateTypeEntry(str9));
                        }
                    }
                    base.Context.InitialSessionState.Types.Clear();
                    Collection<string> errors = new Collection<string>();
                    bool clearTable = true;
                    foreach (SessionStateTypeEntry entry2 in collection3)
                    {
                        try
                        {
                            if (entry2.TypeTable != null)
                            {
                                PSInvalidOperationException exception2 = new PSInvalidOperationException(UpdateDataStrings.CannotUpdateTypeWithTypeTable);
                                base.WriteError(new ErrorRecord(exception2, "CannotUpdateTypeWithTypeTable", ErrorCategory.InvalidOperation, null));
                                continue;
                            }
                            if (entry2.FileName != null)
                            {
                                bool flag2;
                                base.Context.TypeTable.Update(entry2.FileName, errors, clearTable, base.Context.AuthorizationManager, base.Context.InitialSessionState.Host, out flag2);
                            }
                            else
                            {
                                base.Context.TypeTable.Update(entry2.TypeData, errors, entry2.IsRemove, clearTable);
                            }
                        }
                        catch (RuntimeException exception3)
                        {
                            base.WriteError(new ErrorRecord(exception3, "TypesXmlUpdateException", ErrorCategory.InvalidOperation, null));
                        }
                        base.Context.InitialSessionState.Types.Add(entry2);
                        if (errors.Count > 0)
                        {
                            foreach (string str12 in errors)
                            {
                                RuntimeException exception4 = new RuntimeException(str12);
                                base.WriteError(new ErrorRecord(exception4, "TypesXmlUpdateException", ErrorCategory.InvalidOperation, null));
                            }
                            errors.Clear();
                        }
                        clearTable = false;
                    }
                }
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ParameterSetName="DynamicTypeSet")]
        public string DefaultDisplayProperty
        {
            get
            {
                return this._defaultDisplayProperty;
            }
            set
            {
                this._defaultDisplayProperty = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ParameterSetName="DynamicTypeSet")]
        public string[] DefaultDisplayPropertySet
        {
            get
            {
                return this._defaultDisplayPropertySet;
            }
            set
            {
                this._defaultDisplayPropertySet = value;
            }
        }

        [Parameter(ParameterSetName="DynamicTypeSet"), ValidateNotNullOrEmpty]
        public string[] DefaultKeyPropertySet
        {
            get
            {
                return this._defaultKeyPropertySet;
            }
            set
            {
                this._defaultKeyPropertySet = value;
            }
        }

        [Parameter(ParameterSetName="DynamicTypeSet"), Parameter(ParameterSetName="TypeDataSet")]
        public SwitchParameter Force
        {
            get
            {
                return this.force;
            }
            set
            {
                this.force = (bool) value;
            }
        }

        [ValidateNotNull, Parameter(ParameterSetName="DynamicTypeSet")]
        public bool? InheritPropertySerializationSet
        {
            get
            {
                return this._inheritPropertySerializationSet;
            }
            set
            {
                this._inheritPropertySerializationSet = value;
            }
        }

        [Parameter(ParameterSetName="DynamicTypeSet"), ValidateNotNullOrEmpty]
        public string MemberName
        {
            get
            {
                return this.memberName;
            }
            set
            {
                this.memberName = value;
            }
        }

        [ValidateNotNullOrEmpty, ValidateSet(new string[] { "NoteProperty", "AliasProperty", "ScriptProperty", "CodeProperty", "ScriptMethod", "CodeMethod" }, IgnoreCase=true), Parameter(ParameterSetName="DynamicTypeSet")]
        public PSMemberTypes MemberType
        {
            get
            {
                return this.memberType;
            }
            set
            {
                this.memberType = value;
                this.isMemberTypeSet = true;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ParameterSetName="DynamicTypeSet")]
        public string[] PropertySerializationSet
        {
            get
            {
                return this._propertySerializationSet;
            }
            set
            {
                this._propertySerializationSet = value;
            }
        }

        [ValidateNotNull, Parameter(ParameterSetName="DynamicTypeSet")]
        public object SecondValue
        {
            get
            {
                return this.value2;
            }
            set
            {
                this.value2 = value;
            }
        }

        [ValidateNotNull, Parameter(ParameterSetName="DynamicTypeSet"), ValidateRange(0, 0x7fffffff)]
        public int SerializationDepth
        {
            get
            {
                return this._serializationDepth;
            }
            set
            {
                this._serializationDepth = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ParameterSetName="DynamicTypeSet")]
        public string SerializationMethod
        {
            get
            {
                return this._serializationMethod;
            }
            set
            {
                this._serializationMethod = value;
            }
        }

        [Parameter(ParameterSetName="DynamicTypeSet"), ValidateNotNullOrEmpty]
        public string StringSerializationSource
        {
            get
            {
                return this._stringSerializationSource;
            }
            set
            {
                this._stringSerializationSource = value;
            }
        }

        [Parameter(ParameterSetName="DynamicTypeSet"), ValidateNotNull]
        public Type TargetTypeForDeserialization
        {
            get
            {
                return this._targetTypeForDeserialization;
            }
            set
            {
                this._targetTypeForDeserialization = value;
            }
        }

        [ValidateNotNull, Parameter(ParameterSetName="DynamicTypeSet")]
        public Type TypeAdapter
        {
            get
            {
                return this.typeAdapter;
            }
            set
            {
                this.typeAdapter = value;
            }
        }

        [Parameter(ParameterSetName="DynamicTypeSet"), ValidateNotNull]
        public Type TypeConverter
        {
            get
            {
                return this.typeConverter;
            }
            set
            {
                this.typeConverter = value;
            }
        }

        [Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="TypeDataSet")]
        public System.Management.Automation.Runspaces.TypeData[] TypeData
        {
            get
            {
                return this._typeData;
            }
            set
            {
                this._typeData = value;
            }
        }

        [ArgumentToTypeNameTransformation, Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="DynamicTypeSet"), ValidateNotNullOrEmpty]
        public string TypeName
        {
            get
            {
                return this._typeName;
            }
            set
            {
                this._typeName = value;
            }
        }

        [Parameter(ParameterSetName="DynamicTypeSet")]
        public object Value
        {
            get
            {
                return this.value1;
            }
            set
            {
                this.value1 = value;
            }
        }
    }
}

