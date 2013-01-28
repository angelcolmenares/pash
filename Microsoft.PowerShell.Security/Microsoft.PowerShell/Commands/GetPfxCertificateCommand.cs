using Microsoft.PowerShell;
using Microsoft.PowerShell.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "PfxCertificate", DefaultParameterSetName="ByPath", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113323")]
	[OutputType(new Type[] { typeof(X509Certificate2) })]
	public sealed class GetPfxCertificateCommand : PSCmdlet
	{
		private string[] path;

		private bool isLiteralPath;

		private ArrayList filesNotFound;

		[Parameter(Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, Mandatory=true, ParameterSetName="ByPath")]
		public string[] FilePath
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
			}
		}

		[Alias(new string[] { "PSPath" })]
		[Parameter(ValueFromPipelineByPropertyName=true, Mandatory=true, ParameterSetName="ByLiteralPath")]
		public string[] LiteralPath
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
				this.isLiteralPath = true;
			}
		}

		public GetPfxCertificateCommand()
		{
			this.filesNotFound = new ArrayList();
		}

		private static X509Certificate2 GetCertFromPfxFile(string path)
		{
			X509Certificate2 x509Certificate2 = new X509Certificate2();
			x509Certificate2.Import(path);
			return x509Certificate2;
		}

		private static X509Certificate2 GetCertFromPfxFile(string path, SecureString password)
		{
			X509Certificate2 x509Certificate2 = new X509Certificate2();
			string stringFromSecureString = SecurityUtils.GetStringFromSecureString(password);
			x509Certificate2.Import(path, stringFromSecureString, X509KeyStorageFlags.DefaultKeySet);
			return x509Certificate2;
		}

		protected override void ProcessRecord()
		{
			X509Certificate2 certFromPfxFile = null;
			string[] filePath = this.FilePath;
			for (int i = 0; i < (int)filePath.Length; i++)
			{
				string str = filePath[i];
				List<string> strs = new List<string>();
				if (!this.isLiteralPath)
				{
					try
					{
						foreach (PathInfo resolvedPSPathFromPSPath in base.SessionState.Path.GetResolvedPSPathFromPSPath(str))
						{
							strs.Add(resolvedPSPathFromPSPath.ProviderPath);
						}
					}
					catch (ItemNotFoundException itemNotFoundException)
					{
						this.filesNotFound.Add(str);
					}
				}
				else
				{
					strs.Add(base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(str));
				}
				foreach (string str1 in strs)
				{
					string filePathOfExistingFile = SecurityUtils.GetFilePathOfExistingFile(this, str1);
					if (filePathOfExistingFile != null)
					{
						try
						{
							certFromPfxFile = GetPfxCertificateCommand.GetCertFromPfxFile(filePathOfExistingFile);
						}
						catch (CryptographicException cryptographicException2)
						{
							string getPfxCertPasswordPrompt = CertificateCommands.GetPfxCertPasswordPrompt;
							SecureString secureString = SecurityUtils.PromptForSecureString(base.Host.UI, getPfxCertPasswordPrompt);
							try
							{
								certFromPfxFile = GetPfxCertificateCommand.GetCertFromPfxFile(filePathOfExistingFile, secureString);
							}
							catch (CryptographicException cryptographicException1)
							{
								CryptographicException cryptographicException = cryptographicException1;
								ErrorRecord errorRecord = new ErrorRecord(cryptographicException, "GetPfxCertificateUnknownCryptoError", ErrorCategory.NotSpecified, null);
								base.WriteError(errorRecord);
								continue;
							}
						}
						base.WriteObject(certFromPfxFile);
					}
					else
					{
						this.filesNotFound.Add(str);
					}
				}
			}
			if (this.filesNotFound.Count > 0)
			{
				if (this.filesNotFound.Count != (int)this.FilePath.Length)
				{
					foreach (string str2 in this.filesNotFound)
					{
						object[] objArray = new object[1];
						objArray[0] = str2;
						ErrorRecord errorRecord1 = SecurityUtils.CreateFileNotFoundErrorRecord(CertificateCommands.FileNotFound, "GetPfxCertCommandFileNotFound", objArray);
						base.WriteError(errorRecord1);
					}
				}
				else
				{
					ErrorRecord errorRecord2 = SecurityUtils.CreateFileNotFoundErrorRecord(CertificateCommands.NoneOfTheFilesFound, "GetPfxCertCommandNoneOfTheFilesFound", new object[0]);
					base.ThrowTerminatingError(errorRecord2);
					return;
				}
			}
		}
	}
}