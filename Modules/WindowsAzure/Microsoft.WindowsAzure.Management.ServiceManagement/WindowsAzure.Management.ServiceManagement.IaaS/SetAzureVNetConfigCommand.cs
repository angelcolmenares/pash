using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.IO;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Set", "AzureVNetConfig")]
	public class SetAzureVNetConfigCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, HelpMessage="Path to the Network Configuration file (.xml).")]
		[ValidateNotNullOrEmpty]
		public string ConfigurationPath
		{
			get;
			set;
		}

		public SetAzureVNetConfigCommand()
		{
		}

		public SetAzureVNetConfigCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.SetVirtualNetworkConfigProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void SetVirtualNetworkConfigProcess()
		{
			Action<string> action = null;
			this.ValidateParameters();
			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(this.ConfigurationPath, FileMode.Open);
				using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
				{
					try
					{
						SetAzureVNetConfigCommand setAzureVNetConfigCommand = this;
						if (action == null)
						{
							action = (string s) => base.Channel.SetNetworkConfiguration(s, fileStream);
						}
						((CmdletBase<IServiceManagement>)setAzureVNetConfigCommand).RetryCall(action);
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
			finally
			{
				if (fileStream != null)
				{
					fileStream.Close();
				}
			}
		}

		private void ValidateParameters()
		{
			if (File.Exists(this.ConfigurationPath))
			{
				return;
			}
			else
			{
				throw new ArgumentException("The specified network configuration file path does not exist.");
			}
		}
	}
}