using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmValueTermReferenceExpression : IEdmExpression, IEdmElement
	{
		IEdmExpression Base
		{
			get;
		}

		string Qualifier
		{
			get;
		}

		IEdmValueTerm Term
		{
			get;
		}

	}
}