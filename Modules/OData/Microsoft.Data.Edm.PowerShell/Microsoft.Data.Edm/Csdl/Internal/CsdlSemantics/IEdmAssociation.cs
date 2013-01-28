using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal interface IEdmAssociation : IEdmNamedElement, IEdmElement
	{
		IEdmAssociationEnd End1
		{
			get;
		}

		IEdmAssociationEnd End2
		{
			get;
		}

		string Namespace
		{
			get;
		}

		CsdlSemanticsReferentialConstraint ReferentialConstraint
		{
			get;
		}

	}
}