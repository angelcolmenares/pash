using System;

namespace Microsoft.Data.Edm
{
	internal interface IEdmDocumentation
	{
		string Description
		{
			get;
		}

		string Summary
		{
			get;
		}

	}
}