using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal interface IEdmAssociationSetEnd : IEdmElement
	{
		IEdmEntitySet EntitySet
		{
			get;
		}

		IEdmAssociationEnd Role
		{
			get;
		}

	}
}