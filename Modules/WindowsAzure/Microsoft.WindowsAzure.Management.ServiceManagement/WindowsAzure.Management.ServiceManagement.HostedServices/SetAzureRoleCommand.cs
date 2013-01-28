using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
	[Cmdlet("Set", "AzureRole", DefaultParameterSetName="ParameterSetDeploymentSlot")]
	public class SetAzureRoleCommand : ServiceManagementCmdletBase
	{
		private Deployment currentDeployment;

		private Operation getDeploymentOperation;

		[Parameter(Position=3, Mandatory=true, HelpMessage="Instance count.")]
		[ValidateNotNullOrEmpty]
		public int Count
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=true, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string RoleName
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true)]
		public string ServiceName
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Slot of the deployment.")]
		[ValidateNotNullOrEmpty]
		public string Slot
		{
			get;
			set;
		}

		public SetAzureRoleCommand()
		{
		}

		public SetAzureRoleCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		private void GetCurrentDeployment()
		{
			Func<string, Deployment> func = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					new List<PersistentVMRoleContext>();
					SetAzureRoleCommand setAzureRoleCommand = this;
					SetAzureRoleCommand setAzureRoleCommand1 = this;
					if (func == null)
					{
						func = (string s) => base.Channel.GetDeploymentBySlot(s, this.ServiceName, this.Slot);
					}
					setAzureRoleCommand.currentDeployment = ((CmdletBase<IServiceManagement>)setAzureRoleCommand1).RetryCall<Deployment>(func);
					this.getDeploymentOperation = base.WaitForOperation("Get Deployment");
				}
				catch (CommunicationException communicationException)
				{
					throw;
				}
			}
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.SetRoleInstanceCountProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void SetRoleInstanceCountProcess()
		{
			Func<XElement, bool> func = null;
			this.GetCurrentDeployment();
			if (this.currentDeployment != null)
			{
				using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
				{
					try
					{
						XNamespace xNamespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration";
						XDocument xDocument = XDocument.Parse(ServiceManagementHelper.DecodeFromBase64String(this.currentDeployment.Configuration));
						IEnumerable<XElement> xElements = xDocument.Root.Elements(xNamespace + "Role");
						if (func == null)
						{
							func = (XElement p) => string.Compare(p.Attribute("name").Value, this.RoleName, true) == 0;
						}
						XElement xElement = xElements.Where<XElement>(func).SingleOrDefault<XElement>();
						if (xElement != null)
						{
							xElement.Element(xNamespace + "Instances").SetAttributeValue("count", this.Count);
						}
						using (OperationContextScope operationContextScope1 = new OperationContextScope((IContextChannel)base.Channel))
						{
							ChangeConfigurationInput changeConfigurationInput = new ChangeConfigurationInput();
							changeConfigurationInput.Configuration = ServiceManagementHelper.EncodeToBase64String(xDocument.ToString());
							ChangeConfigurationInput changeConfigurationInput1 = changeConfigurationInput;
							CmdletExtensions.WriteVerboseOutputForObject(this, xDocument);
							base.RetryCall((string s) => base.Channel.ChangeConfigurationBySlot(s, this.ServiceName, this.Slot, changeConfigurationInput1));
							Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
							ManagementOperationContext managementOperationContext = new ManagementOperationContext();
							managementOperationContext.set_OperationDescription(base.CommandRuntime.ToString());
							managementOperationContext.set_OperationId(operation.OperationTrackingId);
							managementOperationContext.set_OperationStatus(operation.Status);
							ManagementOperationContext managementOperationContext1 = managementOperationContext;
							base.WriteObject(managementOperationContext1, true);
						}
					}
					catch (EndpointNotFoundException endpointNotFoundException1)
					{
						EndpointNotFoundException endpointNotFoundException = endpointNotFoundException1;
						this.WriteErrorDetails(endpointNotFoundException);
					}
					catch (CommunicationException communicationException1)
					{
						CommunicationException communicationException = communicationException1;
						this.WriteErrorDetails(communicationException);
					}
				}
				return;
			}
			else
			{
				return;
			}
		}
	}
}