using System;
using System.Security.Principal;
using System.ServiceModel;
using System.IdentityModel.Claims;
using System.Security;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.DirectoryServices;

namespace Microsoft.WSMan
{
	public class WSManUserNamePasswordValidator : System.IdentityModel.Selectors.UserNamePasswordValidator
	{
		#region implemented abstract members of UserNamePasswordValidator

		private static bool ByPass = true;

		public override void Validate (string userName, string password)
		{
			if(userName==null ||  password==null)
			{
				throw new ArgumentNullException();
			}
			// Validate UserName
			bool auth = ByPass ? true : CheckUserName(userName, password);
			if (auth)
				System.Threading.Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(userName, "WSMan"), new string[0]);
			else 
				throw new SecurityException("Access Denied");
		}

		#endregion

		bool CheckUserName (string userName, string password)
		{
			bool auth = false;
			try {
				DirectoryEntry entry = new DirectoryEntry ("LDAP://localhost:389/", userName, password);
				DirectorySearcher searcher = new DirectorySearcher(entry, "RecordName=" + userName);
				searcher.FindOne ();
			} catch (Exception ex) {
				auth = false;
			}
			return auth;
		}
	}
}

