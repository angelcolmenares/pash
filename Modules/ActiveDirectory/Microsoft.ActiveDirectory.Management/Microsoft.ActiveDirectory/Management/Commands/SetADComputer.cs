using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADComputer", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219346", SupportsShouldProcess=true)]
	public class SetADComputer : ADSetCmdletBase<SetADComputerParameterSet, ADComputerFactory<ADComputer>, ADComputer>
	{
		public SetADComputer()
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