using System;

namespace Microsoft.Data.Edm
{
	internal interface IEdmTypeReference : IEdmElement
	{
		IEdmType Definition
		{
			get;
		}

		bool IsNullable
		{
			get;
		}

	}
}