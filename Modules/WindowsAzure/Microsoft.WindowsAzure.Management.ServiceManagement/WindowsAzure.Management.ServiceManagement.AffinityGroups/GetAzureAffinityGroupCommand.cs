using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.AffinityGroups
{
	[Cmdlet("Get", "AzureAffinityGroup")]
	public class GetAzureAffinityGroupCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=false, ValueFromPipelineByPropertyName=true, HelpMessage="Affinity Group name")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		public GetAzureAffinityGroupCommand()
		{
		}

		public GetAzureAffinityGroupCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public IEnumerable<AffinityGroup> GetAffinityGroupProcess(out Operation operation)
		{
			IEnumerable<AffinityGroup> affinityGroups;
			Func<string, AffinityGroup> func = null;
			Func<string, AffinityGroupList> func1 = null;
			IEnumerable<AffinityGroup> affinityGroups1 = null;
			operation = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					if (this.Name == null)
					{
						GetAzureAffinityGroupCommand getAzureAffinityGroupCommand = this;
						if (func1 == null)
						{
							func1 = (string s) => base.Channel.ListAffinityGroups(s);
						}
						affinityGroups1 = ((CmdletBase<IServiceManagement>)getAzureAffinityGroupCommand).RetryCall<AffinityGroupList>(func1);
					}
					else
					{
						AffinityGroup[] affinityGroupArray = new AffinityGroup[1];
						AffinityGroup[] affinityGroupArray1 = affinityGroupArray;
						int num = 0;
						GetAzureAffinityGroupCommand getAzureAffinityGroupCommand1 = this;
						if (func == null)
						{
							func = (string s) => base.Channel.GetAffinityGroup(s, this.Name);
						}
						affinityGroupArray1[num] = ((CmdletBase<IServiceManagement>)getAzureAffinityGroupCommand1).RetryCall<AffinityGroup>(func);
						affinityGroups1 = affinityGroupArray;
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
						affinityGroups = null;
						return affinityGroups;
					}
				}
				return affinityGroups1;
			}
			return affinityGroups;
		}

		protected override void ProcessRecord()
		{
			try
			{
				Func<AffinityGroup, AffinityGroupContext> func = null;
				base.ProcessRecord();
				Operation operation = null;
				IEnumerable<AffinityGroup> affinityGroupProcess = this.GetAffinityGroupProcess(out operation);
				if (affinityGroupProcess != null)
				{
					IEnumerable<AffinityGroup> affinityGroups = affinityGroupProcess;
					if (func == null)
					{
						func = (AffinityGroup affinityGroup) => {
							string str;
							IEnumerable<AffinityGroupContext.Service> services;
							IEnumerable<AffinityGroupContext.Service> services1;
							AffinityGroupContext affinityGroupContext = new AffinityGroupContext();
							affinityGroupContext.set_OperationId(operation.OperationTrackingId);
							affinityGroupContext.set_OperationDescription(this.CommandRuntime.ToString());
							affinityGroupContext.set_OperationStatus(operation.Status);
							affinityGroupContext.Name = affinityGroup.Name;
							AffinityGroupContext affinityGroupContext1 = affinityGroupContext;
							if (string.IsNullOrEmpty(affinityGroup.Label))
							{
								str = null;
							}
							else
							{
								str = ServiceManagementHelper.DecodeFromBase64String(affinityGroup.Label);
							}
							affinityGroupContext1.Label = str;
							affinityGroupContext.Description = affinityGroup.Description;
							affinityGroupContext.Location = affinityGroup.Location;
							AffinityGroupContext affinityGroupContext2 = affinityGroupContext;
							if (affinityGroup.HostedServices != null)
							{
								HostedServiceList hostedServices = affinityGroup.HostedServices;
								services = hostedServices.Select<HostedService, AffinityGroupContext.Service>((HostedService p) => {
									AffinityGroupContext.Service service = new AffinityGroupContext.Service();
									service.Url = p.Url;
									service.ServiceName = p.ServiceName;
									return service;
								});
							}
							else
							{
								services = new AffinityGroupContext.Service[0];
							}
							affinityGroupContext2.HostedServices = services;
							AffinityGroupContext affinityGroupContext3 = affinityGroupContext;
							if (affinityGroup.StorageServices != null)
							{
								StorageServiceList storageServices = affinityGroup.StorageServices;
								services1 = storageServices.Select<StorageService, AffinityGroupContext.Service>((StorageService p) => {
									AffinityGroupContext.Service service = new AffinityGroupContext.Service();
									service.Url = p.Url;
									service.ServiceName = p.ServiceName;
									return service;
								});
							}
							else
							{
								services1 = new AffinityGroupContext.Service[0];
							}
							affinityGroupContext3.StorageServices = services1;
							return affinityGroupContext;
						}
						;
					}
					IEnumerable<AffinityGroupContext> affinityGroupContexts = affinityGroups.Select<AffinityGroup, AffinityGroupContext>(func);
					base.WriteObject(affinityGroupContexts, true);
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