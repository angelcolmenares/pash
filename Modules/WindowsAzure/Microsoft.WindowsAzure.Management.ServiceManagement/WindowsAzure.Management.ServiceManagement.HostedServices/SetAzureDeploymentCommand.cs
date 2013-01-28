using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Helpers;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
	[Cmdlet("Set", "AzureDeployment")]
	public class SetAzureDeploymentCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, ParameterSetName="Config", HelpMessage="Change Configuration of Deployment")]
		public SwitchParameter Config
		{
			get;
			set;
		}

		[Parameter(Position=3, Mandatory=true, ParameterSetName="Upgrade", HelpMessage="Configuration file path. This parameter should specifiy a .cscfg file on disk.")]
		[Parameter(Position=2, Mandatory=true, ParameterSetName="Config", HelpMessage="Configuration file path. This parameter should specifiy a .cscfg file on disk.")]
		[ValidateNotNullOrEmpty]
		public string Configuration
		{
			get;
			set;
		}

		[Parameter(Position=8, Mandatory=false, ParameterSetName="Upgrade", HelpMessage="Force upgrade.")]
		public SwitchParameter Force
		{
			get;
			set;
		}

		[Parameter(Position=6, Mandatory=false, ParameterSetName="Upgrade", HelpMessage="Label name for the new deployment. Default: <Service Name> + <date time>")]
		[ValidateNotNullOrEmpty]
		public string Label
		{
			get;
			set;
		}

		[Parameter(Position=5, ParameterSetName="Upgrade", HelpMessage="Upgrade mode. Auto | Manual")]
		[ValidateSet(new string[] { "Auto", "Manual" })]
		public string Mode
		{
			get;
			set;
		}

		[Parameter(Position=3, Mandatory=true, ParameterSetName="Status", HelpMessage="New deployment status. Running | Suspended")]
		[ValidateSet(new string[] { "Running", "Suspended" }, IgnoreCase=true)]
		public string NewStatus
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=true, ParameterSetName="Upgrade", HelpMessage="Package location. This parameter should have the local file path or URI to a .cspkg in blob storage whose storage account is part of the same subscription/project.")]
		[ValidateNotNullOrEmpty]
		public string Package
		{
			get;
			set;
		}

		[Parameter(Position=7, Mandatory=false, ParameterSetName="Upgrade", HelpMessage="Name of role to upgrade.")]
		public string RoleName
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, ParameterSetName="Config", ValueFromPipelineByPropertyName=true, HelpMessage="Service name")]
		[Parameter(Position=1, Mandatory=true, ParameterSetName="Status", ValueFromPipelineByPropertyName=true, HelpMessage="Service name")]
		[Parameter(Position=1, Mandatory=true, ParameterSetName="Upgrade", ValueFromPipelineByPropertyName=true, HelpMessage="Service name")]
		[ValidateNotNullOrEmpty]
		public string ServiceName
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=true, ParameterSetName="Status", ValueFromPipelineByPropertyName=true, HelpMessage="Deployment slot. Staging | Production")]
		[Parameter(Position=3, Mandatory=true, ParameterSetName="Config", ValueFromPipelineByPropertyName=true, HelpMessage="Deployment slot. Staging | Production")]
		[Parameter(Position=4, Mandatory=true, ParameterSetName="Upgrade", ValueFromPipelineByPropertyName=true, HelpMessage="Deployment slot. Staging | Production")]
		[ValidateSet(new string[] { "Staging", "Production" }, IgnoreCase=true)]
		public string Slot
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="Status", HelpMessage="Change Status of Deployment")]
		public SwitchParameter Status
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="Upgrade", HelpMessage="Upgrade Deployment")]
		public SwitchParameter Upgrade
		{
			get;
			set;
		}

		public SetAzureDeploymentCommand()
		{
		}

		public SetAzureDeploymentCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.SetDeploymentTmpProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void SetDeploymentTmpProcess()
		{
			SetAzureDeploymentCommand.SetAzureDeploymentCommand variable = null;
			string mode;
			string base64String;
			string empty = string.Empty;
			if (!string.IsNullOrEmpty(this.Configuration))
			{
				empty = Utility.GetConfiguration(this.Configuration);
			}
			if (string.Compare(base.ParameterSetName, "Upgrade", StringComparison.OrdinalIgnoreCase) != 0)
			{
				if (string.Compare(base.ParameterSetName, "Config", StringComparison.OrdinalIgnoreCase) != 0)
				{
					Action<string> action = null;
					UpdateDeploymentStatusInput updateDeploymentStatusInput = new UpdateDeploymentStatusInput();
					updateDeploymentStatusInput.Status = this.NewStatus;
					UpdateDeploymentStatusInput updateDeploymentStatusInput1 = updateDeploymentStatusInput;
					using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
					{
						try
						{
							SetAzureDeploymentCommand setAzureDeploymentCommand = this;
							if (action == null)
							{
								action = (string s) => base.Channel.UpdateDeploymentStatusBySlot(s, this.ServiceName, this.Slot, updateDeploymentStatusInput1);
							}
							((CmdletBase<IServiceManagement>)setAzureDeploymentCommand).RetryCall(action);
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
				}
				else
				{
					Action<string> action1 = null;
					ChangeConfigurationInput changeConfigurationInput = new ChangeConfigurationInput();
					changeConfigurationInput.Configuration = empty;
					ChangeConfigurationInput changeConfigurationInput1 = changeConfigurationInput;
					using (OperationContextScope operationContextScope1 = new OperationContextScope((IContextChannel)base.Channel))
					{
						try
						{
							CmdletExtensions.WriteVerboseOutputForObject(this, changeConfigurationInput1);
							SetAzureDeploymentCommand setAzureDeploymentCommand1 = this;
							if (action1 == null)
							{
								action1 = (string s) => base.Channel.ChangeConfigurationBySlot(s, this.ServiceName, this.Slot, changeConfigurationInput1);
							}
							((CmdletBase<IServiceManagement>)setAzureDeploymentCommand1).RetryCall(action1);
							Operation operation1 = base.WaitForOperation(base.CommandRuntime.ToString());
							ManagementOperationContext managementOperationContext2 = new ManagementOperationContext();
							managementOperationContext2.set_OperationDescription(base.CommandRuntime.ToString());
							managementOperationContext2.set_OperationId(operation1.OperationTrackingId);
							managementOperationContext2.set_OperationStatus(operation1.Status);
							ManagementOperationContext managementOperationContext3 = managementOperationContext2;
							base.WriteObject(managementOperationContext3, true);
						}
						catch (CommunicationException communicationException3)
						{
							CommunicationException communicationException2 = communicationException3;
							this.WriteErrorDetails(communicationException2);
						}
					}
				}
			}
			else
			{
				Func<string, Uri> func = null;
				Action<string> action2 = null;
				Action<string> action3 = null;
				bool flag = false;
				base.CurrentSubscription = CmdletSubscriptionExtensions.GetCurrentSubscription(this);
				string currentStorageAccount = base.CurrentSubscription.CurrentStorageAccount;
				if (this.Package.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) || this.Package.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
				{
					Uri uri = new Uri(this.Package);
				}
				else
				{
					if (!string.IsNullOrEmpty(currentStorageAccount))
					{
						ProgressRecord progressRecord = new ProgressRecord(0, "Please wait...", "Uploading package to blob storage");
						base.WriteProgress(progressRecord);
						flag = true;
						SetAzureDeploymentCommand variable1 = variable;
						SetAzureDeploymentCommand setAzureDeploymentCommand2 = this;
						if (func == null)
						{
							func = (string s) => AzureBlob.UploadPackageToBlob(this.CreateChannel(), currentStorageAccount, s, this.Package);
						}
						packageUrl = ((CmdletBase<IServiceManagement>)setAzureDeploymentCommand2).RetryCall<Uri>(func);
					}
					else
					{
						throw new ArgumentException("CurrentStorageAccount is not set. Use Set-AzureSubscription subname -CurrentStorageAccount storageaccount to set it.");
					}
				}
				SetAzureDeploymentCommand variable2 = variable;
				UpgradeDeploymentInput upgradeDeploymentInput = new UpgradeDeploymentInput();
				UpgradeDeploymentInput upgradeDeploymentInput1 = upgradeDeploymentInput;
				if (this.Mode == null)
				{
					mode = "Auto";
				}
				else
				{
					mode = this.Mode;
				}
				upgradeDeploymentInput1.Mode = mode;
				upgradeDeploymentInput.Configuration = empty;
				upgradeDeploymentInput.PackageUrl = uri;
				UpgradeDeploymentInput upgradeDeploymentInput2 = upgradeDeploymentInput;
				if (this.Label != null)
				{
					base64String = ServiceManagementHelper.EncodeToBase64String(this.Label);
				}
				else
				{
					base64String = ServiceManagementHelper.EncodeToBase64String(this.ServiceName);
				}
				upgradeDeploymentInput2.Label = base64String;
				SwitchParameter force = this.Force;
				upgradeDeploymentInput.Force = new bool?(force.IsPresent);
				variable2.upgradeDeploymentInput = upgradeDeploymentInput;
				if (!string.IsNullOrEmpty(this.RoleName))
				{
					UpgradeDeploymentInput roleName = this.RoleName;
				}
				using (OperationContextScope operationContextScope2 = new OperationContextScope((IContextChannel)base.Channel))
				{
					try
					{
						CmdletExtensions.WriteVerboseOutputForObject(this, roleName);
						SetAzureDeploymentCommand setAzureDeploymentCommand3 = this;
						if (action2 == null)
						{
							action2 = (string s) => this.Channel.UpgradeDeploymentBySlot(s, this.ServiceName, this.Slot, this.upgradeDeploymentInput);
						}
						((CmdletBase<IServiceManagement>)setAzureDeploymentCommand3).RetryCall(action2);
						Operation operation2 = base.WaitForOperation(base.CommandRuntime.ToString());
						ManagementOperationContext managementOperationContext4 = new ManagementOperationContext();
						managementOperationContext4.OperationDescription = base.CommandRuntime.ToString();
						managementOperationContext4.OperationId = operation2.OperationTrackingId;
						managementOperationContext4.OperationStatus = operation2.Status;
						ManagementOperationContext managementOperationContext5 = managementOperationContext4;
						base.WriteObject(managementOperationContext5, true);
						if (flag)
						{
							SetAzureDeploymentCommand setAzureDeploymentCommand4 = this;
							if (action3 == null)
							{
								action3 = (string s) => AzureBlob.DeletePackageFromBlob(base.Channel, currentStorageAccount, s, uri);
							}
							((CmdletBase<IServiceManagement>)setAzureDeploymentCommand4).RetryCall(action3);
						}
					}
					catch (CommunicationException communicationException5)
					{
						CommunicationException communicationException4 = communicationException5;
						this.WriteErrorDetails(communicationException4);
					}
				}
			}
		}

		private void ValidateParameters()
		{
			if (string.Compare(base.ParameterSetName, "Upgrade", StringComparison.OrdinalIgnoreCase) != 0 || !string.IsNullOrEmpty(base.get_CurrentSubscription().get_CurrentStorageAccount()))
			{
				return;
			}
			else
			{
				throw new ArgumentException("CurrentStorageAccount is not set. Use Set-AzureSubscription subname -CurrentStorageAccount storageaccount to set it.");
			}
		}
	}
}