using Microsoft.PowerShell;
using Microsoft.PowerShell.Security;
using System;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Set", "AuthenticodeSignature", SupportsShouldProcess=true, DefaultParameterSetName="ByPath", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113391")]
	[OutputType(new Type[] { typeof(Signature) })]
	public sealed class SetAuthenticodeSignatureCommand : SignatureCommandsBase
	{
		private X509Certificate2 certificate;

		private string includeChain;

		private string timestampServer;

		private string hashAlgorithm;

		private bool force;

		private readonly static SetAuthenticodeSignatureCommand.SigningOptionInfo[] sigOptionInfo;

		[Parameter(Position=1, Mandatory=true)]
		public X509Certificate2 Certificate
		{
			get
			{
				return this.certificate;
			}
			set
			{
				this.certificate = value;
			}
		}

		[Parameter]
		public SwitchParameter Force
		{
			get
			{
				return this.force;
			}
			set
			{
				this.force = value;
			}
		}

		[Parameter(Mandatory=false)]
		public string HashAlgorithm
		{
			get
			{
				return this.hashAlgorithm;
			}
			set
			{
				this.hashAlgorithm = value;
			}
		}

		[Parameter(Mandatory=false)]
		[ValidateSet(new string[] { "signer", "notroot", "all" })]
		public string IncludeChain
		{
			get
			{
				return this.includeChain;
			}
			set
			{
				this.includeChain = value;
			}
		}

		[Parameter(Mandatory=false)]
		public string TimestampServer
		{
			get
			{
				return this.timestampServer;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				this.timestampServer = value;
			}
		}

		static SetAuthenticodeSignatureCommand()
		{
			SetAuthenticodeSignatureCommand.SigningOptionInfo[] signingOptionInfo = new SetAuthenticodeSignatureCommand.SigningOptionInfo[3];
			signingOptionInfo[0] = new SetAuthenticodeSignatureCommand.SigningOptionInfo(SigningOption.AddOnlyCertificate, "signer");
			signingOptionInfo[1] = new SetAuthenticodeSignatureCommand.SigningOptionInfo(SigningOption.AddFullCertificateChainExceptRoot, "notroot");
			signingOptionInfo[2] = new SetAuthenticodeSignatureCommand.SigningOptionInfo(SigningOption.AddFullCertificateChain, "all");
			SetAuthenticodeSignatureCommand.sigOptionInfo = signingOptionInfo;
		}

		public SetAuthenticodeSignatureCommand() : base("set-AuthenticodeSignature")
		{
			this.includeChain = "notroot";
			this.timestampServer = "";
		}

		private static SigningOption GetSigningOption(string optionName)
		{
			SetAuthenticodeSignatureCommand.SigningOptionInfo[] signingOptionInfoArray = SetAuthenticodeSignatureCommand.sigOptionInfo;
			int num = 0;
			while (num < (int)signingOptionInfoArray.Length)
			{
				SetAuthenticodeSignatureCommand.SigningOptionInfo signingOptionInfo = signingOptionInfoArray[num];
				if (!string.Equals(optionName, signingOptionInfo.optionName, StringComparison.OrdinalIgnoreCase))
				{
					num++;
				}
				else
				{
					SigningOption signingOption = signingOptionInfo.option;
					return signingOption;
				}
			}
			return SigningOption.AddFullCertificateChainExceptRoot;
		}

		protected override Signature PerformAction(string filePath)
		{
			Signature signature;
			SigningOption signingOption = SetAuthenticodeSignatureCommand.GetSigningOption(this.IncludeChain);
			if (this.Certificate != null)
			{
				if (SecuritySupport.CertIsGoodForSigning(this.Certificate))
				{
					if (base.ShouldProcess(filePath))
					{
						FileInfo fileInfo = null;
						try
						{
							if (this.Force)
							{
								try
								{
									FileInfo fileInfo1 = new FileInfo(filePath);
									if (fileInfo1 != null && (fileInfo1.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
									{
										fileInfo = fileInfo1;
										FileInfo attributes = fileInfo1;
										attributes.Attributes = attributes.Attributes & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Device | FileAttributes.Normal | 
										                                                 FileAttributes.Temporary | FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed | FileAttributes.Offline | 
										                                                 FileAttributes.NotContentIndexed | FileAttributes.Encrypted
#if !MONO
										                                                 | FileAttributes.IntegrityStream | FileAttributes.NoScrubData
#endif
										                                                 );
									}
								}
								catch (ArgumentException argumentException1)
								{
									ArgumentException argumentException = argumentException1;
									ErrorRecord errorRecord = new ErrorRecord(argumentException, "ForceArgumentException", ErrorCategory.WriteError, filePath);
									base.WriteError(errorRecord);
									signature = null;
									return signature;
								}
								catch (IOException oException1)
								{
									IOException oException = oException1;
									ErrorRecord errorRecord1 = new ErrorRecord(oException, "ForceIOException", ErrorCategory.WriteError, filePath);
									base.WriteError(errorRecord1);
									signature = null;
									return signature;
								}
								catch (UnauthorizedAccessException unauthorizedAccessException1)
								{
									UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
									ErrorRecord errorRecord2 = new ErrorRecord(unauthorizedAccessException, "ForceUnauthorizedAccessException", ErrorCategory.PermissionDenied, filePath);
									base.WriteError(errorRecord2);
									signature = null;
									return signature;
								}
								catch (NotSupportedException notSupportedException1)
								{
									NotSupportedException notSupportedException = notSupportedException1;
									ErrorRecord errorRecord3 = new ErrorRecord(notSupportedException, "ForceNotSupportedException", ErrorCategory.WriteError, filePath);
									base.WriteError(errorRecord3);
									signature = null;
									return signature;
								}
								catch (SecurityException securityException1)
								{
									SecurityException securityException = securityException1;
									ErrorRecord errorRecord4 = new ErrorRecord(securityException, "ForceSecurityException", ErrorCategory.PermissionDenied, filePath);
									base.WriteError(errorRecord4);
									signature = null;
									return signature;
								}
							}
							if (SecurityUtils.GetFileSize(filePath) >= (long)4)
							{
								signature = SignatureHelper.SignFile(signingOption, filePath, this.Certificate, this.TimestampServer, this.hashAlgorithm);
							}
							else
							{
								object[] objArray = new object[1];
								objArray[0] = filePath;
								string str = string.Format(CultureInfo.CurrentCulture, UtilsStrings.FileSmallerThan4Bytes, objArray);
								PSArgumentException pSArgumentException = new PSArgumentException(str, "filePath");
								ErrorRecord errorRecord5 = SecurityUtils.CreateInvalidArgumentErrorRecord(pSArgumentException, "SignatureCommandsBaseFileSmallerThan4Bytes");
								base.WriteError(errorRecord5);
								signature = null;
							}
						}
						finally
						{
							if (fileInfo != null)
							{
								FileInfo attributes1 = fileInfo;
								attributes1.Attributes = attributes1.Attributes | FileAttributes.ReadOnly;
							}
						}
						return signature;
					}
					else
					{
						return null;
					}
				}
				else
				{
					Exception exception = PSTraceSource.NewArgumentException("certificate", "SignatureCommands", "CertNotGoodForSigning", new object[0]);
					throw exception;
				}
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("certificate");
			}
		}

		private struct SigningOptionInfo
		{
			internal SigningOption option;

			internal string optionName;

			internal SigningOptionInfo(SigningOption o, string n)
			{
				this.option = o;
				this.optionName = n;
			}
		}
	}
}