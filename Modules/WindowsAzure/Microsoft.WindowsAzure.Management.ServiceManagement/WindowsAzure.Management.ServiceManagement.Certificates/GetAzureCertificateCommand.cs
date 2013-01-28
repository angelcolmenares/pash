using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Certificates
{
	[Cmdlet("Get", "AzureCertificate")]
	public class GetAzureCertificateCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Hosted Service Name.")]
		[ValidateNotNullOrEmpty]
		public string ServiceName
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ValueFromPipelineByPropertyName=true, HelpMessage="Certificate thumbprint.")]
		[ValidateNotNullOrEmpty]
		public string Thumbprint
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ValueFromPipelineByPropertyName=true, HelpMessage="Certificate thumbprint algorithm.")]
		[ValidateNotNullOrEmpty]
		public string ThumbprintAlgorithm
		{
			get;
			set;
		}

		public GetAzureCertificateCommand()
		{
		}

		public GetAzureCertificateCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public IEnumerable<Certificate> GetCertificateProcess(out Operation operation)
		{
			IEnumerable<Certificate> certificates;
			Func<string, Certificate> func = null;
			Func<string, CertificateList> func1 = null;
			IEnumerable<Certificate> certificates1 = null;
			operation = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					if (this.Thumbprint == null)
					{
						GetAzureCertificateCommand getAzureCertificateCommand = this;
						if (func1 == null)
						{
							func1 = (string s) => base.Channel.ListCertificates(s, this.ServiceName);
						}
						certificates1 = ((CmdletBase<IServiceManagement>)getAzureCertificateCommand).RetryCall<CertificateList>(func1);
					}
					else
					{
						if (this.ThumbprintAlgorithm != null)
						{
							Certificate[] certificateArray = new Certificate[1];
							Certificate[] certificateArray1 = certificateArray;
							int num = 0;
							GetAzureCertificateCommand getAzureCertificateCommand1 = this;
							if (func == null)
							{
								func = (string s) => base.Channel.GetCertificate(s, this.ServiceName, this.ThumbprintAlgorithm, this.Thumbprint);
							}
							certificateArray1[num] = ((CmdletBase<IServiceManagement>)getAzureCertificateCommand1).RetryCall<Certificate>(func);
							certificates1 = certificateArray;
						}
						else
						{
							throw new ArgumentNullException("ThumbprintAlgorithm", "You must specify the thumbprint algorithm.");
						}
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
						certificates = null;
						return certificates;
					}
				}
				return certificates1;
			}
			return certificates;
		}

		protected override void ProcessRecord()
		{
			try
			{
				Func<Certificate, CertificateContext> func = null;
				base.ProcessRecord();
				Operation operation = null;
				IEnumerable<Certificate> certificateProcess = this.GetCertificateProcess(out operation);
				if (certificateProcess != null)
				{
					IEnumerable<Certificate> certificates = certificateProcess;
					if (func == null)
					{
						func = (Certificate certificate) => {
							CertificateContext certificateContext = new CertificateContext();
							certificateContext.ServiceName = this.ServiceName;
							certificateContext.Data = certificate.Data;
							certificateContext.Thumbprint = certificate.Thumbprint;
							certificateContext.ThumbprintAlgorithm = certificate.ThumbprintAlgorithm;
							certificateContext.Url = certificate.CertificateUrl;
							certificateContext.set_OperationId(operation.OperationTrackingId);
							certificateContext.set_OperationDescription(this.CommandRuntime.ToString());
							certificateContext.set_OperationStatus(operation.Status);
							return certificateContext;
						}
						;
					}
					IEnumerable<CertificateContext> certificateContexts = certificates.Select<Certificate, CertificateContext>(func);
					base.WriteObject(certificateContexts, true);
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