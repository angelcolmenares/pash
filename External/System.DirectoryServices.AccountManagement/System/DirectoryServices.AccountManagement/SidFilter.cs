using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class SidFilter : FilterBase
	{
		public const string PropertyNameStatic = "Principal.Sid";

		public override string PropertyName
		{
			get
			{
				return "Principal.Sid";
			}
		}

		public SidFilter()
		{
		}
	}
}