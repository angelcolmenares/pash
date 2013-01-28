using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmEntityType : EdmStructuredType, IEdmEntityType, IEdmStructuredType, IEdmSchemaType, IEdmType, IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly string namespaceName;

		private readonly string name;

		private List<IEdmStructuralProperty> declaredKey;

		public virtual IEnumerable<IEdmStructuralProperty> DeclaredKey
		{
			get
			{
				return this.declaredKey;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public string Namespace
		{
			get
			{
				return this.namespaceName;
			}
		}

		public EdmSchemaElementKind SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.TypeDefinition;
			}
		}

		public EdmTermKind TermKind
		{
			get
			{
				return EdmTermKind.Type;
			}
		}

		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.Entity;
			}
		}

		public EdmEntityType(string namespaceName, string name) : this(namespaceName, name, null, false, false)
		{
		}

		public EdmEntityType(string namespaceName, string name, IEdmEntityType baseType) : this(namespaceName, name, baseType, false, false)
		{
		}

		public EdmEntityType(string namespaceName, string name, IEdmEntityType baseType, bool isAbstract, bool isOpen) : base(isAbstract, isOpen, baseType)
		{
			EdmUtil.CheckArgumentNull<string>(namespaceName, "namespaceName");
			EdmUtil.CheckArgumentNull<string>(name, "name");
			this.namespaceName = namespaceName;
			this.name = name;
		}

		public EdmNavigationProperty AddBidirectionalNavigation(EdmNavigationPropertyInfo propertyInfo, EdmNavigationPropertyInfo partnerInfo)
		{
			EdmUtil.CheckArgumentNull<EdmNavigationPropertyInfo>(propertyInfo, "propertyInfo");
			EdmUtil.CheckArgumentNull<IEdmEntityType>(propertyInfo.Target, "propertyInfo.Target");
			EdmEntityType target = propertyInfo.Target as EdmEntityType;
			if (target != null)
			{
				EdmNavigationProperty edmNavigationProperty = EdmNavigationProperty.CreateNavigationPropertyWithPartner(propertyInfo, this.FixUpDefaultPartnerInfo(propertyInfo, partnerInfo));
				base.AddProperty(edmNavigationProperty);
				target.AddProperty(edmNavigationProperty.Partner);
				return edmNavigationProperty;
			}
			else
			{
				throw new ArgumentException("propertyInfo.Target", Strings.Constructable_TargetMustBeStock(typeof(EdmEntityType).FullName));
			}
		}

		public void AddKeys(IEdmStructuralProperty[] keyProperties)
		{
			this.AddKeys((IEnumerable<IEdmStructuralProperty>)keyProperties);
		}

		public void AddKeys(IEnumerable<IEdmStructuralProperty> keyProperties)
		{
			EdmUtil.CheckArgumentNull<IEnumerable<IEdmStructuralProperty>>(keyProperties, "keyProperties");
			foreach (IEdmStructuralProperty keyProperty in keyProperties)
			{
				if (this.declaredKey == null)
				{
					this.declaredKey = new List<IEdmStructuralProperty>();
				}
				this.declaredKey.Add(keyProperty);
			}
		}

		public EdmNavigationProperty AddUnidirectionalNavigation(EdmNavigationPropertyInfo propertyInfo)
		{
			return this.AddUnidirectionalNavigation(propertyInfo, this.FixUpDefaultPartnerInfo(propertyInfo, null));
		}

		public EdmNavigationProperty AddUnidirectionalNavigation(EdmNavigationPropertyInfo propertyInfo, EdmNavigationPropertyInfo partnerInfo)
		{
			EdmUtil.CheckArgumentNull<EdmNavigationPropertyInfo>(propertyInfo, "propertyInfo");
			EdmNavigationProperty edmNavigationProperty = EdmNavigationProperty.CreateNavigationPropertyWithPartner(propertyInfo, this.FixUpDefaultPartnerInfo(propertyInfo, partnerInfo));
			base.AddProperty(edmNavigationProperty);
			return edmNavigationProperty;
		}

		private EdmNavigationPropertyInfo FixUpDefaultPartnerInfo(EdmNavigationPropertyInfo propertyInfo, EdmNavigationPropertyInfo partnerInfo)
		{
			EdmNavigationPropertyInfo edmNavigationPropertyInfo = null;
			if (partnerInfo == null)
			{
				EdmNavigationPropertyInfo edmNavigationPropertyInfo1 = new EdmNavigationPropertyInfo();
				edmNavigationPropertyInfo = edmNavigationPropertyInfo1;
				partnerInfo = edmNavigationPropertyInfo1;
			}
			if (partnerInfo.Name == null)
			{
				if (edmNavigationPropertyInfo == null)
				{
					edmNavigationPropertyInfo = partnerInfo.Clone();
				}
				EdmNavigationPropertyInfo edmNavigationPropertyInfo2 = edmNavigationPropertyInfo;
				string name = propertyInfo.Name;
				string empty = name;
				if (name == null)
				{
					empty = string.Empty;
				}
				edmNavigationPropertyInfo2.Name = string.Concat(empty, "Partner");
			}
			if (partnerInfo.Target == null)
			{
				if (edmNavigationPropertyInfo == null)
				{
					edmNavigationPropertyInfo = partnerInfo.Clone();
				}
				edmNavigationPropertyInfo.Target = this;
			}
			if (partnerInfo.TargetMultiplicity == EdmMultiplicity.Unknown)
			{
				if (edmNavigationPropertyInfo == null)
				{
					edmNavigationPropertyInfo = partnerInfo.Clone();
				}
				edmNavigationPropertyInfo.TargetMultiplicity = EdmMultiplicity.ZeroOrOne;
			}
			EdmNavigationPropertyInfo edmNavigationPropertyInfo3 = edmNavigationPropertyInfo;
			EdmNavigationPropertyInfo edmNavigationPropertyInfo4 = edmNavigationPropertyInfo3;
			if (edmNavigationPropertyInfo3 == null)
			{
				edmNavigationPropertyInfo4 = partnerInfo;
			}
			return edmNavigationPropertyInfo4;
		}
	}
}