using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Storage
{
	[Cmdlet("Remove", "AzureStorageAccount")]
	public class RemoveAzureStorageAccountCommand : ServiceManagementCmdletBase
	{
		[Alias(new string[] { "ServiceName" })]
		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="The name of the storage account to be removed.")]
		[ValidateNotNullOrEmpty]
		public string StorageAccountName
		{
			get;
			set;
		}

		public RemoveAzureStorageAccountCommand()
		{
		}

		public RemoveAzureStorageAccountCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				string str = this.RemoveStorageAccountProcess();
				if (!string.IsNullOrEmpty(str))
				{
					StorageServiceOperationContext storageServiceOperationContext = new StorageServiceOperationContext();
					storageServiceOperationContext.StorageAccountName = this.StorageAccountName;
					storageServiceOperationContext.set_OperationId(str);
					StorageServiceOperationContext storageServiceOperationContext1 = storageServiceOperationContext;
					base.WriteObject(storageServiceOperationContext1, true);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public string RemoveStorageAccountProcess()
		{
			Action<string> action = null;
			string empty = string.Empty;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					RemoveAzureStorageAccountCommand removeAzureStorageAccountCommand = this;
					if (action == null)
					{
						action = (string s) => base.Channel.DeleteStorageService(s, this.StorageAccountName);
					}
					((CmdletBase<IServiceManagement>)removeAzureStorageAccountCommand).RetryCall(action);
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					ManagementOperationContext managementOperationContext = new ManagementOperationContext();
					managementOperationContext.set_OperationDescription(base.CommandRuntime.ToString());
					managementOperationContext.set_OperationId(operation.OperationTrackingId);
					managementOperationContext.set_OperationStatus(operation.Status);
					ManagementOperationContext managementOperationContext1 = managementOperationContext;
					base.WriteObject(managementOperationContext1, true);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this.WriteErrorDetails(communicationException);
				}
			}
			return empty;
		}
	}
}