using Microsoft.PowerShell;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Security;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("ConvertFrom", "SecureString", DefaultParameterSetName="Secure", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113287")]
	[OutputType(new Type[] { typeof(string) })]
	public sealed class ConvertFromSecureStringCommand : ConvertFromToSecureStringCommandBase
	{
		[Parameter(Position=0, ValueFromPipeline=true, Mandatory=true)]
		public SecureString SecureString
		{
			get
			{
				return base.SecureStringData;
			}
			set
			{
				base.SecureStringData = value;
			}
		}

		public ConvertFromSecureStringCommand() : base("ConvertFrom-SecureString")
		{
		}

		protected override void ProcessRecord()
		{
			string str = null;
			EncryptionResult encryptionResult = null;
			Utils.CheckSecureStringArg(base.SecureStringData, "SecureString");
			if (base.SecureKey == null)
			{
				if (base.Key == null)
				{
					str = SecureStringHelper.Protect(this.SecureString);
				}
				else
				{
					encryptionResult = SecureStringHelper.Encrypt(this.SecureString, base.Key);
				}
			}
			else
			{
				encryptionResult = SecureStringHelper.Encrypt(this.SecureString, base.SecureKey);
			}
			if (encryptionResult == null)
			{
				if (str != null)
				{
					base.WriteObject(str);
				}
				return;
			}
			else
			{
				object[] v = new object[3];
				v[0] = 2;
				v[1] = encryptionResult.IV;
				v[2] = encryptionResult.EncryptedData;
				string str1 = string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}", v);
				byte[] bytes = Encoding.Unicode.GetBytes(str1);
				string base64String = Convert.ToBase64String(bytes);
				base.WriteObject(string.Concat(SecureStringHelper.SecureStringExportHeader, base64String));
				return;
			}
		}
	}
}