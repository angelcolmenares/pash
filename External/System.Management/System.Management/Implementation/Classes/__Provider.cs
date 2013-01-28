using System;
using System.Runtime.InteropServices;

namespace System.Management.Classes
{
	[Guid("2dce690c-3883-41c1-a5c7-281ea4e1b381")]
	internal class __Provider : __SystemClass
	{
		public __Provider ()
		{

		}

		protected override QueryParser Parser { 
			get { return new QueryParser<__SystemClass> (); } 
		}

	}
}

