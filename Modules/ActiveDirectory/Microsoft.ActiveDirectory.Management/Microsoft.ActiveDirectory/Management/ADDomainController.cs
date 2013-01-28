using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADDomainController : ADDirectoryServer
	{
		public string Domain
		{
			get
			{
				return (string)base.GetValue("Domain");
			}
			set
			{
				base.SetValue("Domain", value);
			}
		}

		public string Forest
		{
			get
			{
				return (string)base.GetValue("Forest");
			}
			set
			{
				base.SetValue("Forest", value);
			}
		}

		static ADDomainController()
		{
			ADEntity.RegisterMappingTable(typeof(ADDomainController), ADDomainControllerFactory<ADDomainController>.AttributeTable);
		}

		public ADDomainController()
		{
		}

		public ADDomainController(string identity) : base(identity)
		{
		}

		public ADDomainController(Guid guid) : base(guid)
		{
		}

		public ADDomainController(SecurityIdentifier sid)
		{
			base.Identity = sid;
		}

		public ADDomainController(ADObject identity) : base(identity)
		{
		}
	}
}