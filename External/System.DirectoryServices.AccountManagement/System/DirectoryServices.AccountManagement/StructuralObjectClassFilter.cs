using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class StructuralObjectClassFilter : FilterBase
	{
		public const string PropertyNameStatic = "Principal.StructuralObjectClass";

		public override string PropertyName
		{
			get
			{
				return "Principal.StructuralObjectClass";
			}
		}

		public StructuralObjectClassFilter()
		{
		}
	}
}