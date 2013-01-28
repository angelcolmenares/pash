using Microsoft.Data.Edm;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmApplyExpression : IEdmExpression, IEdmElement
	{
		IEdmExpression AppliedFunction
		{
			get;
		}

		IEnumerable<IEdmExpression> Arguments
		{
			get;
		}

	}
}