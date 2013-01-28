using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal struct SupportedCapability
	{
		public static string ADOid;

		public static string ADAMOid;

		static SupportedCapability()
		{
			SupportedCapability.ADOid = "1.2.840.113556.1.4.800";
			SupportedCapability.ADAMOid = "1.2.840.113556.1.4.1851";
		}
	}
}