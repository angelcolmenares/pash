using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class NameFilter : FilterBase
	{
		public const string PropertyNameStatic = "Principal.Name";

		public override string PropertyName
		{
			get
			{
				return "Principal.Name";
			}
		}

		public NameFilter()
		{
		}
	}
}