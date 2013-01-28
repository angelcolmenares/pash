using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmPropertyConstructor : IEdmElement
	{
		string Name
		{
			get;
		}

		IEdmExpression Value
		{
			get;
		}

	}
}