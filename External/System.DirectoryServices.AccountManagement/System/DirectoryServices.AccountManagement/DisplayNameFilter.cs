using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class DisplayNameFilter : FilterBase
	{
		public const string PropertyNameStatic = "Principal.DisplayName";

		public override string PropertyName
		{
			get
			{
				return "Principal.DisplayName";
			}
		}

		public DisplayNameFilter()
		{
		}
	}
}