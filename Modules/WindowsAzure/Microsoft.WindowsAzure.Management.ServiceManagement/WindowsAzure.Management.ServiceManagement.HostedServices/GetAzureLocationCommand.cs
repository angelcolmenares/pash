using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
	[Cmdlet("Get", "AzureLocation")]
	public class GetAzureLocationCommand : ServiceManagementCmdletBase
	{
		public GetAzureLocationCommand()
		{
		}

		public GetAzureLocationCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public LocationList GetLocationsProcess(out string operationId)
		{
			Func<string, LocationList> func = null;
			LocationList locationList = null;
			operationId = string.Empty;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					GetAzureLocationCommand getAzureLocationCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.ListLocations(base.get_CurrentSubscription().get_SubscriptionId());
					}
					locationList = ((CmdletBase<IServiceManagement>)getAzureLocationCommand).RetryCall<LocationList>(func);
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					IEnumerable<LocationsContext> locationsContexts = locationList.Select<Location, LocationsContext>((Location location) => {
						LocationsContext locationsContext = new LocationsContext();
						locationsContext.set_OperationId(operation.OperationTrackingId);
						locationsContext.set_OperationDescription(this.CommandRuntime.ToString());
						locationsContext.set_OperationStatus(operation.Status);
						locationsContext.DisplayName = location.DisplayName;
						locationsContext.Name = location.Name;
						locationsContext.AvailableServices = location.AvailableServices;
						return locationsContext;
					}
					);
					base.WriteObject(locationsContexts, true);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this.WriteErrorDetails(communicationException);
				}
			}
			return locationList;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				string empty = string.Empty;
				this.GetLocationsProcess(out empty);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}