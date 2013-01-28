using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Storage
{
	[Cmdlet("Get", "AzureStorageKey")]
	public class GetAzureStorageKeyCommand : ServiceManagementCmdletBase
	{
		[Alias(new string[] { "ServiceName" })]
		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Service name.")]
		[ValidateNotNullOrEmpty]
		public string StorageAccountName
		{
			get;
			set;
		}

		public GetAzureStorageKeyCommand()
		{
		}

		public GetAzureStorageKeyCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public void GetStorageKeysProcess()
		{
			Func<string, StorageService> func = null;
			StorageServiceKeyOperationContext storageServiceKeyOperationContext = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					GetAzureStorageKeyCommand getAzureStorageKeyCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.GetStorageKeys(s, this.StorageAccountName);
					}
					StorageService storageService = ((CmdletBase<IServiceManagement>)getAzureStorageKeyCommand).RetryCall<StorageService>(func);
					if (storageService != null)
					{
						StorageServiceKeyOperationContext storageAccountName = new StorageServiceKeyOperationContext();
						storageAccountName.StorageAccountName = this.StorageAccountName;
						storageAccountName.Primary = storageService.StorageServiceKeys.Primary;
						storageAccountName.Secondary = storageService.StorageServiceKeys.Secondary;
						storageServiceKeyOperationContext = storageAccountName;
					}
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					storageServiceKeyOperationContext.set_OperationDescription(base.CommandRuntime.ToString());
					storageServiceKeyOperationContext.set_OperationId(operation.OperationTrackingId);
					storageServiceKeyOperationContext.set_OperationStatus(operation.Status);
					base.WriteObject(storageServiceKeyOperationContext, true);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					if (communicationException as EndpointNotFoundException == null || base.IsVerbose())
					{
						this.WriteErrorDetails(communicationException);
					}
					else
					{
						base.WriteObject(null);
						return;
					}
				}
			}
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.GetStorageKeysProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}