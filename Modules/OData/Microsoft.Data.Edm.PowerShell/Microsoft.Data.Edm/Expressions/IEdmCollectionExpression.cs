using Microsoft.Data.Edm;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmCollectionExpression : IEdmExpression, IEdmElement
	{
		IEdmTypeReference DeclaredType
		{
			get;
		}

		IEnumerable<IEdmExpression> Elements
		{
			get;
		}

	}
}