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
	[Cmdlet("Get", "AzureService")]
	public class GetAzureServiceCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=false, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string ServiceName
		{
			get;
			set;
		}

		public GetAzureServiceCommand()
		{
		}

		public GetAzureServiceCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public IEnumerable<HostedService> GetHostedServiceProcess(out Operation operation)
		{
			IEnumerable<HostedService> hostedServices;
			Func<string, HostedService> func = null;
			Func<string, HostedServiceList> func1 = null;
			IEnumerable<HostedService> hostedServices1 = null;
			operation = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					if (this.ServiceName == null)
					{
						GetAzureServiceCommand getAzureServiceCommand = this;
						if (func1 == null)
						{
							func1 = (string s) => base.Channel.ListHostedServices(s);
						}
						hostedServices1 = ((CmdletBase<IServiceManagement>)getAzureServiceCommand).RetryCall<HostedServiceList>(func1);
					}
					else
					{
						HostedService[] hostedServiceArray = new HostedService[1];
						HostedService[] hostedServiceArray1 = hostedServiceArray;
						int num = 0;
						GetAzureServiceCommand getAzureServiceCommand1 = this;
						if (func == null)
						{
							func = (string s) => base.Channel.GetHostedService(s, this.ServiceName);
						}
						hostedServiceArray1[num] = ((CmdletBase<IServiceManagement>)getAzureServiceCommand1).RetryCall<HostedService>(func);
						hostedServices1 = hostedServiceArray;
					}
					operation = base.WaitForOperation(base.CommandRuntime.ToString());
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
						hostedServices = null;
						return hostedServices;
					}
				}
				return hostedServices1;
			}
			return hostedServices;
		}

		protected override void ProcessRecord()
		{
			try
			{
				Func<HostedService, HostedServiceDetailedContext> func = null;
				base.ProcessRecord();
				Operation operation = null;
				IEnumerable<HostedService> hostedServiceProcess = this.GetHostedServiceProcess(out operation);
				if (hostedServiceProcess != null)
				{
					IEnumerable<HostedService> hostedServices = hostedServiceProcess;
					if (func == null)
					{
						func = (HostedService service) => {
							string serviceName;
							string empty;
							string str;
							string serviceName1;
							if (this.ServiceName == null)
							{
								HostedServiceDetailedContext hostedServiceDetailedContext = new HostedServiceDetailedContext();
								HostedServiceDetailedContext hostedServiceDetailedContext1 = hostedServiceDetailedContext;
								if (service.ServiceName != null)
								{
									serviceName = service.ServiceName;
								}
								else
								{
									serviceName = this.ServiceName;
								}
								hostedServiceDetailedContext1.ServiceName = serviceName;
								hostedServiceDetailedContext.Url = service.Url;
								HostedServiceDetailedContext hostedServiceDetailedContext2 = hostedServiceDetailedContext;
								if (string.IsNullOrEmpty(service.HostedServiceProperties.Label))
								{
									empty = string.Empty;
								}
								else
								{
									empty = ServiceManagementHelper.DecodeFromBase64String(service.HostedServiceProperties.Label);
								}
								hostedServiceDetailedContext2.Label = empty;
								hostedServiceDetailedContext.Description = service.HostedServiceProperties.Description;
								hostedServiceDetailedContext.Location = service.HostedServiceProperties.Location;
								hostedServiceDetailedContext.Status = service.HostedServiceProperties.Status;
								hostedServiceDetailedContext.DateCreated = service.HostedServiceProperties.DateCreated;
								hostedServiceDetailedContext.DateModified = service.HostedServiceProperties.DateLastModified;
								hostedServiceDetailedContext.AffinityGroup = service.HostedServiceProperties.AffinityGroup;
								hostedServiceDetailedContext.set_OperationId(operation.OperationTrackingId);
								hostedServiceDetailedContext.set_OperationDescription(this.CommandRuntime.ToString());
								hostedServiceDetailedContext.set_OperationStatus(operation.Status);
								return hostedServiceDetailedContext;
							}
							else
							{
								if (string.IsNullOrEmpty(service.HostedServiceProperties.Label))
								{
									str = string.Empty;
								}
								else
								{
									str = ServiceManagementHelper.DecodeFromBase64String(service.HostedServiceProperties.Label);
								}
								string str1 = str;
								HostedServiceDetailedContext url = new HostedServiceDetailedContext();
								HostedServiceDetailedContext hostedServiceDetailedContext3 = url;
								if (service.ServiceName != null)
								{
									serviceName1 = service.ServiceName;
								}
								else
								{
									serviceName1 = this.ServiceName;
								}
								hostedServiceDetailedContext3.ServiceName = serviceName1;
								url.Url = service.Url;
								url.Label = str1;
								url.Description = service.HostedServiceProperties.Description;
								url.AffinityGroup = service.HostedServiceProperties.AffinityGroup;
								url.Location = service.HostedServiceProperties.Location;
								url.Status = service.HostedServiceProperties.Status;
								url.DateCreated = service.HostedServiceProperties.DateCreated;
								url.DateModified = service.HostedServiceProperties.DateLastModified;
								url.set_OperationId(operation.OperationTrackingId);
								url.set_OperationDescription(this.CommandRuntime.ToString());
								url.set_OperationStatus(operation.Status);
								return url;
							}
						}
						;
					}
					IEnumerable<HostedServiceDetailedContext> hostedServiceDetailedContexts = hostedServices.Select<HostedService, HostedServiceDetailedContext>(func);
					base.WriteObject(hostedServiceDetailedContexts, true);
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