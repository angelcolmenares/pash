using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library
{
	internal sealed class EdmNavigationProperty : EdmProperty, IEdmNavigationProperty, IEdmProperty, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly bool containsTarget;

		private readonly EdmOnDeleteAction onDelete;

		private EdmNavigationProperty partner;

		private IEnumerable<IEdmStructuralProperty> dependentProperties;

		public bool ContainsTarget
		{
			get
			{
				return this.containsTarget;
			}
		}

		public IEdmEntityType DeclaringEntityType
		{
			get
			{
				return (IEdmEntityType)base.DeclaringType;
			}
		}

		public IEnumerable<IEdmStructuralProperty> DependentProperties
		{
			get
			{
				return this.dependentProperties;
			}
		}

		public bool IsPrincipal
		{
			get
			{
				if (this.DependentProperties != null || this.partner == null)
				{
					return false;
				}
				else
				{
					return this.partner.DependentProperties != null;
				}
			}
		}

		public EdmOnDeleteAction OnDelete
		{
			get
			{
				return this.onDelete;
			}
		}

		public IEdmNavigationProperty Partner
		{
			get
			{
				return this.partner;
			}
		}

		public override EdmPropertyKind PropertyKind
		{
			get
			{
				return EdmPropertyKind.Navigation;
			}
		}

		private EdmNavigationProperty(IEdmEntityType declaringType, string name, IEdmTypeReference type, IEnumerable<IEdmStructuralProperty> dependentProperties, bool containsTarget, EdmOnDeleteAction onDelete) : base(declaringType, name, type)
		{
			this.dependentProperties = dependentProperties;
			this.containsTarget = containsTarget;
			this.onDelete = onDelete;
		}

		private static IEdmTypeReference CreateNavigationPropertyType(IEdmEntityType entityType, EdmMultiplicity multiplicity, string multiplicityParameterName)
		{
			EdmMultiplicity edmMultiplicity = multiplicity;
			switch (edmMultiplicity)
			{
				case EdmMultiplicity.ZeroOrOne:
				{
					return new EdmEntityTypeReference(entityType, true);
				}
				case EdmMultiplicity.One:
				{
					return new EdmEntityTypeReference(entityType, false);
				}
				case EdmMultiplicity.Many:
				{
					return EdmCoreModel.GetCollection(new EdmEntityTypeReference(entityType, false));
				}
			}
			throw new ArgumentOutOfRangeException(multiplicityParameterName, Strings.UnknownEnumVal_Multiplicity(multiplicity));
		}

		public static EdmNavigationProperty CreateNavigationPropertyWithPartner(EdmNavigationPropertyInfo propertyInfo, EdmNavigationPropertyInfo partnerInfo)
		{
			EdmUtil.CheckArgumentNull<EdmNavigationPropertyInfo>(propertyInfo, "propertyInfo");
			EdmUtil.CheckArgumentNull<string>(propertyInfo.Name, "propertyInfo.Name");
			EdmUtil.CheckArgumentNull<IEdmEntityType>(propertyInfo.Target, "propertyInfo.Target");
			EdmUtil.CheckArgumentNull<EdmNavigationPropertyInfo>(partnerInfo, "partnerInfo");
			EdmUtil.CheckArgumentNull<string>(partnerInfo.Name, "partnerInfo.Name");
			EdmUtil.CheckArgumentNull<IEdmEntityType>(partnerInfo.Target, "partnerInfo.Target");
			EdmNavigationProperty edmNavigationProperty = new EdmNavigationProperty(partnerInfo.Target, propertyInfo.Name, EdmNavigationProperty.CreateNavigationPropertyType(propertyInfo.Target, propertyInfo.TargetMultiplicity, "propertyInfo.TargetMultiplicity"), propertyInfo.DependentProperties, propertyInfo.ContainsTarget, propertyInfo.OnDelete);
			EdmNavigationProperty edmNavigationProperty1 = new EdmNavigationProperty(propertyInfo.Target, partnerInfo.Name, EdmNavigationProperty.CreateNavigationPropertyType(partnerInfo.Target, partnerInfo.TargetMultiplicity, "partnerInfo.TargetMultiplicity"), partnerInfo.DependentProperties, partnerInfo.ContainsTarget, partnerInfo.OnDelete);
			edmNavigationProperty.partner = edmNavigationProperty1;
			edmNavigationProperty1.partner = edmNavigationProperty;
			return edmNavigationProperty;
		}

		public static EdmNavigationProperty CreateNavigationPropertyWithPartner(string propertyName, IEdmTypeReference propertyType, IEnumerable<IEdmStructuralProperty> dependentProperties, bool containsTarget, EdmOnDeleteAction onDelete, string partnerPropertyName, IEdmTypeReference partnerPropertyType, IEnumerable<IEdmStructuralProperty> partnerDependentProperties, bool partnerContainsTarget, EdmOnDeleteAction partnerOnDelete)
		{
			EdmUtil.CheckArgumentNull<string>(propertyName, "propertyName");
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(propertyType, "propertyType");
			EdmUtil.CheckArgumentNull<string>(partnerPropertyName, "partnerPropertyName");
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(partnerPropertyType, "partnerPropertyType");
			IEdmEntityType entityType = EdmNavigationProperty.GetEntityType(partnerPropertyType);
			if (entityType != null)
			{
				IEdmEntityType edmEntityType = EdmNavigationProperty.GetEntityType(propertyType);
				if (edmEntityType != null)
				{
					EdmNavigationProperty edmNavigationProperty = new EdmNavigationProperty(entityType, propertyName, propertyType, dependentProperties, containsTarget, onDelete);
					EdmNavigationProperty edmNavigationProperty1 = new EdmNavigationProperty(edmEntityType, partnerPropertyName, partnerPropertyType, partnerDependentProperties, partnerContainsTarget, partnerOnDelete);
					edmNavigationProperty.partner = edmNavigationProperty1;
					edmNavigationProperty1.partner = edmNavigationProperty;
					return edmNavigationProperty;
				}
				else
				{
					throw new ArgumentException(Strings.Constructable_EntityTypeOrCollectionOfEntityTypeExpected, "propertyType");
				}
			}
			else
			{
				throw new ArgumentException(Strings.Constructable_EntityTypeOrCollectionOfEntityTypeExpected, "partnerPropertyType");
			}
		}

		private static IEdmEntityType GetEntityType(IEdmTypeReference type)
		{
			IEdmEntityType definition = null;
			if (!type.IsEntity())
			{
				if (type.IsCollection())
				{
					type = ((IEdmCollectionType)type.Definition).ElementType;
					if (type.IsEntity())
					{
						definition = (IEdmEntityType)type.Definition;
					}
				}
			}
			else
			{
				definition = (IEdmEntityType)type.Definition;
			}
			return definition;
		}
	}
}