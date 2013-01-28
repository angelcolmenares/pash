using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class ExtensionCacheFilter : FilterBase
	{
		public const string PropertyNameStatic = "Principal.ExtensionCache";

		public override string PropertyName
		{
			get
			{
				return "Principal.ExtensionCache";
			}
		}

		public ExtensionCacheFilter()
		{
		}
	}
}