using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmValue : IEdmElement
	{
		IEdmTypeReference Type
		{
			get;
		}

		EdmValueKind ValueKind
		{
			get;
		}

	}
}