using Microsoft.Data.Edm;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmRecordExpression : IEdmExpression, IEdmElement
	{
		IEdmStructuredTypeReference DeclaredType
		{
			get;
		}

		IEnumerable<IEdmPropertyConstructor> Properties
		{
			get;
		}

	}
}