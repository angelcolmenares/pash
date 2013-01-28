using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class DsNameResultItem
	{
		public int status;

		public string domain;

		public string name;

		public DsNameResultItem()
		{
		}
	}
}