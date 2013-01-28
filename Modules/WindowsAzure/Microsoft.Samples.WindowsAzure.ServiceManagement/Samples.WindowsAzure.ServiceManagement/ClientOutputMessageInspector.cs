using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	public class ClientOutputMessageInspector : IClientMessageInspector, IEndpointBehavior
	{
		public ClientOutputMessageInspector()
		{
		}

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}

		public void AfterReceiveReply(ref Message reply, object correlationState)
		{
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			clientRuntime.MessageInspectors.Add(this);
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
		}

		public object BeforeSendRequest(ref Message request, IClientChannel channel)
		{
			HttpRequestMessageProperty item = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
			if (item.Headers["x-ms-version"] == null)
			{
				item.Headers.Add("x-ms-version", "2012-03-01");
			}
			return null;
		}

		public void Validate(ServiceEndpoint endpoint)
		{
		}
	}
}