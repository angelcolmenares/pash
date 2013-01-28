using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADAccount : ADPrincipal
	{
		internal bool DoesNotRequirePreAuth
		{
			get
			{
				return (bool)base.GetValue("DoesNotRequirePreAuth");
			}
			set
			{
				base.SetValue("DoesNotRequirePreAuth", value);
			}
		}

		public bool Enabled
		{
			get
			{
				return (bool)base.GetValue("Enabled");
			}
			set
			{
				base.SetValue("Enabled", value);
			}
		}

		internal bool HomedirRequired
		{
			get
			{
				return (bool)base.GetValue("HomedirRequired");
			}
			set
			{
				base.SetValue("HomedirRequired", value);
			}
		}

		internal bool MNSLogonAccount
		{
			get
			{
				return (bool)base.GetValue("MNSLogonAccount");
			}
			set
			{
				base.SetValue("MNSLogonAccount", value);
			}
		}

		internal bool PasswordExpired
		{
			get
			{
				return (bool)base.GetValue("PasswordExpired");
			}
			set
			{
				base.SetValue("PasswordExpired", value);
			}
		}

		internal bool TrustedToAuthForDelegation
		{
			get
			{
				return (bool)base.GetValue("TrustedToAuthForDelegation");
			}
			set
			{
				base.SetValue("TrustedToAuthForDelegation", value);
			}
		}

		internal bool UseDESKeyOnly
		{
			get
			{
				return (bool)base.GetValue("UseDESKeyOnly");
			}
			set
			{
				base.SetValue("UseDESKeyOnly", value);
			}
		}

		public string UserPrincipalName
		{
			get
			{
				return (string)base.GetValue("UserPrincipalName");
			}
			set
			{
				base.SetValue("UserPrincipalName", value);
			}
		}

		static ADAccount()
		{
			ADEntity.RegisterMappingTable(typeof(ADAccount), ADAccountFactory<ADAccount>.AttributeTable);
		}

		public ADAccount()
		{
		}

		public ADAccount(string identity) : base(identity)
		{
		}

		public ADAccount(Guid guid) : base(guid)
		{
		}

		public ADAccount(SecurityIdentifier sid) : base(sid)
		{
		}

		public ADAccount(ADObject identity) : base(identity)
		{
		}
	}
}