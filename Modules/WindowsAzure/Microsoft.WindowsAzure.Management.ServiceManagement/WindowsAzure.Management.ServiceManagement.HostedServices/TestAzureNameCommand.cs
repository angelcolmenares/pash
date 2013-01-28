using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
	[Cmdlet("Test", "AzureName")]
	public class TestAzureNameCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=1, ParameterSetName="Service", Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Cloud service name.")]
		[Parameter(Position=1, ParameterSetName="Storage", Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Storage account name.")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="Service", HelpMessage="Test for a cloud service name.")]
		public SwitchParameter Service
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="Storage", HelpMessage="Test for a storage account name.")]
		public SwitchParameter Storage
		{
			get;
			set;
		}

		public TestAzureNameCommand()
		{
		}

		public TestAzureNameCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.TestIsNameAvailableProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void TestIsNameAvailableProcess()
		{
			AvailabilityResponse availabilityResponse;
			Func<string, AvailabilityResponse> func = null;
			Func<string, AvailabilityResponse> func1 = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					SwitchParameter service = this.Service;
					if (!service.IsPresent)
					{
						TestAzureNameCommand testAzureNameCommand = this;
						if (func1 == null)
						{
							func1 = (string s) => base.Channel.IsStorageServiceAvailable(s, this.Name);
						}
						availabilityResponse = ((CmdletBase<IServiceManagement>)testAzureNameCommand).RetryCall<AvailabilityResponse>(func1);
					}
					else
					{
						TestAzureNameCommand testAzureNameCommand1 = this;
						if (func == null)
						{
							func = (string s) => base.Channel.IsDNSAvailable(s, this.Name);
						}
						availabilityResponse = ((CmdletBase<IServiceManagement>)testAzureNameCommand1).RetryCall<AvailabilityResponse>(func);
					}
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					base.WriteDebug(string.Concat("OperationID: ", operation.OperationTrackingId));
					bool result = !availabilityResponse.Result;
					base.WriteObject(result, true);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this.WriteErrorDetails(communicationException);
				}
			}
		}
	}
}