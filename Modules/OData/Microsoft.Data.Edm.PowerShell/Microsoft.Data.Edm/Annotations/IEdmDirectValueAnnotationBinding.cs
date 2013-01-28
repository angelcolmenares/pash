using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Annotations
{
	internal interface IEdmDirectValueAnnotationBinding
	{
		IEdmElement Element
		{
			get;
		}

		string Name
		{
			get;
		}

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