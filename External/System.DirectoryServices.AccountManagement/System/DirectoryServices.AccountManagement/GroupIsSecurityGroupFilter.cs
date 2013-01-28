using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class GroupIsSecurityGroupFilter : FilterBase
	{
		public const string PropertyNameStatic = "GroupPrincipal.IsSecurityGroup";

		public override string PropertyName
		{
			get
			{
				return "GroupPrincipal.IsSecurityGroup";
			}
		}

		public GroupIsSecurityGroupFilter()
		{
		}
	}
}