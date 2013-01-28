using Microsoft.Data.Edm.Values;

namespace Microsoft.Data.Edm
{
	internal interface IEdmEnumMember : IEdmNamedElement, IEdmElement
	{
		IEdmEnumType DeclaringType
		{
			get;
		}

		IEdmPrimitiveValue Value
		{
			get;
		}

	}
}