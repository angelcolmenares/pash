using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADComputer : ADAccount
	{
		public string DNSHostName
		{
			get
			{
				return (string)base.GetValue("DNSHostName");
			}
			set
			{
				base.SetValue("DNSHostName", value);
			}
		}

		static ADComputer()
		{
			ADEntity.RegisterMappingTable(typeof(ADComputer), ADComputerFactory<ADComputer>.AttributeTable);
		}

		public ADComputer()
		{
		}

		public ADComputer(ADObject identity) : base(identity)
		{
		}

		public ADComputer(string identity) : base(identity)
		{
		}

		public ADComputer(Guid guid) : base(guid)
		{
		}

		public ADComputer(SecurityIdentifier sid) : base(sid)
		{
		}
	}
}