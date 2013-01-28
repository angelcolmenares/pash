using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class BadEntitySet : BadElement, IEdmEntitySet, IEdmEntityContainerElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly string name;

		private readonly IEdmEntityContainer container;

		public IEdmEntityContainer Container
		{
			get
			{
				return this.container;
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

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public IEnumerable<IEdmNavigationTargetMapping> NavigationTargets
		{
			get
			{
				return Enumerable.Empty<IEdmNavigationTargetMapping>();
			}
		}

		public BadEntitySet(string name, IEdmEntityContainer container, IEnumerable<EdmError> errors) : base(errors)
		{
			string str = name;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			this.name = empty;
			this.container = container;
		}

		public IEdmEntitySet FindNavigationTarget(IEdmNavigationProperty property)
		{
			return null;
		}
	}
}