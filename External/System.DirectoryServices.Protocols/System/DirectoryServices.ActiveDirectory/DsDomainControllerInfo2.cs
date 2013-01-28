using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class DsDomainControllerInfo2
	{
		public string netBiosName;

		public string dnsHostName;

		public string siteName;

		public string siteObjectName;

		public string computerObjectName;

		public string serverObjectName;

		public string ntdsaObjectName;

		public bool isPdc;

		public bool dsEnabled;

		public bool isGC;

		public Guid siteObjectGuid;

		public Guid computerObjectGuid;

		public Guid serverObjectGuid;

		public Guid ntdsDsaObjectGuid;

		public DsDomainControllerInfo2()
		{
		}
	}
}