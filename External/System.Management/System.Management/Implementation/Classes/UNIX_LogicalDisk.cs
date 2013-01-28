using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace System.Management.Classes
{
	[Guid("a389b15b-ac5a-49c0-811d-dfcd2d827457")]
	internal class UNIX_LogicalDisk :  CIM_LogicalDisk
	{
		public UNIX_LogicalDisk ()
		{

		}

		protected override QueryParser Parser { 
			get { return new QueryParser<UNIX_LogicalDisk> (); } 
		}
	}
}

