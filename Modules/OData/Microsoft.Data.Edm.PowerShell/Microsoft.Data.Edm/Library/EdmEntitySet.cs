using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Internal;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmEntitySet : EdmNamedElement, IEdmEntitySet, IEdmEntityContainerElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly IEdmEntityContainer container;

		private readonly IEdmEntityType elementType;

		private readonly Dictionary<IEdmNavigationProperty, IEdmEntitySet> navigationPropertyMappings;

		private readonly Cache<EdmEntitySet, IEnumerable<IEdmNavigationTargetMapping>> navigationTargetsCache;

		private readonly static Func<EdmEntitySet, IEnumerable<IEdmNavigationTargetMapping>> ComputeNavigationTargetsFunc;

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
				return this.elementType;
			}
		}

		public IEnumerable<IEdmNavigationTargetMapping> NavigationTargets
		{
			get
			{
				return this.navigationTargetsCache.GetValue(this, EdmEntitySet.ComputeNavigationTargetsFunc, null);
			}
		}

		static EdmEntitySet()
		{
			EdmEntitySet.ComputeNavigationTargetsFunc = (EdmEntitySet me) => me.ComputeNavigationTargets();
		}

		public EdmEntitySet(IEdmEntityContainer container, string name, IEdmEntityType elementType) : base(name)
		{
			this.navigationPropertyMappings = new Dictionary<IEdmNavigationProperty, IEdmEntitySet>();
			this.navigationTargetsCache = new Cache<EdmEntitySet, IEnumerable<IEdmNavigationTargetMapping>>();
			EdmUtil.CheckArgumentNull<IEdmEntityContainer>(container, "container");
			EdmUtil.CheckArgumentNull<IEdmEntityType>(elementType, "elementType");
			this.container = container;
			this.elementType = elementType;
		}

		public void AddNavigationTarget(IEdmNavigationProperty property, IEdmEntitySet target)
		{
			this.navigationPropertyMappings[property] = target;
			this.navigationTargetsCache.Clear(null);
		}

		private IEnumerable<IEdmNavigationTargetMapping> ComputeNavigationTargets()
		{
			List<IEdmNavigationTargetMapping> edmNavigationTargetMappings = new List<IEdmNavigationTargetMapping>();
			foreach (KeyValuePair<IEdmNavigationProperty, IEdmEntitySet> navigationPropertyMapping in this.navigationPropertyMappings)
			{
				edmNavigationTargetMappings.Add(new EdmNavigationTargetMapping(navigationPropertyMapping.Key, navigationPropertyMapping.Value));
			}
			return edmNavigationTargetMappings;
		}

		public IEdmEntitySet FindNavigationTarget(IEdmNavigationProperty property)
		{
			IEdmEntitySet edmEntitySet = null;
			IEdmNavigationProperty edmNavigationProperty = property;
			if (edmNavigationProperty == null || !this.navigationPropertyMappings.TryGetValue(edmNavigationProperty, out edmEntitySet))
			{
				return null;
			}
			else
			{
				return edmEntitySet;
			}
		}
	}
}