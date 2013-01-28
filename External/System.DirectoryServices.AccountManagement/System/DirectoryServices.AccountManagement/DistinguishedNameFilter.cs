using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class DistinguishedNameFilter : FilterBase
	{
		public const string PropertyNameStatic = "Principal.DistinguishedName";

		public override string PropertyName
		{
			get
			{
				return "Principal.DistinguishedName";
			}
		}

		public DistinguishedNameFilter()
		{
		}
	}
}