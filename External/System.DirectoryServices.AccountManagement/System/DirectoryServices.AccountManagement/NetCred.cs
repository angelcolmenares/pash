using System;
using System.Security;
using System.Text;

namespace System.DirectoryServices.AccountManagement
{
	internal class NetCred
	{
		private string username;

		private string password;

		private string domainname;

		private string parsedUserName;

		public string Domain
		{
			[SecurityCritical]
			get
			{
				if (this.parsedUserName == null)
				{
					this.SplitUsername(this.username, ref this.parsedUserName, ref this.domainname);
				}
				return this.domainname;
			}
		}

		public string ParsedUserName
		{
			[SecurityCritical]
			get
			{
				if (this.parsedUserName == null)
				{
					this.SplitUsername(this.username, ref this.parsedUserName, ref this.domainname);
				}
				return this.parsedUserName;
			}
		}

		public string Password
		{
			get
			{
				return this.password;
			}
		}

		public string UserName
		{
			get
			{
				return this.username;
			}
		}

		public NetCred(string username, string password)
		{
			this.username = username;
			this.password = password;
		}

		[SecurityCritical]
		private void SplitUsername(string username, ref string parsedUserName, ref string parsedDomainName)
		{
			if (username != null)
			{
				StringBuilder stringBuilder = new StringBuilder(0x202);
				StringBuilder stringBuilder1 = new StringBuilder(0x152);
				int num = UnsafeNativeMethods.CredUIParseUserName(username, stringBuilder, stringBuilder.Capacity, stringBuilder1, stringBuilder1.Capacity);
				if (num == 0)
				{
					parsedDomainName = stringBuilder1.ToString();
					parsedUserName = stringBuilder.ToString();
					return;
				}
				else
				{
					parsedDomainName = null;
					parsedUserName = username;
					return;
				}
			}
			else
			{
				parsedDomainName = null;
				parsedUserName = null;
				return;
			}
		}
	}
}