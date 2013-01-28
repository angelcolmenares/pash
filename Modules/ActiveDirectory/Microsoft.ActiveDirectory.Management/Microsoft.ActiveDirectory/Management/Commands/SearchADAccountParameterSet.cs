using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SearchADAccountParameterSet : ADParameterSet
	{
		[Parameter(Mandatory=true, ParameterSetName="AccountDisabled")]
		public SwitchParameter AccountDisabled
		{
			get
			{
				return base.GetSwitchParameter("AccountDisabled");
			}
			set
			{
				base["AccountDisabled"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="AccountExpired")]
		public SwitchParameter AccountExpired
		{
			get
			{
				return base.GetSwitchParameter("AccountExpired");
			}
			set
			{
				base["AccountExpired"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="AccountExpiring")]
		public SwitchParameter AccountExpiring
		{
			get
			{
				return base.GetSwitchParameter("AccountExpiring");
			}
			set
			{
				base["AccountExpiring"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="AccountInactive")]
		public SwitchParameter AccountInactive
		{
			get
			{
				return base.GetSwitchParameter("AccountInactive");
			}
			set
			{
				base["AccountInactive"] = value;
			}
		}

		[Parameter]
		public ADAuthType AuthType
		{
			get
			{
				return (ADAuthType)base["AuthType"];
			}
			set
			{
				base["AuthType"] = value;
			}
		}

		[Parameter]
		public SwitchParameter ComputersOnly
		{
			get
			{
				return base.GetSwitchParameter("ComputersOnly");
			}
			set
			{
				base["ComputersOnly"] = value;
			}
		}

		[Credential]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public PSCredential Credential
		{
			get
			{
				return base["Credential"] as PSCredential;
			}
			set
			{
				base["Credential"] = value;
			}
		}

		[Parameter(ParameterSetName="AccountExpiring")]
		[Parameter(ParameterSetName="AccountInactive")]
		public DateTime DateTime
		{
			get
			{
				return (DateTime)base["DateTime"];
			}
			set
			{
				base["DateTime"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="LockedOut")]
		public SwitchParameter LockedOut
		{
			get
			{
				return base.GetSwitchParameter("LockedOut");
			}
			set
			{
				base["LockedOut"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="PasswordExpired")]
		public SwitchParameter PasswordExpired
		{
			get
			{
				return base.GetSwitchParameter("PasswordExpired");
			}
			set
			{
				base["PasswordExpired"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="PasswordNeverExpires")]
		public SwitchParameter PasswordNeverExpires
		{
			get
			{
				return base.GetSwitchParameter("PasswordNeverExpires");
			}
			set
			{
				base["PasswordNeverExpires"] = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		[ValidateRange(0, 0x7fffffff)]
		public int ResultPageSize
		{
			get
			{
				return (int)base["ResultPageSize"];
			}
			set
			{
				base["ResultPageSize"] = value;
			}
		}

		[Parameter]
		[ValidateNullableRange(1, 0x7fffffff)]
		public int? ResultSetSize
		{
			get
			{
				return (int?)(base["ResultSetSize"] as int?);
			}
			set
			{
				base["ResultSetSize"] = value;
			}
		}

		[Parameter]
		[ValidateNotNull]
		public string SearchBase
		{
			get
			{
				return base["SearchBase"] as string;
			}
			set
			{
				base["SearchBase"] = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public ADSearchScope SearchScope
		{
			get
			{
				return (ADSearchScope)base["SearchScope"];
			}
			set
			{
				base["SearchScope"] = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string Server
		{
			get
			{
				return base["Server"] as string;
			}
			set
			{
				base["Server"] = value;
			}
		}

		[Parameter(ParameterSetName="AccountExpiring")]
		[Parameter(ParameterSetName="AccountInactive")]
		public TimeSpan TimeSpan
		{
			get
			{
				return (TimeSpan)base["TimeSpan"];
			}
			set
			{
				base["TimeSpan"] = value;
			}
		}

		[Parameter]
		public SwitchParameter UsersOnly
		{
			get
			{
				return base.GetSwitchParameter("UsersOnly");
			}
			set
			{
				base["UsersOnly"] = value;
			}
		}

		public SearchADAccountParameterSet()
		{
		}
	}
}