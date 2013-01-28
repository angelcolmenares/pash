using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class DnsSrvData
	{
		public string targetName;

		public short priority;

		public short weight;

		public short port;

		public short pad;

		public DnsSrvData()
		{
		}
	}
}