using System;

namespace System.Management.Automation.Extensions
{
	public class WSManUserNamePasswordValidator : System.IdentityModel.Selectors.UserNamePasswordValidator
	{
		public WSManUserNamePasswordValidator ()
		{

		}

		#region implemented abstract members of UserNamePasswordValidator

		public override void Validate (string userName, string password)
		{
			if(userName==null ||  password==null)
			{
				throw new ArgumentNullException();
			}
		}

		#endregion
	}
}

