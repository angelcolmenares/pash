using System;
using System.Runtime.InteropServices;

namespace System.Management.Classes
{
	[Guid("723acec9-7cd5-47e9-89b8-c0ba3f463505")]
	[MetaImplementation(typeof(CIM_ComputerSystem_MetaImplementation))]
	internal abstract class CIM_ComputerSystem : CIM_ManagedSystemElement
	{
		public CIM_ComputerSystem ()
		{

		}
	}

	internal class CIM_ComputerSystem_MetaImplementation : CIM_ComputerSystem
	{
		protected override QueryParser Parser { 
			get { return new QueryParser<CIM_ComputerSystem_MetaImplementation> (); } 
		}


		protected override bool IsMetaImplementation
		{
			get { return true; }
		}
	}
}

