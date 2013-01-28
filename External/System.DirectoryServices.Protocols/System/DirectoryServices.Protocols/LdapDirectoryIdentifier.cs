using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class LdapDirectoryIdentifier : DirectoryIdentifier
	{
		private string[] servers;

		private bool fullyQualifiedDnsHostName;

		private bool connectionless;

		private int portNumber;

		public bool Connectionless
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.connectionless;
			}
		}

		public bool FullyQualifiedDnsHostName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.fullyQualifiedDnsHostName;
			}
		}

		public int PortNumber
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.portNumber;
			}
		}

		public string[] Servers
		{
			get
			{
				if (this.servers != null)
				{
					string[] strArrays = new string[(int)this.servers.Length];
					for (int i = 0; i < (int)this.servers.Length; i++)
					{
						if (this.servers[i] == null)
						{
							strArrays[i] = null;
						}
						else
						{
							strArrays[i] = string.Copy(this.servers[i]);
						}
					}
					return strArrays;
				}
				else
				{
					return new string[0];
				}
			}
		}

		public LdapDirectoryIdentifier(string server)
			: this(new string[] { server }, false, false)
		{

		}

		public LdapDirectoryIdentifier(string server, int portNumber)
			: this(new string[] { server }, portNumber, false, false)
		{
	
		}

		public LdapDirectoryIdentifier(string server, bool fullyQualifiedDnsHostName, bool connectionless)
			: this(new string[] { server }, fullyQualifiedDnsHostName, connectionless)
		{

		}

		public LdapDirectoryIdentifier(string server, int portNumber, bool fullyQualifiedDnsHostName, bool connectionless)
			: this(new string[] { server }, portNumber, fullyQualifiedDnsHostName, connectionless)
		{

		}

		public LdapDirectoryIdentifier(string[] servers, bool fullyQualifiedDnsHostName, bool connectionless)
		{
			this.portNumber = 0x185;
			if (servers != null)
			{
				this.servers = new string[(int)servers.Length];
				for (int i = 0; i < (int)servers.Length; i++)
				{
					if (servers[i] != null)
					{
						string str = servers[i].Trim();
						char[] chrArray = new char[1];
						chrArray[0] = ' ';
						string[] strArrays = str.Split(chrArray);
						if ((int)strArrays.Length <= 1)
						{
							this.servers[i] = str;
						}
						else
						{
							throw new ArgumentException(Res.GetString("WhiteSpaceServerName"));
						}
					}
				}
			}
			this.fullyQualifiedDnsHostName = fullyQualifiedDnsHostName;
			this.connectionless = connectionless;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public LdapDirectoryIdentifier(string[] servers, int portNumber, bool fullyQualifiedDnsHostName, bool connectionless) 
			: this(servers, fullyQualifiedDnsHostName, connectionless)
		{
			this.portNumber = portNumber;
		}
	}
}