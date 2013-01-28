using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADUser : ADAccount
	{
		public string GivenName
		{
			get
			{
				return (string)base.GetValue("GivenName");
			}
			set
			{
				base.SetValue("GivenName", value);
			}
		}

		public string Surname
		{
			get
			{
				return (string)base.GetValue("Surname");
			}
			set
			{
				base.SetValue("Surname", value);
			}
		}

		static ADUser()
		{
			ADEntity.RegisterMappingTable(typeof(ADUser), ADUserFactory<ADUser>.AttributeTable);
		}

		public ADUser()
		{
		}

		public ADUser(string identity) : base(identity)
		{
		}

		public ADUser(Guid guid) : base(guid)
		{
		}

		public ADUser(SecurityIdentifier sid) : base(sid)
		{
		}

		public ADUser(ADObject identity) : base(identity)
		{
		}
	}
}