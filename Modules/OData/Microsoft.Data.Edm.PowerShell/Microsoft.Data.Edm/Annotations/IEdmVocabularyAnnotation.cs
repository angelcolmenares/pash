using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Annotations
{
	internal interface IEdmVocabularyAnnotation : IEdmElement
	{
		string Qualifier
		{
			get;
		}

		IEdmVocabularyAnnotatable Target
		{
			get;
		}

		IEdmTerm Term
		{
			get;
		}

	}
}