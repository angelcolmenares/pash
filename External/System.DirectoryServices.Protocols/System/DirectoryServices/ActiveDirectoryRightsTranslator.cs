using System;

namespace System.DirectoryServices
{
	internal sealed class ActiveDirectoryRightsTranslator
	{
		public ActiveDirectoryRightsTranslator()
		{

		}

		internal static int AccessMaskFromRights(ActiveDirectoryRights adRights)
		{
			return (int)adRights;
		}

		internal static ActiveDirectoryRights RightsFromAccessMask(int accessMask)
		{
			return (ActiveDirectoryRights)accessMask;
		}
	}
}