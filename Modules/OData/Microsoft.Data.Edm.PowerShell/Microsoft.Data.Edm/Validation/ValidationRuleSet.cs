using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Validation
{
	internal sealed class ValidationRuleSet : IEnumerable<ValidationRule>, IEnumerable
	{
		private readonly Dictionary<Type, List<ValidationRule>> rules;

		private readonly static ValidationRuleSet BaseRuleSet;

		private readonly static ValidationRuleSet V1RuleSet;

		private readonly static ValidationRuleSet V1_1RuleSet;

		private readonly static ValidationRuleSet V1_2RuleSet;

		private readonly static ValidationRuleSet V2RuleSet;

		private readonly static ValidationRuleSet V3RuleSet;

		static ValidationRuleSet()
		{
			ValidationRule[] entityTypeKeyPropertyMustBelongToEntity = new ValidationRule[92];
			entityTypeKeyPropertyMustBelongToEntity[0] = ValidationRules.EntityTypeKeyPropertyMustBelongToEntity;
			entityTypeKeyPropertyMustBelongToEntity[1] = ValidationRules.StructuredTypePropertiesDeclaringTypeMustBeCorrect;
			entityTypeKeyPropertyMustBelongToEntity[2] = ValidationRules.NamedElementNameMustNotBeEmptyOrWhiteSpace;
			entityTypeKeyPropertyMustBelongToEntity[3] = ValidationRules.NamedElementNameIsTooLong;
			entityTypeKeyPropertyMustBelongToEntity[4] = ValidationRules.NamedElementNameIsNotAllowed;
			entityTypeKeyPropertyMustBelongToEntity[5] = ValidationRules.SchemaElementNamespaceIsNotAllowed;
			entityTypeKeyPropertyMustBelongToEntity[6] = ValidationRules.SchemaElementNamespaceIsTooLong;
			entityTypeKeyPropertyMustBelongToEntity[7] = ValidationRules.SchemaElementNamespaceMustNotBeEmptyOrWhiteSpace;
			entityTypeKeyPropertyMustBelongToEntity[8] = ValidationRules.SchemaElementSystemNamespaceEncountered;
			entityTypeKeyPropertyMustBelongToEntity[9] = ValidationRules.EntityContainerDuplicateEntityContainerMemberName;
			entityTypeKeyPropertyMustBelongToEntity[10] = ValidationRules.EntityTypeDuplicatePropertyNameSpecifiedInEntityKey;
			entityTypeKeyPropertyMustBelongToEntity[11] = ValidationRules.EntityTypeInvalidKeyNullablePart;
			entityTypeKeyPropertyMustBelongToEntity[12] = ValidationRules.EntityTypeEntityKeyMustBeScalar;
			entityTypeKeyPropertyMustBelongToEntity[13] = ValidationRules.EntityTypeInvalidKeyKeyDefinedInBaseClass;
			entityTypeKeyPropertyMustBelongToEntity[14] = ValidationRules.EntityTypeKeyMissingOnEntityType;
			entityTypeKeyPropertyMustBelongToEntity[15] = ValidationRules.StructuredTypeInvalidMemberNameMatchesTypeName;
			entityTypeKeyPropertyMustBelongToEntity[16] = ValidationRules.StructuredTypePropertyNameAlreadyDefined;
			entityTypeKeyPropertyMustBelongToEntity[17] = ValidationRules.StructuralPropertyInvalidPropertyType;
			entityTypeKeyPropertyMustBelongToEntity[18] = ValidationRules.ComplexTypeInvalidAbstractComplexType;
			entityTypeKeyPropertyMustBelongToEntity[19] = ValidationRules.ComplexTypeInvalidPolymorphicComplexType;
			entityTypeKeyPropertyMustBelongToEntity[20] = ValidationRules.FunctionBaseParameterNameAlreadyDefinedDuplicate;
			entityTypeKeyPropertyMustBelongToEntity[21] = ValidationRules.FunctionImportReturnEntitiesButDoesNotSpecifyEntitySet;
			entityTypeKeyPropertyMustBelongToEntity[22] = ValidationRules.FunctionImportEntityTypeDoesNotMatchEntitySet;
			entityTypeKeyPropertyMustBelongToEntity[23] = ValidationRules.ComposableFunctionImportMustHaveReturnType;
			entityTypeKeyPropertyMustBelongToEntity[24] = ValidationRules.StructuredTypeBaseTypeMustBeSameKindAsDerivedKind;
			entityTypeKeyPropertyMustBelongToEntity[25] = ValidationRules.RowTypeBaseTypeMustBeNull;
			entityTypeKeyPropertyMustBelongToEntity[26] = ValidationRules.NavigationPropertyWithRecursiveContainmentTargetMustBeOptional;
			entityTypeKeyPropertyMustBelongToEntity[27] = ValidationRules.NavigationPropertyWithRecursiveContainmentSourceMustBeFromZeroOrOne;
			entityTypeKeyPropertyMustBelongToEntity[28] = ValidationRules.NavigationPropertyWithNonRecursiveContainmentSourceMustBeFromOne;
			entityTypeKeyPropertyMustBelongToEntity[29] = ValidationRules.EntitySetInaccessibleEntityType;
			entityTypeKeyPropertyMustBelongToEntity[30] = ValidationRules.StructuredTypeInaccessibleBaseType;
			entityTypeKeyPropertyMustBelongToEntity[31] = ValidationRules.EntityReferenceTypeInaccessibleEntityType;
			entityTypeKeyPropertyMustBelongToEntity[32] = ValidationRules.TypeReferenceInaccessibleSchemaType;
			entityTypeKeyPropertyMustBelongToEntity[33] = ValidationRules.EntitySetTypeHasNoKeys;
			entityTypeKeyPropertyMustBelongToEntity[34] = ValidationRules.FunctionOnlyInputParametersAllowedInFunctions;
			entityTypeKeyPropertyMustBelongToEntity[35] = ValidationRules.RowTypeMustContainProperties;
			entityTypeKeyPropertyMustBelongToEntity[36] = ValidationRules.DecimalTypeReferenceScaleOutOfRange;
			entityTypeKeyPropertyMustBelongToEntity[37] = ValidationRules.BinaryTypeReferenceBinaryMaxLengthNegative;
			entityTypeKeyPropertyMustBelongToEntity[38] = ValidationRules.StringTypeReferenceStringMaxLengthNegative;
			entityTypeKeyPropertyMustBelongToEntity[39] = ValidationRules.StructuralPropertyInvalidPropertyTypeConcurrencyMode;
			entityTypeKeyPropertyMustBelongToEntity[40] = ValidationRules.EnumMemberValueMustHaveSameTypeAsUnderlyingType;
			entityTypeKeyPropertyMustBelongToEntity[41] = ValidationRules.EnumTypeEnumMemberNameAlreadyDefined;
			entityTypeKeyPropertyMustBelongToEntity[42] = ValidationRules.FunctionImportBindableFunctionImportMustHaveParameters;
			entityTypeKeyPropertyMustBelongToEntity[43] = ValidationRules.FunctionImportComposableFunctionImportCannotBeSideEffecting;
			entityTypeKeyPropertyMustBelongToEntity[44] = ValidationRules.FunctionImportEntitySetExpressionIsInvalid;
			entityTypeKeyPropertyMustBelongToEntity[45] = ValidationRules.BinaryTypeReferenceBinaryUnboundedNotValidForMaxLength;
			entityTypeKeyPropertyMustBelongToEntity[46] = ValidationRules.StringTypeReferenceStringUnboundedNotValidForMaxLength;
			entityTypeKeyPropertyMustBelongToEntity[47] = ValidationRules.ImmediateValueAnnotationElementAnnotationIsValid;
			entityTypeKeyPropertyMustBelongToEntity[48] = ValidationRules.ValueAnnotationAssertCorrectExpressionType;
			entityTypeKeyPropertyMustBelongToEntity[49] = ValidationRules.IfExpressionAssertCorrectTestType;
			entityTypeKeyPropertyMustBelongToEntity[50] = ValidationRules.CollectionExpressionAllElementsCorrectType;
			entityTypeKeyPropertyMustBelongToEntity[51] = ValidationRules.RecordExpressionPropertiesMatchType;
			entityTypeKeyPropertyMustBelongToEntity[52] = ValidationRules.NavigationPropertyDependentPropertiesMustBelongToDependentEntity;
			entityTypeKeyPropertyMustBelongToEntity[53] = ValidationRules.NavigationPropertyInvalidOperationMultipleEndsInAssociation;
			entityTypeKeyPropertyMustBelongToEntity[54] = ValidationRules.NavigationPropertyEndWithManyMultiplicityCannotHaveOperationsSpecified;
			entityTypeKeyPropertyMustBelongToEntity[55] = ValidationRules.NavigationPropertyTypeMismatchRelationshipConstraint;
			entityTypeKeyPropertyMustBelongToEntity[56] = ValidationRules.NavigationPropertyDuplicateDependentProperty;
			entityTypeKeyPropertyMustBelongToEntity[57] = ValidationRules.NavigationPropertyPrincipalEndMultiplicity;
			entityTypeKeyPropertyMustBelongToEntity[58] = ValidationRules.NavigationPropertyDependentEndMultiplicity;
			entityTypeKeyPropertyMustBelongToEntity[59] = ValidationRules.NavigationPropertyCorrectType;
			entityTypeKeyPropertyMustBelongToEntity[60] = ValidationRules.ImmediateValueAnnotationElementAnnotationHasNameAndNamespace;
			entityTypeKeyPropertyMustBelongToEntity[61] = ValidationRules.FunctionApplicationExpressionParametersMatchAppliedFunction;
			entityTypeKeyPropertyMustBelongToEntity[62] = ValidationRules.VocabularyAnnotatableNoDuplicateAnnotations;
			entityTypeKeyPropertyMustBelongToEntity[63] = ValidationRules.TemporalTypeReferencePrecisionOutOfRange;
			entityTypeKeyPropertyMustBelongToEntity[64] = ValidationRules.DecimalTypeReferencePrecisionOutOfRange;
			entityTypeKeyPropertyMustBelongToEntity[65] = ValidationRules.ModelDuplicateEntityContainerName;
			entityTypeKeyPropertyMustBelongToEntity[66] = ValidationRules.FunctionImportParametersCannotHaveModeOfNone;
			entityTypeKeyPropertyMustBelongToEntity[67] = ValidationRules.TypeMustNotHaveKindOfNone;
			entityTypeKeyPropertyMustBelongToEntity[68] = ValidationRules.PrimitiveTypeMustNotHaveKindOfNone;
			entityTypeKeyPropertyMustBelongToEntity[69] = ValidationRules.PropertyMustNotHaveKindOfNone;
			entityTypeKeyPropertyMustBelongToEntity[70] = ValidationRules.TermMustNotHaveKindOfNone;
			entityTypeKeyPropertyMustBelongToEntity[71] = ValidationRules.SchemaElementMustNotHaveKindOfNone;
			entityTypeKeyPropertyMustBelongToEntity[72] = ValidationRules.EntityContainerElementMustNotHaveKindOfNone;
			entityTypeKeyPropertyMustBelongToEntity[73] = ValidationRules.PrimitiveValueValidForType;
			entityTypeKeyPropertyMustBelongToEntity[74] = ValidationRules.EntitySetCanOnlyBeContainedByASingleNavigationProperty;
			entityTypeKeyPropertyMustBelongToEntity[75] = ValidationRules.EntitySetNavigationMappingMustBeBidirectional;
			entityTypeKeyPropertyMustBelongToEntity[76] = ValidationRules.EntitySetNavigationPropertyMappingsMustBeUnique;
			entityTypeKeyPropertyMustBelongToEntity[77] = ValidationRules.TypeAnnotationAssertMatchesTermType;
			entityTypeKeyPropertyMustBelongToEntity[78] = ValidationRules.TypeAnnotationInaccessibleTerm;
			entityTypeKeyPropertyMustBelongToEntity[79] = ValidationRules.PropertyValueBindingValueIsCorrectType;
			entityTypeKeyPropertyMustBelongToEntity[80] = ValidationRules.EnumMustHaveIntegerUnderlyingType;
			entityTypeKeyPropertyMustBelongToEntity[81] = ValidationRules.ValueAnnotationInaccessibleTerm;
			entityTypeKeyPropertyMustBelongToEntity[82] = ValidationRules.ElementDirectValueAnnotationFullNameMustBeUnique;
			entityTypeKeyPropertyMustBelongToEntity[83] = ValidationRules.VocabularyAnnotationInaccessibleTarget;
			entityTypeKeyPropertyMustBelongToEntity[84] = ValidationRules.ComplexTypeMustContainProperties;
			entityTypeKeyPropertyMustBelongToEntity[85] = ValidationRules.EntitySetAssociationSetNameMustBeValid;
			entityTypeKeyPropertyMustBelongToEntity[86] = ValidationRules.NavigationPropertyAssociationEndNameIsValid;
			entityTypeKeyPropertyMustBelongToEntity[87] = ValidationRules.NavigationPropertyAssociationNameIsValid;
			entityTypeKeyPropertyMustBelongToEntity[88] = ValidationRules.OnlyEntityTypesCanBeOpen;
			entityTypeKeyPropertyMustBelongToEntity[89] = ValidationRules.NavigationPropertyEntityMustNotIndirectlyContainItself;
			entityTypeKeyPropertyMustBelongToEntity[90] = ValidationRules.EntitySetRecursiveNavigationPropertyMappingsMustPointBackToSourceEntitySet;
			entityTypeKeyPropertyMustBelongToEntity[91] = ValidationRules.EntitySetNavigationPropertyMappingMustPointToValidTargetForProperty;
			ValidationRuleSet.BaseRuleSet = new ValidationRuleSet(entityTypeKeyPropertyMustBelongToEntity);
			ValidationRule[] navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2 = new ValidationRule[17];
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[0] = ValidationRules.NavigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[1] = ValidationRules.FunctionsNotSupportedBeforeV2;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[2] = ValidationRules.FunctionImportUnsupportedReturnTypeV1;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[3] = ValidationRules.FunctionImportParametersIncorrectTypeBeforeV3;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[4] = ValidationRules.FunctionImportIsSideEffectingNotSupportedBeforeV3;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[5] = ValidationRules.FunctionImportIsComposableNotSupportedBeforeV3;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[6] = ValidationRules.FunctionImportIsBindableNotSupportedBeforeV3;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[7] = ValidationRules.EntityTypeEntityKeyMustNotBeBinaryBeforeV2;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[8] = ValidationRules.EnumTypeEnumsNotSupportedBeforeV3;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[9] = ValidationRules.NavigationPropertyContainsTargetNotSupportedBeforeV3;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[10] = ValidationRules.StructuralPropertyNullableComplexType;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[11] = ValidationRules.ValueTermsNotSupportedBeforeV3;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[12] = ValidationRules.VocabularyAnnotationsNotSupportedBeforeV3;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[13] = ValidationRules.OpenTypesNotSupported;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[14] = ValidationRules.StreamTypeReferencesNotSupportedBeforeV3;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[15] = ValidationRules.SpatialTypeReferencesNotSupportedBeforeV3;
			navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2[16] = ValidationRules.ModelDuplicateSchemaElementNameBeforeV3;
			ValidationRuleSet.V1RuleSet = new ValidationRuleSet(ValidationRuleSet.BaseRuleSet, navigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2);
			ValidationRuleSet baseRuleSet = ValidationRuleSet.BaseRuleSet;
			ValidationRule[] functionsNotSupportedBeforeV2 = new ValidationRule[16];
			functionsNotSupportedBeforeV2[0] = ValidationRules.NavigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2;
			functionsNotSupportedBeforeV2[1] = ValidationRules.FunctionsNotSupportedBeforeV2;
			functionsNotSupportedBeforeV2[2] = ValidationRules.FunctionImportUnsupportedReturnTypeAfterV1;
			functionsNotSupportedBeforeV2[3] = ValidationRules.FunctionImportIsSideEffectingNotSupportedBeforeV3;
			functionsNotSupportedBeforeV2[4] = ValidationRules.FunctionImportIsComposableNotSupportedBeforeV3;
			functionsNotSupportedBeforeV2[5] = ValidationRules.FunctionImportIsBindableNotSupportedBeforeV3;
			functionsNotSupportedBeforeV2[6] = ValidationRules.EntityTypeEntityKeyMustNotBeBinaryBeforeV2;
			functionsNotSupportedBeforeV2[7] = ValidationRules.FunctionImportParametersIncorrectTypeBeforeV3;
			functionsNotSupportedBeforeV2[8] = ValidationRules.EnumTypeEnumsNotSupportedBeforeV3;
			functionsNotSupportedBeforeV2[9] = ValidationRules.NavigationPropertyContainsTargetNotSupportedBeforeV3;
			functionsNotSupportedBeforeV2[10] = ValidationRules.ValueTermsNotSupportedBeforeV3;
			functionsNotSupportedBeforeV2[11] = ValidationRules.VocabularyAnnotationsNotSupportedBeforeV3;
			functionsNotSupportedBeforeV2[12] = ValidationRules.OpenTypesNotSupported;
			functionsNotSupportedBeforeV2[13] = ValidationRules.StreamTypeReferencesNotSupportedBeforeV3;
			functionsNotSupportedBeforeV2[14] = ValidationRules.SpatialTypeReferencesNotSupportedBeforeV3;
			functionsNotSupportedBeforeV2[15] = ValidationRules.ModelDuplicateSchemaElementNameBeforeV3;
			ValidationRuleSet.V1_1RuleSet = new ValidationRuleSet(baseRuleSet.Where<ValidationRule>((ValidationRule r) => {
				if (r == ValidationRules.ComplexTypeInvalidAbstractComplexType)
				{
					return false;
				}
				else
				{
					return r != ValidationRules.ComplexTypeInvalidPolymorphicComplexType;
				}
			}
			), functionsNotSupportedBeforeV2);
			ValidationRuleSet validationRuleSets = ValidationRuleSet.BaseRuleSet;
			ValidationRule[] functionImportUnsupportedReturnTypeAfterV1 = new ValidationRule[15];
			functionImportUnsupportedReturnTypeAfterV1[0] = ValidationRules.NavigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2;
			functionImportUnsupportedReturnTypeAfterV1[1] = ValidationRules.FunctionsNotSupportedBeforeV2;
			functionImportUnsupportedReturnTypeAfterV1[2] = ValidationRules.FunctionImportUnsupportedReturnTypeAfterV1;
			functionImportUnsupportedReturnTypeAfterV1[3] = ValidationRules.FunctionImportParametersIncorrectTypeBeforeV3;
			functionImportUnsupportedReturnTypeAfterV1[4] = ValidationRules.FunctionImportIsSideEffectingNotSupportedBeforeV3;
			functionImportUnsupportedReturnTypeAfterV1[5] = ValidationRules.FunctionImportIsComposableNotSupportedBeforeV3;
			functionImportUnsupportedReturnTypeAfterV1[6] = ValidationRules.FunctionImportIsBindableNotSupportedBeforeV3;
			functionImportUnsupportedReturnTypeAfterV1[7] = ValidationRules.EntityTypeEntityKeyMustNotBeBinaryBeforeV2;
			functionImportUnsupportedReturnTypeAfterV1[8] = ValidationRules.EnumTypeEnumsNotSupportedBeforeV3;
			functionImportUnsupportedReturnTypeAfterV1[9] = ValidationRules.NavigationPropertyContainsTargetNotSupportedBeforeV3;
			functionImportUnsupportedReturnTypeAfterV1[10] = ValidationRules.ValueTermsNotSupportedBeforeV3;
			functionImportUnsupportedReturnTypeAfterV1[11] = ValidationRules.VocabularyAnnotationsNotSupportedBeforeV3;
			functionImportUnsupportedReturnTypeAfterV1[12] = ValidationRules.StreamTypeReferencesNotSupportedBeforeV3;
			functionImportUnsupportedReturnTypeAfterV1[13] = ValidationRules.SpatialTypeReferencesNotSupportedBeforeV3;
			functionImportUnsupportedReturnTypeAfterV1[14] = ValidationRules.ModelDuplicateSchemaElementNameBeforeV3;
			ValidationRuleSet.V1_2RuleSet = new ValidationRuleSet(validationRuleSets.Where<ValidationRule>((ValidationRule r) => {
				if (r == ValidationRules.ComplexTypeInvalidAbstractComplexType)
				{
					return false;
				}
				else
				{
					return r != ValidationRules.ComplexTypeInvalidPolymorphicComplexType;
				}
			}
			), functionImportUnsupportedReturnTypeAfterV1);
			ValidationRule[] functionImportParametersIncorrectTypeBeforeV3 = new ValidationRule[14];
			functionImportParametersIncorrectTypeBeforeV3[0] = ValidationRules.FunctionImportParametersIncorrectTypeBeforeV3;
			functionImportParametersIncorrectTypeBeforeV3[1] = ValidationRules.FunctionImportUnsupportedReturnTypeAfterV1;
			functionImportParametersIncorrectTypeBeforeV3[2] = ValidationRules.FunctionImportIsSideEffectingNotSupportedBeforeV3;
			functionImportParametersIncorrectTypeBeforeV3[3] = ValidationRules.FunctionImportIsComposableNotSupportedBeforeV3;
			functionImportParametersIncorrectTypeBeforeV3[4] = ValidationRules.FunctionImportIsBindableNotSupportedBeforeV3;
			functionImportParametersIncorrectTypeBeforeV3[5] = ValidationRules.EnumTypeEnumsNotSupportedBeforeV3;
			functionImportParametersIncorrectTypeBeforeV3[6] = ValidationRules.NavigationPropertyContainsTargetNotSupportedBeforeV3;
			functionImportParametersIncorrectTypeBeforeV3[7] = ValidationRules.StructuralPropertyNullableComplexType;
			functionImportParametersIncorrectTypeBeforeV3[8] = ValidationRules.ValueTermsNotSupportedBeforeV3;
			functionImportParametersIncorrectTypeBeforeV3[9] = ValidationRules.VocabularyAnnotationsNotSupportedBeforeV3;
			functionImportParametersIncorrectTypeBeforeV3[10] = ValidationRules.OpenTypesNotSupported;
			functionImportParametersIncorrectTypeBeforeV3[11] = ValidationRules.StreamTypeReferencesNotSupportedBeforeV3;
			functionImportParametersIncorrectTypeBeforeV3[12] = ValidationRules.SpatialTypeReferencesNotSupportedBeforeV3;
			functionImportParametersIncorrectTypeBeforeV3[13] = ValidationRules.ModelDuplicateSchemaElementNameBeforeV3;
			ValidationRuleSet.V2RuleSet = new ValidationRuleSet(ValidationRuleSet.BaseRuleSet, functionImportParametersIncorrectTypeBeforeV3);
			ValidationRule[] modelDuplicateSchemaElementName = new ValidationRule[2];
			modelDuplicateSchemaElementName[0] = ValidationRules.FunctionImportUnsupportedReturnTypeAfterV1;
			modelDuplicateSchemaElementName[1] = ValidationRules.ModelDuplicateSchemaElementName;
			ValidationRuleSet.V3RuleSet = new ValidationRuleSet(ValidationRuleSet.BaseRuleSet, modelDuplicateSchemaElementName);
		}

		public ValidationRuleSet(IEnumerable<ValidationRule> baseSet, IEnumerable<ValidationRule> newRules) : this(EdmUtil.CheckArgumentNull<IEnumerable<ValidationRule>>(baseSet, "baseSet").Concat<ValidationRule>(EdmUtil.CheckArgumentNull<IEnumerable<ValidationRule>>(newRules, "newRules")))
		{
		}

		public ValidationRuleSet(IEnumerable<ValidationRule> rules)
		{
			EdmUtil.CheckArgumentNull<IEnumerable<ValidationRule>>(rules, "rules");
			this.rules = new Dictionary<Type, List<ValidationRule>>();
			foreach (ValidationRule rule in rules)
			{
				this.AddRule(rule);
			}
		}

		private void AddRule(ValidationRule rule)
		{
			List<ValidationRule> validationRules = null;
			if (!this.rules.TryGetValue(rule.ValidatedType, out validationRules))
			{
				validationRules = new List<ValidationRule>();
				this.rules[rule.ValidatedType] = validationRules;
			}
			if (!validationRules.Contains(rule))
			{
				validationRules.Add(rule);
				return;
			}
			else
			{
				throw new InvalidOperationException(Strings.RuleSet_DuplicateRulesExistInRuleSet);
			}
		}

		public static ValidationRuleSet GetEdmModelRuleSet(Version version)
		{
			if (version != EdmConstants.EdmVersion1)
			{
				if (version != EdmConstants.EdmVersion1_1)
				{
					if (version != EdmConstants.EdmVersion1_2)
					{
						if (version != EdmConstants.EdmVersion2)
						{
							if (version != EdmConstants.EdmVersion3)
							{
								throw new InvalidOperationException(Strings.Serializer_UnknownEdmVersion);
							}
							else
							{
								return ValidationRuleSet.V3RuleSet;
							}
						}
						else
						{
							return ValidationRuleSet.V2RuleSet;
						}
					}
					else
					{
						return ValidationRuleSet.V1_2RuleSet;
					}
				}
				else
				{
					return ValidationRuleSet.V1_1RuleSet;
				}
			}
			else
			{
				return ValidationRuleSet.V1RuleSet;
			}
		}

		public IEnumerator<ValidationRule> GetEnumerator()
		{
			foreach (List<ValidationRule> validationRules in this.rules.Values)
			{
				List<ValidationRule>.Enumerator enumerator = validationRules.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						ValidationRule validationRule = enumerator.Current;
						yield return validationRule;
					}
				}
				finally
				{
					enumerator.Dispose();
				}
			}
		}

		internal IEnumerable<ValidationRule> GetRules(Type t)
		{
			List<ValidationRule> validationRules = null;
			if (this.rules.TryGetValue(t, out validationRules))
			{
				return validationRules;
			}
			else
			{
				return Enumerable.Empty<ValidationRule>();
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}