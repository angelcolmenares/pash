using System;
using System.Collections.Generic;

namespace System.Management.Classes
{
	internal class UNIX_MethodParameterClass : CIMWbemClassBase
	{
		#region implemented abstract members of CIMWbemClassBase

		public UNIX_MethodParameterClass()
			: base(typeof(UNIX_MethodParameterClass))
		{

		}

		public override string Namespace {
			get {
				return CimNamespaces.CimV2;
			}
		}		

		protected override QueryParser Parser {
			get {
				return new QueryParser<UNIX_MethodParameterClass>();
			}
		}		

		public override string PathField {
			get {
				return "__CLASS";
			}
		}
		#endregion

	}
}

