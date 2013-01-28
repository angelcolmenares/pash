namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.Serialization;

    [Serializable]
    public class ExtendedTypeSystemException : RuntimeException
    {
        internal const string AccessMemberOutsidePSObjectMsg = "AccessMemberOutsidePSObject";
        internal const string BaseName = "ExtendedTypeSystem";
        internal const string CannotAddPropertyOrMethodMsg = "CannotAddPropertyOrMethod";
        internal const string CannotChangeReservedMemberMsg = "CannotChangeReservedMember";
        internal const string CannotSetValueForMemberTypeMsg = "CannotSetValueForMemberType";
        internal const string ChangeStaticMemberMsg = "ChangeStaticMember";
        internal const string CodeMethodMethodFormatMsg = "CodeMethodMethodFormat";
        internal const string CodePropertyGetterAndSetterNullMsg = "CodePropertyGetterAndSetterNull";
        internal const string CodePropertyGetterFormatMsg = "CodePropertyGetterFormat";
        internal const string CodePropertySetterFormatMsg = "CodePropertySetterFormat";
        internal const string CycleInAliasMsg = "CycleInAlias";
        internal const string EnumerationExceptionMsg = "EnumerationException";
        internal const string ExceptionGettingMemberMsg = "ExceptionGettingMember";
        internal const string ExceptionGettingMembersMsg = "ExceptionGettingMembers";
        internal const string ExceptionRetrievingMethodDefinitionsMsg = "ExceptionRetrievingMethodDefinitions";
        internal const string ExceptionRetrievingMethodStringMsg = "ExceptionRetrievingMethodString";
        internal const string ExceptionRetrievingParameterizedPropertyDefinitionsMsg = "ExceptionRetrievingParameterizedPropertyDefinitions";
        internal const string ExceptionRetrievingParameterizedPropertyReadStateMsg = "ExceptionRetrievingParameterizedPropertyReadState";
        internal const string ExceptionRetrievingParameterizedPropertyStringMsg = "ExceptionRetrievingParameterizedPropertyString";
        internal const string ExceptionRetrievingParameterizedPropertytypeMsg = "ExceptionRetrievingParameterizedPropertytype";
        internal const string ExceptionRetrievingParameterizedPropertyWriteStateMsg = "ExceptionRetrievingParameterizedPropertyWriteState";
        internal const string ExceptionRetrievingPropertyAttributesMsg = "ExceptionRetrievingPropertyAttributes";
        internal const string ExceptionRetrievingPropertyReadStateMsg = "ExceptionRetrievingPropertyReadState";
        internal const string ExceptionRetrievingPropertyStringMsg = "ExceptionRetrievingPropertyString";
        internal const string ExceptionRetrievingPropertyTypeMsg = "ExceptionRetrievingPropertyType";
        internal const string ExceptionRetrievingPropertyWriteStateMsg = "ExceptionRetrievingPropertyWriteState";
        internal const string ExceptionRetrievingTypeNameHierarchyMsg = "ExceptionRetrievingTypeNameHierarchy";
        internal const string GetProperties = "GetProperties";
        internal const string GetProperty = "GetProperty";
        internal const string GetTypeNameHierarchyError = "GetTypeNameHierarchyError";
        internal const string MemberAlreadyPresentFromTypesXmlMsg = "MemberAlreadyPresentFromTypesXml";
        internal const string MemberAlreadyPresentMsg = "MemberAlreadyPresent";
        internal const string MemberNotPresentMsg = "MemberNotPresent";
        internal const string NotAClsCompliantFieldPropertyMsg = "NotAClsCompliantFieldProperty";
        internal const string NotTheSameTypeOrNotIcomparableMsg = "NotTheSameTypeOrNotIcomparable";
        internal const string NullReturnValueError = "NullReturnValueError";
        internal const string PropertyGetError = "PropertyGetError";
        internal const string PropertyIsGettableError = "PropertyIsGettableError";
        internal const string PropertyIsSettableError = "PropertyIsSettableError";
        internal const string PropertyNotFoundInTypeDescriptorMsg = "PropertyNotFoundInTypeDescriptor";
        internal const string PropertySetError = "PropertySetError";
        internal const string PropertyTypeError = "PropertyTypeError";
        internal const string ReservedMemberNameMsg = "ReservedMemberName";
        internal const string ToStringExceptionMsg = "ToStringException";
        internal const string TypesXmlErrorMsg = "TypesXmlError";

        public ExtendedTypeSystemException() : base(typeof(ExtendedTypeSystemException).FullName)
        {
        }

        public ExtendedTypeSystemException(string message) : base(message)
        {
        }

        protected ExtendedTypeSystemException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ExtendedTypeSystemException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal ExtendedTypeSystemException(string errorId, Exception innerException, string resourceString, params object[] arguments) : base(StringUtil.Format(resourceString, arguments), innerException)
        {
            base.SetErrorId(errorId);
        }
    }
}

