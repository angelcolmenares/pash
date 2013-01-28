using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Storage
{
	[Cmdlet("Set", "AzureStorageAccount")]
	public class SetAzureStorageAccountCommand : ServiceManagementCmdletBase
	{
		[Parameter(HelpMessage="Description of the storage account.")]
		[ValidateLength(0, 0x400)]
		public string Description
		{
			get;
			set;
		}

		[Parameter(HelpMessage="Enable or Disable Geo Replication")]
		public bool? GeoReplicationEnabled
		{
			get;
			set;
		}

		[Parameter(HelpMessage="Label of the storage account.")]
		[ValidateLength(0, 100)]
		public string Label
		{
			get;
			set;
		}

		[Alias(new string[] { "ServiceName" })]
		[Parameter(Position=0, Mandatory=true, HelpMessage="Name of the storage account.")]
		[ValidateNotNullOrEmpty]
		public string StorageAccountName
		{
			get;
			set;
		}

		public SetAzureStorageAccountCommand()
		{
		}

		public SetAzureStorageAccountCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.SetStorageAccountProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void SetStorageAccountProcess()
		{
			if (this.Label == null && this.Description == null)
			{
				base.ThrowTerminatingError(new ErrorRecord(new Exception("You must specify a value for either Label or Description."), string.Empty, ErrorCategory.InvalidData, null));
			}
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					UpdateStorageServiceInput updateStorageServiceInput = new UpdateStorageServiceInput();
					bool? geoReplicationEnabled = this.GeoReplicationEnabled;
					if (geoReplicationEnabled.HasValue)
					{
						bool? nullable = this.GeoReplicationEnabled;
						updateStorageServiceInput.GeoReplicationEnabled = new bool?(nullable.Value);
					}
					if (this.Description != null)
					{
						updateStorageServiceInput.Description = this.Description;
					}
					if (this.Label != null)
					{
						updateStorageServiceInput.Label = ServiceManagementHelper.EncodeToBase64String(this.Label);
					}
					CmdletExtensions.WriteVerboseOutputForObject(this, updateStorageServiceInput);
					base.RetryCall((string s) => base.Channel.UpdateStorageService(s, this.StorageAccountName, updateStorageServiceInput));
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					ManagementOperationContext managementOperationContext = new ManagementOperationContext();
					managementOperationContext.OperationDescription = base.CommandRuntime.ToString();
					managementOperationContext.OperationId = operation.OperationTrackingId;
					managementOperationContext.OperationStatus = operation.Status;
					ManagementOperationContext managementOperationContext1 = managementOperationContext;
					base.WriteObject(managementOperationContext1, true);
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