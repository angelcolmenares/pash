using System;
using System.Runtime.InteropServices;

namespace System.Management.Classes
{
	[Guid("3e901358-04e2-413e-ac05-3c8dc7225ef9")]
	[MetaImplementation(typeof(CIM_OperatingSystem_MetaImplementation))]
	internal abstract class CIM_OperatingSystem : CIM_ManagedSystemElement
	{
		public CIM_OperatingSystem ()
		{

		}
	}

	internal class CIM_OperatingSystem_MetaImplementation : CIM_OperatingSystem
	{
		
		protected override QueryParser Parser { 
			get { return new QueryParser<CIM_OperatingSystem_MetaImplementation> (); } 
		}

		protected override bool IsMetaImplementation
		{
			get { return true; }
		}
	}
}

