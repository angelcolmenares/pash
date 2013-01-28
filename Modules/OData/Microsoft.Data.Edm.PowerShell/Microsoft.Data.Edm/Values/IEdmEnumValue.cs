using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmEnumValue : IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		IEdmPrimitiveValue Value
		{
			get;
		}

	}
}