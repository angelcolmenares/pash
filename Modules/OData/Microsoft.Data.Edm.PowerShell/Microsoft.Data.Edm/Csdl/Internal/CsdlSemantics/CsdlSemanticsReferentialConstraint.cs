using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsReferentialConstraint : CsdlSemanticsElement, IEdmCheckable
	{
		private readonly CsdlSemanticsAssociation context;

		private readonly CsdlReferentialConstraint constraint;

		private readonly Cache<CsdlSemanticsReferentialConstraint, IEdmAssociationEnd> principalCache;

		private readonly static Func<CsdlSemanticsReferentialConstraint, IEdmAssociationEnd> ComputePrincipalFunc;

		private readonly Cache<CsdlSemanticsReferentialConstraint, IEnumerable<IEdmStructuralProperty>> dependentPropertiesCache;

		private readonly static Func<CsdlSemanticsReferentialConstraint, IEnumerable<IEdmStructuralProperty>> ComputeDependentPropertiesFunc;

		private readonly Cache<CsdlSemanticsReferentialConstraint, IEnumerable<EdmError>> errorsCache;

		private readonly static Func<CsdlSemanticsReferentialConstraint, IEnumerable<EdmError>> ComputeErrorsFunc;

		private readonly Cache<CsdlSemanticsReferentialConstraint, IEnumerable<string>> principalKeyPropertiesNotFoundInPrincipalPropertiesCache;

		private readonly static Func<CsdlSemanticsReferentialConstraint, IEnumerable<string>> ComputePrincipalKeyPropertiesNotFoundInPrincipalPropertiesFunc;

		private readonly Cache<CsdlSemanticsReferentialConstraint, IEnumerable<string>> dependentPropertiesNotFoundInDependentTypeCache;

		private readonly static Func<CsdlSemanticsReferentialConstraint, IEnumerable<string>> ComputeDependentPropertiesNotFoundInDependentTypeFunc;

		private IEdmAssociationEnd DependentEnd
		{
			get
			{
				IEdmAssociation declaringAssociation = this.PrincipalEnd.DeclaringAssociation;
				if (this.PrincipalEnd == declaringAssociation.End1)
				{
					return declaringAssociation.End2;
				}
				else
				{
					return declaringAssociation.End1;
				}
			}
		}

		public IEnumerable<IEdmStructuralProperty> DependentProperties
		{
			get
			{
				return this.dependentPropertiesCache.GetValue(this, CsdlSemanticsReferentialConstraint.ComputeDependentPropertiesFunc, null);
			}
		}

		private IEnumerable<string> DependentPropertiesNotFoundInDependentType
		{
			get
			{
				return this.dependentPropertiesNotFoundInDependentTypeCache.GetValue(this, CsdlSemanticsReferentialConstraint.ComputeDependentPropertiesNotFoundInDependentTypeFunc, null);
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.constraint;
			}
		}

		public IEnumerable<EdmError> Errors
		{
			get
			{
				return this.errorsCache.GetValue(this, CsdlSemanticsReferentialConstraint.ComputeErrorsFunc, null);
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.context.Model;
			}
		}

		public IEdmAssociationEnd PrincipalEnd
		{
			get
			{
				return this.principalCache.GetValue(this, CsdlSemanticsReferentialConstraint.ComputePrincipalFunc, null);
			}
		}

		private IEnumerable<string> PrincipalKeyPropertiesNotFoundInPrincipalProperties
		{
			get
			{
				return this.principalKeyPropertiesNotFoundInPrincipalPropertiesCache.GetValue(this, CsdlSemanticsReferentialConstraint.ComputePrincipalKeyPropertiesNotFoundInPrincipalPropertiesFunc, null);
			}
		}

		static CsdlSemanticsReferentialConstraint()
		{
			CsdlSemanticsReferentialConstraint.ComputePrincipalFunc = (CsdlSemanticsReferentialConstraint me) => me.ComputePrincipal();
			CsdlSemanticsReferentialConstraint.ComputeDependentPropertiesFunc = (CsdlSemanticsReferentialConstraint me) => me.ComputeDependentProperties();
			CsdlSemanticsReferentialConstraint.ComputeErrorsFunc = (CsdlSemanticsReferentialConstraint me) => me.ComputeErrors();
			CsdlSemanticsReferentialConstraint.ComputePrincipalKeyPropertiesNotFoundInPrincipalPropertiesFunc = (CsdlSemanticsReferentialConstraint me) => me.ComputePrincipalKeyPropertiesNotFoundInPrincipalProperties();
			CsdlSemanticsReferentialConstraint.ComputeDependentPropertiesNotFoundInDependentTypeFunc = (CsdlSemanticsReferentialConstraint me) => me.ComputeDependentPropertiesNotFoundInDependentType();
		}

		public CsdlSemanticsReferentialConstraint(CsdlSemanticsAssociation context, CsdlReferentialConstraint constraint) : base(constraint)
		{
			this.principalCache = new Cache<CsdlSemanticsReferentialConstraint, IEdmAssociationEnd>();
			this.dependentPropertiesCache = new Cache<CsdlSemanticsReferentialConstraint, IEnumerable<IEdmStructuralProperty>>();
			this.errorsCache = new Cache<CsdlSemanticsReferentialConstraint, IEnumerable<EdmError>>();
			this.principalKeyPropertiesNotFoundInPrincipalPropertiesCache = new Cache<CsdlSemanticsReferentialConstraint, IEnumerable<string>>();
			this.dependentPropertiesNotFoundInDependentTypeCache = new Cache<CsdlSemanticsReferentialConstraint, IEnumerable<string>>();
			this.context = context;
			this.constraint = constraint;
		}

		private IEnumerable<IEdmStructuralProperty> ComputeDependentProperties()
		{
			List<IEdmStructuralProperty> edmStructuralProperties = new List<IEdmStructuralProperty>();
			IEdmEntityType entityType = this.DependentEnd.EntityType;
			var principalRoleType = this.PrincipalEnd.EntityType;
			CsdlReferentialConstraintRole principal = this.constraint.Principal;
			CsdlReferentialConstraintRole dependent = this.constraint.Dependent;
			if (principalRoleType.Key().Count<IEdmStructuralProperty>() != principal.Properties.Count<CsdlPropertyReference>() || principal.Properties.Count<CsdlPropertyReference>() != dependent.Properties.Count<CsdlPropertyReference>() || this.PrincipalKeyPropertiesNotFoundInPrincipalProperties.Count<string>() != 0 || this.DependentPropertiesNotFoundInDependentType.Count<string>() != 0)
			{
				edmStructuralProperties = new List<IEdmStructuralProperty>();
				foreach (CsdlPropertyReference property in dependent.Properties)
				{
					EdmError[] edmError = new EdmError[1];
					edmError[0] = new EdmError(base.Location, EdmErrorCode.TypeMismatchRelationshipConstraint, Strings.CsdlSemantics_ReferentialConstraintMismatch);
					edmStructuralProperties.Add(new BadProperty(entityType, property.PropertyName, edmError));
				}
			}
			else
			{
				IEnumerator<IEdmStructuralProperty> enumerator = this.PrincipalEnd.EntityType.Key().GetEnumerator();
				using (enumerator)
				{
					Func<CsdlPropertyReference, bool> func = null;
					while (enumerator.MoveNext())
					{
						IEdmStructuralProperty current = enumerator.Current;
						IEnumerable<CsdlPropertyReference> properties = principal.Properties;
						if (func == null)
						{
							func = (CsdlPropertyReference reference) => principalRoleType.FindProperty(reference.PropertyName).Equals(current);
						}
						CsdlPropertyReference csdlPropertyReference = properties.Where<CsdlPropertyReference>(func).FirstOrDefault<CsdlPropertyReference>();
						int num = principal.IndexOf(csdlPropertyReference);
						CsdlPropertyReference csdlPropertyReference1 = dependent.Properties.ElementAt<CsdlPropertyReference>(num);
						IEdmStructuralProperty edmStructuralProperty = entityType.FindProperty(csdlPropertyReference1.PropertyName) as IEdmStructuralProperty;
						edmStructuralProperties.Add(edmStructuralProperty);
					}
				}
			}
			return edmStructuralProperties;
		}

		private IEnumerable<string> ComputeDependentPropertiesNotFoundInDependentType()
		{
			List<string> strs = new List<string>();
			IEdmEntityType entityType = this.DependentEnd.EntityType;
			foreach (CsdlPropertyReference property in this.constraint.Dependent.Properties)
			{
				if (entityType.FindProperty(property.PropertyName) != null)
				{
					continue;
				}
				strs = CsdlSemanticsElement.AllocateAndAdd<string>(strs, property.PropertyName);
			}
			List<string> strs1 = strs;
			IEnumerable<string> strs2 = strs1;
			if (strs1 == null)
			{
				strs2 = Enumerable.Empty<string>();
			}
			return strs2;
		}

		private IEnumerable<EdmError> ComputeErrors()
		{
			List<EdmError> edmErrors = null;
			IEdmEntityType entityType = this.PrincipalEnd.EntityType;
			CsdlReferentialConstraintRole principal = this.constraint.Principal;
			CsdlReferentialConstraintRole dependent = this.constraint.Dependent;
			if (this.constraint.Principal.Role == this.constraint.Dependent.Role)
			{
				edmErrors = CsdlSemanticsElement.AllocateAndAdd<EdmError>(edmErrors, new EdmError(base.Location, EdmErrorCode.SameRoleReferredInReferentialConstraint, Strings.EdmModel_Validator_Semantic_SameRoleReferredInReferentialConstraint(this.constraint.Principal.Role)));
			}
			if (this.constraint.Dependent.Role != this.context.End1.Name && this.constraint.Dependent.Role != this.context.End2.Name)
			{
				edmErrors = CsdlSemanticsElement.AllocateAndAdd<EdmError>(edmErrors, new EdmError(base.Location, EdmErrorCode.InvalidRoleInRelationshipConstraint, Strings.CsdlParser_InvalidEndRoleInRelationshipConstraint(this.constraint.Dependent.Role, this.context.Name)));
			}
			if (this.constraint.Principal.Role != this.context.End1.Name && this.constraint.Principal.Role != this.context.End2.Name)
			{
				edmErrors = CsdlSemanticsElement.AllocateAndAdd<EdmError>(edmErrors, new EdmError(base.Location, EdmErrorCode.InvalidRoleInRelationshipConstraint, Strings.CsdlParser_InvalidEndRoleInRelationshipConstraint(this.constraint.Principal.Role, this.context.Name)));
			}
			if (edmErrors == null)
			{
				if (principal.Properties.Count<CsdlPropertyReference>() != dependent.Properties.Count<CsdlPropertyReference>())
				{
					edmErrors = CsdlSemanticsElement.AllocateAndAdd<EdmError>(edmErrors, new EdmError(base.Location, EdmErrorCode.MismatchNumberOfPropertiesInRelationshipConstraint, Strings.EdmModel_Validator_Semantic_MismatchNumberOfPropertiesinRelationshipConstraint));
				}
				if (entityType.Key().Count<IEdmStructuralProperty>() != principal.Properties.Count<CsdlPropertyReference>() || this.PrincipalKeyPropertiesNotFoundInPrincipalProperties.Count<string>() != 0)
				{
					string str = Strings.EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraintPrimaryEnd(string.Concat(this.DependentEnd.DeclaringAssociation.Namespace, (char)46, this.DependentEnd.DeclaringAssociation.Name), this.constraint.Principal.Role);
					edmErrors = CsdlSemanticsElement.AllocateAndAdd<EdmError>(edmErrors, new EdmError(base.Location, EdmErrorCode.BadPrincipalPropertiesInReferentialConstraint, str));
				}
				foreach (string dependentPropertiesNotFoundInDependentType in this.DependentPropertiesNotFoundInDependentType)
				{
					string str1 = Strings.EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraintDependentEnd(dependentPropertiesNotFoundInDependentType, this.constraint.Dependent.Role);
					edmErrors = CsdlSemanticsElement.AllocateAndAdd<EdmError>(edmErrors, new EdmError(base.Location, EdmErrorCode.InvalidPropertyInRelationshipConstraint, str1));
				}
			}
			List<EdmError> edmErrors1 = edmErrors;
			IEnumerable<EdmError> edmErrors2 = edmErrors1;
			if (edmErrors1 == null)
			{
				edmErrors2 = Enumerable.Empty<EdmError>();
			}
			return edmErrors2;
		}

		private IEdmAssociationEnd ComputePrincipal()
		{
			IEdmAssociationEnd end1 = this.context.End1;
			if (end1.Name != this.constraint.Principal.Role)
			{
				end1 = this.context.End2;
			}
			if (end1.Name != this.constraint.Principal.Role)
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(base.Location, EdmErrorCode.BadNonComputableAssociationEnd, Strings.Bad_UncomputableAssociationEnd(this.constraint.Principal.Role));
				end1 = new BadAssociationEnd(this.context, this.constraint.Principal.Role, edmError);
			}
			return end1;
		}

		private IEnumerable<string> ComputePrincipalKeyPropertiesNotFoundInPrincipalProperties()
		{
			List<string> strs = null;
			IEnumerator<IEdmStructuralProperty> enumerator = this.PrincipalEnd.EntityType.Key().GetEnumerator();
			using (enumerator)
			{
				Func<CsdlPropertyReference, bool> func = null;
				while (enumerator.MoveNext())
				{
					IEdmStructuralProperty current = enumerator.Current;
					CsdlReferentialConstraintRole principal = this.constraint.Principal;
					IEnumerable<CsdlPropertyReference> properties = principal.Properties;
					if (func == null)
					{
						func = (CsdlPropertyReference reference) => reference.PropertyName == current.Name;
					}
					CsdlPropertyReference csdlPropertyReference = properties.Where<CsdlPropertyReference>(func).FirstOrDefault<CsdlPropertyReference>();
					if (csdlPropertyReference != null)
					{
						continue;
					}
					strs = CsdlSemanticsElement.AllocateAndAdd<string>(strs, current.Name);
				}
			}
			List<string> strs1 = strs;
			IEnumerable<string> strs2 = strs1;
			if (strs1 == null)
			{
				strs2 = Enumerable.Empty<string>();
			}
			return strs2;
		}
	}
}