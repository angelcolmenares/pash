using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal interface IEdmAssociationSet : IEdmNamedElement, IEdmElement
	{
		IEdmAssociation Association
		{
			get;
		}

		IEdmEntityContainer Container
		{
			get;
		}

		IEdmAssociationSetEnd End1
		{
			get;
		}

		IEdmAssociationSetEnd End2
		{
			get;
		}

	}
}