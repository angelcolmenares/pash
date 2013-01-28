using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class SamAccountNameFilter : FilterBase
	{
		public const string PropertyNameStatic = "Principal.SamAccountName";

		public override string PropertyName
		{
			get
			{
				return "Principal.SamAccountName";
			}
		}

		public SamAccountNameFilter()
		{
		}
	}
}