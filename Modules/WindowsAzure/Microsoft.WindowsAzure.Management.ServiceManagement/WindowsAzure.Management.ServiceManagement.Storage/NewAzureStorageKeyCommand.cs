using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Storage
{
	[Cmdlet("New", "AzureStorageKey")]
	public class NewAzureStorageKeyCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, HelpMessage="Key to regenerate. Primary | Secondary")]
		[ValidateSet(new string[] { "Primary", "Secondary" }, IgnoreCase=true)]
		public string KeyType
		{
			get;
			set;
		}

		[Alias(new string[] { "ServiceName" })]
		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Service name.")]
		[ValidateNotNullOrEmpty]
		public string StorageAccountName
		{
			get;
			set;
		}

		public NewAzureStorageKeyCommand()
		{
		}

		public NewAzureStorageKeyCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public void NewStorageKeyProcess()
		{
			Func<string, StorageService> func = null;
			StorageServiceKeyOperationContext storageServiceKeyOperationContext = null;
			RegenerateKeys regenerateKey = new RegenerateKeys();
			regenerateKey.KeyType = this.KeyType;
			RegenerateKeys regenerateKey1 = regenerateKey;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					CmdletExtensions.WriteVerboseOutputForObject(this, regenerateKey1);
					NewAzureStorageKeyCommand newAzureStorageKeyCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.RegenerateStorageServiceKeys(s, this.StorageAccountName, regenerateKey1);
					}
					StorageService storageService = ((CmdletBase<IServiceManagement>)newAzureStorageKeyCommand).RetryCall<StorageService>(func);
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
					this.WriteErrorDetails(communicationException);
				}
			}
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.NewStorageKeyProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}