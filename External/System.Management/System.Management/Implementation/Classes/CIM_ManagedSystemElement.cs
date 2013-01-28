using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Management.Classes
{
	[Guid("7d25a7d7-8471-4568-a176-1917b4feaa5e")]
	[MetaImplementation(typeof(CIM_ManagedSystemElement_MetaImplementation))]
	internal abstract class CIM_ManagedSystemElement : CIMWbemClassBase
	{
		public CIM_ManagedSystemElement ()
			: base(typeof(CIM_ManagedSystemElement))
		{
			RegisterQualifiers();
		}
	

		protected override bool IsMetaImplementation
		{
			get { return false; }
		}

		private void RegisterQualifiers ()
		{
			RegisterQualifier (new UnixWbemQualiferInfo { Name = "IsDynamic", Type = CimType.Boolean, Origin = QualifierOrigin.Local, Overridable = false, Ammendable = false, PropagateToDerivedClasses = true, PropagateToInstance = true, OriginType = typeof(CIM_ManagedSystemElement), Value = true } );
			RegisterQualifier (new UnixWbemQualiferInfo { Name = "IsAbstract", Type = CimType.Boolean, Origin = QualifierOrigin.Local, Overridable = false, Ammendable = false, PropagateToDerivedClasses = true, PropagateToInstance = true, OriginType = typeof(CIM_ManagedSystemElement), Value = IsMetaImplementation } );
			RegisterQualifier (new UnixWbemQualiferInfo { Name = "Provider", Type = CimType.String, Origin = QualifierOrigin.Local, Overridable = true, Ammendable = false, PropagateToDerivedClasses = true, PropagateToInstance = true, OriginType = typeof(CIM_ManagedSystemElement), Value = "CIMUnix" } );
			RegisterQualifier (new UnixWbemQualiferInfo { Name = "UUID", Type = CimType.String, Origin = QualifierOrigin.Local, Overridable = false, Ammendable = false, PropagateToDerivedClasses = true, PropagateToInstance = true, OriginType = this.GetType (), Value = GetClassUid ().ToString ("B") } );
		}

	

		public override string Namespace {
			get { return CimNamespaces.CimV2; }
		}

		public override string PathField {
			get { return "__UID"; }
		}

	}

	internal class CIM_ManagedSystemElement_MetaImplementation : CIM_ManagedSystemElement
	{
		protected override QueryParser Parser { 
			get { return new QueryParser<CIM_ManagedSystemElement_MetaImplementation> (); } 
		}

		protected override bool IsMetaImplementation
		{
			get { return true; }
		}
	}
}

