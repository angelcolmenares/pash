using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADDefaultDomainPasswordPolicy : ADEntity
	{
		public bool? ComplexityEnabled
		{
			get
			{
				return (bool?)base.GetValue("ComplexityEnabled");
			}
			set
			{
				base.SetValue("ComplexityEnabled", value);
			}
		}

		internal string DistinguishedName
		{
			get
			{
				return base.GetValue("DistinguishedName") as string;
			}
			set
			{
				base.SetValue("DistinguishedName", value);
			}
		}

		internal override string IdentifyingString
		{
			get
			{
				return this.DistinguishedName;
			}
		}

		public TimeSpan? LockoutDuration
		{
			get
			{
				return (TimeSpan?)base.GetValue("LockoutDuration");
			}
			set
			{
				base.SetValue("LockoutDuration", value);
			}
		}

		public TimeSpan? LockoutObservationWindow
		{
			get
			{
				return (TimeSpan?)base.GetValue("LockoutObservationWindow");
			}
			set
			{
				base.SetValue("LockoutObservationWindow", value);
			}
		}

		public int? LockoutThreshold
		{
			get
			{
				return (int?)base.GetValue("LockoutThreshold");
			}
			set
			{
				base.SetValue("LockoutThreshold", value);
			}
		}

		public TimeSpan? MaxPasswordAge
		{
			get
			{
				return (TimeSpan?)base.GetValue("MaxPasswordAge");
			}
			set
			{
				base.SetValue("MaxPasswordAge", value);
			}
		}

		public TimeSpan? MinPasswordAge
		{
			get
			{
				return (TimeSpan?)base.GetValue("MinPasswordAge");
			}
			set
			{
				base.SetValue("MinPasswordAge", value);
			}
		}

		public int? MinPasswordLength
		{
			get
			{
				return (int?)base.GetValue("MinPasswordLength");
			}
			set
			{
				base.SetValue("MinPasswordLength", value);
			}
		}

		public int? PasswordHistoryCount
		{
			get
			{
				return (int?)base.GetValue("PasswordHistoryCount");
			}
			set
			{
				base.SetValue("PasswordHistoryCount", value);
			}
		}

		public bool? ReversibleEncryptionEnabled
		{
			get
			{
				return (bool?)base.GetValue("ReversibleEncryptionEnabled");
			}
			set
			{
				base.SetValue("ReversibleEncryptionEnabled", value);
			}
		}

		static ADDefaultDomainPasswordPolicy()
		{
			ADEntity.RegisterMappingTable(typeof(ADDefaultDomainPasswordPolicy), ADDefaultDomainPasswordPolicyFactory<ADDefaultDomainPasswordPolicy>.AttributeTable);
		}

		public ADDefaultDomainPasswordPolicy()
		{
		}

		public ADDefaultDomainPasswordPolicy(string identity)
		{
			base.Identity = identity;
		}

		public ADDefaultDomainPasswordPolicy(Guid guid)
		{
			base.Identity = guid;
		}

		public ADDefaultDomainPasswordPolicy(ADEntity adentity)
		{
			if (adentity != null)
			{
				base.Identity = adentity;
				if (adentity.IsSearchResult)
				{
					base.SessionInfo = adentity.SessionInfo;
				}
				return;
			}
			else
			{
				throw new ArgumentException("adentity");
			}
		}

		public override string ToString()
		{
			if (!base.IsSearchResult)
			{
				if (this.Identity == null)
				{
					return base.ToString();
				}
				else
				{
					return this.Identity.ToString();
				}
			}
			else
			{
				return this.DistinguishedName;
			}
		}
	}
}