using System;
using System.Runtime.InteropServices;

namespace System.Management.Classes
{
	[Guid("3f2224ea-24d6-4251-987e-2836bf58d49a")]
	internal class __SystemClass : CIMWbemClassBase
	{
		public __SystemClass ()
			: base(typeof(__SystemClass))
		{

		}

		#region implemented abstract members of CIMWbemClassBase

		
		protected override QueryParser Parser { 
			get { return new QueryParser<__SystemClass> (); } 
		}


		public override string Namespace {
			get {
				return CimNamespaces.CimV2;
			}
		}

		public override string PathField {
			get {
				return "__UID";
			}
		}

		#endregion
	}
}

