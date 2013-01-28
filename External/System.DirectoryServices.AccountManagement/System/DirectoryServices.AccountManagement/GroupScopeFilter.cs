using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class GroupScopeFilter : FilterBase
	{
		public const string PropertyNameStatic = "GroupPrincipal.GroupScope";

		public override string PropertyName
		{
			get
			{
				return "GroupPrincipal.GroupScope";
			}
		}

		public GroupScopeFilter()
		{
		}
	}
}