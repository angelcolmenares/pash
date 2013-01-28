using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Certificates
{
	[Cmdlet("Add", "AzureCertificate")]
	public class AddAzureCertificateCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=1, Mandatory=true, HelpMessage="Certificate to deploy.")]
		[ValidateNotNullOrEmpty]
		public object CertToDeploy
		{
			get;
			set;
		}

		[Parameter(HelpMessage="Certificate password.")]
		public string Password
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Hosted Service Name.")]
		[ValidateNotNullOrEmpty]
		public string ServiceName
		{
			get;
			set;
		}

		public AddAzureCertificateCommand()
		{
		}

		public AddAzureCertificateCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		internal void AddCertificateProcess()
		{
			Action<string> action = null;
			this.ValidateParameters();
			byte[] certificateData = this.GetCertificateData();
			CertificateFile certificateFile = new CertificateFile();
			certificateFile.Data = Convert.ToBase64String(certificateData);
			certificateFile.Password = this.Password;
			certificateFile.CertificateFormat = "pfx";
			CertificateFile certificateFile1 = certificateFile;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					AddAzureCertificateCommand addAzureCertificateCommand = this;
					if (action == null)
					{
						action = (string s) => base.Channel.AddCertificates(s, this.ServiceName, certificateFile1);
					}
					((CmdletBase<IServiceManagement>)addAzureCertificateCommand).RetryCall(action);
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

		private byte[] GetCertificateData()
		{
			byte[] numArray;
			byte[] numArray1;
			byte[] numArray2;
			byte[] rawData;
			X509Certificate2 x509Certificate2 = new X509Certificate2();
			if ((this.CertToDeploy as PSObject == null || ((PSObject)this.CertToDeploy).ImmediateBaseObject as X509Certificate == null) && this.CertToDeploy as X509Certificate == null)
			{
				x509Certificate2.Import(CmdletExtensions.ResolvePath(this, this.CertToDeploy.ToString()), this.Password, X509KeyStorageFlags.Exportable);
				if (x509Certificate2.HasPrivateKey)
				{
					numArray1 = x509Certificate2.Export(X509ContentType.Pfx, this.Password);
				}
				else
				{
					numArray1 = x509Certificate2.Export(X509ContentType.Pfx);
				}
				numArray = numArray1;
			}
			else
			{
				x509Certificate2 = ((PSObject)this.CertToDeploy).ImmediateBaseObject as X509Certificate2;
				try
				{
					if (x509Certificate2.HasPrivateKey)
					{
						numArray2 = x509Certificate2.Export(X509ContentType.Pfx);
					}
					else
					{
						numArray2 = x509Certificate2.Export(X509ContentType.Pfx);
					}
					numArray = numArray2;
				}
				catch (CryptographicException cryptographicException)
				{
					if (x509Certificate2.HasPrivateKey)
					{
						rawData = x509Certificate2.RawData;
					}
					else
					{
						rawData = x509Certificate2.Export(X509ContentType.Pfx);
					}
					numArray = rawData;
				}
			}
			return numArray;
		}

		[PermissionSet(SecurityAction.Demand, Name="FullTrust")]
		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.AddCertificateProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		private void ValidateParameters()
		{
			string empty;
			AddAzureCertificateCommand addAzureCertificateCommand = this;
			if (this.Password == null)
			{
				empty = string.Empty;
			}
			else
			{
				empty = this.Password;
			}
			addAzureCertificateCommand.Password = empty;
		}
	}
}