using Microsoft.PowerShell;
using Microsoft.PowerShell.Security;
using System;
using System.Management.Automation;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("ConvertTo", "SecureString", DefaultParameterSetName="Secure", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113291")]
	[OutputType(new Type[] { typeof(SecureString) })]
	public sealed class ConvertToSecureStringCommand : ConvertFromToSecureStringCommandBase
	{
		private string s;

		private bool asPlainText;

		private bool force;

		[Parameter(Position=1, ParameterSetName="PlainText")]
		public SwitchParameter AsPlainText
		{
			get
			{
				return this.asPlainText;
			}
			set
			{
				this.asPlainText = value;
			}
		}

		[Parameter(Position=2, ParameterSetName="PlainText")]
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

		[Parameter(Position=0, ValueFromPipeline=true, Mandatory=true)]
		public string String
		{
			get
			{
				return this.s;
			}
			set
			{
				this.s = value;
			}
		}

		public ConvertToSecureStringCommand() : base("ConvertTo-SecureString")
		{
		}

		protected override void ProcessRecord()
		{
			SecureString secureString = null;
			Utils.CheckArgForNullOrEmpty(this.s, "String");
			try
			{
				string str = this.String;
				byte[] numArray = null;
				if (this.String.IndexOf(SecureStringHelper.SecureStringExportHeader, StringComparison.OrdinalIgnoreCase) == 0)
				{
					try
					{
						string str1 = this.String.Substring(SecureStringHelper.SecureStringExportHeader.Length, this.String.Length - SecureStringHelper.SecureStringExportHeader.Length);
						byte[] numArray1 = Convert.FromBase64String(str1);
						string str2 = Encoding.Unicode.GetString(numArray1);
						char[] chrArray = new char[1];
						chrArray[0] = '|';
						string[] strArrays = str2.Split(chrArray);
						if ((int)strArrays.Length == 3)
						{
							str = strArrays[2];
							numArray = Convert.FromBase64String(strArrays[1]);
						}
					}
					catch (FormatException formatException)
					{
						str = this.String;
						numArray = null;
					}
				}
				if (base.SecureKey == null)
				{
					if (base.Key == null)
					{
						if (this.AsPlainText)
						{
							if (this.Force)
							{
								secureString = new SecureString();
								string str3 = this.String;
								for (int i = 0; i < str3.Length; i++)
								{
									char chr = str3[i];
									secureString.AppendChar(chr);
								}
							}
							else
							{
								string forceRequired = SecureStringCommands.ForceRequired;
								Exception argumentException = new ArgumentException(forceRequired);
								base.WriteError(new ErrorRecord(argumentException, "ImportSecureString_ForceRequired", ErrorCategory.InvalidArgument, null));
							}
						}
						else
						{
							secureString = SecureStringHelper.Unprotect(this.String);
						}
					}
					else
					{
						secureString = SecureStringHelper.Decrypt(str, base.Key, numArray);
					}
				}
				else
				{
					secureString = SecureStringHelper.Decrypt(str, base.SecureKey, numArray);
				}
			}
			catch (ArgumentException argumentException2)
			{
				ArgumentException argumentException1 = argumentException2;
				ErrorRecord errorRecord = SecurityUtils.CreateInvalidArgumentErrorRecord(argumentException1, "ImportSecureString_InvalidArgument");
				base.WriteError(errorRecord);
			}
			catch (CryptographicException cryptographicException1)
			{
				CryptographicException cryptographicException = cryptographicException1;
				ErrorRecord errorRecord1 = SecurityUtils.CreateInvalidArgumentErrorRecord(cryptographicException, "ImportSecureString_InvalidArgument_CryptographicError");
				base.WriteError(errorRecord1);
			}
			if (secureString != null)
			{
				base.WriteObject(secureString);
			}
		}
	}
}