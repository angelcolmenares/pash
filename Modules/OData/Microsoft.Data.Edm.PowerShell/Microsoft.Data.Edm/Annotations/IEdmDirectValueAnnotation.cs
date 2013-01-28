using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Annotations
{
	internal interface IEdmDirectValueAnnotation : IEdmNamedElement, IEdmElement
	{
		string NamespaceUri
		{
			get;
		}

		object Value
		{
			get;
		}

	}
}