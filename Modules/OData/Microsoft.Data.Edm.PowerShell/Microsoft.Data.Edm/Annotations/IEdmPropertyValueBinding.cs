using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;

namespace Microsoft.Data.Edm.Annotations
{
	internal interface IEdmPropertyValueBinding : IEdmElement
	{
		IEdmProperty BoundProperty
		{
			get;
		}

		IEdmExpression Value
		{
			get;
		}

	}
}