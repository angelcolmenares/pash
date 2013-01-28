namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class CompiledCommandParameter
    {
        private bool _isMandatoryInSomeParameterSet;
        private bool _isPipelineParameterInSomeParameterSet;
        private Dictionary<string, ParameterSetSpecificMetadata> _parameterSetData;
        private int _parameterSetFlags;
        private Collection<string> aliases;
        private bool allowsEmptyCollectionArgument;
        private bool allowsEmptyStringArgument;
        private bool allowsNullArgument;
        private Collection<ArgumentTransformationAttribute> argumentTransformationAttributes;
        private Collection<Attribute> attributes;
        private ParameterCollectionTypeInformation collectionTypeInformation;
        private System.Type declaringType;
        private bool isDynamic;
        private bool isInAllSets;
        private string name;
        private System.Type type;
        private string typeName;
        private Collection<ValidateArgumentsAttribute> validationAttributes;

        internal CompiledCommandParameter(RuntimeDefinedParameter runtimeDefinedParameter, bool processingDynamicParameters)
        {
            this.name = string.Empty;
            this.typeName = string.Empty;
            this.argumentTransformationAttributes = new Collection<ArgumentTransformationAttribute>();
            this.validationAttributes = new Collection<ValidateArgumentsAttribute>();
            this.aliases = new Collection<string>();
            if (runtimeDefinedParameter == null)
            {
                throw PSTraceSource.NewArgumentNullException("runtimeDefinedParameter");
            }
            this.name = runtimeDefinedParameter.Name;
            this.type = runtimeDefinedParameter.ParameterType;
            this.isDynamic = processingDynamicParameters;
            this.collectionTypeInformation = new ParameterCollectionTypeInformation(runtimeDefinedParameter.ParameterType);
            this.ConstructCompiledAttributesUsingRuntimeDefinedParameter(runtimeDefinedParameter);
        }

        internal CompiledCommandParameter(MemberInfo member, bool processingDynamicParameters)
        {
            this.name = string.Empty;
            this.typeName = string.Empty;
            this.argumentTransformationAttributes = new Collection<ArgumentTransformationAttribute>();
            this.validationAttributes = new Collection<ValidateArgumentsAttribute>();
            this.aliases = new Collection<string>();
            if (member == null)
            {
                throw PSTraceSource.NewArgumentNullException("member");
            }
            this.name = member.Name;
            this.declaringType = member.DeclaringType;
            this.isDynamic = processingDynamicParameters;
            if (member.MemberType == MemberTypes.Property)
            {
                this.type = ((PropertyInfo) member).PropertyType;
            }
            else
            {
                if (member.MemberType != MemberTypes.Field)
                {
                    throw PSTraceSource.NewArgumentException("member", "DiscoveryExceptions", "CompiledCommandParameterMemberMustBeFieldOrProperty", new object[0]);
                }
                this.type = ((FieldInfo) member).FieldType;
            }
            this.collectionTypeInformation = new ParameterCollectionTypeInformation(this.type);
            this.ConstructCompiledAttributesUsingReflection(member);
        }

        private void ConstructCompiledAttributesUsingReflection(MemberInfo member)
        {
            this.attributes = new Collection<Attribute>();
            this._parameterSetData = new Dictionary<string, ParameterSetSpecificMetadata>(StringComparer.OrdinalIgnoreCase);
            object[] customAttributes = member.GetCustomAttributes(false);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                foreach (Attribute attribute in customAttributes)
                {
                    this.ProcessAttribute(member.Name, attribute);
                }
            }
        }

        private void ConstructCompiledAttributesUsingRuntimeDefinedParameter(RuntimeDefinedParameter runtimeDefinedParameter)
        {
            this.attributes = new Collection<Attribute>();
            this._parameterSetData = new Dictionary<string, ParameterSetSpecificMetadata>(StringComparer.OrdinalIgnoreCase);
            foreach (Attribute attribute in runtimeDefinedParameter.Attributes)
            {
                this.ProcessAttribute(runtimeDefinedParameter.Name, attribute);
            }
        }

        internal bool DoesParameterSetTakePipelineInput(int validParameterSetFlags)
        {
            if (this._isPipelineParameterInSomeParameterSet)
            {
                foreach (ParameterSetSpecificMetadata metadata in this.ParameterSetData.Values)
                {
                    if ((metadata.IsInAllSets || ((metadata.ParameterSetFlag & validParameterSetFlags) != 0)) && (metadata.ValueFromPipeline || metadata.ValueFromPipelineByPropertyName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal IEnumerable<ParameterSetSpecificMetadata> GetMatchingParameterSetData(int parameterSetFlags)
        {
            foreach (ParameterSetSpecificMetadata iteratorVariable0 in this.ParameterSetData.Values)
            {
                if (iteratorVariable0.IsInAllSets)
                {
                    yield return iteratorVariable0;
                }
                else if ((iteratorVariable0.ParameterSetFlag & parameterSetFlags) != 0)
                {
                    yield return iteratorVariable0;
                }
            }
        }

        internal ParameterSetSpecificMetadata GetParameterSetData(int parameterSetFlag)
        {
            ParameterSetSpecificMetadata metadata = null;
            foreach (ParameterSetSpecificMetadata metadata2 in this.ParameterSetData.Values)
            {
                if (metadata2.IsInAllSets)
                {
                    metadata = metadata2;
                }
                else if ((metadata2.ParameterSetFlag & parameterSetFlag) != 0)
                {
                    return metadata2;
                }
            }
            return metadata;
        }

        private void ProcessAliasAttribute(AliasAttribute attribute)
        {
            foreach (string str in attribute.aliasNames)
            {
                this.aliases.Add(str);
            }
        }

        private void ProcessAttribute(string memberName, Attribute attribute)
        {
            if (attribute != null)
            {
                this.attributes.Add(attribute);
                ParameterAttribute parameter = attribute as ParameterAttribute;
                if (parameter != null)
                {
                    this.ProcessParameterAttribute(memberName, parameter);
                }
                else
                {
                    AliasAttribute attribute3 = attribute as AliasAttribute;
                    if (attribute3 != null)
                    {
                        this.ProcessAliasAttribute(attribute3);
                    }
                    else
                    {
                        ArgumentTransformationAttribute item = attribute as ArgumentTransformationAttribute;
                        if (item != null)
                        {
                            this.argumentTransformationAttributes.Add(item);
                        }
                        else
                        {
                            ValidateArgumentsAttribute attribute5 = attribute as ValidateArgumentsAttribute;
                            if (attribute5 != null)
                            {
                                this.validationAttributes.Add(attribute5);
                            }
                            else if (attribute is AllowNullAttribute)
                            {
                                this.allowsNullArgument = true;
                            }
                            else if (attribute is AllowEmptyStringAttribute)
                            {
                                this.allowsEmptyStringArgument = true;
                            }
                            else if (attribute is AllowEmptyCollectionAttribute)
                            {
                                this.allowsEmptyCollectionArgument = true;
                            }
                            else
                            {
                                PSTypeNameAttribute attribute9 = attribute as PSTypeNameAttribute;
                                if (attribute9 != null)
                                {
                                    this.PSTypeName = attribute9.PSTypeName;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ProcessParameterAttribute(string parameterName, ParameterAttribute parameter)
        {
            if (this._parameterSetData.ContainsKey(parameter.ParameterSetName))
            {
                System.Management.Automation.MetadataException exception = new System.Management.Automation.MetadataException("ParameterDeclaredInParameterSetMultipleTimes", null, DiscoveryExceptions.ParameterDeclaredInParameterSetMultipleTimes, new object[] { parameterName, parameter.ParameterSetName });
                throw exception;
            }
            if (parameter.ValueFromPipeline || parameter.ValueFromPipelineByPropertyName)
            {
                this._isPipelineParameterInSomeParameterSet = true;
            }
            if (parameter.Mandatory)
            {
                this._isMandatoryInSomeParameterSet = true;
            }
            ParameterSetSpecificMetadata metadata = new ParameterSetSpecificMetadata(parameter);
            this._parameterSetData.Add(parameter.ParameterSetName, metadata);
        }

        public override string ToString()
        {
            return this.Name;
        }

        internal ReadOnlyCollection<string> Aliases
        {
            get
            {
                return new ReadOnlyCollection<string>(this.aliases);
            }
        }

        internal bool AllowsEmptyCollectionArgument
        {
            get
            {
                return this.allowsEmptyCollectionArgument;
            }
        }

        internal bool AllowsEmptyStringArgument
        {
            get
            {
                return this.allowsEmptyStringArgument;
            }
        }

        internal bool AllowsNullArgument
        {
            get
            {
                return this.allowsNullArgument;
            }
        }

        internal ReadOnlyCollection<ArgumentTransformationAttribute> ArgumentTransformationAttributes
        {
            get
            {
                return new ReadOnlyCollection<ArgumentTransformationAttribute>(this.argumentTransformationAttributes);
            }
        }

        internal ParameterCollectionTypeInformation CollectionTypeInformation
        {
            get
            {
                if (this.collectionTypeInformation == null)
                {
                    this.collectionTypeInformation = new ParameterCollectionTypeInformation(this.Type);
                }
                return this.collectionTypeInformation;
            }
        }

        internal Collection<Attribute> CompiledAttributes
        {
            get
            {
                if (this.attributes == null)
                {
                    MemberInfo[] member = this.Type.GetMember(this.Name, InternalParameterMetadata.metaDataBindingFlags);
                    if (member.Length > 0)
                    {
                        this.ConstructCompiledAttributesUsingReflection(member[0]);
                    }
                }
                return this.attributes;
            }
        }

        internal System.Type DeclaringType
        {
            get
            {
                return this.declaringType;
            }
        }

        internal bool IsDynamic
        {
            get
            {
                return this.isDynamic;
            }
        }

        internal bool IsInAllSets
        {
            get
            {
                return this.isInAllSets;
            }
            set
            {
                this.isInAllSets = value;
            }
        }

        internal bool IsMandatoryInSomeParameterSet
        {
            get
            {
                return this._isMandatoryInSomeParameterSet;
            }
        }

        internal bool IsPipelineParameterInSomeParameterSet
        {
            get
            {
                return this._isPipelineParameterInSomeParameterSet;
            }
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal Dictionary<string, ParameterSetSpecificMetadata> ParameterSetData
        {
            get
            {
                return this._parameterSetData;
            }
        }

        internal int ParameterSetFlags
        {
            get
            {
                return this._parameterSetFlags;
            }
            set
            {
                this._parameterSetFlags = value;
            }
        }

        internal string PSTypeName { get; private set; }

        internal System.Type Type
        {
            get
            {
                if (this.type == null)
                {
                    this.type = System.Type.GetType(this.typeName);
                }
                return this.type;
            }
        }

        internal ReadOnlyCollection<ValidateArgumentsAttribute> ValidationAttributes
        {
            get
            {
                return new ReadOnlyCollection<ValidateArgumentsAttribute>(this.validationAttributes);
            }
        }

        
    }
}

