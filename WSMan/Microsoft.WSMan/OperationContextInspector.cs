using System;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Microsoft.WSMan
{
	public class OperationContextBehavior : IEndpointBehavior
	{
		#region IEndpointBehavior implementation		
		public void AddBindingParameters (ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection parameters)
		{

		}		

		public void ApplyDispatchBehavior (ServiceEndpoint serviceEndpoint, EndpointDispatcher dispatcher)
		{

		}		

		public void ApplyClientBehavior (ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
		{
			behavior.MessageInspectors.Add (new OperationContextInspector());
		}		

		public void Validate (ServiceEndpoint serviceEndpoint)
		{

		}		
		#endregion
	}

	public class OperationContextInspector : IClientMessageInspector
	{
		public OperationContextInspector ()
		{

		}

		#region IClientMessageInspector implementation

		public void AfterReceiveReply (ref System.ServiceModel.Channels.Message message, object correlationState)
		{
			OperationContextProxy.Current.IncomingMessageHeaders = message.Headers;
		}

		public object BeforeSendRequest (ref System.ServiceModel.Channels.Message message, IClientChannel channel)
		{
			return  null;
		}
		#endregion
	}
}

