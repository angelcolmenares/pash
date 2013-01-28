using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADServiceAccount : ADAccount
	{
		public string[] ServicePrincipalNames
		{
			get
			{
				if (base.GetValue("ServicePrincipalNames") as string == null)
				{
					return (string[])base.GetValue("ServicePrincipalNames");
				}
				else
				{
					string[] value = new string[1];
					value[0] = (string)base.GetValue("ServicePrincipalNames");
					return value;
				}
			}
			set
			{
				base.SetValue("ServicePrincipalNames", value);
			}
		}

		static ADServiceAccount()
		{
			ADEntity.RegisterMappingTable(typeof(ADServiceAccount), ADServiceAccountFactory<ADServiceAccount>.AttributeTable);
		}

		public ADServiceAccount()
		{
		}

		public ADServiceAccount(ADObject identity) : base(identity)
		{
		}

		public ADServiceAccount(string identity) : base(identity)
		{
		}

		public ADServiceAccount(Guid guid) : base(guid)
		{
		}

		public ADServiceAccount(SecurityIdentifier sid) : base(sid)
		{
		}
	}
}