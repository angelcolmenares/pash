using System;

namespace Microsoft.Data.Edm
{
	internal interface IEdmStructuralProperty : IEdmProperty, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		EdmConcurrencyMode ConcurrencyMode
		{
			get;
		}

		string DefaultValueString
		{
			get;
		}

	}
}