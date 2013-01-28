using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADUser", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219345", SupportsShouldProcess=true)]
	public class SetADUser : ADSetCmdletBase<SetADUserParameterSet, ADUserFactory<ADUser>, ADUser>
	{
		public SetADUser()
		{
		}

		protected internal override void ValidateParameters()
		{
			base.ValidateParameters();
			bool? item = (bool?)this._cmdletParameters["ChangePasswordAtLogon"];
			if (item.HasValue)
			{
				bool? nullable = (bool?)this._cmdletParameters["PasswordNeverExpires"];
				if (nullable.HasValue)
				{
					bool? item1 = (bool?)this._cmdletParameters["ChangePasswordAtLogon"];
					if (item1.Value)
					{
						bool? nullable1 = (bool?)this._cmdletParameters["PasswordNeverExpires"];
						if (nullable1.Value)
						{
							object[] objArray = new object[1];
							objArray[0] = "PasswordNeverExpires";
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.AcctChangePwdNotWorksWhenPwdNotExpires, objArray));
						}
					}
				}
			}
		}
	}
}