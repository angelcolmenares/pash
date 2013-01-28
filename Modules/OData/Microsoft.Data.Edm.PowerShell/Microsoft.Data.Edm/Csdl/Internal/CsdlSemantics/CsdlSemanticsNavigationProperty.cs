using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsNavigationProperty : CsdlSemanticsElement, IEdmNavigationProperty, IEdmProperty, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement, IEdmCheckable
	{
		private readonly CsdlNavigationProperty navigationProperty;

		private readonly CsdlSemanticsEntityTypeDefinition declaringType;

		private readonly Cache<CsdlSemanticsNavigationProperty, IEdmTypeReference> typeCache;

		private readonly static Func<CsdlSemanticsNavigationProperty, IEdmTypeReference> ComputeTypeFunc;

		private readonly Cache<CsdlSemanticsNavigationProperty, IEdmAssociation> associationCache;

		private readonly static Func<CsdlSemanticsNavigationProperty, IEdmAssociation> ComputeAssociationFunc;

		private readonly Cache<CsdlSemanticsNavigationProperty, IEdmAssociationEnd> toCache;

		private readonly static Func<CsdlSemanticsNavigationProperty, IEdmAssociationEnd> ComputeToFunc;

		private readonly Cache<CsdlSemanticsNavigationProperty, IEdmAssociationEnd> fromCache;

		private readonly static Func<CsdlSemanticsNavigationProperty, IEdmAssociationEnd> ComputeFromFunc;

		private readonly Cache<CsdlSemanticsNavigationProperty, IEdmNavigationProperty> partnerCache;

		private readonly static Func<CsdlSemanticsNavigationProperty, IEdmNavigationProperty> ComputePartnerFunc;

		public IEdmAssociation Association
		{
			get
			{
				return this.associationCache.GetValue(this, CsdlSemanticsNavigationProperty.ComputeAssociationFunc, null);
			}
		}

		public bool ContainsTarget
		{
			get
			{
				return this.navigationProperty.ContainsTarget;
			}
		}

		public IEdmStructuredType DeclaringType
		{
			get
			{
				return this.declaringType;
			}
		}

		public IEnumerable<IEdmStructuralProperty> DependentProperties
		{
			get
			{
				CsdlSemanticsReferentialConstraint referentialConstraint = this.Association.ReferentialConstraint;
				if (referentialConstraint == null || referentialConstraint.PrincipalEnd != this.To)
				{
					return null;
				}
				else
				{
					return referentialConstraint.DependentProperties;
				}
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.navigationProperty;
			}
		}

		public IEnumerable<EdmError> Errors
		{
			get
			{
				List<EdmError> edmErrors = new List<EdmError>();
				if (this.Association as UnresolvedAssociation != null)
				{
					edmErrors.AddRange(this.Association.Errors());
				}
				if (this.From as CsdlSemanticsNavigationProperty.BadCsdlSemanticsNavigationPropertyToEnd != null)
				{
					edmErrors.AddRange(this.From.Errors());
				}
				if (this.To as CsdlSemanticsNavigationProperty.BadCsdlSemanticsNavigationPropertyToEnd != null)
				{
					edmErrors.AddRange(this.To.Errors());
				}
				return edmErrors;
			}
		}

		private IEdmAssociationEnd From
		{
			get
			{
				return this.fromCache.GetValue(this, CsdlSemanticsNavigationProperty.ComputeFromFunc, null);
			}
		}

		public bool IsPrincipal
		{
			get
			{
				CsdlSemanticsReferentialConstraint referentialConstraint = this.Association.ReferentialConstraint;
				if (referentialConstraint == null)
				{
					return false;
				}
				else
				{
					return referentialConstraint.PrincipalEnd != this.To;
				}
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.declaringType.Model;
			}
		}

		public string Name
		{
			get
			{
				return this.navigationProperty.Name;
			}
		}

		public EdmOnDeleteAction OnDelete
		{
			get
			{
				IEdmAssociationEnd from = this.From;
				if (from != null)
				{
					return from.OnDelete;
				}
				else
				{
					return EdmOnDeleteAction.None;
				}
			}
		}

		public IEdmNavigationProperty Partner
		{
			get
			{
				return this.partnerCache.GetValue(this, CsdlSemanticsNavigationProperty.ComputePartnerFunc, null);
			}
		}

		public EdmPropertyKind PropertyKind
		{
			get
			{
				return EdmPropertyKind.Navigation;
			}
		}

		public IEdmAssociationEnd To
		{
			get
			{
				return this.toCache.GetValue(this, CsdlSemanticsNavigationProperty.ComputeToFunc, null);
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return this.typeCache.GetValue(this, CsdlSemanticsNavigationProperty.ComputeTypeFunc, null);
			}
		}

		static CsdlSemanticsNavigationProperty()
		{
			CsdlSemanticsNavigationProperty.ComputeTypeFunc = (CsdlSemanticsNavigationProperty me) => me.ComputeType();
			CsdlSemanticsNavigationProperty.ComputeAssociationFunc = (CsdlSemanticsNavigationProperty me) => me.ComputeAssociation();
			CsdlSemanticsNavigationProperty.ComputeToFunc = (CsdlSemanticsNavigationProperty me) => me.ComputeTo();
			CsdlSemanticsNavigationProperty.ComputeFromFunc = (CsdlSemanticsNavigationProperty me) => me.ComputeFrom();
			CsdlSemanticsNavigationProperty.ComputePartnerFunc = (CsdlSemanticsNavigationProperty me) => me.ComputePartner();
		}

		public CsdlSemanticsNavigationProperty(CsdlSemanticsEntityTypeDefinition declaringType, CsdlNavigationProperty navigationProperty) : base(navigationProperty)
		{
			this.typeCache = new Cache<CsdlSemanticsNavigationProperty, IEdmTypeReference>();
			this.associationCache = new Cache<CsdlSemanticsNavigationProperty, IEdmAssociation>();
			this.toCache = new Cache<CsdlSemanticsNavigationProperty, IEdmAssociationEnd>();
			this.fromCache = new Cache<CsdlSemanticsNavigationProperty, IEdmAssociationEnd>();
			this.partnerCache = new Cache<CsdlSemanticsNavigationProperty, IEdmNavigationProperty>();
			this.declaringType = declaringType;
			this.navigationProperty = navigationProperty;
		}

		private IEdmAssociation ComputeAssociation()
		{
			CsdlSemanticsElement csdlSemanticsElement;
			CsdlSemanticsElement csdlSemanticsElement1;
			IEnumerable<IEdmDirectValueAnnotation> directValueAnnotations;
			IEdmAssociation edmAssociation = this.declaringType.Context.FindAssociation(this.navigationProperty.Relationship);
			IEdmAssociation unresolvedAssociation = edmAssociation;
			if (edmAssociation == null)
			{
				unresolvedAssociation = new UnresolvedAssociation(this.navigationProperty.Relationship, base.Location);
			}
			IEdmAssociation edmAssociation1 = unresolvedAssociation;
			this.Model.SetAssociationNamespace(this, edmAssociation1.Namespace);
			this.Model.SetAssociationName(this, edmAssociation1.Name);
			CsdlSemanticsAssociation csdlSemanticsAssociation = edmAssociation1 as CsdlSemanticsAssociation;
			CsdlSemanticsAssociationEnd end1 = edmAssociation1.End1 as CsdlSemanticsAssociationEnd;
			CsdlSemanticsAssociationEnd end2 = edmAssociation1.End2 as CsdlSemanticsAssociationEnd;
			if (csdlSemanticsAssociation != null && end1 != null && end2 != null)
			{
				CsdlSemanticsModel model = this.Model;
				CsdlSemanticsNavigationProperty csdlSemanticsNavigationProperty = this;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations = csdlSemanticsAssociation.DirectValueAnnotations;
				if (this.navigationProperty.FromRole == end1.Name)
				{
					csdlSemanticsElement = end1;
				}
				else
				{
					csdlSemanticsElement = end2;
				}
				IEnumerable<IEdmDirectValueAnnotation> directValueAnnotations1 = csdlSemanticsElement.DirectValueAnnotations;
				if (this.navigationProperty.FromRole == end1.Name)
				{
					csdlSemanticsElement1 = end2;
				}
				else
				{
					csdlSemanticsElement1 = end1;
				}
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations1 = csdlSemanticsElement1.DirectValueAnnotations;
				if (edmAssociation1.ReferentialConstraint != null)
				{
					directValueAnnotations = edmAssociation1.ReferentialConstraint.DirectValueAnnotations;
				}
				else
				{
					directValueAnnotations = Enumerable.Empty<IEdmDirectValueAnnotation>();
				}
				model.SetAssociationAnnotations(csdlSemanticsNavigationProperty, edmDirectValueAnnotations, directValueAnnotations1, edmDirectValueAnnotations1, directValueAnnotations);
			}
			return edmAssociation1;
		}

		private IEdmAssociationEnd ComputeFrom()
		{
			IEdmAssociation association = this.Association;
			string fromRole = this.navigationProperty.FromRole;
			if (association.End1.Name != fromRole)
			{
				if (association.End2.Name != fromRole)
				{
					EdmError[] edmError = new EdmError[1];
					edmError[0] = new EdmError(base.Location, EdmErrorCode.BadNavigationProperty, Strings.EdmModel_Validator_Semantic_BadNavigationPropertyUndefinedRole(this.Name, fromRole, association.Name));
					return new CsdlSemanticsNavigationProperty.BadCsdlSemanticsNavigationPropertyToEnd(this.Association, fromRole, edmError);
				}
				else
				{
					return association.End2;
				}
			}
			else
			{
				return association.End1;
			}
		}

		protected override IEnumerable<IEdmVocabularyAnnotation> ComputeInlineVocabularyAnnotations()
		{
			return this.Model.WrapInlineVocabularyAnnotations(this, this.declaringType.Context);
		}

		private IEdmNavigationProperty ComputePartner()
		{
			IEdmNavigationProperty edmNavigationProperty;
			int num = 0;
			foreach (IEdmNavigationProperty edmNavigationProperty1 in this.declaringType.NavigationProperties())
			{
				if (edmNavigationProperty1 == this)
				{
					break;
				}
				CsdlSemanticsNavigationProperty csdlSemanticsNavigationProperty = edmNavigationProperty1 as CsdlSemanticsNavigationProperty;
				if (csdlSemanticsNavigationProperty == null || csdlSemanticsNavigationProperty.Association != this.Association || csdlSemanticsNavigationProperty.To != this.To)
				{
					continue;
				}
				num++;
			}
			IEnumerator<IEdmNavigationProperty> enumerator = this.To.EntityType.NavigationProperties().GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					IEdmNavigationProperty current = enumerator.Current;
					CsdlSemanticsNavigationProperty csdlSemanticsNavigationProperty1 = current as CsdlSemanticsNavigationProperty;
					if (csdlSemanticsNavigationProperty1 == null)
					{
						if (current.Partner != this)
						{
							continue;
						}
						edmNavigationProperty = current;
						return edmNavigationProperty;
					}
					else
					{
						if (csdlSemanticsNavigationProperty1.Association != this.Association || csdlSemanticsNavigationProperty1.To != this.From)
						{
							continue;
						}
						if (num != 0)
						{
							num--;
						}
						else
						{
							edmNavigationProperty = current;
							return edmNavigationProperty;
						}
					}
				}
				return new CsdlSemanticsNavigationProperty.SilentPartner(this);
			}
			return edmNavigationProperty;
		}

		private IEdmAssociationEnd ComputeTo()
		{
			string toRole = this.navigationProperty.ToRole;
			string fromRole = this.navigationProperty.FromRole;
			this.Model.SetAssociationEndName(this, fromRole);
			IEdmAssociation association = this.Association;
			if (toRole != fromRole)
			{
				if (association.End1.Name != toRole)
				{
					if (association.End2.Name != toRole)
					{
						EdmError[] edmError = new EdmError[1];
						edmError[0] = new EdmError(base.Location, EdmErrorCode.BadNavigationProperty, Strings.EdmModel_Validator_Semantic_BadNavigationPropertyUndefinedRole(this.Name, toRole, association.Name));
						return new CsdlSemanticsNavigationProperty.BadCsdlSemanticsNavigationPropertyToEnd(this.Association, toRole, edmError);
					}
					else
					{
						return association.End2;
					}
				}
				else
				{
					return association.End1;
				}
			}
			else
			{
				EdmError[] edmErrorArray = new EdmError[1];
				edmErrorArray[0] = new EdmError(base.Location, EdmErrorCode.BadNavigationProperty, Strings.EdmModel_Validator_Semantic_BadNavigationPropertyRolesCannotBeTheSame(this.Name));
				return new CsdlSemanticsNavigationProperty.BadCsdlSemanticsNavigationPropertyToEnd(association, toRole, edmErrorArray);
			}
		}

		private IEdmTypeReference ComputeType()
		{
			EdmMultiplicity multiplicity = this.To.Multiplicity;
			switch (multiplicity)
			{
				case EdmMultiplicity.ZeroOrOne:
				{
					return new EdmEntityTypeReference(this.To.EntityType, true);
				}
				case EdmMultiplicity.One:
				{
					return new EdmEntityTypeReference(this.To.EntityType, false);
				}
				case EdmMultiplicity.Many:
				{
					return new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(this.To.EntityType, false)), false);
				}
			}
			EdmError[] edmError = new EdmError[1];
			edmError[0] = new EdmError(base.Location, EdmErrorCode.NavigationPropertyTypeInvalidBecauseOfBadAssociation, Strings.EdmModel_Validator_Semantic_BadNavigationPropertyCouldNotDetermineType(this.To.EntityType.Name));
			return new BadEntityTypeReference(this.To.EntityType.FullName(), false, edmError);
		}

		private class BadCsdlSemanticsNavigationPropertyToEnd : BadAssociationEnd
		{
			public BadCsdlSemanticsNavigationPropertyToEnd(IEdmAssociation declaringAssociation, string role, IEnumerable<EdmError> errors) : base(declaringAssociation, role, errors)
			{
			}
		}

		private class SilentPartner : EdmElement, IEdmNavigationProperty, IEdmProperty, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
		{
			private readonly CsdlSemanticsNavigationProperty partner;

			private readonly Cache<CsdlSemanticsNavigationProperty.SilentPartner, IEdmTypeReference> typeCache;

			private readonly static Func<CsdlSemanticsNavigationProperty.SilentPartner, IEdmTypeReference> ComputeTypeFunc;

			public bool ContainsTarget
			{
				get
				{
					return false;
				}
			}

			public IEdmStructuredType DeclaringType
			{
				get
				{
					return this.partner.ToEntityType();
				}
			}

			public IEnumerable<IEdmStructuralProperty> DependentProperties
			{
				get
				{
					CsdlSemanticsReferentialConstraint referentialConstraint = this.partner.Association.ReferentialConstraint;
					if (referentialConstraint == null || this.IsPrincipal)
					{
						return null;
					}
					else
					{
						return referentialConstraint.DependentProperties;
					}
				}
			}

			public bool IsPrincipal
			{
				get
				{
					CsdlSemanticsReferentialConstraint referentialConstraint = this.partner.Association.ReferentialConstraint;
					if (referentialConstraint == null)
					{
						return false;
					}
					else
					{
						return referentialConstraint.PrincipalEnd == this.partner.To;
					}
				}
			}

			public string Name
			{
				get
				{
					return this.partner.From.Name;
				}
			}

			public EdmOnDeleteAction OnDelete
			{
				get
				{
					return this.partner.To.OnDelete;
				}
			}

			public IEdmNavigationProperty Partner
			{
				get
				{
					return this.partner;
				}
			}

			public EdmPropertyKind PropertyKind
			{
				get
				{
					return EdmPropertyKind.Navigation;
				}
			}

			public IEdmTypeReference Type
			{
				get
				{
					return this.typeCache.GetValue(this, CsdlSemanticsNavigationProperty.SilentPartner.ComputeTypeFunc, null);
				}
			}

			static SilentPartner()
			{
				CsdlSemanticsNavigationProperty.SilentPartner.ComputeTypeFunc = (CsdlSemanticsNavigationProperty.SilentPartner me) => me.ComputeType();
			}

			public SilentPartner(CsdlSemanticsNavigationProperty partner)
			{
				this.typeCache = new Cache<CsdlSemanticsNavigationProperty.SilentPartner, IEdmTypeReference>();
				this.partner = partner;
				partner.Model.SetAssociationNamespace(this, partner.Association.Namespace);
				partner.Model.SetAssociationName(this, partner.Association.Name);
				partner.Model.SetAssociationEndName(this, partner.To.Name);
			}

			private IEdmTypeReference ComputeType()
			{
				EdmMultiplicity multiplicity = this.partner.From.Multiplicity;
				switch (multiplicity)
				{
					case EdmMultiplicity.ZeroOrOne:
					{
						return new EdmEntityTypeReference(this.partner.DeclaringEntityType(), true);
					}
					case EdmMultiplicity.One:
					{
						return new EdmEntityTypeReference(this.partner.DeclaringEntityType(), false);
					}
					case EdmMultiplicity.Many:
					{
						return new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(this.partner.DeclaringEntityType(), false)), false);
					}
				}
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(this.partner.To.Location(), EdmErrorCode.NavigationPropertyTypeInvalidBecauseOfBadAssociation, Strings.EdmModel_Validator_Semantic_BadNavigationPropertyCouldNotDetermineType(this.partner.DeclaringEntityType().Name));
				return new BadEntityTypeReference(this.partner.DeclaringEntityType().FullName(), false, edmError);
			}
		}
	}
}