using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;
using System.Threading;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Update", "AzureVM")]
	public class UpdateAzureVMCommand : IaaSDeploymentManagementCmdletBase
	{
		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="The name of the Virtual Machine to update.")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		[Alias(new string[] { "InputObject" })]
		[Parameter(Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, HelpMessage="Virtual Machine to update.")]
		[ValidateNotNullOrEmpty]
		public PersistentVM VM
		{
			get;
			set;
		}

		public UpdateAzureVMCommand()
		{
		}

		public UpdateAzureVMCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.ValidateParameters();
				this.UpdateVirtualMachineProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public string UpdateVirtualMachineProcess()
		{
			Action<string> action = null;
			SubscriptionData currentSubscription = CmdletSubscriptionExtensions.GetCurrentSubscription(this);
			if (base.CurrentDeployment != null)
			{
				foreach (DataVirtualHardDisk dataVirtualHardDisk in this.VM.DataVirtualHardDisks)
				{
					if (dataVirtualHardDisk.MediaLink == null && string.IsNullOrEmpty(dataVirtualHardDisk.DiskName))
					{
						CloudStorageAccount currentStorageAccount = currentSubscription.GetCurrentStorageAccount(base.Channel);
						if (currentStorageAccount != null)
						{
							DateTime now = DateTime.Now;
							string roleName = this.VM.RoleName;
							if (dataVirtualHardDisk.DiskLabel != null)
							{
								roleName = string.Concat(roleName, "-", dataVirtualHardDisk.DiskLabel);
							}
							object[] serviceName = new object[6];
							serviceName[0] = this.ServiceName;
							serviceName[1] = roleName;
							serviceName[2] = now.Year;
							serviceName[3] = now.Month;
							serviceName[4] = now.Day;
							serviceName[5] = now.Millisecond;
							string str = string.Format("{0}-{1}-{2}-{3}-{4}-{5}.vhd", serviceName);
							string absoluteUri = currentStorageAccount.BlobEndpoint.AbsoluteUri;
							if (!absoluteUri.EndsWith("/"))
							{
								absoluteUri = string.Concat(absoluteUri, "/");
							}
							dataVirtualHardDisk.MediaLink = new Uri(string.Concat(absoluteUri, "vhds/", str));
						}
						else
						{
							throw new ArgumentException("CurrentStorageAccount is not set or not accessible. Use Set-AzureSubscription subname -CurrentStorageAccount storageaccount to set it.");
						}
					}
					if (this.VM.DataVirtualHardDisks.Count <= 1)
					{
						continue;
					}
					Thread.Sleep(1);
				}
				PersistentVMRole persistentVMRole = new PersistentVMRole();
				persistentVMRole.AvailabilitySetName = this.VM.AvailabilitySetName;
				persistentVMRole.ConfigurationSets = this.VM.ConfigurationSets;
				persistentVMRole.DataVirtualHardDisks = this.VM.DataVirtualHardDisks;
				persistentVMRole.Label = this.VM.Label;
				persistentVMRole.OSVirtualHardDisk = this.VM.OSVirtualHardDisk;
				persistentVMRole.RoleName = this.VM.RoleName;
				persistentVMRole.RoleSize = this.VM.RoleSize;
				persistentVMRole.RoleType = this.VM.RoleType;
				PersistentVMRole persistentVMRole1 = persistentVMRole;
				using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
				{
					try
					{
						CmdletExtensions.WriteVerboseOutputForObject(this, persistentVMRole1);
						UpdateAzureVMCommand updateAzureVMCommand = this;
						if (action == null)
						{
							action = (string s) => base.Channel.UpdateRole(s, this.ServiceName, this.CurrentDeployment.Name, this.Name, persistentVMRole1);
						}
						((CmdletBase<IServiceManagement>)updateAzureVMCommand).RetryCall(action);
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
				return null;
			}
			else
			{
				return null;
			}
		}
	}
}