using System;
using System.Runtime.InteropServices;

namespace System.Management.Classes
{
	[Guid("f8e42a1d-7441-4c87-80fd-ff2da2672c93")]
	internal class __NAMESPACE : __SystemClass
	{
		public __NAMESPACE ()
		{

		}

		protected override QueryParser Parser { 
			get { return new QueryParser<__SystemClass> (); } 
		}

	}
}

