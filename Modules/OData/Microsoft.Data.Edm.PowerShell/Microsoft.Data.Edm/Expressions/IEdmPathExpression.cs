using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmPathExpression : IEdmExpression, IEdmElement
	{
		IEnumerable<string> Path
		{
			get;
		}

	}
}