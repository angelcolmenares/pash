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
	[Cmdlet("Get", "AzureOSVersion")]
	public class GetAzureOSVersionCommand : ServiceManagementCmdletBase
	{
		public GetAzureOSVersionCommand()
		{
		}

		public GetAzureOSVersionCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public OperatingSystemList GetOSVersionsProcess(out Operation operation)
		{
			Func<string, OperatingSystemList> func = null;
			OperatingSystemList operatingSystemList = null;
			operation = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					GetAzureOSVersionCommand getAzureOSVersionCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.ListOperatingSystems(s);
					}
					operatingSystemList = ((CmdletBase<IServiceManagement>)getAzureOSVersionCommand).RetryCall<OperatingSystemList>(func);
					operation = base.WaitForOperation(base.CommandRuntime.ToString());
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this.WriteErrorDetails(communicationException);
				}
			}
			return operatingSystemList;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				Operation operation = null;
				OperatingSystemList oSVersionsProcess = this.GetOSVersionsProcess(out operation);
				IEnumerable<OSVersionsContext> oSVersionsContexts = oSVersionsProcess.Select<OperatingSystem, OSVersionsContext>((OperatingSystem os) => {
					string str;
					string str1;
					OSVersionsContext oSVersionsContext = new OSVersionsContext();
					oSVersionsContext.set_OperationId(operation.OperationTrackingId);
					oSVersionsContext.set_OperationDescription(this.CommandRuntime.ToString());
					oSVersionsContext.set_OperationStatus(operation.Status);
					oSVersionsContext.Family = os.Family;
					OSVersionsContext oSVersionsContext1 = oSVersionsContext;
					if (string.IsNullOrEmpty(os.FamilyLabel))
					{
						str = null;
					}
					else
					{
						str = ServiceManagementHelper.DecodeFromBase64String(os.FamilyLabel);
					}
					oSVersionsContext1.FamilyLabel = str;
					oSVersionsContext.IsActive = os.IsActive;
					oSVersionsContext.IsDefault = os.IsDefault;
					oSVersionsContext.Version = os.Version;
					OSVersionsContext oSVersionsContext2 = oSVersionsContext;
					if (string.IsNullOrEmpty(os.Label))
					{
						str1 = null;
					}
					else
					{
						str1 = ServiceManagementHelper.DecodeFromBase64String(os.Label);
					}
					oSVersionsContext2.Label = str1;
					return oSVersionsContext;
				}
				);
				base.WriteObject(oSVersionsContexts, true);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}