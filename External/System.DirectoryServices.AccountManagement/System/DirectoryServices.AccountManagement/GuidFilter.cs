using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class GuidFilter : FilterBase
	{
		public const string PropertyNameStatic = "Principal.Guid";

		public override string PropertyName
		{
			get
			{
				return "Principal.Guid";
			}
		}

		public GuidFilter()
		{
		}
	}
}