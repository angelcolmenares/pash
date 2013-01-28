using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections;

namespace Microsoft.Management.Infrastructure.Native
{
	internal struct NativeCimInstance
	{
		public string ServerName { get; set; }

		public string Namespace { get; set; }

		public string ClassName { get;set; }

		public string CimClassName { get;set; }

		public string SystemProperties { get; set;}

		public string Properties { get; set;}

		public string Qualifiers { get; set;}

		public IntPtr SessionHandle { get;set; }

	}
}

