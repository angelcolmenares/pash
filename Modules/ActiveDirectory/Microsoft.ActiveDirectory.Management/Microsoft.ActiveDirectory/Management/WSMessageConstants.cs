using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal class WSMessageConstants
	{
		public const string SyntheticPrefix = "ad";

		public const string DirectoryPrefix = "addata";

		public const string AddataPrefix = "addata";

		public const string AdPrefix = "ad";

		public const string AdLqPrefix = "adlq";

		public const string EnumerationPrefix = "wsen";

		public const string DirectoryAccessPrefix = "da";

		public const string TransferPrefix = "wst";

		public const string XsdInstancePrefix = "xsd";

		public const string XsdPrefix = "xs";

		public const string XsiInstancePrefix = "xsi";

		public const string WsAdressingPrefix = "wsa";

		public const string ControlsElement = "controls";

		public const string ControlElement = "control";

		public const string ControlValueAttribute = "controlValue";

		public const string Criticality = "criticality";

		public const string ControlType = "type";

		public const string ControlValue = "controlValue";

		public const string True = "true";

		public const string False = "false";

		public const string AdValueElement = "value";

		public const string AdValueTypeAttribute = "type";

		public const string SyntheticAttributeNs = "http://schemas.microsoft.com/2008/1/ActiveDirectory";

		public const string DirectoryAttributeNs = "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data";

		public const string xsiType = "type";

		public const string xsdStringType = "string";

		public const string xsdBinaryType = "base64Binary";

		public const string xsdBooleanType = "boolean";

		public const string xsdIntType = "int";

		public const string xsdLongType = "long";

		public const string xsdDateTimeType = "dateTime";

		public const string ChangeOperationAdd = "add";

		public const string ChangeOperationDelete = "delete";

		public const string ChangeOperationReplace = "replace";

		public const string Instance = "instance";

		public const string ContainerHierarchyParent = "container-hierarchy-parent";

		public const string DistinguishedName = "distinguishedName";

		public const string RelativeDistinguishedName = "relativeDistinguishedName";

		public const string All = "ad:all";

		public const string Action = "Action";

		public const string ObjectReferenceProperty = "objectReferenceProperty";

		public const string Put = "Put";

		public const string ModifyRequest = "ModifyRequest";

		public const string PutResponse = "PutResponse";

		public const string Get = "Get";

		public const string BaseObjectSearchRequest = "BaseObjectSearchRequest";

		public const string GetResponse = "GetResponse";

		public const string BaseObjectSearchResponse = "BaseObjectSearchResponse";

		public const string ReferenceParameters = "ReferenceParameters";

		public const string Address = "Address";

		public const string Create = "Create";

		public const string AddRequest = "AddRequest";

		public const string ResourceCreated = "ResourceCreated";

		public const string IdentityManagementOperation = "IdentityManagementOperation";

		public const string Change = "Change";

		public const string AttributeTypeAndValue = "AttributeTypeAndValue";

		public const string AttributeType = "AttributeType";

		public const string Operation = "Operation";

		public const string Dialect = "Dialect";

		public const string PartialAttribute = "PartialAttribute";

		public const string AttributeValue = "AttributeValue";

		public const string Enumerate = "Enumerate";

		public const string EnumerateResponse = "EnumerateResponse";

		public const string EnumerationContext = "EnumerationContext";

		public const string EnumerationDetail = "EnumerationDetaill";

		public const string EnumerationDirection = "EnumerationDirection";

		public const string LdapQuery = "LdapQuery";

		public const string Filter = "Filter";

		public const string BaseObject = "BaseObject";

		public const string Scope = "Scope";

		public const string ScopeOneLevel = "onelevel";

		public const string ScopeSubTree = "subtree";

		public const string Selection = "Selection";

		public const string SelectionProperty = "SelectionProperty";

		public const string Sorting = "Sorting";

		public const string SortingProperty = "SortingProperty";

		public const string AscendingAttribute = "Ascending";

		public const string SortModeAscending = "true";

		public const string SortModeDescending = "false";

		public const string GetStatus = "GetStatus";

		public const string GetStatusResponse = "GetStatusResponse";

		public const string Expires = "Expires";

		public const string RenewResponse = "RenewResponse";

		public const string Renew = "Renew";

		public const string Pull = "Pull";

		public const string PullResponse = "PullResponse";

		public const string MaxTime = "MaxTime";

		public const string MaxCharacters = "MaxCharacters";

		public const string MaxElements = "MaxElements";

		public const string PullAdjustment = "PullAdjustment";

		public const string StartingIndex = "StartingIndex";

		public const string StartingValue = "StartingValue";

		public const string Items = "Items";

		public const string Release = "Release";

		public const string ReleaseResponse = "ReleaseResponse";

		public const string OptRangeHigh = "RangeHigh";

		public const string OptRangeLow = "RangeLow";

		private WSMessageConstants()
		{
		}

		public static class Uris
		{
			public const string AddataNs = "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data";

			public const string AdFault = "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault";

			public const string AdNs = "http://schemas.microsoft.com/2008/1/ActiveDirectory";

			public const string AdLq = "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery";

			public const string Context = "http://schemas.microsoft.com/ws/2006/05/context";

			public const string Create = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Create";

			public const string CreateResponse = "http://schemas.xmlsoap.org/ws/2004/09/transfer/CreateResponse";

			public const string Delete = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Delete";

			public const string DeleteResponse = "http://schemas.xmlsoap.org/ws/2004/09/transfer/DeleteResponse";

			public const string Enumerate = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Enumerate";

			public const string EnumerateResponse = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/EnumerateResponse";

			public const string Get = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get";

			public const string GetResponse = "http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse";

			public const string GetStatus = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/GetStatus";

			public const string GetStatusResponse = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/GetStatusResponse";

			public const string MailSecurityToken = "http://schemas.microsoft.com/2006/11/MailSecurityToken";

			public const string MailSenderClaim = "http://schemas.microsoft.com/2006/11/MailSender";

			public const string MailSenderIdentifierClaim = "http://schemas.microsoft.com/2006/11/MailSenderIdentifier";

			public const string MetadirectoryServices = "http://schemas.microsoft.com/2006/11/MetadirectoryServices";

			public const string Psha1 = "http://schemas.xmlsoap.org/ws/2005/02/trust/CK/PSHA1";

			public const string Pull = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Pull";

			public const string PullResponse = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/PullResponse";

			public const string Put = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Put";

			public const string PutResponse = "http://schemas.xmlsoap.org/ws/2004/09/transfer/PutResponse";

			public const string QueryDialect = "http://schemas.microsoft.com/2006/11/MetadirectoryServices/Dialect/Query";

			public const string Release = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Release";

			public const string ReleaseResponse = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/ReleaseResponse";

			public const string Renew = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Renew";

			public const string RenewResponse = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/RenewResponse";

			public const string RequestSecurityTokenIssue = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue";

			public const string RequestSecurityTokenResponseIssue = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue";

			public const string SamlTokenProfile = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1";

			public const string WSAddressing = "http://schemas.xmlsoap.org/ws/2004/08/addressing";

			public const string WSAddressingAugust2005 = "http://www.w3.org/2005/08/addressing";

			public const string WSAddressingFault = "http://www.w3.org/2005/08/addressing/fault";

			public const string WSEnumeration = "http://schemas.xmlsoap.org/ws/2004/09/enumeration";

			public const string WSEnumerationFault = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault";

			public const string WSMetadataExchange = "http://schemas/xmlsoap.org/ws/2004/09/mex";

			public const string WSPolicy = "http://schemas.xmlsoap.org/ws/2004/09/policy";

			public const string WSResourceManagement = "http://schemas.microsoft.com/2006/11/ResourceManagement";

			public const string WSResourceManagementFault = "http://schemas.microsoft.com/2006/11/ResourceManagement/fault";

			public const string IdentityManagementDirectoryAccess = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess";

			public const string WSSecurityExtension = "http://www.docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

			public const string WSTransfer = "http://schemas.xmlsoap.org/ws/2004/09/transfer";

			public const string WSTransferFault = "http://schemas.xmlsoap.org/ws/2004/09/transfer/fault";

			public const string WSTrust = "http://schemas.xmlsoap.org/ws/2005/02/trust";

			public const string XmlSchema = "http://www.w3.org/2001/XMLSchema";

			public const string XmlSchemaInstance = "http://www.w3.org/2001/XMLSchema-instance";

			public const string XPath1Dialect = "http://www.w3.org/TR/1999/REC-xpath-19991116";

			public const string XPath2Dialect = "http://www.w3.org/TR/2007/REC-xpath20-20070123";

			public const string XPathDialect = "http://schemas.microsoft.com/2006/11/XPathFilterDialect";

			public const string XPathLevel1Dialect = "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/XPath-Level-1";

			public const string LdapQueryDialect = "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery";

			public const string WSManagement = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd";

		}
	}
}