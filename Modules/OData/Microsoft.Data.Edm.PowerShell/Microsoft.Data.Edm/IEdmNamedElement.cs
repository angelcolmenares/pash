using System;

namespace Microsoft.Data.Edm
{
	internal interface IEdmNamedElement : IEdmElement
	{
		string Name
		{
			get;
		}

	}
}