using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Certificates
{
	[Cmdlet("Remove", "AzureCertificate")]
	public class RemoveAzureCertificateCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Hosted Service Name.")]
		[ValidateNotNullOrEmpty]
		public string ServiceName
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Certificate thumbprint.")]
		[ValidateNotNullOrEmpty]
		public string Thumbprint
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Certificate thumbprint algorithm.")]
		[ValidateNotNullOrEmpty]
		public string ThumbprintAlgorithm
		{
			get;
			set;
		}

		public RemoveAzureCertificateCommand()
		{
		}

		public RemoveAzureCertificateCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.RemoveCertificateProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void RemoveCertificateProcess()
		{
			Action<string> action = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					RemoveAzureCertificateCommand removeAzureCertificateCommand = this;
					if (action == null)
					{
						action = (string s) => base.Channel.DeleteCertificate(s, this.ServiceName, this.ThumbprintAlgorithm, this.Thumbprint);
					}
					((CmdletBase<IServiceManagement>)removeAzureCertificateCommand).RetryCall(action);
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
	}
}