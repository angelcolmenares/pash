using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class DefaultAttributes
	{
		internal static string[] attributes;

		static DefaultAttributes()
		{
			string[] strArrays = new string[3];
			strArrays[0] = "distinguishedName";
			strArrays[1] = "objectClass";
			strArrays[2] = "objectGuid";
			DefaultAttributes.attributes = strArrays;
		}
	}
}