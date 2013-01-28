using System;

namespace Microsoft.Management.Infrastructure.Native
{
	internal struct NativeCimClass
	{
		public string Namespace { get;set; }

		public string ServerName { get;set; }

		public string ClassName { get; set; }

		public string Properties { get; set;}

		public string SystemProperties { get; set;}

		public string Methods { get; set;}
		
		public string Qualifiers { get; set;}

		public IntPtr SessionHandle { get;set; }
	}
}

