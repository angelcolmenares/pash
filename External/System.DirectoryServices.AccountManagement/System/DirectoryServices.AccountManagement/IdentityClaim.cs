using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class IdentityClaim
	{
		private string urnValue;

		private string urnScheme;

		public string UrnScheme
		{
			get
			{
				return this.urnScheme;
			}
			set
			{
				this.urnScheme = value;
			}
		}

		public string UrnValue
		{
			get
			{
				return this.urnValue;
			}
			set
			{
				this.urnValue = value;
			}
		}

		public IdentityClaim()
		{
		}
	}
}