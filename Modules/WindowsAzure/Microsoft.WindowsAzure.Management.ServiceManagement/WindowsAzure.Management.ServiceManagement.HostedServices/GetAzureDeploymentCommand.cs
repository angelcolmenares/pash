using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.Net;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
	[Cmdlet("Get", "AzureDeployment")]
	public class GetAzureDeploymentCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Service name.")]
		public string ServiceName
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=false, HelpMessage="Deployment slot. Staging | Production (default Production)")]
		[ValidateSet(new string[] { "Staging", "Production" }, IgnoreCase=true)]
		public string Slot
		{
			get;
			set;
		}

		public GetAzureDeploymentCommand()
		{
		}

		public GetAzureDeploymentCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public Deployment GetDeploymentProcess(out Operation operation)
		{
			Deployment deployment;
			Func<string, Deployment> func = null;
			operation = null;
			try
			{
				if (string.IsNullOrEmpty(this.Slot))
				{
					this.Slot = "Production";
				}
				using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
				{
					GetAzureDeploymentCommand getAzureDeploymentCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.GetDeploymentBySlot(s, this.ServiceName, this.Slot);
					}
					Deployment deployment1 = ((CmdletBase<IServiceManagement>)getAzureDeploymentCommand).RetryCall<Deployment>(func);
					operation = base.WaitForOperation(base.CommandRuntime.ToString());
					deployment = deployment1;
				}
			}
			catch (CommunicationException communicationException1)
			{
				CommunicationException communicationException = communicationException1;
				if (communicationException as EndpointNotFoundException == null || base.IsVerbose())
				{
					this.WriteErrorDetails(communicationException);
					return null;
				}
				else
				{
					deployment = null;
				}
			}
			return deployment;
		}

		private static bool IsStatusResourceNotFound(CommunicationException ex)
		{
			WebException innerException = ex.InnerException as WebException;
			if (innerException != null)
			{
				HttpWebResponse response = innerException.Response as HttpWebResponse;
				if (response != null && response.StatusCode == HttpStatusCode.NotFound)
				{
					return true;
				}
			}
			return false;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				Operation operation = null;
				Deployment deploymentProcess = this.GetDeploymentProcess(out operation);
				if (deploymentProcess != null)
				{
					if (string.IsNullOrEmpty(deploymentProcess.DeploymentSlot))
					{
						deploymentProcess.DeploymentSlot = this.Slot;
					}
					DeploymentInfoContext deploymentInfoContext = new DeploymentInfoContext(deploymentProcess);
					deploymentInfoContext.ServiceName = this.ServiceName;
					deploymentInfoContext.set_OperationId(operation.OperationTrackingId);
					deploymentInfoContext.set_OperationDescription(base.CommandRuntime.ToString());
					deploymentInfoContext.set_OperationStatus(operation.Status);
					DeploymentInfoContext deploymentInfoContext1 = deploymentInfoContext;
					base.WriteObject(deploymentInfoContext1, true);
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