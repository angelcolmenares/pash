using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm
{
	internal interface IEdmNavigationProperty : IEdmProperty, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		bool ContainsTarget
		{
			get;
		}

		IEnumerable<IEdmStructuralProperty> DependentProperties
		{
			get;
		}

		bool IsPrincipal
		{
			get;
		}

		EdmOnDeleteAction OnDelete
		{
			get;
		}

		IEdmNavigationProperty Partner
		{
			get;
		}

	}
}