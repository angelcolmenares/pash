using System;
using Microsoft.WSMan.Management;
using System.ServiceModel;
using System.Linq;

namespace Microsoft.WSMan.PowerShell
{
	public class PowerShellTransferHandler : IManagementRequestHandler
	{

		private static readonly WSManPowerShellService _service = new WSManPowerShellService();

		public PowerShellTransferHandler ()
		{

		}


		#region IManagementRequestHandler implementation
		public bool CanHandle (string resourceUri)
		{
			return resourceUri != null && resourceUri.StartsWith (PowerShellNamespaces.Namespace, StringComparison.OrdinalIgnoreCase);
		}

		public object HandleGet (string fragmentExpression, System.Collections.Generic.IEnumerable<Selector> selectors)
		{
			Guid sessionId;
			Guid runspacePoolId;
			Guid commandId;
			object result = null;
			switch (fragmentExpression) {
				case "CreateSession":
					sessionId = _service.CreateSession ();
					result = new XmlFragment<SessionData>(new SessionData { Id = sessionId });
					break;
				case "CreateShell":
					sessionId = selectors.GetSelectorValueAsGuid ("SessionId");
					runspacePoolId = selectors.GetSelectorValueAsGuid ("RunspacePoolId");
					byte[] openContent = selectors.GetSelectorValueAsByteArray ("Stream");
					_service.CreateShell (sessionId, runspacePoolId, openContent);
					result = new XmlFragment<CreateShellData>(new CreateShellData {  });
					break;
				case "ConnectShell":
					sessionId = selectors.GetSelectorValueAsGuid ("SessionId");
					runspacePoolId = selectors.GetSelectorValueAsGuid ("RunspacePoolId");
					byte[] connectContent = selectors.GetSelectorValueAsByteArray ("Stream");
					var response = _service.ConnectShell(sessionId, runspacePoolId, connectContent);
					result = new XmlFragment<ConnectShellData>(new ConnectShellData { Stream = new StreamData { Name = "stdout", Value = Convert.ToBase64String (response) }});
					break;
				case "CloseSession":
					sessionId = selectors.GetSelectorValueAsGuid ("SessionId");
					_service.CloseSession (sessionId);
					result = new XmlFragment<SessionData>(new SessionData { Id = sessionId });
					break;
				case "CloseShell":
					sessionId = selectors.GetSelectorValueAsGuid ("SessionId");
					runspacePoolId = selectors.GetSelectorValueAsGuid ("RunspacePoolId");
					_service.CloseShell (sessionId, runspacePoolId);
					result = new XmlFragment<SessionData>(new SessionData { Id = sessionId });
					break;
				case "CompleteCommand":
					sessionId = selectors.GetSelectorValueAsGuid ("SessionId");
					_service.CompleteCommand(sessionId);
					result = new XmlFragment<SessionData>(new SessionData { Id = sessionId });
					break;
				case "RunCommand":
					sessionId = selectors.GetSelectorValueAsGuid ("SessionId");
					runspacePoolId = selectors.GetSelectorValueAsGuid ("RunspacePoolId");
					commandId = selectors.GetSelectorValueAsGuid ("CommandId");
					byte[] arguments = selectors.GetSelectorValueAsByteArray ("Stream");
					_service.RunCommand(sessionId, "", runspacePoolId, commandId, arguments);
					result = new XmlFragment<CommandData>(new CommandData {  });
					break;
				case "SendInput":
					sessionId = selectors.GetSelectorValueAsGuid ("SessionId");
					runspacePoolId = selectors.GetSelectorValueAsGuid ("RunspacePoolId");
					commandId = selectors.GetSelectorValueAsGuid ("CommandId");
					string streamName = selectors.GetSelectorValueAsString ("StreamName");
					byte[] inputContent = selectors.GetSelectorValueAsByteArray ("Stream");
					_service.SendInput (sessionId, runspacePoolId, commandId, streamName, inputContent);
					result = new XmlFragment<SendInputData>(new SendInputData { });
					break;
				case "ReceiveData":
					sessionId = selectors.GetSelectorValueAsGuid ("SessionId");
					commandId = selectors.GetSelectorValueAsGuid ("CommandId");
					byte[] receiveData = _service.ReceiveData (sessionId, commandId);
					result = new XmlFragment<ReceiveResponseData>(new ReceiveResponseData { Stream = new StreamData { Name = "stdout", Value = Convert.ToBase64String (receiveData) } });
					break;
			}
			return result;
		}

		public object HandlePut (string fragmentExpression, System.Collections.Generic.IEnumerable<Selector> selectors, Microsoft.WSMan.Transfer.ExtractBodyDelegate extractBodyCallback)
		{
			throw new NotImplementedException ();
		}

		public EndpointAddress HandleCreate (Microsoft.WSMan.Transfer.ExtractBodyDelegate extractBodyCallback)
		{
			throw new NotImplementedException ();
		}

		public void HandlerDelete (System.Collections.Generic.IEnumerable<Selector> selectors)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

