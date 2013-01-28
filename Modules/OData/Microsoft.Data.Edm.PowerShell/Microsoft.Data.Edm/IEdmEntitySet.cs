using System.Collections.Generic;

namespace Microsoft.Data.Edm
{
	internal interface IEdmEntitySet : IEdmEntityContainerElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		IEdmEntityType ElementType
		{
			get;
		}

		IEnumerable<IEdmNavigationTargetMapping> NavigationTargets
		{
			get;
		}

		IEdmEntitySet FindNavigationTarget(IEdmNavigationProperty navigationProperty);
	}
}