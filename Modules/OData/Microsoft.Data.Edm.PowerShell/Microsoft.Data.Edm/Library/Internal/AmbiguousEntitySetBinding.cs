using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class AmbiguousEntitySetBinding : AmbiguousBinding<IEdmEntitySet>, IEdmEntitySet, IEdmEntityContainerElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		public IEdmEntityContainer Container
		{
			get
			{
				IEdmEntitySet edmEntitySet = base.Bindings.FirstOrDefault<IEdmEntitySet>();
				if (edmEntitySet != null)
				{
					return edmEntitySet.Container;
				}
				else
				{
					return null;
				}
			}
		}

		public EdmContainerElementKind ContainerElementKind
		{
			get
			{
				return EdmContainerElementKind.EntitySet;
			}
		}

		public IEdmEntityType ElementType
		{
			get
			{
				return new BadEntityType(string.Empty, base.Errors);
			}
		}

		public IEnumerable<IEdmNavigationTargetMapping> NavigationTargets
		{
			get
			{
				return Enumerable.Empty<IEdmNavigationTargetMapping>();
			}
		}

		public AmbiguousEntitySetBinding(IEdmEntitySet first, IEdmEntitySet second) : base(first, second)
		{
		}

		public IEdmEntitySet FindNavigationTarget(IEdmNavigationProperty property)
		{
			return null;
		}
	}
}