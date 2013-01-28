using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmPropertyReferenceExpression : IEdmExpression, IEdmElement
	{
		IEdmExpression Base
		{
			get;
		}

		IEdmProperty ReferencedProperty
		{
			get;
		}

	}
}