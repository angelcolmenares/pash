using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsEntitySet : CsdlSemanticsElement, IEdmEntitySet, IEdmEntityContainerElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly CsdlEntitySet entitySet;

		private readonly CsdlSemanticsEntityContainer container;

		private readonly Cache<CsdlSemanticsEntitySet, IEdmEntityType> elementTypeCache;

		private readonly static Func<CsdlSemanticsEntitySet, IEdmEntityType> ComputeElementTypeFunc;

		private readonly Cache<CsdlSemanticsEntitySet, IEnumerable<IEdmNavigationTargetMapping>> navigationTargetsCache;

		private readonly static Func<CsdlSemanticsEntitySet, IEnumerable<IEdmNavigationTargetMapping>> ComputeNavigationTargetsFunc;

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

		public override CsdlElement Element
		{
			get
			{
				return this.entitySet;
			}
		}

		public IEdmEntityType ElementType
		{
			get
			{
				return this.elementTypeCache.GetValue(this, CsdlSemanticsEntitySet.ComputeElementTypeFunc, null);
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.container.Model;
			}
		}

		public string Name
		{
			get
			{
				return this.entitySet.Name;
			}
		}

		public IEnumerable<IEdmNavigationTargetMapping> NavigationTargets
		{
			get
			{
				return this.navigationTargetsCache.GetValue(this, CsdlSemanticsEntitySet.ComputeNavigationTargetsFunc, null);
			}
		}

		static CsdlSemanticsEntitySet()
		{
			CsdlSemanticsEntitySet.ComputeElementTypeFunc = (CsdlSemanticsEntitySet me) => me.ComputeElementType();
			CsdlSemanticsEntitySet.ComputeNavigationTargetsFunc = (CsdlSemanticsEntitySet me) => me.ComputeNavigationTargets();
		}

		public CsdlSemanticsEntitySet(CsdlSemanticsEntityContainer container, CsdlEntitySet entitySet) : base(entitySet)
		{
			this.elementTypeCache = new Cache<CsdlSemanticsEntitySet, IEdmEntityType>();
			this.navigationTargetsCache = new Cache<CsdlSemanticsEntitySet, IEnumerable<IEdmNavigationTargetMapping>>();
			this.container = container;
			this.entitySet = entitySet;
		}

		private IEdmEntityType ComputeElementType()
		{
			IEdmEntityType edmEntityType = this.container.Context.FindType(this.entitySet.EntityType) as IEdmEntityType;
			IEdmEntityType unresolvedEntityType = edmEntityType;
			if (edmEntityType == null)
			{
				unresolvedEntityType = new UnresolvedEntityType(this.entitySet.EntityType, base.Location);
			}
			return unresolvedEntityType;
		}

		protected override IEnumerable<IEdmVocabularyAnnotation> ComputeInlineVocabularyAnnotations()
		{
			return this.Model.WrapInlineVocabularyAnnotations(this, this.container.Context);
		}

		private IEnumerable<IEdmNavigationTargetMapping> ComputeNavigationTargets()
		{
			List<IEdmNavigationTargetMapping> edmNavigationTargetMappings = new List<IEdmNavigationTargetMapping>();
			foreach (IEdmNavigationProperty edmNavigationProperty in this.ElementType.NavigationProperties())
			{
				IEdmEntitySet edmEntitySet = this.FindNavigationTarget(edmNavigationProperty);
				if (edmEntitySet == null)
				{
					continue;
				}
				edmNavigationTargetMappings.Add(new EdmNavigationTargetMapping(edmNavigationProperty, edmEntitySet));
			}
			foreach (IEdmEntityType edmEntityType in this.ElementType.DeclaredNavigationProperties())
			{
				IEnumerator<IEdmNavigationProperty> enumerator = edmEntityType.DeclaredNavigationProperties().GetEnumerator();
				using (enumerator)
				{
					while (enumerator.MoveNext())
					{
						IEdmNavigationProperty edmNavigationProperty1 = enumerator.Current;
						IEdmEntitySet edmEntitySet1 = this.FindNavigationTarget(edmNavigationProperty1);
						if (edmEntitySet1 == null)
						{
							continue;
						}
						edmNavigationTargetMappings.Add(new EdmNavigationTargetMapping(edmNavigationProperty1, edmEntitySet1));
					}
				}
			}
			return edmNavigationTargetMappings;
		}

		public IEdmEntitySet FindNavigationTarget(IEdmNavigationProperty property)
		{
			IEdmEntitySet entitySet;
			CsdlSemanticsNavigationProperty csdlSemanticsNavigationProperty = property as CsdlSemanticsNavigationProperty;
			if (csdlSemanticsNavigationProperty != null)
			{
				IEnumerator<IEdmEntityContainer> enumerator = this.Model.EntityContainers().GetEnumerator();
				using (enumerator)
				{
					while (enumerator.MoveNext())
					{
						CsdlSemanticsEntityContainer current = (CsdlSemanticsEntityContainer)enumerator.Current;
						IEnumerable<CsdlSemanticsAssociationSet> csdlSemanticsAssociationSets = current.FindAssociationSets(csdlSemanticsNavigationProperty.Association);
						if (csdlSemanticsAssociationSets == null)
						{
							continue;
						}
						IEnumerator<CsdlSemanticsAssociationSet> enumerator1 = csdlSemanticsAssociationSets.GetEnumerator();
						using (enumerator1)
						{
							while (enumerator1.MoveNext())
							{
								CsdlSemanticsAssociationSet csdlSemanticsAssociationSet = enumerator1.Current;
								CsdlSemanticsAssociationSetEnd end1 = csdlSemanticsAssociationSet.End1 as CsdlSemanticsAssociationSetEnd;
								CsdlSemanticsAssociationSetEnd end2 = csdlSemanticsAssociationSet.End2 as CsdlSemanticsAssociationSetEnd;
								if (csdlSemanticsAssociationSet.End1.EntitySet != this || csdlSemanticsNavigationProperty.To != csdlSemanticsAssociationSet.End2.Role)
								{
									if (csdlSemanticsAssociationSet.End2.EntitySet != this || csdlSemanticsNavigationProperty.To != csdlSemanticsAssociationSet.End1.Role)
									{
										continue;
									}
									this.Model.SetAssociationSetName(csdlSemanticsAssociationSet.End2.EntitySet, property, csdlSemanticsAssociationSet.Name);
									if (end1 != null && end2 != null)
									{
										this.Model.SetAssociationSetAnnotations(end2.EntitySet, property, csdlSemanticsAssociationSet.DirectValueAnnotations, end2.DirectValueAnnotations, end1.DirectValueAnnotations);
									}
									entitySet = csdlSemanticsAssociationSet.End1.EntitySet;
									return entitySet;
								}
								else
								{
									this.Model.SetAssociationSetName(csdlSemanticsAssociationSet.End1.EntitySet, property, csdlSemanticsAssociationSet.Name);
									if (end1 != null && end2 != null)
									{
										this.Model.SetAssociationSetAnnotations(end1.EntitySet, property, csdlSemanticsAssociationSet.DirectValueAnnotations, end1.DirectValueAnnotations, end2.DirectValueAnnotations);
									}
									entitySet = csdlSemanticsAssociationSet.End2.EntitySet;
									return entitySet;
								}
							}
						}
					}
					return null;
				}
				return entitySet;
			}
			return null;
		}
	}
}