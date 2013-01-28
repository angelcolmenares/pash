using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADUser", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219324", SupportsShouldProcess=true)]
	public class NewADUser : ADNewCmdletBase<NewADUserParameterSet, ADUserFactory<ADUser>, ADUser>
	{
		private const string _debugCategory = "NewADUser";

		public NewADUser()
		{
		}

		protected internal override string GenerateObjectClass(ADUserFactory<ADUser> factory, ADParameterSet cmdletParameters, NewADUserParameterSet dynamicParameters)
		{
			string item = null;
			if (dynamicParameters != null && dynamicParameters.Contains("Type"))
			{
				item = (string)dynamicParameters["Type"];
				if (item != null)
				{
					ADSchemaUtil aDSchemaUtil = new ADSchemaUtil(this.GetCmdletSessionInfo().ADSessionInfo);
					HashSet<string> userSubClasses = aDSchemaUtil.GetUserSubClasses();
					if (!userSubClasses.Contains(item))
					{
						throw new ArgumentException(string.Format(StringResources.UnsupportedObjectClass, item), "Type");
					}
				}
			}
			if (item == null)
			{
				item = factory.StructuralObjectClass;
			}
			return item;
		}

		protected internal override string GetDefaultCreationPathBase()
		{
			return Utils.GetWellKnownGuidDN(this.GetSessionInfo(), this.GetDefaultPartitionPath(), WellKnownGuids.UsersContainerGuid);
		}

		protected internal override void ValidateParameters()
		{
			base.ValidateParameters();
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			if (cmdletSessionInfo.ADRootDSE.ServerType == ADServerType.ADDS && string.IsNullOrEmpty((string)this._cmdletParameters["SamAccountName"]))
			{
				this._cmdletParameters["SamAccountName"] = this._cmdletParameters["Name"];
			}
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