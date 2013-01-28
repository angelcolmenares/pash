using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class DirectoryAttrConstants
	{
		internal const string DistinguishedName = "distinguishedName";

		internal const string GUID = "objectGuid";

		internal const string objectClass = "objectClass";

		internal static string[] InternalLdapAttributes;

		static DirectoryAttrConstants()
		{
			string[] strArrays = new string[3];
			strArrays[0] = "groupType";
			strArrays[1] = "userAccountControl";
			strArrays[2] = "msDS-User-Account-Control-Computed";
			DirectoryAttrConstants.InternalLdapAttributes = strArrays;
		}
	}
}