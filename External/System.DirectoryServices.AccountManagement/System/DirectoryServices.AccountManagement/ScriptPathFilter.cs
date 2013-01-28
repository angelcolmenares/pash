using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class ScriptPathFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.AccountInfo.ScriptPath";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.AccountInfo.ScriptPath";
			}
		}

		public ScriptPathFilter()
		{
		}
	}
}