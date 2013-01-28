using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmEntitySetReferenceExpression : IEdmExpression, IEdmElement
	{
		IEdmEntitySet ReferencedEntitySet
		{
			get;
		}

	}
}