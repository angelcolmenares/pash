using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.WebServices.Proxy;
using System;
using System.Collections;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.Management.Faults
{
	internal class AdwsFaultUtil
	{
		private const string _debugCategory = "AdwsFaultUtil";

		private static Hashtable _faultSubCodeType;

		private static Hashtable _faultSubCodeMessage;

		private static Hashtable _shortErrorMessage;

		static AdwsFaultUtil()
		{
			AdwsFaultUtil._faultSubCodeType = new Hashtable(StringComparer.OrdinalIgnoreCase);
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd:FragmentDialectNotSupported", typeof(FragmentDialect));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd:CannotProcessFilter", typeof(AttributeTypeNotValid));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.xmlsoap.org/ws/2004/09/enumeration:CannotProcessFilter", typeof(EnumerateFault));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd:SchemaValidationError", typeof(FaultDetail));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd:AccessDenied", typeof(FaultDetail));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess:UnwillingToPerform", typeof(FaultDetail));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.xmlsoap.org/ws/2004/08/addressing:EndpointUnavailable", typeof(FaultDetail));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.xmlsoap.org/ws/2004/08/addressing:DestinationUnreachable", typeof(FaultDetail));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.xmlsoap.org/ws/2004/09/transfer:InvalidRepresentation", typeof(FaultDetail));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.xmlsoap.org/ws/2004/08/addressing:ActionNotSupportedFault", typeof(FaultDetail));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd:AlreadyExists", typeof(FaultDetail));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.xmlsoap.org/ws/2004/09/enumeration:InvalidEnumerationContext", typeof(FaultDetail));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd:EncodingLimit", typeof(FaultDetail1));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.microsoft.com/2008/1/ActiveDirectory:UnsupportedSelectOrSortDialectFault", typeof(SupportedSelectOrSortDialect));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.xmlsoap.org/ws/2004/09/enumeration:FilterDialectRequestedUnavailable", typeof(SupportedDialect));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.microsoft.com/2008/1/ActiveDirectory:InvalidPropertyFault", typeof(EnumerateFault));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.microsoft.com/2008/1/ActiveDirectory:EnumerationContextLimitExceeded", typeof(EnumerateFault));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.xmlsoap.org/ws/2004/09/enumeration:InvalidExpirationTime", typeof(EnumerateFault));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.microsoft.com/2008/1/ActiveDirectory:MaxTimeExceedsLimit", typeof(PullFault));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.microsoft.com/2008/1/ActiveDirectory:MaxCharsNotSupported", typeof(PullFault));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.xmlsoap.org/ws/2004/09/enumeration:TimedOut", typeof(PullFault));
			AdwsFaultUtil._faultSubCodeType.Add("http://schemas.xmlsoap.org/ws/2004/09/enumeration:UnableToRenew", typeof(RenewFault));
		}

		public AdwsFaultUtil()
		{
		}

		public static AdwsFault ConstructFault(Message response)
		{
			AdwsFault adwsFault;
			MessageBuffer buffer = AdwsMessage.MessageToBuffer(response);
			MessageFault messageFault = MessageFault.CreateFault(AdwsMessage.BufferToMessage(buffer), 0x500000);
			object obj = null;
			if (messageFault.HasDetail)
			{
				try
				{
					string str = string.Concat(messageFault.Code.SubCode.Namespace, ":", messageFault.Code.SubCode.Name);
					Type item = (Type)AdwsFaultUtil._faultSubCodeType[str];
					if (item != null)
					{
						XmlReader readerAtDetailContents = messageFault.GetReaderAtDetailContents();
						XmlSerializer xmlSerializer = new XmlSerializer(item);
						obj = xmlSerializer.Deserialize(readerAtDetailContents);
					}
				}
				catch (Exception exception)
				{
				}
			}
			if (obj == null)
			{
				adwsFault = new AdwsFault(messageFault);
			}
			else
			{
				adwsFault = new AdwsFault(messageFault, obj);
			}
			return adwsFault;
		}

		public static FaultException ConstructFaultException(Message response)
		{
			Type[] typeArray = new Type[9];
			typeArray[0] = typeof(FragmentDialect);
			typeArray[1] = typeof(AttributeTypeNotValid);
			typeArray[2] = typeof(EnumerateFault);
			typeArray[3] = typeof(FaultDetail);
			typeArray[4] = typeof(FaultDetail1);
			typeArray[5] = typeof(SupportedSelectOrSortDialect);
			typeArray[6] = typeof(SupportedDialect);
			typeArray[7] = typeof(PullFault);
			typeArray[8] = typeof(RenewFault);
			Type[] typeArray1 = typeArray;
			return FaultException.CreateFault(MessageFault.CreateFault(response, 0x500000), typeArray1);
		}

		public static FaultException ConstructFaultException(MessageFault msgFault)
		{
			Type[] typeArray = new Type[9];
			typeArray[0] = typeof(FragmentDialect);
			typeArray[1] = typeof(AttributeTypeNotValid);
			typeArray[2] = typeof(EnumerateFault);
			typeArray[3] = typeof(FaultDetail);
			typeArray[4] = typeof(FaultDetail1);
			typeArray[5] = typeof(SupportedSelectOrSortDialect);
			typeArray[6] = typeof(SupportedDialect);
			typeArray[7] = typeof(PullFault);
			typeArray[8] = typeof(RenewFault);
			Type[] typeArray1 = typeArray;
			return FaultException.CreateFault(msgFault, typeArray1);
		}

		public static string GetShortErrorMessage(string shortError)
		{
			if (AdwsFaultUtil._shortErrorMessage == null)
			{
				AdwsFaultUtil._shortErrorMessage = new Hashtable(StringComparer.OrdinalIgnoreCase);
				Win32Exception win32Exception = new Win32Exception(0x2032);
				AdwsFaultUtil._shortErrorMessage.Add("InvalidDnWithStringBinaryAccessPointValue", win32Exception.Message);
				win32Exception = new Win32Exception(0x2081);
				AdwsFaultUtil._shortErrorMessage.Add("DuplicateAttributeWithValues", win32Exception.Message);
				win32Exception = new Win32Exception(0x200c);
				AdwsFaultUtil._shortErrorMessage.Add("UnknownAttributeType", win32Exception.Message);
				win32Exception = new Win32Exception(0x2077);
				AdwsFaultUtil._shortErrorMessage.Add("EModifyOperationUnsupported", win32Exception.Message);
				win32Exception = new Win32Exception(0x2040);
				AdwsFaultUtil._shortErrorMessage.Add("ENotSupported", win32Exception.Message);
				win32Exception = new Win32Exception(0x2030);
				AdwsFaultUtil._shortErrorMessage.Add("EObjectNotFound", win32Exception.Message);
				win32Exception = new Win32Exception(86);
				AdwsFaultUtil._shortErrorMessage.Add("EPassword", win32Exception.Message);
				win32Exception = new Win32Exception(5);
				AdwsFaultUtil._shortErrorMessage.Add("EUnauthorizedAccess", win32Exception.Message);
				AdwsFaultUtil._shortErrorMessage.Add("AnonymousNotAllowed", StringResources.ServerAnonymousNotAllowed);
				AdwsFaultUtil._shortErrorMessage.Add("OperationTimeout", StringResources.TimeoutError);
				AdwsFaultUtil._shortErrorMessage.Add("UnknownAttribute", StringResources.InvalidProperty);
				AdwsFaultUtil._shortErrorMessage.Add("InvalidSearchFilterForQuery", StringResources.InvalidFilter);
				AdwsFaultUtil._shortErrorMessage.Add("NotCorrectFilterType", StringResources.InvalidFilter);
				AdwsFaultUtil._shortErrorMessage.Add("MaxEnumCtxsTotalReached", StringResources.ServerEnumerationContextLimitExceeded);
				AdwsFaultUtil._shortErrorMessage.Add("InvalidInstanceInTheHeader", StringResources.ServerInvalidInstance);
				AdwsFaultUtil._shortErrorMessage.Add("InvalidExpirationTimeDetail", StringResources.ServerInvalidExpirationTime);
				AdwsFaultUtil._shortErrorMessage.Add("MaxTimeExceedsLimitDetail", StringResources.TimeoutError);
				AdwsFaultUtil._shortErrorMessage.Add("EOutOfMemory", StringResources.ServerOutOfMemory);
				AdwsFaultUtil._shortErrorMessage.Add("EMultipleMatchingSecurityPrincipals", StringResources.ServerMultipleMatchingSecurityPrincipals);
				AdwsFaultUtil._shortErrorMessage.Add("ENoMatchingSecurityPrincipal", StringResources.ServerNoMatchingSecurityPrincipal);
				AdwsFaultUtil._shortErrorMessage.Add("EUnknownAttribute", StringResources.InvalidProperty);
			}
			return (string)AdwsFaultUtil._shortErrorMessage[shortError];
		}

		public static string GetSubCodeMessage(FaultCode faultSubCode)
		{
			if (AdwsFaultUtil._faultSubCodeMessage == null)
			{
				AdwsFaultUtil._faultSubCodeMessage = new Hashtable(StringComparer.OrdinalIgnoreCase);
				Win32Exception win32Exception = new Win32Exception(5);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd:AccessDenied", win32Exception.Message);
				win32Exception = new Win32Exception(0x1392);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd:AlreadyExists", win32Exception.Message);
				win32Exception = new Win32Exception(0x2030);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.xmlsoap.org/ws/2004/08/addressing:DestinationUnreachable", win32Exception.Message);
				win32Exception = new Win32Exception(0x202f);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.xmlsoap.org/ws/2004/09/transfer:InvalidRepresentation", win32Exception.Message);
				win32Exception = new Win32Exception(0x2035);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess:UnwillingToPerform", win32Exception.Message);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.xmlsoap.org/ws/2004/08/addressing:ActionNotSupportedFault", StringResources.ServerActionNotSupportedFault);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd:CannotProcessFilter", StringResources.ServerCannotProcessFilter);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.xmlsoap.org/ws/2004/09/enumeration:CannotProcessFilter", StringResources.ServerCannotProcessFilter);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd:EncodingLimit", StringResources.ServerEncodingLimit);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.microsoft.com/2008/1/ActiveDirectory:EnumerationContextLimitExceeded", StringResources.ServerEnumerationContextLimitExceeded);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.xmlsoap.org/ws/2004/09/enumeration:FilterDialectRequestedUnavailable", StringResources.ServerFilterDialectRequestedUnavailable);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd:FragmentDialectNotSupported", StringResources.ServerFragmentDialectNotSupported);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.xmlsoap.org/ws/2004/09/enumeration:InvalidEnumerationContext", StringResources.ServerInvalidEnumerationContext);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.xmlsoap.org/ws/2004/09/enumeration:InvalidExpirationTime", StringResources.ServerInvalidExpirationTime);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.microsoft.com/2008/1/ActiveDirectory:InvalidPropertyFault", StringResources.InvalidProperty);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.microsoft.com/2008/1/ActiveDirectory:MaxTimeExceedsLimit", StringResources.TimeoutError);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd:SchemaValidationError", StringResources.ServerSchemaValidationError);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.xmlsoap.org/ws/2004/09/enumeration:TimedOut", StringResources.TimeoutError);
				AdwsFaultUtil._faultSubCodeMessage.Add("http://schemas.microsoft.com/2008/1/ActiveDirectory:UnsupportedSelectOrSortDialectFault", StringResources.ServerUnsupportedSelectOrSortDialectFault);
			}
			return (string)AdwsFaultUtil._faultSubCodeMessage[string.Concat(faultSubCode.Namespace, ":", faultSubCode.Name)];
		}
	}
}