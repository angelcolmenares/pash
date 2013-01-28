using System;

namespace System.Management
{
	internal struct UnixWbemPropertyInfo
	{
		public string Name { get;set; }

		public CimType Type { get;set;}

		public int Flavor { get; set; }
	}
}

