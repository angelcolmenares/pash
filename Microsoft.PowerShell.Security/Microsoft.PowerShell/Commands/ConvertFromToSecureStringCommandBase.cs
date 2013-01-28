using System;
using System.Management.Automation;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
	public abstract class ConvertFromToSecureStringCommandBase : SecureStringCommandBase
	{
		private SecureString secureKey;

		private byte[] key;

		[Parameter(ParameterSetName="Open")]
		public byte[] Key
		{
			get
			{
				return this.key;
			}
			set
			{
				this.key = value;
			}
		}

		[Parameter(Position=1, ParameterSetName="Secure")]
		public SecureString SecureKey
		{
			get
			{
				return this.secureKey;
			}
			set
			{
				this.secureKey = value;
			}
		}

		protected ConvertFromToSecureStringCommandBase(string name) : base(name)
		{
		}
	}
}