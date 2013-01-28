using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class DescriptionFilter : FilterBase
	{
		public const string PropertyNameStatic = "Principal.Description";

		public override string PropertyName
		{
			get
			{
				return "Principal.Description";
			}
		}

		public DescriptionFilter()
		{
		}
	}
}