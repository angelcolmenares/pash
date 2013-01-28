using System.CodeDom.Compiler;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[ServiceContract(Namespace="http://schemas.xmlsoap.org/ws/2004/09/enumeration", ConfigurationName="Search", SessionMode=SessionMode.Required)]
	internal interface Search
	{
		[FaultContract(typeof(FaultDetail), Action="http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name="DestinationUnreachable", Namespace="http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(EnumerateFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="InvalidSortKey", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory")]
		[FaultContract(typeof(EnumerateFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="InvalidPropertyFault", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory")]
		[FaultContract(typeof(SupportedSelectOrSortDialect), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="UnsupportedSelectOrSortDialectFault", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory")]
		[FaultContract(typeof(FaultDetail), Action="http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name="EndpointUnavailable", Namespace="http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(EnumerateFault), Action="http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name="CannotProcessFilter")]
		[FaultContract(typeof(SupportedDialect), Action="http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name="FilterDialectRequestedUnavailable")]
		[FaultContract(typeof(EnumerateFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="EnumerationContextLimitExceeded", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory")]
		[FaultContract(typeof(EnumerateFault), Action="http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name="InvalidExpirationTime")]
		[OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/09/enumeration/Enumerate", ReplyAction="http://schemas.xmlsoap.org/ws/2004/09/enumeration/EnumerateResponse")]
		[XmlSerializerFormat(SupportFaults=true)]
		Message Enumerate(Message request);

		[FaultContract(typeof(FaultDetail), Action="http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name="EndpointUnavailable", Namespace="http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(FaultDetail), Action="http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name="InvalidEnumerationContext")]
		[OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/09/enumeration/GetStatus", ReplyAction="http://schemas.xmlsoap.org/ws/2004/09/enumeration/GetStatusResponse")]
		[XmlSerializerFormat(SupportFaults=true)]
		Message GetStatus(Message request);

		[FaultContract(typeof(PullFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="MaxTimeExceedsLimit", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory")]
		[FaultContract(typeof(FaultDetail), Action="http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name="InvalidEnumerationContext")]
		[FaultContract(typeof(PullFault), Action="http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name="TimedOut")]
		[FaultContract(typeof(FaultDetail), Action="http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name="EndpointUnavailable", Namespace="http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(PullFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="MaxCharsNotSupported", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory")]
		[FaultContract(typeof(FaultDetail), Action="http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name="DestinationUnreachable", Namespace="http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/09/enumeration/Pull", ReplyAction="http://schemas.xmlsoap.org/ws/2004/09/enumeration/PullResponse")]
		[XmlSerializerFormat(SupportFaults=true)]
		Message Pull(Message request);

		[FaultContract(typeof(FaultDetail), Action="http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name="EndpointUnavailable", Namespace="http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/09/enumeration/Release", ReplyAction="http://schemas.xmlsoap.org/ws/2004/09/enumeration/ReleaseResponse")]
		[XmlSerializerFormat(SupportFaults=true)]
		Message Release(Message request);

		[FaultContract(typeof(FaultDetail), Action="http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name="EndpointUnavailable", Namespace="http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(RenewFault), Action="http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name="UnableToRenew")]
		[FaultContract(typeof(FaultDetail), Action="http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name="InvalidEnumerationContext")]
		[OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/09/enumeration/Renew", ReplyAction="http://schemas.xmlsoap.org/ws/2004/09/enumeration/RenewResponse")]
		[XmlSerializerFormat(SupportFaults=true)]
		Message Renew(Message request);
	}
}