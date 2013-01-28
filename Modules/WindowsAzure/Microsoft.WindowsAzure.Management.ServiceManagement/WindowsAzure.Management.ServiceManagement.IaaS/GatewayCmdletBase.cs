using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Service;
using Microsoft.WindowsAzure.Management.Service.Gateway;
using System;
using System.Globalization;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	public class GatewayCmdletBase : CmdletBase<IGatewayServiceManagement>
	{
		public GatewayCmdletBase()
		{
		}

		protected override IGatewayServiceManagement CreateChannel()
		{
			if (base.get_ServiceBinding() == null)
			{
				base.set_ServiceBinding(ConfigurationConstants.WebHttpBinding());
			}
			if (!string.IsNullOrEmpty(base.get_CurrentSubscription().get_ServiceEndpoint()))
			{
				base.set_ServiceEndpoint(base.get_CurrentSubscription().get_ServiceEndpoint());
			}
			else
			{
				base.set_ServiceEndpoint("https://management.core.windows.net");
			}
			return GatewayManagementHelper.CreateGatewayManagementChannel(base.get_ServiceBinding(), new Uri(base.get_ServiceEndpoint()), base.get_CurrentSubscription().get_Certificate());
		}

		protected override void WriteErrorDetails(CommunicationException exception)
		{
			ServiceManagementError serviceManagementError = null;
			GatewayManagementHelper.TryGetExceptionDetails(exception, out serviceManagementError);
			if (serviceManagementError != null)
			{
				object[] code = new object[2];
				code[0] = serviceManagementError.Code;
				code[1] = serviceManagementError.Message;
				string str = string.Format(CultureInfo.InvariantCulture, "HTTP Status Code: {0} - HTTP Error Message: {1}", code);
				base.WriteError(new ErrorRecord(new CommunicationException(str), string.Empty, ErrorCategory.CloseError, null));
				return;
			}
			else
			{
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
				return;
			}
		}
	}
}