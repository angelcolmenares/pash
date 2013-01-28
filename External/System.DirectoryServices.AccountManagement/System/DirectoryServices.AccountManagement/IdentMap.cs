using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class IdentMap
	{
		internal static object[,] StringMap;

		static IdentMap()
		{
			object[,] objArray = new object[6, 2];
			objArray[0, 0] = IdentityType.SamAccountName;
			objArray[0, 1] = "ms-nt4account";
			objArray[1, 0] = IdentityType.Name;
			objArray[1, 1] = "ms-name";
			objArray[2, 0] = IdentityType.UserPrincipalName;
			objArray[2, 1] = "ms-upn";
			objArray[3, 0] = IdentityType.DistinguishedName;
			objArray[3, 1] = "ldap-dn";
			objArray[4, 0] = IdentityType.Sid;
			objArray[4, 1] = "ms-sid";
			objArray[5, 0] = IdentityType.Guid;
			objArray[5, 1] = "ms-guid";
			IdentMap.StringMap = objArray;
		}

		private IdentMap()
		{
		}
	}
}