namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Reflection;

    [Cmdlet("Add", "Member", DefaultParameterSetName="TypeNameSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113280", RemotingCapability=RemotingCapability.None)]
    public class AddMemberCommand : PSCmdlet
    {
        private string _notePropertyName;
        private object _notePropertyValue;
        private IDictionary _property;
        private bool force;
        private PSObject inputObject;
        private string memberName;
        private PSMemberTypes memberType;
        private const string NotePropertyMultiMemberSet = "NotePropertyMultiMemberSet";
        private const string NotePropertySingleMemberSet = "NotePropertySingleMemberSet";
        private static object notSpecified = new object();
        private bool passThru;
        private string typeName;
        private object value1 = notSpecified;
        private object value2 = notSpecified;

        private bool AddMemberToTarget(PSMemberInfo member)
        {
            PSMemberInfo info = this.inputObject.Members[member.Name];
            if (info != null)
            {
                if (!this.force)
                {
                    base.WriteError(this.NewError("MemberAlreadyExists", "MemberAlreadyExists", this.inputObject, new object[] { member.Name }));
                    return false;
                }
                if (!info.IsInstance)
                {
                    base.WriteError(this.NewError("CannotRemoveTypeDataMember", "CannotRemoveTypeDataMember", this.inputObject, new object[] { member.Name, info.MemberType }));
                    return false;
                }
                this.inputObject.Members.Remove(member.Name);
            }
            this.inputObject.Members.Add(member);
            return true;
        }

        private void EnsureValue1AndValue2AreNotBothNull()
        {
            if ((this.value1 == null) && ((this.value2 == null) || !HasBeenSpecified(this.value2)))
            {
                base.ThrowTerminatingError(this.NewError("Value1AndValue2AreNotBothNull", "Value1AndValue2AreNotBothNull", null, new object[] { this.memberType }));
            }
        }

        private void EnsureValue1HasBeenSpecified()
        {
            if (!HasBeenSpecified(this.value1))
            {
                Collection<FieldDescription> descriptions = new Collection<FieldDescription> {
                    new FieldDescription("Value")
                };
                string caption = StringUtil.Format(AddMember.Value1Prompt, this.memberType);
                Dictionary<string, PSObject> dictionary = base.Host.UI.Prompt(caption, null, descriptions);
                if (dictionary != null)
                {
                    this.value1 = dictionary["Value"].BaseObject;
                }
            }
        }

        private void EnsureValue1IsNotNull()
        {
            if (this.value1 == null)
            {
                base.ThrowTerminatingError(this.NewError("Value1ShouldNotBeNull", "Value1ShouldNotBeNull", null, new object[] { this.memberType }));
            }
        }

        private void EnsureValue2HasNotBeenSpecified()
        {
            if (HasBeenSpecified(this.value2))
            {
                base.ThrowTerminatingError(this.NewError("Value2ShouldNotBeSpecified", "Value2ShouldNotBeSpecified", null, new object[] { this.memberType }));
            }
        }

        private void EnsureValue2IsNotNull()
        {
            if (this.value2 == null)
            {
                base.ThrowTerminatingError(this.NewError("Value2ShouldNotBeNull", "Value2ShouldNotBeNull", null, new object[] { this.memberType }));
            }
        }

        private PSMemberInfo GetAliasProperty()
        {
            this.EnsureValue1HasBeenSpecified();
            this.EnsureValue1IsNotNull();
            string parameterType = (string) GetParameterType(this.value1, typeof(string));
            if (HasBeenSpecified(this.value2))
            {
                this.EnsureValue2IsNotNull();
                return new PSAliasProperty(this.memberName, parameterType, (Type) GetParameterType(this.value2, typeof(Type)));
            }
            return new PSAliasProperty(this.memberName, parameterType);
        }

        private PSMemberInfo GetCodeMethod()
        {
            this.EnsureValue1HasBeenSpecified();
            this.EnsureValue1IsNotNull();
            this.EnsureValue2HasNotBeenSpecified();
            return new PSCodeMethod(this.memberName, (MethodInfo) GetParameterType(this.value1, typeof(MethodInfo)));
        }

        private PSMemberInfo GetCodeProperty()
        {
            this.EnsureValue1HasBeenSpecified();
            this.EnsureValue1AndValue2AreNotBothNull();
            MethodInfo getterCodeReference = null;
            if (HasBeenSpecified(this.value1))
            {
                getterCodeReference = (MethodInfo) GetParameterType(this.value1, typeof(MethodInfo));
            }
            MethodInfo setterCodeReference = null;
            if (HasBeenSpecified(this.value2))
            {
                setterCodeReference = (MethodInfo) GetParameterType(this.value2, typeof(MethodInfo));
            }
            return new PSCodeProperty(this.memberName, getterCodeReference, setterCodeReference);
        }

        private PSMemberInfo GetMemberSet()
        {
            this.EnsureValue2HasNotBeenSpecified();
            if ((this.value1 == null) || !HasBeenSpecified(this.value1))
            {
                return new PSMemberSet(this.memberName);
            }
            return new PSMemberSet(this.memberName, (Collection<PSMemberInfo>) GetParameterType(this.value1, typeof(Collection<PSMemberInfo>)));
        }

        private PSMemberInfo GetNoteProperty()
        {
            this.EnsureValue1HasBeenSpecified();
            this.EnsureValue2HasNotBeenSpecified();
            return new PSNoteProperty(this.memberName, this.value1);
        }

        private static object GetParameterType(object sourceValue, Type destinationType)
        {
            return LanguagePrimitives.ConvertTo(sourceValue, destinationType, CultureInfo.InvariantCulture);
        }

        private PSMemberInfo GetPropertySet()
        {
            this.EnsureValue2HasNotBeenSpecified();
            this.EnsureValue1HasBeenSpecified();
            this.EnsureValue1IsNotNull();
            return new PSPropertySet(this.memberName, (Collection<string>) GetParameterType(this.value1, typeof(Collection<string>)));
        }

        private PSMemberInfo GetScriptMethod()
        {
            this.EnsureValue2HasNotBeenSpecified();
            this.EnsureValue1HasBeenSpecified();
            this.EnsureValue1IsNotNull();
            return new PSScriptMethod(this.memberName, (ScriptBlock) GetParameterType(this.value1, typeof(ScriptBlock)));
        }

        private PSMemberInfo GetScriptProperty()
        {
            this.EnsureValue1HasBeenSpecified();
            this.EnsureValue1AndValue2AreNotBothNull();
            ScriptBlock getterScript = null;
            if (HasBeenSpecified(this.value1))
            {
                getterScript = (ScriptBlock) GetParameterType(this.value1, typeof(ScriptBlock));
            }
            ScriptBlock setterScript = null;
            if (HasBeenSpecified(this.value2))
            {
                setterScript = (ScriptBlock) GetParameterType(this.value2, typeof(ScriptBlock));
            }
            return new PSScriptProperty(this.memberName, getterScript, setterScript);
        }

        private static bool HasBeenSpecified(object obj)
        {
            return !object.ReferenceEquals(obj, notSpecified);
        }

        private ErrorRecord NewError(string errorId, string resourceId, object targetObject, params object[] args)
        {
            ErrorDetails details = new ErrorDetails(base.GetType().Assembly, "AddMember", resourceId, args);
            return new ErrorRecord(new InvalidOperationException(details.Message), errorId, ErrorCategory.InvalidOperation, targetObject);
        }

        private void ProcessNotePropertyMultiMemberSet()
        {
            bool flag = false;
            foreach (DictionaryEntry entry in this._property)
            {
                string str = PSObject.ToStringParser(base.Context, entry.Key);
                object obj2 = entry.Value;
                if (string.IsNullOrEmpty(str))
                {
                    base.WriteError(this.NewError("NotePropertyNameShouldNotBeNull", "NotePropertyNameShouldNotBeNull", str, new object[0]));
                }
                else
                {
                    PSMemberInfo member = new PSNoteProperty(str, obj2);
                    if (this.AddMemberToTarget(member) && !flag)
                    {
                        flag = true;
                    }
                }
            }
            if (flag && (this.typeName != null))
            {
                this.UpdateTypeNames();
            }
            if (flag && this.passThru)
            {
                base.WriteObject(this.inputObject);
            }
        }

        protected override void ProcessRecord()
        {
            if ((this.typeName != null) && string.IsNullOrWhiteSpace(this.typeName))
            {
                base.ThrowTerminatingError(this.NewError("TypeNameShouldNotBeEmpty", "TypeNameShouldNotBeEmpty", this.typeName, new object[0]));
            }
            if (base.ParameterSetName == "TypeNameSet")
            {
                this.UpdateTypeNames();
                if (this.passThru)
                {
                    base.WriteObject(this.inputObject);
                }
                return;
            }
            if (base.ParameterSetName == "NotePropertyMultiMemberSet")
            {
                this.ProcessNotePropertyMultiMemberSet();
                return;
            }
            PSMemberInfo member = null;
            if (base.ParameterSetName == "NotePropertySingleMemberSet")
            {
                member = new PSNoteProperty(this._notePropertyName, this._notePropertyValue);
            }
            else
            {
                int memberType = (int) this.memberType;
                int num2 = 0;
                while (memberType != 0)
                {
                    if ((memberType & 1) != 0)
                    {
                        num2++;
                    }
                    memberType = memberType >> 1;
                }
                if (num2 != 1)
                {
                    base.ThrowTerminatingError(this.NewError("WrongMemberCount", "WrongMemberCount", null, new object[] { this.memberType.ToString() }));
                    return;
                }
                switch (this.memberType)
                {
                    case PSMemberTypes.AliasProperty:
                        member = this.GetAliasProperty();
                        goto Label_01D1;

                    case PSMemberTypes.CodeProperty:
                        member = this.GetCodeProperty();
                        goto Label_01D1;

                    case PSMemberTypes.NoteProperty:
                        member = this.GetNoteProperty();
                        goto Label_01D1;

                    case PSMemberTypes.ScriptProperty:
                        member = this.GetScriptProperty();
                        goto Label_01D1;

                    case PSMemberTypes.ScriptMethod:
                        member = this.GetScriptMethod();
                        goto Label_01D1;

                    case PSMemberTypes.MemberSet:
                        member = this.GetMemberSet();
                        goto Label_01D1;

                    case PSMemberTypes.PropertySet:
                        member = this.GetPropertySet();
                        goto Label_01D1;

                    case PSMemberTypes.CodeMethod:
                        member = this.GetCodeMethod();
                        goto Label_01D1;
                }
                base.ThrowTerminatingError(this.NewError("CannotAddMemberType", "CannotAddMemberType", null, new object[] { this.memberType.ToString() }));
            }
        Label_01D1:
            if (member == null)
            {
                return;
            }
            if (this.AddMemberToTarget(member))
            {
                if (this.typeName != null)
                {
                    this.UpdateTypeNames();
                }
                if (this.passThru)
                {
                    base.WriteObject(this.inputObject);
                }
            }
        }

        private void UpdateTypeNames()
        {
            Type type;
            string typeName = this.typeName;
            if (LanguagePrimitives.TryConvertTo<Type>(this.typeName, out type))
            {
                typeName = type.FullName;
            }
            this.inputObject.TypeNames.Insert(0, typeName);
        }

        [Parameter(ParameterSetName="MemberSet"), Parameter(ParameterSetName="NotePropertySingleMemberSet"), Parameter(ParameterSetName="NotePropertyMultiMemberSet")]
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

        [Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="NotePropertySingleMemberSet"), Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="NotePropertyMultiMemberSet"), Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="TypeNameSet"), Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="MemberSet")]
        public PSObject InputObject
        {
            get
            {
                return this.inputObject;
            }
            set
            {
                this.inputObject = value;
            }
        }

        [Parameter(Mandatory=true, Position=0, ParameterSetName="MemberSet"), Alias(new string[] { "Type" })]
        public PSMemberTypes MemberType
        {
            get
            {
                return this.memberType;
            }
            set
            {
                this.memberType = value;
            }
        }

        [Parameter(Mandatory=true, Position=1, ParameterSetName="MemberSet")]
        public string Name
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

        [Parameter(Mandatory=true, Position=0, ParameterSetName="NotePropertyMultiMemberSet"), ValidateNotNullOrEmpty]
        public IDictionary NotePropertyMembers
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

        [Parameter(Mandatory=true, Position=0, ParameterSetName="NotePropertySingleMemberSet"), ValidateNotePropertyName, NotePropertyTransformation, ValidateNotNullOrEmpty]
        public string NotePropertyName
        {
            get
            {
                return this._notePropertyName;
            }
            set
            {
                this._notePropertyName = value;
            }
        }

        [AllowNull, Parameter(Mandatory=true, Position=1, ParameterSetName="NotePropertySingleMemberSet")]
        public object NotePropertyValue
        {
            get
            {
                return this._notePropertyValue;
            }
            set
            {
                this._notePropertyValue = value;
            }
        }

        [Parameter(ParameterSetName="NotePropertyMultiMemberSet"), Parameter(ParameterSetName="TypeNameSet"), Parameter(ParameterSetName="NotePropertySingleMemberSet"), Parameter(ParameterSetName="MemberSet")]
        public SwitchParameter PassThru
        {
            get
            {
                return this.passThru;
            }
            set
            {
                this.passThru = (bool) value;
            }
        }

        [Parameter(Position=3, ParameterSetName="MemberSet")]
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

        [Parameter(ParameterSetName="MemberSet"), Parameter(ParameterSetName="NotePropertyMultiMemberSet"), Parameter(ParameterSetName="NotePropertySingleMemberSet"), ValidateNotNullOrEmpty, Parameter(Mandatory=true, ParameterSetName="TypeNameSet")]
        public string TypeName
        {
            get
            {
                return this.typeName;
            }
            set
            {
                this.typeName = value;
            }
        }

        [Parameter(Position=2, ParameterSetName="MemberSet")]
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

        internal sealed class NotePropertyTransformationAttribute : ArgumentTransformationAttribute
        {
            public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
            {
                object valueToConvert = PSObject.Base(inputData);
                if ((valueToConvert != null) && valueToConvert.GetType().IsNumeric())
                {
                    return LanguagePrimitives.ConvertTo<string>(valueToConvert);
                }
                return inputData;
            }
        }

        private sealed class ValidateNotePropertyNameAttribute : ValidateArgumentsAttribute
        {
            protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
            {
                PSMemberTypes types;
                string valueToConvert = arguments as string;
                if ((valueToConvert == null) || !LanguagePrimitives.TryConvertTo<PSMemberTypes>(valueToConvert, out types))
                {
                    return;
                }
                PSMemberTypes types2 = types;
                if (types2 <= PSMemberTypes.ScriptProperty)
                {
                    switch (types2)
                    {
                        case PSMemberTypes.AliasProperty:
                        case PSMemberTypes.CodeProperty:
                        case PSMemberTypes.NoteProperty:
                        case PSMemberTypes.ScriptProperty:
                            goto Label_005C;
                    }
                    return;
                }
                if (types2 <= PSMemberTypes.CodeMethod)
                {
                    if ((types2 != PSMemberTypes.PropertySet) && (types2 != PSMemberTypes.CodeMethod))
                    {
                        return;
                    }
                }
                else if ((types2 != PSMemberTypes.ScriptMethod) && (types2 != PSMemberTypes.MemberSet))
                {
                    return;
                }
            Label_005C:
				return;
                //TODO: REVIEW WHY? throw new ValidationMetadataException(StringUtil.Format(AddMember.InvalidValueForNotePropertyName, typeof(PSMemberTypes).FullName), true);
            }
        }
    }
}

