using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal interface IEdmAssociationEnd : IEdmNamedElement, IEdmElement
	{
		IEdmAssociation DeclaringAssociation
		{
			get;
		}

		IEdmEntityType EntityType
		{
			get;
		}

		EdmMultiplicity Multiplicity
		{
			get;
		}

		EdmOnDeleteAction OnDelete
		{
			get;
		}

	}
}