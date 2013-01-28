using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Storage
{
	[Cmdlet("Get", "AzureStorageAccount")]
	public class GetAzureStorageAccountCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=false, ValueFromPipelineByPropertyName=true, HelpMessage="Storage Account Name.")]
		[ValidateNotNullOrEmpty]
		public string StorageAccountName
		{
			get;
			set;
		}

		public GetAzureStorageAccountCommand()
		{
		}

		public GetAzureStorageAccountCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public IEnumerable<StorageServicePropertiesOperationContext> GetStorageServicesProcess()
		{
			IEnumerable<StorageServicePropertiesOperationContext> storageServicePropertiesOperationContexts;
			Func<string, StorageServiceList> func = null;
			Func<StorageServicePropertiesOperationContext, bool> func1 = null;
			IEnumerable<StorageServicePropertiesOperationContext> storageServicePropertiesOperationContexts1 = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					Func<StorageService, StorageServicePropertiesOperationContext> func2 = null;
					Func<StorageService, StorageServicePropertiesOperationContext> func3 = null;
					GetAzureStorageAccountCommand getAzureStorageAccountCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.ListStorageServices(s);
					}
					StorageServiceList storageServiceList = ((CmdletBase<IServiceManagement>)getAzureStorageAccountCommand).RetryCall<StorageServiceList>(func);
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					if (storageServiceList != null)
					{
						if (!string.IsNullOrEmpty(this.StorageAccountName))
						{
							StorageServiceList storageServiceList1 = storageServiceList;
							if (func3 == null)
							{
								func3 = (StorageService service) => {
									string empty;
									StorageServicePropertiesOperationContext storageServicePropertiesOperationContext = new StorageServicePropertiesOperationContext();
									storageServicePropertiesOperationContext.StorageAccountName = service.ServiceName;
									storageServicePropertiesOperationContext.set_OperationId(this.operation.OperationTrackingId);
									storageServicePropertiesOperationContext.set_OperationDescription(this.CommandRuntime.ToString());
									storageServicePropertiesOperationContext.set_OperationStatus(this.operation.Status);
									storageServicePropertiesOperationContext.AffinityGroup = service.StorageServiceProperties.AffinityGroup;
									storageServicePropertiesOperationContext.StorageAccountDescription = service.StorageServiceProperties.Description;
									StorageServicePropertiesOperationContext storageServicePropertiesOperationContext1 = storageServicePropertiesOperationContext;
									if (service.StorageServiceProperties.Label != null)
									{
										empty = ServiceManagementHelper.DecodeFromBase64String(service.StorageServiceProperties.Label);
									}
									else
									{
										empty = string.Empty;
									}
									storageServicePropertiesOperationContext1.Label = empty;
									storageServicePropertiesOperationContext.Location = service.StorageServiceProperties.Location;
									storageServicePropertiesOperationContext.Endpoints = service.StorageServiceProperties.Endpoints;
									storageServicePropertiesOperationContext.StorageAccountStatus = service.StorageServiceProperties.Status;
									storageServicePropertiesOperationContext.GeoReplicationEnabled = service.StorageServiceProperties.GeoReplicationEnabled;
									storageServicePropertiesOperationContext.GeoPrimaryLocation = service.StorageServiceProperties.GeoPrimaryRegion;
									storageServicePropertiesOperationContext.GeoSecondaryLocation = service.StorageServiceProperties.StatusOfSecondary;
									storageServicePropertiesOperationContext.StatusOfPrimary = service.StorageServiceProperties.StatusOfPrimary;
									storageServicePropertiesOperationContext.StatusOfSecondary = service.StorageServiceProperties.StatusOfSecondary;
									return storageServicePropertiesOperationContext;
								}
								;
							}
							IEnumerable<StorageServicePropertiesOperationContext> storageServicePropertiesOperationContexts2 = storageServiceList1.Select<StorageService, StorageServicePropertiesOperationContext>(func3);
							if (func1 == null)
							{
								func1 = (StorageServicePropertiesOperationContext s) => s.StorageAccountName.Equals(this.StorageAccountName, StringComparison.InvariantCultureIgnoreCase);
							}
							storageServicePropertiesOperationContexts1 = storageServicePropertiesOperationContexts2.Where<StorageServicePropertiesOperationContext>(func1);
						}
						else
						{
							StorageServiceList storageServiceList2 = storageServiceList;
							if (func2 == null)
							{
								func2 = (StorageService service) => {
									string empty;
									StorageServicePropertiesOperationContext storageServicePropertiesOperationContext = new StorageServicePropertiesOperationContext();
									storageServicePropertiesOperationContext.StorageAccountName = service.ServiceName;
									storageServicePropertiesOperationContext.OperationId = operation.OperationTrackingId;
									storageServicePropertiesOperationContext.OperationDescription = this.CommandRuntime.ToString();
									storageServicePropertiesOperationContext.OperationStatus = operation.Status;
									storageServicePropertiesOperationContext.AffinityGroup = service.StorageServiceProperties.AffinityGroup;
									storageServicePropertiesOperationContext.StorageAccountDescription = service.StorageServiceProperties.Description;
									StorageServicePropertiesOperationContext storageServicePropertiesOperationContext1 = storageServicePropertiesOperationContext;
									if (service.StorageServiceProperties.Label != null)
									{
										empty = ServiceManagementHelper.DecodeFromBase64String(service.StorageServiceProperties.Label);
									}
									else
									{
										empty = string.Empty;
									}
									storageServicePropertiesOperationContext1.Label = empty;
									storageServicePropertiesOperationContext.Location = service.StorageServiceProperties.Location;
									storageServicePropertiesOperationContext.Endpoints = service.StorageServiceProperties.Endpoints;
									storageServicePropertiesOperationContext.StorageAccountStatus = service.StorageServiceProperties.Status;
									storageServicePropertiesOperationContext.GeoReplicationEnabled = service.StorageServiceProperties.GeoReplicationEnabled;
									storageServicePropertiesOperationContext.GeoPrimaryLocation = service.StorageServiceProperties.GeoPrimaryRegion;
									storageServicePropertiesOperationContext.GeoSecondaryLocation = service.StorageServiceProperties.StatusOfSecondary;
									storageServicePropertiesOperationContext.StatusOfPrimary = service.StorageServiceProperties.StatusOfPrimary;
									storageServicePropertiesOperationContext.StatusOfSecondary = service.StorageServiceProperties.StatusOfSecondary;
									return storageServicePropertiesOperationContext;
								}
								;
							}
							storageServicePropertiesOperationContexts1 = storageServiceList2.Select<StorageService, StorageServicePropertiesOperationContext>(func2);
						}
					}
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
						storageServicePropertiesOperationContexts = null;
						return storageServicePropertiesOperationContexts;
					}
				}
				return storageServicePropertiesOperationContexts1;
			}
			return storageServicePropertiesOperationContexts;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				IEnumerable<StorageServicePropertiesOperationContext> storageServicesProcess = this.GetStorageServicesProcess();
				if (storageServicesProcess != null)
				{
					base.WriteObject(storageServicesProcess, true);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}