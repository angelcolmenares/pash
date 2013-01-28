using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.IO;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Get", "AzureVNetConfig")]
	public class GetAzureVNetConfigCommand : ServiceManagementCmdletBase
	{
		[Parameter(HelpMessage="The file path to save the network configuration to.")]
		[ValidateNotNullOrEmpty]
		public string ExportToFile
		{
			get;
			set;
		}

		public GetAzureVNetConfigCommand()
		{
		}

		public GetAzureVNetConfigCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public VirtualNetworkConfigContext GetVirtualNetworkConfigProcess()
		{
			VirtualNetworkConfigContext virtualNetworkConfigContext;
			Func<string, Stream> func = null;
			this.ValidateParameters();
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					GetAzureVNetConfigCommand getAzureVNetConfigCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.GetNetworkConfiguration(s);
					}
					Stream stream = ((CmdletBase<IServiceManagement>)getAzureVNetConfigCommand).RetryCall<Stream>(func);
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					if (stream != null)
					{
						StreamReader streamReader = new StreamReader(stream);
						string end = streamReader.ReadToEnd();
						VirtualNetworkConfigContext virtualNetworkConfigContext1 = new VirtualNetworkConfigContext();
						virtualNetworkConfigContext1.XMLConfiguration = end;
						virtualNetworkConfigContext1.set_OperationId(operation.OperationTrackingId);
						virtualNetworkConfigContext1.set_OperationDescription(base.CommandRuntime.ToString());
						virtualNetworkConfigContext1.set_OperationStatus(operation.Status);
						VirtualNetworkConfigContext virtualNetworkConfigContext2 = virtualNetworkConfigContext1;
						if (!string.IsNullOrEmpty(this.ExportToFile))
						{
							virtualNetworkConfigContext2.ExportToFile(this.ExportToFile);
						}
						virtualNetworkConfigContext = virtualNetworkConfigContext2;
						return virtualNetworkConfigContext;
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
						virtualNetworkConfigContext = null;
						return virtualNetworkConfigContext;
					}
				}
				virtualNetworkConfigContext = null;
			}
			return virtualNetworkConfigContext;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				VirtualNetworkConfigContext virtualNetworkConfigProcess = this.GetVirtualNetworkConfigProcess();
				if (virtualNetworkConfigProcess != null)
				{
					base.WriteObject(virtualNetworkConfigProcess, true);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		private void ValidateParameters()
		{
			if (string.IsNullOrEmpty(this.ExportToFile) || Directory.Exists(Path.GetDirectoryName(this.ExportToFile)))
			{
				return;
			}
			else
			{
				throw new ArgumentException("The directory specified to export the network configuration does not exist.");
			}
		}
	}
}