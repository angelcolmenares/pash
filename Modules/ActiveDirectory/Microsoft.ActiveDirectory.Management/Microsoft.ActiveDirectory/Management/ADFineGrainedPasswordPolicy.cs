using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADFineGrainedPasswordPolicy : ADObject
	{
		public ADPropertyValueCollection AppliesTo
		{
			get
			{
				return base["AppliesTo"];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

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

		public int? Precedence
		{
			get
			{
				return (int?)base.GetValue("Precedence");
			}
			set
			{
				base.SetValue("Precedence", value);
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

		static ADFineGrainedPasswordPolicy()
		{
			ADEntity.RegisterMappingTable(typeof(ADFineGrainedPasswordPolicy), ADFineGrainedPasswordPolicyFactory<ADFineGrainedPasswordPolicy>.AttributeTable);
		}

		public ADFineGrainedPasswordPolicy()
		{
		}

		public ADFineGrainedPasswordPolicy(string identity) : base(identity)
		{
		}

		public ADFineGrainedPasswordPolicy(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADFineGrainedPasswordPolicy(ADObject adobject)
		{
			if (adobject != null)
			{
				base.Identity = adobject;
				if (adobject.IsSearchResult)
				{
					base.SessionInfo = adobject.SessionInfo;
				}
				return;
			}
			else
			{
				throw new ArgumentException("adobject");
			}
		}
	}
}