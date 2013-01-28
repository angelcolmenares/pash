using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation.Internal;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Validation
{
	internal static class ValidationRules
	{
		public readonly static ValidationRule<IEdmElement> ElementDirectValueAnnotationFullNameMustBeUnique;

		public readonly static ValidationRule<IEdmNamedElement> NamedElementNameMustNotBeEmptyOrWhiteSpace;

		public readonly static ValidationRule<IEdmNamedElement> NamedElementNameIsTooLong;

		public readonly static ValidationRule<IEdmNamedElement> NamedElementNameIsNotAllowed;

		public readonly static ValidationRule<IEdmSchemaElement> SchemaElementNamespaceMustNotBeEmptyOrWhiteSpace;

		public readonly static ValidationRule<IEdmSchemaElement> SchemaElementNamespaceIsTooLong;

		public readonly static ValidationRule<IEdmSchemaElement> SchemaElementNamespaceIsNotAllowed;

		public readonly static ValidationRule<IEdmSchemaElement> SchemaElementSystemNamespaceEncountered;

		public readonly static ValidationRule<IEdmSchemaElement> SchemaElementMustNotHaveKindOfNone;

		public readonly static ValidationRule<IEdmEntityContainerElement> EntityContainerElementMustNotHaveKindOfNone;

		public readonly static ValidationRule<IEdmEntityContainer> EntityContainerDuplicateEntityContainerMemberName;

		public readonly static ValidationRule<IEdmEntitySet> EntitySetTypeHasNoKeys;

		public readonly static ValidationRule<IEdmEntitySet> EntitySetInaccessibleEntityType;

		public readonly static ValidationRule<IEdmEntitySet> EntitySetCanOnlyBeContainedByASingleNavigationProperty;

		public readonly static ValidationRule<IEdmEntitySet> EntitySetNavigationMappingMustBeBidirectional;

		public readonly static ValidationRule<IEdmEntitySet> EntitySetAssociationSetNameMustBeValid;

		public readonly static ValidationRule<IEdmEntitySet> EntitySetNavigationPropertyMappingsMustBeUnique;

		public readonly static ValidationRule<IEdmEntitySet> EntitySetRecursiveNavigationPropertyMappingsMustPointBackToSourceEntitySet;

		public readonly static ValidationRule<IEdmEntitySet> EntitySetNavigationPropertyMappingMustPointToValidTargetForProperty;

		public readonly static ValidationRule<IEdmStructuredType> StructuredTypeInvalidMemberNameMatchesTypeName;

		public readonly static ValidationRule<IEdmStructuredType> StructuredTypePropertyNameAlreadyDefined;

		public readonly static ValidationRule<IEdmStructuredType> StructuredTypeBaseTypeMustBeSameKindAsDerivedKind;

		public readonly static ValidationRule<IEdmStructuredType> StructuredTypeInaccessibleBaseType;

		public readonly static ValidationRule<IEdmStructuredType> StructuredTypePropertiesDeclaringTypeMustBeCorrect;

		public readonly static ValidationRule<IEdmStructuredType> OpenTypesNotSupported;

		public readonly static ValidationRule<IEdmStructuredType> OnlyEntityTypesCanBeOpen;

		public readonly static ValidationRule<IEdmEnumType> EnumTypeEnumsNotSupportedBeforeV3;

		public readonly static ValidationRule<IEdmEnumType> EnumTypeEnumMemberNameAlreadyDefined;

		public readonly static ValidationRule<IEdmEnumType> EnumMustHaveIntegerUnderlyingType;

		public readonly static ValidationRule<IEdmEnumMember> EnumMemberValueMustHaveSameTypeAsUnderlyingType;

		public readonly static ValidationRule<IEdmEntityType> EntityTypeDuplicatePropertyNameSpecifiedInEntityKey;

		public readonly static ValidationRule<IEdmEntityType> EntityTypeInvalidKeyNullablePart;

		public readonly static ValidationRule<IEdmEntityType> EntityTypeEntityKeyMustBeScalar;

		public readonly static ValidationRule<IEdmEntityType> EntityTypeEntityKeyMustNotBeBinaryBeforeV2;

		public readonly static ValidationRule<IEdmEntityType> EntityTypeInvalidKeyKeyDefinedInBaseClass;

		public readonly static ValidationRule<IEdmEntityType> EntityTypeKeyMissingOnEntityType;

		public readonly static ValidationRule<IEdmEntityType> EntityTypeKeyPropertyMustBelongToEntity;

		public readonly static ValidationRule<IEdmEntityReferenceType> EntityReferenceTypeInaccessibleEntityType;

		public readonly static ValidationRule<IEdmType> TypeMustNotHaveKindOfNone;

		public readonly static ValidationRule<IEdmPrimitiveType> PrimitiveTypeMustNotHaveKindOfNone;

		public readonly static ValidationRule<IEdmComplexType> ComplexTypeInvalidAbstractComplexType;

		public readonly static ValidationRule<IEdmComplexType> ComplexTypeInvalidPolymorphicComplexType;

		public readonly static ValidationRule<IEdmComplexType> ComplexTypeMustContainProperties;

		public readonly static ValidationRule<IEdmRowType> RowTypeBaseTypeMustBeNull;

		public readonly static ValidationRule<IEdmRowType> RowTypeMustContainProperties;

		public readonly static ValidationRule<IEdmStructuralProperty> StructuralPropertyNullableComplexType;

		public readonly static ValidationRule<IEdmStructuralProperty> StructuralPropertyInvalidPropertyType;

		public readonly static ValidationRule<IEdmStructuralProperty> StructuralPropertyInvalidPropertyTypeConcurrencyMode;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyInvalidOperationMultipleEndsInAssociation;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyCorrectType;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyDuplicateDependentProperty;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyPrincipalEndMultiplicity;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyDependentEndMultiplicity;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyDependentPropertiesMustBelongToDependentEntity;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyEndWithManyMultiplicityCannotHaveOperationsSpecified;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyContainsTargetNotSupportedBeforeV3;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyWithRecursiveContainmentTargetMustBeOptional;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyWithRecursiveContainmentSourceMustBeFromZeroOrOne;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyWithNonRecursiveContainmentSourceMustBeFromOne;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyEntityMustNotIndirectlyContainItself;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyTypeMismatchRelationshipConstraint;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyAssociationNameIsValid;

		public readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyAssociationEndNameIsValid;

		public readonly static ValidationRule<IEdmProperty> PropertyMustNotHaveKindOfNone;

		public readonly static ValidationRule<IEdmFunction> FunctionsNotSupportedBeforeV2;

		public readonly static ValidationRule<IEdmFunction> FunctionOnlyInputParametersAllowedInFunctions;

		public readonly static ValidationRule<IEdmFunctionImport> FunctionImportUnsupportedReturnTypeV1;

		public readonly static ValidationRule<IEdmFunctionImport> FunctionImportUnsupportedReturnTypeAfterV1;

		public readonly static ValidationRule<IEdmFunctionImport> FunctionImportReturnEntitiesButDoesNotSpecifyEntitySet;

		public readonly static ValidationRule<IEdmFunctionImport> FunctionImportEntitySetExpressionIsInvalid;

		public readonly static ValidationRule<IEdmFunctionImport> FunctionImportEntityTypeDoesNotMatchEntitySet;

		public readonly static ValidationRule<IEdmFunctionImport> ComposableFunctionImportMustHaveReturnType;

		public readonly static ValidationRule<IEdmFunctionImport> FunctionImportParametersIncorrectTypeBeforeV3;

		public readonly static ValidationRule<IEdmFunctionImport> FunctionImportIsSideEffectingNotSupportedBeforeV3;

		public readonly static ValidationRule<IEdmFunctionImport> FunctionImportIsComposableNotSupportedBeforeV3;

		public readonly static ValidationRule<IEdmFunctionImport> FunctionImportIsBindableNotSupportedBeforeV3;

		public readonly static ValidationRule<IEdmFunctionImport> FunctionImportComposableFunctionImportCannotBeSideEffecting;

		public readonly static ValidationRule<IEdmFunctionImport> FunctionImportBindableFunctionImportMustHaveParameters;

		public readonly static ValidationRule<IEdmFunctionImport> FunctionImportParametersCannotHaveModeOfNone;

		public readonly static ValidationRule<IEdmFunctionBase> FunctionBaseParameterNameAlreadyDefinedDuplicate;

		public readonly static ValidationRule<IEdmTypeReference> TypeReferenceInaccessibleSchemaType;

		public readonly static ValidationRule<IEdmPrimitiveTypeReference> StreamTypeReferencesNotSupportedBeforeV3;

		public readonly static ValidationRule<IEdmPrimitiveTypeReference> SpatialTypeReferencesNotSupportedBeforeV3;

		public readonly static ValidationRule<IEdmDecimalTypeReference> DecimalTypeReferenceScaleOutOfRange;

		public readonly static ValidationRule<IEdmDecimalTypeReference> DecimalTypeReferencePrecisionOutOfRange;

		public readonly static ValidationRule<IEdmStringTypeReference> StringTypeReferenceStringMaxLengthNegative;

		public readonly static ValidationRule<IEdmStringTypeReference> StringTypeReferenceStringUnboundedNotValidForMaxLength;

		public readonly static ValidationRule<IEdmBinaryTypeReference> BinaryTypeReferenceBinaryMaxLengthNegative;

		public readonly static ValidationRule<IEdmBinaryTypeReference> BinaryTypeReferenceBinaryUnboundedNotValidForMaxLength;

		public readonly static ValidationRule<IEdmTemporalTypeReference> TemporalTypeReferencePrecisionOutOfRange;

		public readonly static ValidationRule<IEdmModel> ModelDuplicateSchemaElementNameBeforeV3;

		public readonly static ValidationRule<IEdmModel> ModelDuplicateSchemaElementName;

		public readonly static ValidationRule<IEdmModel> ModelDuplicateEntityContainerName;

		public readonly static ValidationRule<IEdmDirectValueAnnotation> ImmediateValueAnnotationElementAnnotationIsValid;

		public readonly static ValidationRule<IEdmDirectValueAnnotation> ImmediateValueAnnotationElementAnnotationHasNameAndNamespace;

		public readonly static ValidationRule<IEdmVocabularyAnnotation> VocabularyAnnotationsNotSupportedBeforeV3;

		public readonly static ValidationRule<IEdmVocabularyAnnotation> VocabularyAnnotationInaccessibleTarget;

		public readonly static ValidationRule<IEdmValueAnnotation> ValueAnnotationAssertCorrectExpressionType;

		public readonly static ValidationRule<IEdmValueAnnotation> ValueAnnotationInaccessibleTerm;

		public readonly static ValidationRule<IEdmTypeAnnotation> TypeAnnotationInaccessibleTerm;

		public readonly static ValidationRule<IEdmTypeAnnotation> TypeAnnotationAssertMatchesTermType;

		public readonly static ValidationRule<IEdmPropertyValueBinding> PropertyValueBindingValueIsCorrectType;

		public readonly static ValidationRule<IEdmValueTerm> ValueTermsNotSupportedBeforeV3;

		public readonly static ValidationRule<IEdmTerm> TermMustNotHaveKindOfNone;

		public readonly static ValidationRule<IEdmIfExpression> IfExpressionAssertCorrectTestType;

		public readonly static ValidationRule<IEdmCollectionExpression> CollectionExpressionAllElementsCorrectType;

		public readonly static ValidationRule<IEdmRecordExpression> RecordExpressionPropertiesMatchType;

		public readonly static ValidationRule<IEdmApplyExpression> FunctionApplicationExpressionParametersMatchAppliedFunction;

		public readonly static ValidationRule<IEdmVocabularyAnnotatable> VocabularyAnnotatableNoDuplicateAnnotations;

		public readonly static ValidationRule<IEdmPrimitiveValue> PrimitiveValueValidForType;

		static ValidationRules()
		{
			ValidationRules.ElementDirectValueAnnotationFullNameMustBeUnique = new ValidationRule<IEdmElement>((ValidationContext context, IEdmElement item) => {
				HashSetInternal<string> strs = new HashSetInternal<string>();
				foreach (IEdmDirectValueAnnotation directValueAnnotation in context.Model.DirectValueAnnotationsManager.GetDirectValueAnnotations(item))
				{
					if (strs.Add(string.Concat(directValueAnnotation.NamespaceUri, ":", directValueAnnotation.Name)))
					{
						continue;
					}
					context.AddError(directValueAnnotation.Location(), EdmErrorCode.DuplicateDirectValueAnnotationFullName, Strings.EdmModel_Validator_Semantic_ElementDirectValueAnnotationFullNameMustBeUnique(directValueAnnotation.NamespaceUri, directValueAnnotation.Name));
				}
			}
			);
			ValidationRules.NamedElementNameMustNotBeEmptyOrWhiteSpace = new ValidationRule<IEdmNamedElement>((ValidationContext context, IEdmNamedElement item) => {
				if (EdmUtil.IsNullOrWhiteSpaceInternal(item.Name) || item.Name.Length == 0)
				{
					context.AddError(item.Location(), EdmErrorCode.InvalidName, Strings.EdmModel_Validator_Syntactic_MissingName);
				}
			}
			);
			ValidationRules.NamedElementNameIsTooLong = new ValidationRule<IEdmNamedElement>((ValidationContext context, IEdmNamedElement item) => {
				if (!EdmUtil.IsNullOrWhiteSpaceInternal(item.Name) && item.Name.Length > 0x1e0)
				{
					context.AddError(item.Location(), EdmErrorCode.NameTooLong, Strings.EdmModel_Validator_Syntactic_EdmModel_NameIsTooLong(item.Name));
				}
			}
			);
			ValidationRules.NamedElementNameIsNotAllowed = new ValidationRule<IEdmNamedElement>((ValidationContext context, IEdmNamedElement item) => {
				if (!EdmUtil.IsNullOrWhiteSpaceInternal(item.Name) && item.Name.Length <= 0x1e0 && item.Name.Length > 0 && !EdmUtil.IsValidUndottedName(item.Name))
				{
					context.AddError(item.Location(), EdmErrorCode.InvalidName, Strings.EdmModel_Validator_Syntactic_EdmModel_NameIsNotAllowed(item.Name));
				}
			}
			);
			ValidationRules.SchemaElementNamespaceMustNotBeEmptyOrWhiteSpace = new ValidationRule<IEdmSchemaElement>((ValidationContext context, IEdmSchemaElement item) => {
				if (EdmUtil.IsNullOrWhiteSpaceInternal(item.Namespace) || item.Namespace.Length == 0)
				{
					context.AddError(item.Location(), EdmErrorCode.InvalidNamespaceName, Strings.EdmModel_Validator_Syntactic_MissingNamespaceName);
				}
			}
			);
			ValidationRules.SchemaElementNamespaceIsTooLong = new ValidationRule<IEdmSchemaElement>((ValidationContext context, IEdmSchemaElement item) => {
				if (item.Namespace.Length > 0x200)
				{
					context.AddError(item.Location(), EdmErrorCode.InvalidNamespaceName, Strings.EdmModel_Validator_Syntactic_EdmModel_NamespaceNameIsTooLong(item.Namespace));
				}
			}
			);
			ValidationRules.SchemaElementNamespaceIsNotAllowed = new ValidationRule<IEdmSchemaElement>((ValidationContext context, IEdmSchemaElement item) => {
				if (item.Namespace.Length <= 0x200 && item.Namespace.Length > 0 && !EdmUtil.IsNullOrWhiteSpaceInternal(item.Namespace) && !EdmUtil.IsValidDottedName(item.Namespace))
				{
					context.AddError(item.Location(), EdmErrorCode.InvalidNamespaceName, Strings.EdmModel_Validator_Syntactic_EdmModel_NamespaceNameIsNotAllowed(item.Namespace));
				}
			}
			);
			ValidationRules.SchemaElementSystemNamespaceEncountered = new ValidationRule<IEdmSchemaElement>((ValidationContext context, IEdmSchemaElement element) => {
				if (ValidationHelper.IsEdmSystemNamespace(element.Namespace))
				{
					context.AddError(element.Location(), EdmErrorCode.SystemNamespaceEncountered, Strings.EdmModel_Validator_Semantic_SystemNamespaceEncountered(element.Namespace));
				}
			}
			);
			ValidationRules.SchemaElementMustNotHaveKindOfNone = new ValidationRule<IEdmSchemaElement>((ValidationContext context, IEdmSchemaElement element) => {
				if (element.SchemaElementKind == EdmSchemaElementKind.None && !context.IsBad(element))
				{
					context.AddError(element.Location(), EdmErrorCode.SchemaElementMustNotHaveKindOfNone, Strings.EdmModel_Validator_Semantic_SchemaElementMustNotHaveKindOfNone(element.FullName()));
				}
			}
			);
			ValidationRules.EntityContainerElementMustNotHaveKindOfNone = new ValidationRule<IEdmEntityContainerElement>((ValidationContext context, IEdmEntityContainerElement element) => {
				if (element.ContainerElementKind == EdmContainerElementKind.None && !context.IsBad(element))
				{
					context.AddError(element.Location(), EdmErrorCode.EntityContainerElementMustNotHaveKindOfNone, Strings.EdmModel_Validator_Semantic_EntityContainerElementMustNotHaveKindOfNone(string.Concat(element.Container.FullName(), (char)47, element.Name)));
				}
			}
			);
			ValidationRules.EntityContainerDuplicateEntityContainerMemberName = new ValidationRule<IEdmEntityContainer>((ValidationContext context, IEdmEntityContainer entityContainer) => {
				List<IEdmFunctionImport> edmFunctionImports = null;
				HashSetInternal<string> strs = new HashSetInternal<string>();
				Dictionary<string, List<IEdmFunctionImport>> strs1 = new Dictionary<string, List<IEdmFunctionImport>>();
				foreach (IEdmEntityContainerElement edmEntityContainerElement in entityContainer.Elements)
				{
					IEdmFunctionImport edmFunctionImport = edmEntityContainerElement as IEdmFunctionImport;
					if (edmFunctionImport == null)
					{
						if (!ValidationHelper.AddMemberNameToHashSet(edmEntityContainerElement, strs, context, EdmErrorCode.DuplicateEntityContainerMemberName, Strings.EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName(edmEntityContainerElement.Name), false) || !strs1.ContainsKey(edmEntityContainerElement.Name))
						{
							continue;
						}
						context.AddError(edmEntityContainerElement.Location(), EdmErrorCode.DuplicateEntityContainerMemberName, Strings.EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName(edmEntityContainerElement.Name));
					}
					else
					{
						if (strs.Contains(edmEntityContainerElement.Name))
						{
							context.AddError(edmEntityContainerElement.Location(), EdmErrorCode.DuplicateEntityContainerMemberName, Strings.EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName(edmEntityContainerElement.Name));
						}
						if (!strs1.TryGetValue(edmFunctionImport.Name, out edmFunctionImports))
						{
							edmFunctionImports = new List<IEdmFunctionImport>();
						}
						else
						{
							foreach (IEdmFunctionImport edmFunctionImport1 in edmFunctionImports)
							{
								if (!edmFunctionImport.IsFunctionSignatureEquivalentTo(edmFunctionImport1))
								{
									continue;
								}
								context.AddError(edmEntityContainerElement.Location(), EdmErrorCode.DuplicateEntityContainerMemberName, Strings.EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName(edmEntityContainerElement.Name));
								break;
							}
						}
						edmFunctionImports.Add(edmFunctionImport);
					}
				}
			}
			);
			ValidationRules.EntitySetTypeHasNoKeys = new ValidationRule<IEdmEntitySet>((ValidationContext context, IEdmEntitySet entitySet) => {
				if ((entitySet.ElementType.Key() == null || entitySet.ElementType.Key().Count<IEdmStructuralProperty>() == 0) && !context.IsBad(entitySet.ElementType))
				{
					string str = Strings.EdmModel_Validator_Semantic_EntitySetTypeHasNoKeys(entitySet.Name, entitySet.ElementType.Name);
					context.AddError(entitySet.Location(), EdmErrorCode.EntitySetTypeHasNoKeys, str);
				}
			}
			);
			ValidationRules.EntitySetInaccessibleEntityType = new ValidationRule<IEdmEntitySet>((ValidationContext context, IEdmEntitySet entitySet) => {
				if (!context.IsBad(entitySet.ElementType))
				{
					ValidationRules.CheckForUnreacheableTypeError(context, entitySet.ElementType, entitySet.Location());
				}
			}
			);
			ValidationRules.EntitySetCanOnlyBeContainedByASingleNavigationProperty = new ValidationRule<IEdmEntitySet>((ValidationContext context, IEdmEntitySet set) => {
				bool flag = false;
			
				foreach (IEdmEntitySet navigationTarget in set.NavigationTargets)
				{
					IEnumerator<IEdmNavigationTargetMapping> enumerator = navigationTarget.NavigationTargets.GetEnumerator();
					using (enumerator)
					{
						while (enumerator.MoveNext())
						{
							IEdmNavigationTargetMapping edmNavigationTargetMapping = enumerator.Current;
							IEdmNavigationProperty edmNavigationProperty = edmNavigationTargetMapping.NavigationProperty;
							if (edmNavigationTargetMapping.TargetEntitySet != set || !edmNavigationProperty.ContainsTarget)
							{
								continue;
							}
							if (flag)
							{
								context.AddError(set.Location(), EdmErrorCode.EntitySetCanOnlyBeContainedByASingleNavigationProperty, Strings.EdmModel_Validator_Semantic_EntitySetCanOnlyBeContainedByASingleNavigationProperty(string.Concat(set.Container.FullName(), ".", set.Name)));
							}
							flag = true;
						}
					}
				}
			}
			);
			ValidationRules.EntitySetNavigationMappingMustBeBidirectional = new ValidationRule<IEdmEntitySet>((ValidationContext context, IEdmEntitySet set) => {
				foreach (IEdmNavigationTargetMapping navigationTarget in set.NavigationTargets)
				{
					IEdmNavigationProperty edmNavigationProperty1 = navigationTarget.NavigationProperty;
					IEdmEntitySet edmEntitySet = navigationTarget.TargetEntitySet.FindNavigationTarget(edmNavigationProperty1.Partner);
					if (edmEntitySet == null && edmNavigationProperty1.Partner.DeclaringEntityType().FindProperty(edmNavigationProperty1.Partner.Name) != edmNavigationProperty1.Partner || edmEntitySet == set)
					{
						continue;
					}
					context.AddError(set.Location(), EdmErrorCode.EntitySetNavigationMappingMustBeBidirectional, Strings.EdmModel_Validator_Semantic_EntitySetNavigationMappingMustBeBidirectional(string.Concat(set.Container.FullName(), ".", set.Name), edmNavigationProperty1.Name));
				}
			}
			);
			ValidationRules.EntitySetAssociationSetNameMustBeValid = new ValidationRule<IEdmEntitySet>((ValidationContext context, IEdmEntitySet set) => {
				foreach (IEdmNavigationTargetMapping navigationTarget in set.NavigationTargets)
				{
					if (navigationTarget.NavigationProperty.GetPrimary() != navigationTarget.NavigationProperty)
					{
						continue;
					}
					ValidationRules.CheckForNameError(context, context.Model.GetAssociationSetName(set, navigationTarget.NavigationProperty), set.Location());
				}
			}
			);
			ValidationRules.EntitySetNavigationPropertyMappingsMustBeUnique = new ValidationRule<IEdmEntitySet>((ValidationContext context, IEdmEntitySet set) => {
				foreach (IEdmNavigationTargetMapping navigationTarget in set.NavigationTargets)
				{
					HashSetInternal<IEdmNavigationProperty> edmNavigationProperties = new HashSetInternal<IEdmNavigationProperty>();
					if (edmNavigationProperties.Add(navigationTarget.NavigationProperty))
					{
						continue;
					}
					context.AddError(set.Location(), EdmErrorCode.DuplicateNavigationPropertyMapping, Strings.EdmModel_Validator_Semantic_DuplicateNavigationPropertyMapping(string.Concat(set.Container.FullName(), ".", set.Name), navigationTarget.NavigationProperty.Name));
				}
			}
			);
			ValidationRules.EntitySetRecursiveNavigationPropertyMappingsMustPointBackToSourceEntitySet = new ValidationRule<IEdmEntitySet>((ValidationContext context, IEdmEntitySet set) => {
				foreach (IEdmNavigationTargetMapping navigationTarget in set.NavigationTargets)
				{
					if (!navigationTarget.NavigationProperty.ContainsTarget || !navigationTarget.NavigationProperty.DeclaringType.IsOrInheritsFrom(navigationTarget.NavigationProperty.ToEntityType()) || navigationTarget.TargetEntitySet == set)
					{
						continue;
					}
					context.AddError(set.Location(), EdmErrorCode.EntitySetRecursiveNavigationPropertyMappingsMustPointBackToSourceEntitySet, Strings.EdmModel_Validator_Semantic_EntitySetRecursiveNavigationPropertyMappingsMustPointBackToSourceEntitySet(navigationTarget.NavigationProperty, set.Name));
				}
			}
			);
			ValidationRules.EntitySetNavigationPropertyMappingMustPointToValidTargetForProperty = new ValidationRule<IEdmEntitySet>((ValidationContext context, IEdmEntitySet set) => {
				foreach (IEdmNavigationTargetMapping navigationTarget in set.NavigationTargets)
				{
					if (navigationTarget.TargetEntitySet.ElementType.IsOrInheritsFrom(navigationTarget.NavigationProperty.ToEntityType()) || navigationTarget.NavigationProperty.ToEntityType().IsOrInheritsFrom(navigationTarget.TargetEntitySet.ElementType) || context.IsBad(navigationTarget.TargetEntitySet))
					{
						continue;
					}
					context.AddError(set.Location(), EdmErrorCode.EntitySetNavigationPropertyMappingMustPointToValidTargetForProperty, Strings.EdmModel_Validator_Semantic_EntitySetNavigationPropertyMappingMustPointToValidTargetForProperty(navigationTarget.NavigationProperty.Name, navigationTarget.TargetEntitySet.Name));
				}
			}
			);
			ValidationRules.StructuredTypeInvalidMemberNameMatchesTypeName = new ValidationRule<IEdmStructuredType>((ValidationContext context, IEdmStructuredType structuredType) => {
				IEdmSchemaType edmSchemaType = structuredType as IEdmSchemaType;
				if (edmSchemaType != null)
				{
					List<IEdmProperty> list = structuredType.Properties().ToList<IEdmProperty>();
					if (list.Count > 0)
					{
						foreach (IEdmProperty edmProperty in list)
						{
							if (edmProperty == null || !edmProperty.Name.EqualsOrdinal(edmSchemaType.Name))
							{
								continue;
							}
							context.AddError(edmProperty.Location(), EdmErrorCode.BadProperty, Strings.EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName(edmProperty.Name));
						}
					}
				}
			}
			);
			ValidationRules.StructuredTypePropertyNameAlreadyDefined = new ValidationRule<IEdmStructuredType>((ValidationContext context, IEdmStructuredType structuredType) => {
				HashSetInternal<string> strs = new HashSetInternal<string>();
				foreach (IEdmProperty edmProperty in structuredType.Properties())
				{
					if (edmProperty == null)
					{
						continue;
					}
					ValidationHelper.AddMemberNameToHashSet(edmProperty, strs, context, EdmErrorCode.AlreadyDefined, Strings.EdmModel_Validator_Semantic_PropertyNameAlreadyDefined(edmProperty.Name), !structuredType.DeclaredProperties.Contains<IEdmProperty>(edmProperty));
				}
			}
			);
			ValidationRules.StructuredTypeBaseTypeMustBeSameKindAsDerivedKind = new ValidationRule<IEdmStructuredType>((ValidationContext context, IEdmStructuredType structuredType) => {
				EdmErrorCode edmErrorCode;
				if (structuredType as IEdmSchemaType != null && structuredType.BaseType != null && structuredType.BaseType.TypeKind != structuredType.TypeKind)
				{
					ValidationContext validationContext = context;
					EdmLocation edmLocation = structuredType.Location();
					if (structuredType.TypeKind == EdmTypeKind.Entity)
					{
						edmErrorCode = EdmErrorCode.EntityMustHaveEntityBaseType;
					}
					else
					{
						edmErrorCode = EdmErrorCode.ComplexTypeMustHaveComplexBaseType;
					}
					validationContext.AddError(edmLocation, edmErrorCode, Strings.EdmModel_Validator_Semantic_BaseTypeMustHaveSameTypeKind);
				}
			}
			);
			ValidationRules.StructuredTypeInaccessibleBaseType = new ValidationRule<IEdmStructuredType>((ValidationContext context, IEdmStructuredType structuredType) => {
				IEdmSchemaType baseType = structuredType.BaseType as IEdmSchemaType;
				if (baseType != null && !context.IsBad(baseType))
				{
					ValidationRules.CheckForUnreacheableTypeError(context, baseType, structuredType.Location());
				}
			}
			);
			ValidationRules.StructuredTypePropertiesDeclaringTypeMustBeCorrect = new ValidationRule<IEdmStructuredType>((ValidationContext context, IEdmStructuredType structuredType) => {
				foreach (IEdmProperty declaredProperty in structuredType.DeclaredProperties)
				{
					if (declaredProperty == null || declaredProperty.DeclaringType.Equals(structuredType))
					{
						continue;
					}
					context.AddError(declaredProperty.Location(), EdmErrorCode.DeclaringTypeMustBeCorrect, Strings.EdmModel_Validator_Semantic_DeclaringTypeMustBeCorrect(declaredProperty.Name));
				}
			}
			);
			ValidationRules.OpenTypesNotSupported = new ValidationRule<IEdmStructuredType>((ValidationContext context, IEdmStructuredType structuredType) => {
				if (structuredType.IsOpen)
				{
					context.AddError(structuredType.Location(), EdmErrorCode.OpenTypeNotSupported, Strings.EdmModel_Validator_Semantic_OpenTypesSupportedOnlyInV12AndAfterV3);
				}
			}
			);
			ValidationRules.OnlyEntityTypesCanBeOpen = new ValidationRule<IEdmStructuredType>((ValidationContext context, IEdmStructuredType structuredType) => {
				if (structuredType.IsOpen && structuredType.TypeKind != EdmTypeKind.Entity)
				{
					context.AddError(structuredType.Location(), EdmErrorCode.OpenTypeNotSupported, Strings.EdmModel_Validator_Semantic_OpenTypesSupportedForEntityTypesOnly);
				}
			}
			);
			ValidationRules.EnumTypeEnumsNotSupportedBeforeV3 = new ValidationRule<IEdmEnumType>((ValidationContext context, IEdmEnumType enumType) => context.AddError(enumType.Location(), EdmErrorCode.EnumsNotSupportedBeforeV3, Strings.EdmModel_Validator_Semantic_EnumsNotSupportedBeforeV3));
			ValidationRules.EnumTypeEnumMemberNameAlreadyDefined = new ValidationRule<IEdmEnumType>((ValidationContext context, IEdmEnumType enumType) => {
				HashSetInternal<string> strs = new HashSetInternal<string>();
				foreach (IEdmEnumMember member in enumType.Members)
				{
					if (member == null)
					{
						continue;
					}
					ValidationHelper.AddMemberNameToHashSet(member, strs, context, EdmErrorCode.AlreadyDefined, Strings.EdmModel_Validator_Semantic_EnumMemberNameAlreadyDefined(member.Name), false);
				}
			}
			);
			ValidationRules.EnumMustHaveIntegerUnderlyingType = new ValidationRule<IEdmEnumType>((ValidationContext context, IEdmEnumType enumType) => {
				if (!enumType.UnderlyingType.PrimitiveKind.IsIntegral() && !context.IsBad(enumType.UnderlyingType))
				{
					context.AddError(enumType.Location(), EdmErrorCode.EnumMustHaveIntegerUnderlyingType, Strings.EdmModel_Validator_Semantic_EnumMustHaveIntegralUnderlyingType(enumType.FullName()));
				}
			}
			);
			ValidationRules.EnumMemberValueMustHaveSameTypeAsUnderlyingType = new ValidationRule<IEdmEnumMember>((ValidationContext context, IEdmEnumMember enumMember) => {
				IEnumerable<EdmError> edmErrors = null;
				if (!context.IsBad(enumMember.DeclaringType) && !context.IsBad(enumMember.DeclaringType.UnderlyingType) && !enumMember.Value.TryAssertPrimitiveAsType(enumMember.DeclaringType.UnderlyingType.GetPrimitiveTypeReference(false), out edmErrors))
				{
					context.AddError(enumMember.Location(), EdmErrorCode.EnumMemberTypeMustMatchEnumUnderlyingType, Strings.EdmModel_Validator_Semantic_EnumMemberTypeMustMatchEnumUnderlyingType(enumMember.Name));
				}
			}
			);
			ValidationRules.EntityTypeDuplicatePropertyNameSpecifiedInEntityKey = new ValidationRule<IEdmEntityType>((ValidationContext context, IEdmEntityType entityType) => {
				if (entityType.DeclaredKey != null)
				{
					HashSetInternal<string> strs = new HashSetInternal<string>();
					foreach (IEdmStructuralProperty declaredKey in entityType.DeclaredKey)
					{
						ValidationHelper.AddMemberNameToHashSet(declaredKey, strs, context, EdmErrorCode.DuplicatePropertySpecifiedInEntityKey, Strings.EdmModel_Validator_Semantic_DuplicatePropertyNameSpecifiedInEntityKey(entityType.Name, declaredKey.Name), false);
					}
				}
			}
			);
			ValidationRules.EntityTypeInvalidKeyNullablePart = new ValidationRule<IEdmEntityType>((ValidationContext context, IEdmEntityType entityType) => {
				if (entityType.Key() != null)
				{
					foreach (IEdmStructuralProperty edmStructuralProperty in entityType.Key())
					{
						if (!edmStructuralProperty.Type.IsPrimitive() || !edmStructuralProperty.Type.IsNullable)
						{
							continue;
						}
						context.AddError(edmStructuralProperty.Location(), EdmErrorCode.InvalidKey, Strings.EdmModel_Validator_Semantic_InvalidKeyNullablePart(edmStructuralProperty.Name, entityType.Name));
					}
				}
			}
			);
			ValidationRules.EntityTypeEntityKeyMustBeScalar = new ValidationRule<IEdmEntityType>((ValidationContext context, IEdmEntityType entityType) => {
				if (entityType.Key() != null)
				{
					foreach (IEdmStructuralProperty edmStructuralProperty in entityType.Key())
					{
						if (edmStructuralProperty.Type.IsPrimitive() || context.IsBad(edmStructuralProperty))
						{
							continue;
						}
						context.AddError(edmStructuralProperty.Location(), EdmErrorCode.EntityKeyMustBeScalar, Strings.EdmModel_Validator_Semantic_EntityKeyMustBeScalar(edmStructuralProperty.Name, entityType.Name));
					}
				}
			}
			);
			ValidationRules.EntityTypeEntityKeyMustNotBeBinaryBeforeV2 = new ValidationRule<IEdmEntityType>((ValidationContext context, IEdmEntityType entityType) => {
				if (entityType.Key() != null)
				{
					foreach (IEdmStructuralProperty edmStructuralProperty in entityType.Key())
					{
						if (!edmStructuralProperty.Type.IsBinary() || context.IsBad(edmStructuralProperty.Type.Definition))
						{
							continue;
						}
						context.AddError(edmStructuralProperty.Location(), EdmErrorCode.EntityKeyMustNotBeBinary, Strings.EdmModel_Validator_Semantic_EntityKeyMustNotBeBinaryBeforeV2(edmStructuralProperty.Name, entityType.Name));
					}
				}
			}
			);
			ValidationRules.EntityTypeInvalidKeyKeyDefinedInBaseClass = new ValidationRule<IEdmEntityType>((ValidationContext context, IEdmEntityType entityType) => {
				if (entityType.BaseType != null && entityType.DeclaredKey != null && entityType.BaseType.TypeKind == EdmTypeKind.Entity && entityType.BaseEntityType().DeclaredKey != null)
				{
					context.AddError(entityType.Location(), EdmErrorCode.InvalidKey, Strings.EdmModel_Validator_Semantic_InvalidKeyKeyDefinedInBaseClass(entityType.Name, entityType.BaseEntityType().Name));
				}
			}
			);
			ValidationRules.EntityTypeKeyMissingOnEntityType = new ValidationRule<IEdmEntityType>((ValidationContext context, IEdmEntityType entityType) => {
				if ((entityType.Key() == null || entityType.Key().Count<IEdmStructuralProperty>() == 0) && entityType.BaseType == null)
				{
					context.AddError(entityType.Location(), EdmErrorCode.KeyMissingOnEntityType, Strings.EdmModel_Validator_Semantic_KeyMissingOnEntityType(entityType.Name));
				}
			}
			);
			ValidationRules.EntityTypeKeyPropertyMustBelongToEntity = new ValidationRule<IEdmEntityType>((ValidationContext context, IEdmEntityType entityType) => {
				if (entityType.DeclaredKey != null)
				{
					foreach (IEdmStructuralProperty declaredKey in entityType.DeclaredKey)
					{
						if (declaredKey.DeclaringType == entityType || context.IsBad(declaredKey))
						{
							continue;
						}
						context.AddError(entityType.Location(), EdmErrorCode.KeyPropertyMustBelongToEntity, Strings.EdmModel_Validator_Semantic_KeyPropertyMustBelongToEntity(declaredKey.Name, entityType.Name));
					}
				}
			}
			);
			ValidationRules.EntityReferenceTypeInaccessibleEntityType = new ValidationRule<IEdmEntityReferenceType>((ValidationContext context, IEdmEntityReferenceType entityReferenceType) => {
				if (!context.IsBad(entityReferenceType.EntityType))
				{
					ValidationRules.CheckForUnreacheableTypeError(context, entityReferenceType.EntityType, entityReferenceType.Location());
				}
			}
			);
			ValidationRules.TypeMustNotHaveKindOfNone = new ValidationRule<IEdmType>((ValidationContext context, IEdmType type) => {
				if (type.TypeKind == EdmTypeKind.None && !context.IsBad(type))
				{
					context.AddError(type.Location(), EdmErrorCode.TypeMustNotHaveKindOfNone, Strings.EdmModel_Validator_Semantic_TypeMustNotHaveKindOfNone);
				}
			}
			);
			ValidationRules.PrimitiveTypeMustNotHaveKindOfNone = new ValidationRule<IEdmPrimitiveType>((ValidationContext context, IEdmPrimitiveType type) => {
				if (type.PrimitiveKind == EdmPrimitiveTypeKind.None && !context.IsBad(type))
				{
					context.AddError(type.Location(), EdmErrorCode.PrimitiveTypeMustNotHaveKindOfNone, Strings.EdmModel_Validator_Semantic_PrimitiveTypeMustNotHaveKindOfNone(type.FullName()));
				}
			}
			);
			ValidationRules.ComplexTypeInvalidAbstractComplexType = new ValidationRule<IEdmComplexType>((ValidationContext context, IEdmComplexType complexType) => {
				if (complexType.IsAbstract)
				{
					context.AddError(complexType.Location(), EdmErrorCode.InvalidAbstractComplexType, Strings.EdmModel_Validator_Semantic_InvalidComplexTypeAbstract(complexType.FullName()));
				}
			}
			);
			ValidationRules.ComplexTypeInvalidPolymorphicComplexType = new ValidationRule<IEdmComplexType>((ValidationContext context, IEdmComplexType edmComplexType) => {
				if (edmComplexType.BaseType != null)
				{
					context.AddError(edmComplexType.Location(), EdmErrorCode.InvalidPolymorphicComplexType, Strings.EdmModel_Validator_Semantic_InvalidComplexTypePolymorphic(edmComplexType.FullName()));
				}
			}
			);
			ValidationRules.ComplexTypeMustContainProperties = new ValidationRule<IEdmComplexType>((ValidationContext context, IEdmComplexType complexType) => {
				if (!complexType.Properties().Any<IEdmProperty>())
				{
					context.AddError(complexType.Location(), EdmErrorCode.ComplexTypeMustHaveProperties, Strings.EdmModel_Validator_Semantic_ComplexTypeMustHaveProperties(complexType.FullName()));
				}
			}
			);
			ValidationRules.RowTypeBaseTypeMustBeNull = new ValidationRule<IEdmRowType>((ValidationContext context, IEdmRowType rowType) => {
				if (rowType.BaseType != null)
				{
					context.AddError(rowType.Location(), EdmErrorCode.RowTypeMustNotHaveBaseType, Strings.EdmModel_Validator_Semantic_RowTypeMustNotHaveBaseType);
				}
			}
			);
			ValidationRules.RowTypeMustContainProperties = new ValidationRule<IEdmRowType>((ValidationContext context, IEdmRowType rowType) => {
				if (!rowType.Properties().Any<IEdmProperty>())
				{
					context.AddError(rowType.Location(), EdmErrorCode.RowTypeMustHaveProperties, Strings.EdmModel_Validator_Semantic_RowTypeMustHaveProperties);
				}
			}
			);
			ValidationRules.StructuralPropertyNullableComplexType = new ValidationRule<IEdmStructuralProperty>((ValidationContext context, IEdmStructuralProperty property) => {
				if (property.Type.IsComplex() && property.Type.IsNullable)
				{
					context.AddError(property.Location(), EdmErrorCode.NullableComplexTypeProperty, Strings.EdmModel_Validator_Semantic_NullableComplexTypeProperty(property.Name));
				}
			}
			);
			ValidationRules.StructuralPropertyInvalidPropertyType = new ValidationRule<IEdmStructuralProperty>((ValidationContext context, IEdmStructuralProperty property) => {
				IEdmType definition;
				if (property.DeclaringType.TypeKind != EdmTypeKind.Row)
				{
					if (!property.Type.IsCollection())
					{
						definition = property.Type.Definition;
					}
					else
					{
						definition = property.Type.AsCollection().ElementType().Definition;
					}
					if (definition.TypeKind != EdmTypeKind.Primitive && definition.TypeKind != EdmTypeKind.Enum && definition.TypeKind != EdmTypeKind.Complex && !context.IsBad(definition))
					{
						context.AddError(property.Location(), EdmErrorCode.InvalidPropertyType, Strings.EdmModel_Validator_Semantic_InvalidPropertyType(property.Type.TypeKind().ToString()));
					}
				}
			}
			);
			ValidationRules.StructuralPropertyInvalidPropertyTypeConcurrencyMode = new ValidationRule<IEdmStructuralProperty>((ValidationContext context, IEdmStructuralProperty property) => {
				object str;
				if (property.ConcurrencyMode == EdmConcurrencyMode.Fixed && !property.Type.IsPrimitive() && !context.IsBad(property.Type.Definition))
				{
					ValidationContext validationContext = context;
					EdmLocation edmLocation = property.Location();
					int num = 44;
					if (property.Type.IsCollection())
					{
						str = "Collection";
					}
					else
					{
						str = property.Type.TypeKind().ToString();
					}
					validationContext.AddError(edmLocation, (EdmErrorCode)num, Strings.EdmModel_Validator_Semantic_InvalidPropertyTypeConcurrencyMode(str));
				}
			}
			);
			ValidationRules.NavigationPropertyInvalidOperationMultipleEndsInAssociation = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty navigationProperty) => {
				if (navigationProperty.OnDelete != EdmOnDeleteAction.None && navigationProperty.Partner.OnDelete != EdmOnDeleteAction.None)
				{
					context.AddError(navigationProperty.Location(), EdmErrorCode.InvalidAction, Strings.EdmModel_Validator_Semantic_InvalidOperationMultipleEndsInAssociation);
				}
			}
			);
			ValidationRules.NavigationPropertyCorrectType = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty property) => {
				bool flag = false;
				if (property.ToEntityType() == property.Partner.DeclaringEntityType())
				{
					EdmMultiplicity edmMultiplicity = property.Partner.Multiplicity();
					switch (edmMultiplicity)
					{
						case EdmMultiplicity.ZeroOrOne:
						{
							if (!property.Type.IsCollection() && property.Type.IsNullable)
							{
								break;
							}
							flag = true;
							break;
						}
						case EdmMultiplicity.One:
						{
							if (!property.Type.IsCollection() && !property.Type.IsNullable)
							{
								break;
							}
							flag = true;
							break;
						}
						case EdmMultiplicity.Many:
						{
							if (property.Type.IsCollection())
							{
								break;
							}
							flag = true;
							break;
						}
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					context.AddError(property.Location(), EdmErrorCode.InvalidNavigationPropertyType, Strings.EdmModel_Validator_Semantic_InvalidNavigationPropertyType(property.Name));
				}
			}
			);
			ValidationRules.NavigationPropertyDuplicateDependentProperty = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty navigationProperty) => {
				IEnumerable<IEdmStructuralProperty> dependentProperties = navigationProperty.DependentProperties;
				if (dependentProperties != null)
				{
					HashSetInternal<string> strs = new HashSetInternal<string>();
					foreach (IEdmStructuralProperty dependentProperty in navigationProperty.DependentProperties)
					{
						if (dependentProperty == null)
						{
							continue;
						}
						ValidationHelper.AddMemberNameToHashSet(dependentProperty, strs, context, EdmErrorCode.DuplicateDependentProperty, Strings.EdmModel_Validator_Semantic_DuplicateDependentProperty(dependentProperty.Name, navigationProperty.Name), false);
					}
				}
			}
			);
			ValidationRules.NavigationPropertyPrincipalEndMultiplicity = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty navigationProperty) => {
				IEnumerable<IEdmStructuralProperty> dependentProperties = navigationProperty.DependentProperties;
				if (dependentProperties != null)
				{
					if (!ValidationHelper.AllPropertiesAreNullable(dependentProperties))
					{
						if (ValidationHelper.HasNullableProperty(dependentProperties))
						{
							if (navigationProperty.Partner.Multiplicity() != EdmMultiplicity.One && navigationProperty.Partner.Multiplicity() != EdmMultiplicity.ZeroOrOne)
							{
								context.AddError(navigationProperty.Partner.Location(), EdmErrorCode.InvalidMultiplicityOfPrincipalEnd, Strings.EdmModel_Validator_Semantic_NavigationPropertyPrincipalEndMultiplicityUpperBoundMustBeOne(navigationProperty.Name));
							}
						}
						else
						{
							if (navigationProperty.Partner.Multiplicity() != EdmMultiplicity.One)
							{
								context.AddError(navigationProperty.Partner.Location(), EdmErrorCode.InvalidMultiplicityOfPrincipalEnd, Strings.EdmModel_Validator_Semantic_InvalidMultiplicityOfPrincipalEndDependentPropertiesAllNonnullable(navigationProperty.Partner.Name, navigationProperty.Name));
								return;
							}
						}
					}
					else
					{
						if (navigationProperty.Partner.Multiplicity() != EdmMultiplicity.ZeroOrOne)
						{
							context.AddError(navigationProperty.Partner.Location(), EdmErrorCode.InvalidMultiplicityOfPrincipalEnd, Strings.EdmModel_Validator_Semantic_InvalidMultiplicityOfPrincipalEndDependentPropertiesAllNullable(navigationProperty.Partner.Name, navigationProperty.Name));
							return;
						}
					}
				}
			}
			);
			ValidationRules.NavigationPropertyDependentEndMultiplicity = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty navigationProperty) => {
				IEnumerable<IEdmStructuralProperty> dependentProperties = navigationProperty.DependentProperties;
				if (dependentProperties != null)
				{
					if (!ValidationHelper.PropertySetsAreEquivalent(navigationProperty.DeclaringEntityType().Key(), dependentProperties))
					{
						if (navigationProperty.Multiplicity() != EdmMultiplicity.Many)
						{
							context.AddError(navigationProperty.Location(), EdmErrorCode.InvalidMultiplicityOfDependentEnd, Strings.EdmModel_Validator_Semantic_InvalidMultiplicityOfDependentEndMustBeMany(navigationProperty.Name));
						}
					}
					else
					{
						if (navigationProperty.Multiplicity() != EdmMultiplicity.ZeroOrOne && navigationProperty.Multiplicity() != EdmMultiplicity.One)
						{
							context.AddError(navigationProperty.Location(), EdmErrorCode.InvalidMultiplicityOfDependentEnd, Strings.EdmModel_Validator_Semantic_InvalidMultiplicityOfDependentEndMustBeZeroOneOrOne(navigationProperty.Name));
							return;
						}
					}
				}
			}
			);
			ValidationRules.NavigationPropertyDependentPropertiesMustBelongToDependentEntity = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty navigationProperty) => {
				IEnumerable<IEdmStructuralProperty> dependentProperties = navigationProperty.DependentProperties;
				if (dependentProperties != null)
				{
					IEdmEntityType edmEntityType = navigationProperty.DeclaringEntityType();
					foreach (IEdmStructuralProperty dependentProperty in dependentProperties)
					{
						if (context.IsBad(dependentProperty))
						{
							continue;
						}
						IEdmProperty edmProperty = edmEntityType.FindProperty(dependentProperty.Name);
						if (edmProperty == dependentProperty)
						{
							continue;
						}
						context.AddError(navigationProperty.Location(), EdmErrorCode.DependentPropertiesMustBelongToDependentEntity, Strings.EdmModel_Validator_Semantic_DependentPropertiesMustBelongToDependentEntity(dependentProperty.Name, edmEntityType.Name));
					}
				}
			}
			);
			ValidationRules.NavigationPropertyInvalidToPropertyInRelationshipConstraintBeforeV2 = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty navigationProperty) => {
				IEnumerable<IEdmStructuralProperty> dependentProperties = navigationProperty.DependentProperties;
				if (dependentProperties != null && !ValidationHelper.PropertySetIsSubset(navigationProperty.DeclaringEntityType().Key(), dependentProperties))
				{
					string propertyInRelationshipConstraint = Strings.EdmModel_Validator_Semantic_InvalidToPropertyInRelationshipConstraint(navigationProperty.Name, navigationProperty.DeclaringEntityType().FullName());
					context.AddError(navigationProperty.Location(), EdmErrorCode.InvalidPropertyInRelationshipConstraint, propertyInRelationshipConstraint);
				}
			}
			);
			ValidationRules.NavigationPropertyEndWithManyMultiplicityCannotHaveOperationsSpecified = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty end) => {
				if (end.Multiplicity() == EdmMultiplicity.Many && end.OnDelete != EdmOnDeleteAction.None)
				{
					string str = Strings.EdmModel_Validator_Semantic_EndWithManyMultiplicityCannotHaveOperationsSpecified(end.Name);
					context.AddError(end.Location(), EdmErrorCode.EndWithManyMultiplicityCannotHaveOperationsSpecified, str);
				}
			}
			);
			ValidationRules.NavigationPropertyContainsTargetNotSupportedBeforeV3 = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty property) => {
				if (property.ContainsTarget)
				{
					context.AddError(property.Location(), EdmErrorCode.NavigationPropertyContainsTargetNotSupportedBeforeV3, Strings.EdmModel_Validator_Semantic_NavigationPropertyContainsTargetNotSupportedBeforeV3);
				}
			}
			);
			ValidationRules.NavigationPropertyWithRecursiveContainmentTargetMustBeOptional = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty property) => {
				if (property.ContainsTarget && property.DeclaringType.IsOrInheritsFrom(property.ToEntityType()) && !property.Type.IsCollection() && !property.Type.IsNullable)
				{
					context.AddError(property.Location(), EdmErrorCode.NavigationPropertyWithRecursiveContainmentTargetMustBeOptional, Strings.EdmModel_Validator_Semantic_NavigationPropertyWithRecursiveContainmentTargetMustBeOptional(property.Name));
				}
			}
			);
			ValidationRules.NavigationPropertyWithRecursiveContainmentSourceMustBeFromZeroOrOne = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty property) => {
				if (property.ContainsTarget && property.DeclaringType.IsOrInheritsFrom(property.ToEntityType()) && property.Multiplicity() != EdmMultiplicity.ZeroOrOne)
				{
					context.AddError(property.Location(), EdmErrorCode.NavigationPropertyWithRecursiveContainmentSourceMustBeFromZeroOrOne, Strings.EdmModel_Validator_Semantic_NavigationPropertyWithRecursiveContainmentSourceMustBeFromZeroOrOne(property.Name));
				}
			}
			);
			ValidationRules.NavigationPropertyWithNonRecursiveContainmentSourceMustBeFromOne = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty property) => {
				if (property.ContainsTarget && !property.DeclaringType.IsOrInheritsFrom(property.ToEntityType()) && property.Multiplicity() != EdmMultiplicity.One)
				{
					context.AddError(property.Location(), EdmErrorCode.NavigationPropertyWithNonRecursiveContainmentSourceMustBeFromOne, Strings.EdmModel_Validator_Semantic_NavigationPropertyWithNonRecursiveContainmentSourceMustBeFromOne(property.Name));
				}
			}
			);
			ValidationRules.NavigationPropertyEntityMustNotIndirectlyContainItself = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty property) => {
				if (property.ContainsTarget && !property.DeclaringType.IsOrInheritsFrom(property.ToEntityType()) && ValidationHelper.TypeIndirectlyContainsTarget(property.ToEntityType(), property.DeclaringEntityType(), new HashSetInternal<IEdmEntityType>(), context.Model))
				{
					context.AddError(property.Location(), EdmErrorCode.NavigationPropertyEntityMustNotIndirectlyContainItself, Strings.EdmModel_Validator_Semantic_NavigationPropertyEntityMustNotIndirectlyContainItself(property.Name));
				}
			}
			);
			ValidationRules.NavigationPropertyTypeMismatchRelationshipConstraint = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty navigationProperty) => {
				IEnumerable<IEdmStructuralProperty> dependentProperties = navigationProperty.DependentProperties;
				if (dependentProperties != null)
				{
					int num = dependentProperties.Count<IEdmStructuralProperty>();
					IEdmEntityType edmEntityType = navigationProperty.Partner.DeclaringEntityType();
					IEnumerable<IEdmStructuralProperty> edmStructuralProperties = edmEntityType.Key();
					if (num == edmStructuralProperties.Count<IEdmStructuralProperty>())
					{
						for (int i = 0; i < num; i++)
						{
							if (!navigationProperty.DependentProperties.ElementAtOrDefault<IEdmStructuralProperty>(i).Type.Definition.IsEquivalentTo(edmStructuralProperties.ElementAtOrDefault<IEdmStructuralProperty>(i).Type.Definition))
							{
								string str = Strings.EdmModel_Validator_Semantic_TypeMismatchRelationshipConstraint(navigationProperty.DependentProperties.ToList<IEdmStructuralProperty>()[i].Name, navigationProperty.DeclaringEntityType().FullName(), edmStructuralProperties.ToList<IEdmStructuralProperty>()[i].Name, edmEntityType.Name, "Fred");
								context.AddError(navigationProperty.Location(), EdmErrorCode.TypeMismatchRelationshipConstraint, str);
							}
						}
					}
				}
			}
			);
			ValidationRules.NavigationPropertyAssociationNameIsValid = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty property) => {
				if (property.IsPrincipal)
				{
					ValidationRules.CheckForNameError(context, context.Model.GetAssociationName(property), property.Location());
				}
			}
			);
			ValidationRules.NavigationPropertyAssociationEndNameIsValid = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty property) => ValidationRules.CheckForNameError(context, context.Model.GetAssociationEndName(property), property.Location()));
			ValidationRules.PropertyMustNotHaveKindOfNone = new ValidationRule<IEdmProperty>((ValidationContext context, IEdmProperty property) => {
				if (property.PropertyKind == EdmPropertyKind.None && !context.IsBad(property))
				{
					context.AddError(property.Location(), EdmErrorCode.PropertyMustNotHaveKindOfNone, Strings.EdmModel_Validator_Semantic_PropertyMustNotHaveKindOfNone(property.Name));
				}
			}
			);
			ValidationRules.FunctionsNotSupportedBeforeV2 = new ValidationRule<IEdmFunction>((ValidationContext context, IEdmFunction function) => context.AddError(function.Location(), EdmErrorCode.FunctionsNotSupportedBeforeV2, Strings.EdmModel_Validator_Semantic_FunctionsNotSupportedBeforeV2));
			ValidationRules.FunctionOnlyInputParametersAllowedInFunctions = new ValidationRule<IEdmFunction>((ValidationContext context, IEdmFunction function) => {
				foreach (IEdmFunctionParameter parameter in function.Parameters)
				{
					if (parameter.Mode == EdmFunctionParameterMode.In)
					{
						continue;
					}
					context.AddError(parameter.Location(), EdmErrorCode.OnlyInputParametersAllowedInFunctions, Strings.EdmModel_Validator_Semantic_OnlyInputParametersAllowedInFunctions(parameter.Name, function.Name));
				}
			}
			);
			ValidationRules.FunctionImportUnsupportedReturnTypeV1 = new ValidationRule<IEdmFunctionImport>((ValidationContext context, IEdmFunctionImport functionImport) => {
				bool flag;
				bool flag1;
				if (functionImport.ReturnType != null)
				{
					if (!functionImport.ReturnType.IsCollection())
					{
						flag = true;
					}
					else
					{
						IEdmTypeReference edmTypeReference = functionImport.ReturnType.AsCollection().ElementType();
						if (edmTypeReference.IsPrimitive() || edmTypeReference.IsEntity())
						{
							flag1 = false;
						}
						else
						{
							flag1 = !context.IsBad(edmTypeReference.Definition);
						}
						flag = flag1;
					}
					if (flag && !context.IsBad(functionImport.ReturnType.Definition))
					{
						context.AddError(functionImport.Location(), EdmErrorCode.FunctionImportUnsupportedReturnType, Strings.EdmModel_Validator_Semantic_FunctionImportWithUnsupportedReturnTypeV1(functionImport.Name));
					}
				}
			}
			);
			ValidationRules.FunctionImportUnsupportedReturnTypeAfterV1 = new ValidationRule<IEdmFunctionImport>((ValidationContext context, IEdmFunctionImport functionImport) => {
				IEdmTypeReference returnType;
				if (functionImport.ReturnType != null)
				{
					if (functionImport.ReturnType.IsCollection())
					{
						returnType = functionImport.ReturnType.AsCollection().ElementType();
					}
					else
					{
						returnType = functionImport.ReturnType;
					}
					IEdmTypeReference edmTypeReference = returnType;
					if (!edmTypeReference.IsPrimitive() && !edmTypeReference.IsEntity() && !edmTypeReference.IsComplex() && !edmTypeReference.IsEnum() && !context.IsBad(edmTypeReference.Definition))
					{
						context.AddError(functionImport.Location(), EdmErrorCode.FunctionImportUnsupportedReturnType, Strings.EdmModel_Validator_Semantic_FunctionImportWithUnsupportedReturnTypeAfterV1(functionImport.Name));
					}
				}
			}
			);
			ValidationRules.FunctionImportReturnEntitiesButDoesNotSpecifyEntitySet = new ValidationRule<IEdmFunctionImport>((ValidationContext context, IEdmFunctionImport functionImport) => {
				IEdmTypeReference returnType;
				if (functionImport.ReturnType != null && functionImport.EntitySet == null)
				{
					if (functionImport.ReturnType.IsCollection())
					{
						returnType = functionImport.ReturnType.AsCollection().ElementType();
					}
					else
					{
						returnType = functionImport.ReturnType;
					}
					IEdmTypeReference edmTypeReference = returnType;
					if (edmTypeReference.IsEntity() && !context.IsBad(edmTypeReference.Definition))
					{
						context.AddError(functionImport.Location(), EdmErrorCode.FunctionImportReturnsEntitiesButDoesNotSpecifyEntitySet, Strings.EdmModel_Validator_Semantic_FunctionImportReturnEntitiesButDoesNotSpecifyEntitySet(functionImport.Name));
					}
				}
			}
			);
			ValidationRules.FunctionImportEntitySetExpressionIsInvalid = new ValidationRule<IEdmFunctionImport>((ValidationContext context, IEdmFunctionImport functionImport) => {
				IEdmEntitySet edmEntitySet = null;
				IEdmFunctionParameter edmFunctionParameter = null;
				IEnumerable<IEdmNavigationProperty> edmNavigationProperties = null;
				if (functionImport.EntitySet != null)
				{
					if (functionImport.EntitySet.ExpressionKind == EdmExpressionKind.EntitySetReference || functionImport.EntitySet.ExpressionKind == EdmExpressionKind.Path)
					{
						if (!functionImport.TryGetStaticEntitySet(out edmEntitySet) && !functionImport.TryGetRelativeEntitySetPath(context.Model, out edmFunctionParameter, out edmNavigationProperties))
						{
							context.AddError(functionImport.Location(), EdmErrorCode.FunctionImportEntitySetExpressionIsInvalid, Strings.EdmModel_Validator_Semantic_FunctionImportEntitySetExpressionIsInvalid(functionImport.Name));
						}
					}
					else
					{
						context.AddError(functionImport.Location(), EdmErrorCode.FunctionImportEntitySetExpressionIsInvalid, Strings.EdmModel_Validator_Semantic_FunctionImportEntitySetExpressionKindIsInvalid(functionImport.Name, functionImport.EntitySet.ExpressionKind));
						return;
					}
				}
			}
			);
			ValidationRules.FunctionImportEntityTypeDoesNotMatchEntitySet = new ValidationRule<IEdmFunctionImport>((ValidationContext context, IEdmFunctionImport functionImport) => {
				IEdmEntitySet edmEntitySet = null;
				IEdmFunctionParameter edmFunctionParameter = null;
				IEnumerable<IEdmNavigationProperty> edmNavigationProperties = null;
				IEdmTypeReference returnType;
				IEdmTypeReference edmTypeReference4;
				IEdmTypeReference edmTypeReference;
				if (functionImport.EntitySet != null && functionImport.ReturnType != null)
				{
					if (functionImport.ReturnType.IsCollection())
					{
						returnType = functionImport.ReturnType.AsCollection().ElementType();
					}
					else
					{
						returnType = functionImport.ReturnType;
					}
					IEdmTypeReference edmTypeReference1 = returnType;
					if (!edmTypeReference1.IsEntity())
					{
						if (!context.IsBad(edmTypeReference1.Definition))
						{
							context.AddError(functionImport.Location(), EdmErrorCode.FunctionImportSpecifiesEntitySetButDoesNotReturnEntityType, Strings.EdmModel_Validator_Semantic_FunctionImportSpecifiesEntitySetButNotEntityType(functionImport.Name));
						}
					}
					else
					{
						IEdmEntityType edmEntityType = edmTypeReference1.AsEntity().EntityDefinition();
						if (!functionImport.TryGetStaticEntitySet(out edmEntitySet))
						{
							if (functionImport.TryGetRelativeEntitySetPath(context.Model, out edmFunctionParameter, out edmNavigationProperties))
							{
								List<IEdmNavigationProperty> list = edmNavigationProperties.ToList<IEdmNavigationProperty>();
								if (list.Count == 0)
								{
									edmTypeReference4 = edmFunctionParameter.Type;
								}
								else
								{
									edmTypeReference4 = edmNavigationProperties.Last<IEdmNavigationProperty>().Type;
								}
								IEdmTypeReference edmTypeReference2 = edmTypeReference4;
								if (edmTypeReference2.IsCollection())
								{
									edmTypeReference = edmTypeReference2.AsCollection().ElementType();
								}
								else
								{
									edmTypeReference = edmTypeReference2;
								}
								IEdmTypeReference edmTypeReference3 = edmTypeReference;
								if (!edmEntityType.IsOrInheritsFrom(edmTypeReference3.Definition) && !context.IsBad(edmEntityType) && !context.IsBad(edmTypeReference3.Definition))
								{
									context.AddError(functionImport.Location(), EdmErrorCode.FunctionImportEntityTypeDoesNotMatchEntitySet, Strings.EdmModel_Validator_Semantic_FunctionImportEntityTypeDoesNotMatchEntitySet2(functionImport.Name, edmTypeReference1.FullName()));
									return;
								}
							}
						}
						else
						{
							string str = Strings.EdmModel_Validator_Semantic_FunctionImportEntityTypeDoesNotMatchEntitySet(functionImport.Name, edmEntityType.FullName(), edmEntitySet.Name);
							IEdmEntityType elementType = edmEntitySet.ElementType;
							if (!edmEntityType.IsOrInheritsFrom(elementType) && !context.IsBad(edmEntityType) && !context.IsBad(edmEntitySet) && !context.IsBad(elementType))
							{
								context.AddError(functionImport.Location(), EdmErrorCode.FunctionImportEntityTypeDoesNotMatchEntitySet, str);
								return;
							}
						}
					}
				}
			}
			);
			ValidationRules.ComposableFunctionImportMustHaveReturnType = new ValidationRule<IEdmFunctionImport>((ValidationContext context, IEdmFunctionImport functionImport) => {
				if (functionImport.IsComposable && functionImport.ReturnType == null)
				{
					context.AddError(functionImport.Location(), EdmErrorCode.ComposableFunctionImportMustHaveReturnType, Strings.EdmModel_Validator_Semantic_ComposableFunctionImportMustHaveReturnType(functionImport.Name));
				}
			}
			);
			ValidationRules.FunctionImportParametersIncorrectTypeBeforeV3 = new ValidationRule<IEdmFunctionImport>((ValidationContext context, IEdmFunctionImport functionImport) => {
				foreach (IEdmFunctionParameter parameter in functionImport.Parameters)
				{
					IEdmTypeReference edmTypeReference5 = parameter.Type;
					if (edmTypeReference5.IsPrimitive() || edmTypeReference5.IsComplex() || context.IsBad(edmTypeReference5.Definition))
					{
						continue;
					}
					context.AddError(parameter.Location(), EdmErrorCode.FunctionImportParameterIncorrectType, Strings.EdmModel_Validator_Semantic_FunctionImportParameterIncorrectType(edmTypeReference5.FullName(), parameter.Name));
				}
			}
			);
			ValidationRules.FunctionImportIsSideEffectingNotSupportedBeforeV3 = new ValidationRule<IEdmFunctionImport>((ValidationContext context, IEdmFunctionImport functionImport) => {
				if (!functionImport.IsSideEffecting)
				{
					context.AddError(functionImport.Location(), EdmErrorCode.FunctionImportSideEffectingNotSupportedBeforeV3, Strings.EdmModel_Validator_Semantic_FunctionImportSideEffectingNotSupportedBeforeV3);
				}
			}
			);
			ValidationRules.FunctionImportIsComposableNotSupportedBeforeV3 = new ValidationRule<IEdmFunctionImport>((ValidationContext context, IEdmFunctionImport functionImport) => {
				if (functionImport.IsComposable)
				{
					context.AddError(functionImport.Location(), EdmErrorCode.FunctionImportComposableNotSupportedBeforeV3, Strings.EdmModel_Validator_Semantic_FunctionImportComposableNotSupportedBeforeV3);
				}
			}
			);
			ValidationRules.FunctionImportIsBindableNotSupportedBeforeV3 = new ValidationRule<IEdmFunctionImport>((ValidationContext context, IEdmFunctionImport functionImport) => {
				if (functionImport.IsBindable)
				{
					context.AddError(functionImport.Location(), EdmErrorCode.FunctionImportBindableNotSupportedBeforeV3, Strings.EdmModel_Validator_Semantic_FunctionImportBindableNotSupportedBeforeV3);
				}
			}
			);
			ValidationRules.FunctionImportComposableFunctionImportCannotBeSideEffecting = new ValidationRule<IEdmFunctionImport>((ValidationContext context, IEdmFunctionImport functionImport) => {
				if (functionImport.IsComposable && functionImport.IsSideEffecting)
				{
					context.AddError(functionImport.Location(), EdmErrorCode.ComposableFunctionImportCannotBeSideEffecting, Strings.EdmModel_Validator_Semantic_ComposableFunctionImportCannotBeSideEffecting(functionImport.Name));
				}
			}
			);
			ValidationRules.FunctionImportBindableFunctionImportMustHaveParameters = new ValidationRule<IEdmFunctionImport>((ValidationContext context, IEdmFunctionImport functionImport) => {
				if (functionImport.IsBindable && functionImport.Parameters.Count<IEdmFunctionParameter>() == 0)
				{
					context.AddError(functionImport.Location(), EdmErrorCode.BindableFunctionImportMustHaveParameters, Strings.EdmModel_Validator_Semantic_BindableFunctionImportMustHaveParameters(functionImport.Name));
				}
			}
			);
			ValidationRules.FunctionImportParametersCannotHaveModeOfNone = new ValidationRule<IEdmFunctionImport>((ValidationContext context, IEdmFunctionImport function) => {
				foreach (IEdmFunctionParameter parameter in function.Parameters)
				{
					if (parameter.Mode != EdmFunctionParameterMode.None || context.IsBad(function))
					{
						continue;
					}
					context.AddError(parameter.Location(), EdmErrorCode.InvalidFunctionImportParameterMode, Strings.EdmModel_Validator_Semantic_InvalidFunctionImportParameterMode(parameter.Name, function.Name));
				}
			}
			);
			ValidationRules.FunctionBaseParameterNameAlreadyDefinedDuplicate = new ValidationRule<IEdmFunctionBase>((ValidationContext context, IEdmFunctionBase edmFunction) => {
				HashSetInternal<string> strs = new HashSetInternal<string>();
				if (edmFunction.Parameters != null)
				{
					foreach (IEdmFunctionParameter parameter in edmFunction.Parameters)
					{
						ValidationHelper.AddMemberNameToHashSet(parameter, strs, context, EdmErrorCode.AlreadyDefined, Strings.EdmModel_Validator_Semantic_ParameterNameAlreadyDefinedDuplicate(parameter.Name), false);
					}
				}
			}
			);
			ValidationRules.TypeReferenceInaccessibleSchemaType = new ValidationRule<IEdmTypeReference>((ValidationContext context, IEdmTypeReference typeReference) => {
				IEdmSchemaType definition = typeReference.Definition as IEdmSchemaType;
				if (definition != null && !context.IsBad(definition))
				{
					ValidationRules.CheckForUnreacheableTypeError(context, definition, typeReference.Location());
				}
			}
			);
			ValidationRules.StreamTypeReferencesNotSupportedBeforeV3 = new ValidationRule<IEdmPrimitiveTypeReference>((ValidationContext context, IEdmPrimitiveTypeReference type) => {
				if (type.IsStream())
				{
					context.AddError(type.Location(), EdmErrorCode.StreamTypeReferencesNotSupportedBeforeV3, Strings.EdmModel_Validator_Semantic_StreamTypeReferencesNotSupportedBeforeV3);
				}
			}
			);
			ValidationRules.SpatialTypeReferencesNotSupportedBeforeV3 = new ValidationRule<IEdmPrimitiveTypeReference>((ValidationContext context, IEdmPrimitiveTypeReference type) => {
				if (type.IsSpatial())
				{
					context.AddError(type.Location(), EdmErrorCode.SpatialTypeReferencesNotSupportedBeforeV3, Strings.EdmModel_Validator_Semantic_SpatialTypeReferencesNotSupportedBeforeV3);
				}
			}
			);
			ValidationRules.DecimalTypeReferenceScaleOutOfRange = new ValidationRule<IEdmDecimalTypeReference>((ValidationContext context, IEdmDecimalTypeReference type) => {
				bool hasValue;
				bool flag;
				int? scale = type.Scale;
				int? precision = type.Precision;
				if (scale.GetValueOrDefault() <= precision.GetValueOrDefault())
				{
					hasValue = false;
				}
				else
				{
					hasValue = scale.HasValue & precision.HasValue;
				}
				if (!hasValue)
				{
					int? nullable = type.Scale;
					if (nullable.GetValueOrDefault() >= 0)
					{
						flag = false;
					}
					else
					{
						flag = nullable.HasValue;
					}
					if (!flag)
					{
						return;
					}
				}
				context.AddError(type.Location(), EdmErrorCode.ScaleOutOfRange, Strings.EdmModel_Validator_Semantic_ScaleOutOfRange);
			}
			);
			ValidationRules.DecimalTypeReferencePrecisionOutOfRange = new ValidationRule<IEdmDecimalTypeReference>((ValidationContext context, IEdmDecimalTypeReference type) => {
				bool hasValue;
				bool flag;
				int? precision = type.Precision;
				if (precision.GetValueOrDefault() <= 0x7fffffff)
				{
					hasValue = false;
				}
				else
				{
					hasValue = precision.HasValue;
				}
				if (!hasValue)
				{
					int? nullable = type.Precision;
					if (nullable.GetValueOrDefault() >= 0)
					{
						flag = false;
					}
					else
					{
						flag = nullable.HasValue;
					}
					if (!flag)
					{
						return;
					}
				}
				context.AddError(type.Location(), EdmErrorCode.PrecisionOutOfRange, Strings.EdmModel_Validator_Semantic_PrecisionOutOfRange);
			}
			);
			ValidationRules.StringTypeReferenceStringMaxLengthNegative = new ValidationRule<IEdmStringTypeReference>((ValidationContext context, IEdmStringTypeReference type) => {
				bool hasValue;
				int? maxLength = type.MaxLength;
				if (maxLength.GetValueOrDefault() >= 0)
				{
					hasValue = false;
				}
				else
				{
					hasValue = maxLength.HasValue;
				}
				if (hasValue)
				{
					context.AddError(type.Location(), EdmErrorCode.MaxLengthOutOfRange, Strings.EdmModel_Validator_Semantic_StringMaxLengthOutOfRange);
				}
			}
			);
			ValidationRules.StringTypeReferenceStringUnboundedNotValidForMaxLength = new ValidationRule<IEdmStringTypeReference>((ValidationContext context, IEdmStringTypeReference type) => {
				int? maxLength = type.MaxLength;
				if (maxLength.HasValue && type.IsUnbounded)
				{
					context.AddError(type.Location(), EdmErrorCode.IsUnboundedCannotBeTrueWhileMaxLengthIsNotNull, Strings.EdmModel_Validator_Semantic_IsUnboundedCannotBeTrueWhileMaxLengthIsNotNull);
				}
			}
			);
			ValidationRules.BinaryTypeReferenceBinaryMaxLengthNegative = new ValidationRule<IEdmBinaryTypeReference>((ValidationContext context, IEdmBinaryTypeReference type) => {
				bool hasValue;
				int? maxLength = type.MaxLength;
				if (maxLength.GetValueOrDefault() >= 0)
				{
					hasValue = false;
				}
				else
				{
					hasValue = maxLength.HasValue;
				}
				if (hasValue)
				{
					context.AddError(type.Location(), EdmErrorCode.MaxLengthOutOfRange, Strings.EdmModel_Validator_Semantic_MaxLengthOutOfRange);
				}
			}
			);
			ValidationRules.BinaryTypeReferenceBinaryUnboundedNotValidForMaxLength = new ValidationRule<IEdmBinaryTypeReference>((ValidationContext context, IEdmBinaryTypeReference type) => {
				int? maxLength = type.MaxLength;
				if (maxLength.HasValue && type.IsUnbounded)
				{
					context.AddError(type.Location(), EdmErrorCode.IsUnboundedCannotBeTrueWhileMaxLengthIsNotNull, Strings.EdmModel_Validator_Semantic_IsUnboundedCannotBeTrueWhileMaxLengthIsNotNull);
				}
			}
			);
			ValidationRules.TemporalTypeReferencePrecisionOutOfRange = new ValidationRule<IEdmTemporalTypeReference>((ValidationContext context, IEdmTemporalTypeReference type) => {
				bool hasValue;
				bool flag;
				int? precision = type.Precision;
				if (precision.GetValueOrDefault() <= 0x7fffffff)
				{
					hasValue = false;
				}
				else
				{
					hasValue = precision.HasValue;
				}
				if (!hasValue)
				{
					int? nullable = type.Precision;
					if (nullable.GetValueOrDefault() >= 0)
					{
						flag = false;
					}
					else
					{
						flag = nullable.HasValue;
					}
					if (!flag)
					{
						return;
					}
				}
				context.AddError(type.Location(), EdmErrorCode.PrecisionOutOfRange, Strings.EdmModel_Validator_Semantic_PrecisionOutOfRange);
			}
			);
			ValidationRules.ModelDuplicateSchemaElementNameBeforeV3 = new ValidationRule<IEdmModel>((ValidationContext context, IEdmModel model) => {
				List<IEdmFunction> edmFunctions = null;
				HashSetInternal<string> strs = new HashSetInternal<string>();
				Dictionary<string, List<IEdmFunction>> strs1 = new Dictionary<string, List<IEdmFunction>>();
				foreach (IEdmSchemaElement schemaElement in model.SchemaElements)
				{
					bool flag = false;
					string str = schemaElement.FullName();
					if (schemaElement.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
					{
						continue;
					}
					Func<IEdmFunction, bool> func = null;
					if (schemaElement as IEdmFunction == null)
					{
						if (strs.Add(str))
						{
							if (!strs1.ContainsKey(str))
							{
								flag = model.ItemExistsInReferencedModel(str, false);
							}
							else
							{
								flag = true;
							}
						}
						else
						{
							flag = true;
						}
					}
					else
					{
						if (!strs.Contains(str))
						{
							if (!strs1.TryGetValue(str, out edmFunctions))
							{
								edmFunctions = new List<IEdmFunction>();
								strs1[str] = edmFunctions;
							}
							else
							{
								List<IEdmFunction> edmFunctions1 = edmFunctions;
								if (func == null)
								{
									func = (IEdmFunction existingFunction) => { 
										return (schemaElement as IEdmFunction).IsFunctionSignatureEquivalentTo(existingFunction); 
									};
								}
								if (edmFunctions1.Any<IEdmFunction>(func))
								{
									flag = true;
								}
							}
							edmFunctions.Add(schemaElement as IEdmFunction);
						}
						else
						{
							flag = true;
						}
						if (!flag)
						{
							flag = model.FunctionOrNameExistsInReferencedModel(schemaElement as IEdmFunction, str, false);
						}
					}
					if (!flag)
					{
						continue;
					}
					context.AddError(schemaElement.Location(), EdmErrorCode.AlreadyDefined, Strings.EdmModel_Validator_Semantic_SchemaElementNameAlreadyDefined(str));
				}
			}
			);
			ValidationRules.ModelDuplicateSchemaElementName = new ValidationRule<IEdmModel>((ValidationContext context, IEdmModel model) => {
				List<IEdmFunction> edmFunctions = null;
				HashSetInternal<string> strs = new HashSetInternal<string>();
				Dictionary<string, List<IEdmFunction>> strs1 = new Dictionary<string, List<IEdmFunction>>();
				foreach (IEdmSchemaElement schemaElement in model.SchemaElements)
				{
					Func<IEdmFunction, bool> func = null;
					bool flag = false;
					string str = schemaElement.FullName();
					IEdmFunction edmFunction2 = schemaElement as IEdmFunction;
					if (edmFunction2 == null)
					{
						if (strs.Add(str))
						{
							if (!strs1.ContainsKey(str))
							{
								flag = model.ItemExistsInReferencedModel(str, true);
							}
							else
							{
								flag = true;
							}
						}
						else
						{
							flag = true;
						}
					}
					else
					{
						if (!strs.Contains(str))
						{
							if (!strs1.TryGetValue(str, out edmFunctions))
							{
								edmFunctions = new List<IEdmFunction>();
								strs1[str] = edmFunctions;
							}
							else
							{
								List<IEdmFunction> edmFunctions1 = edmFunctions;
								if (func == null)
								{
									func = (IEdmFunction existingFunction) => edmFunction2.IsFunctionSignatureEquivalentTo(existingFunction);
								}
								if (edmFunctions1.Any<IEdmFunction>(func))
								{
									flag = true;
								}
							}
							edmFunctions.Add(edmFunction2);
						}
						else
						{
							flag = true;
						}
						if (!flag)
						{
							flag = model.FunctionOrNameExistsInReferencedModel(edmFunction2, str, true);
						}
					}
					if (!flag)
					{
						continue;
					}
					context.AddError(schemaElement.Location(), EdmErrorCode.AlreadyDefined, Strings.EdmModel_Validator_Semantic_SchemaElementNameAlreadyDefined(str));
				}
			}
			);
			ValidationRules.ModelDuplicateEntityContainerName = new ValidationRule<IEdmModel>((ValidationContext context, IEdmModel model) => {
				HashSetInternal<string> strs = new HashSetInternal<string>();
				foreach (IEdmEntityContainer edmEntityContainer in model.EntityContainers())
				{
					ValidationHelper.AddMemberNameToHashSet(edmEntityContainer, strs, context, EdmErrorCode.DuplicateEntityContainerName, Strings.EdmModel_Validator_Semantic_DuplicateEntityContainerName(edmEntityContainer.Name), false);
				}
			}
			);
			ValidationRules.ImmediateValueAnnotationElementAnnotationIsValid = new ValidationRule<IEdmDirectValueAnnotation>((ValidationContext context, IEdmDirectValueAnnotation annotation) => {
				IEdmStringValue edmStringValue = annotation.Value as IEdmStringValue;
				if (edmStringValue != null && edmStringValue.IsSerializedAsElement(context.Model) && (EdmUtil.IsNullOrWhiteSpaceInternal(annotation.NamespaceUri) || EdmUtil.IsNullOrWhiteSpaceInternal(annotation.Name)))
				{
					context.AddError(annotation.Location(), EdmErrorCode.InvalidElementAnnotation, Strings.EdmModel_Validator_Semantic_InvalidElementAnnotationMismatchedTerm);
				}
			}
			);
			ValidationRules.ImmediateValueAnnotationElementAnnotationHasNameAndNamespace = new ValidationRule<IEdmDirectValueAnnotation>((ValidationContext context, IEdmDirectValueAnnotation annotation) => {
				EdmError edmError = null;
				IEdmStringValue edmStringValue1 = annotation.Value as IEdmStringValue;
				if (edmStringValue1 != null && edmStringValue1.IsSerializedAsElement(context.Model) && !ValidationHelper.ValidateValueCanBeWrittenAsXmlElementAnnotation(edmStringValue1, annotation.NamespaceUri, annotation.Name, out edmError))
				{
					context.AddError(edmError);
				}
			}
			);
			ValidationRules.VocabularyAnnotationsNotSupportedBeforeV3 = new ValidationRule<IEdmVocabularyAnnotation>((ValidationContext context, IEdmVocabularyAnnotation vocabularyAnnotation) => context.AddError(vocabularyAnnotation.Location(), EdmErrorCode.VocabularyAnnotationsNotSupportedBeforeV3, Strings.EdmModel_Validator_Semantic_VocabularyAnnotationsNotSupportedBeforeV3));
			ValidationRules.VocabularyAnnotationInaccessibleTarget = new ValidationRule<IEdmVocabularyAnnotation>((ValidationContext context, IEdmVocabularyAnnotation annotation) => {
				IEdmVocabularyAnnotatable target = annotation.Target;
				bool flag = false;
				IEdmEntityContainer edmEntityContainer = target as IEdmEntityContainer;
				if (edmEntityContainer == null)
				{
					IEdmEntitySet edmEntitySet = target as IEdmEntitySet;
					if (edmEntitySet == null)
					{
						IEdmSchemaType edmSchemaType = target as IEdmSchemaType;
						if (edmSchemaType == null)
						{
							IEdmTerm edmTerm = target as IEdmTerm;
							if (edmTerm == null)
							{
								IEdmFunction edmFunction3 = target as IEdmFunction;
								if (edmFunction3 == null)
								{
									IEdmFunctionImport edmFunctionImport = target as IEdmFunctionImport;
									if (edmFunctionImport == null)
									{
										IEdmProperty edmProperty = target as IEdmProperty;
										if (edmProperty == null)
										{
											IEdmFunctionParameter edmFunctionParameter = target as IEdmFunctionParameter;
											if (edmFunctionParameter == null)
											{
												flag = true;
											}
											else
											{
												IEdmFunction declaringFunction = edmFunctionParameter.DeclaringFunction as IEdmFunction;
												if (declaringFunction == null)
												{
													IEdmFunctionImport declaringFunction1 = edmFunctionParameter.DeclaringFunction as IEdmFunctionImport;
													if (declaringFunction1 != null)
													{
														IEdmEntityContainer container = declaringFunction1.Container;
														foreach (IEdmFunctionImport edmFunctionImport1 in container.FindFunctionImports(declaringFunction1.Name))
														{
															if (edmFunctionImport1.FindParameter(edmFunctionParameter.Name) == null)
															{
																continue;
															}
															flag = true;
															break;
														}
													}
												}
												else
												{
													foreach (IEdmFunction edmFunction1 in context.Model.FindDeclaredFunctions(declaringFunction.FullName()))
													{
														if (edmFunction1.FindParameter(edmFunctionParameter.Name) == null)
														{
															continue;
														}
														flag = true;
														break;
													}
												}
											}
										}
										else
										{
											string str = EdmUtil.FullyQualifiedName(edmProperty.DeclaringType as IEdmSchemaType);
											IEdmStructuredType edmStructuredType = context.Model.FindDeclaredType(str) as IEdmStructuredType;
											if (edmStructuredType != null)
											{
												flag = edmStructuredType.FindProperty(edmProperty.Name) != null;
											}
										}
									}
									else
									{
										flag = edmFunctionImport.Container.FindFunctionImports(edmFunctionImport.Name).Count<IEdmFunctionImport>() > 0;
									}
								}
								else
								{
									flag = context.Model.FindDeclaredFunctions(edmFunction3.FullName()).Count<IEdmFunction>() > 0;
								}
							}
							else
							{
								flag = context.Model.FindDeclaredValueTerm(edmTerm.FullName()) != null;
							}
						}
						else
						{
							flag = context.Model.FindDeclaredType(edmSchemaType.FullName()) != null;
						}
					}
					else
					{
						IEdmEntityContainer container1 = edmEntitySet.Container;
						if (container1 != null)
						{
							flag = container1.FindEntitySet(edmEntitySet.Name) != null;
						}
					}
				}
				else
				{
					flag = context.Model.FindDeclaredEntityContainer(edmEntityContainer.FullName()) != null;
				}
				if (!flag)
				{
					context.AddError(annotation.Location(), EdmErrorCode.BadUnresolvedTarget, Strings.EdmModel_Validator_Semantic_InaccessibleTarget(EdmUtil.FullyQualifiedName(target)));
				}
			}
			);
			ValidationRules.ValueAnnotationAssertCorrectExpressionType = new ValidationRule<IEdmValueAnnotation>((ValidationContext context, IEdmValueAnnotation annotation) => {
				IEnumerable<EdmError> edmErrors = null;
				if (!annotation.Value.TryAssertType(((IEdmValueTerm)annotation.Term).Type, out edmErrors))
				{
					foreach (EdmError edmError in edmErrors)
					{
						context.AddError(edmError);
					}
				}
			}
			);
			ValidationRules.ValueAnnotationInaccessibleTerm = new ValidationRule<IEdmValueAnnotation>((ValidationContext context, IEdmValueAnnotation annotation) => {
				IEdmTerm edmTerm1 = annotation.Term;
				if (edmTerm1 as IUnresolvedElement == null && context.Model.FindValueTerm(edmTerm1.FullName()) == null)
				{
					context.AddError(annotation.Location(), EdmErrorCode.BadUnresolvedTerm, Strings.EdmModel_Validator_Semantic_InaccessibleTerm(annotation.Term.FullName()));
				}
			}
			);
			ValidationRules.TypeAnnotationInaccessibleTerm = new ValidationRule<IEdmTypeAnnotation>((ValidationContext context, IEdmTypeAnnotation annotation) => {
				IEdmTerm edmTerm2 = annotation.Term;
				if (edmTerm2 as IUnresolvedElement == null && context.Model.FindType(edmTerm2.FullName()) as IEdmStructuredType == null)
				{
					context.AddError(annotation.Location(), EdmErrorCode.BadUnresolvedTerm, Strings.EdmModel_Validator_Semantic_InaccessibleTerm(annotation.Term.FullName()));
				}
			}
			);
			ValidationRules.TypeAnnotationAssertMatchesTermType = new ValidationRule<IEdmTypeAnnotation>((ValidationContext context, IEdmTypeAnnotation annotation) => {
				IEdmStructuredType edmStructuredType1 = (IEdmStructuredType)annotation.Term;
				HashSetInternal<IEdmProperty> edmProperties = new HashSetInternal<IEdmProperty>();
				foreach (IEdmProperty edmProperty in edmStructuredType1.Properties())
				{
					IEdmPropertyValueBinding edmPropertyValueBinding = annotation.FindPropertyBinding(edmProperty);
					if (edmPropertyValueBinding != null)
					{
						edmProperties.Add(edmProperty);
					}
					else
					{
						context.AddError(new EdmError(annotation.Location(), EdmErrorCode.TypeAnnotationMissingRequiredProperty, Strings.EdmModel_Validator_Semantic_TypeAnnotationMissingRequiredProperty(edmProperty.Name)));
					}
				}
				if (!edmStructuredType1.IsOpen)
				{
					foreach (IEdmPropertyValueBinding propertyValueBinding in annotation.PropertyValueBindings)
					{
						if (edmProperties.Contains(propertyValueBinding.BoundProperty) || context.IsBad(propertyValueBinding))
						{
							continue;
						}
						context.AddError(new EdmError(propertyValueBinding.Location(), EdmErrorCode.TypeAnnotationHasExtraProperties, Strings.EdmModel_Validator_Semantic_TypeAnnotationHasExtraProperties(propertyValueBinding.BoundProperty.Name)));
					}
				}
			}
			);
			ValidationRules.PropertyValueBindingValueIsCorrectType = new ValidationRule<IEdmPropertyValueBinding>((ValidationContext context, IEdmPropertyValueBinding binding) => {
				IEnumerable<EdmError> edmErrors = null;
				if (!binding.Value.TryAssertType(binding.BoundProperty.Type, out edmErrors) && !context.IsBad(binding) && !context.IsBad(binding.BoundProperty))
				{
					foreach (EdmError edmError in edmErrors)
					{
						context.AddError(edmError);
					}
				}
			}
			);
			ValidationRules.ValueTermsNotSupportedBeforeV3 = new ValidationRule<IEdmValueTerm>((ValidationContext context, IEdmValueTerm valueTerm) => context.AddError(valueTerm.Location(), EdmErrorCode.ValueTermsNotSupportedBeforeV3, Strings.EdmModel_Validator_Semantic_ValueTermsNotSupportedBeforeV3));
			ValidationRules.TermMustNotHaveKindOfNone = new ValidationRule<IEdmTerm>((ValidationContext context, IEdmTerm term) => {
				if (term.TermKind == EdmTermKind.None && !context.IsBad(term))
				{
					context.AddError(term.Location(), EdmErrorCode.TermMustNotHaveKindOfNone, Strings.EdmModel_Validator_Semantic_TermMustNotHaveKindOfNone(term.FullName()));
				}
			}
			);
			ValidationRules.IfExpressionAssertCorrectTestType = new ValidationRule<IEdmIfExpression>((ValidationContext context, IEdmIfExpression expression) => {
				IEnumerable<EdmError> edmErrors = null;
				if (!expression.TestExpression.TryAssertType(EdmCoreModel.Instance.GetBoolean(false), out edmErrors))
				{
					foreach (EdmError edmError in edmErrors)
					{
						context.AddError(edmError);
					}
				}
			}
			);
			ValidationRules.CollectionExpressionAllElementsCorrectType = new ValidationRule<IEdmCollectionExpression>((ValidationContext context, IEdmCollectionExpression expression) => {
				IEnumerable<EdmError> edmErrors = null;
				if (expression.DeclaredType != null && !context.IsBad(expression) && !context.IsBad(expression.DeclaredType))
				{
					expression.TryAssertCollectionAsType(expression.DeclaredType, out edmErrors);
					foreach (EdmError edmError in edmErrors)
					{
						context.AddError(edmError);
					}
				}
			}
			);
			ValidationRules.RecordExpressionPropertiesMatchType = new ValidationRule<IEdmRecordExpression>((ValidationContext context, IEdmRecordExpression expression) => {
				IEnumerable<EdmError> edmErrors = null;
				if (expression.DeclaredType != null && !context.IsBad(expression) && !context.IsBad(expression.DeclaredType))
				{
					expression.TryAssertRecordAsType(expression.DeclaredType, out edmErrors);
					foreach (EdmError edmError in edmErrors)
					{
						context.AddError(edmError);
					}
				}
			}
			);
			ValidationRules.FunctionApplicationExpressionParametersMatchAppliedFunction = new ValidationRule<IEdmApplyExpression>((ValidationContext context, IEdmApplyExpression expression) => {
				IEnumerable<EdmError> edmErrors = null;
				IEdmFunctionReferenceExpression appliedFunction = expression.AppliedFunction as IEdmFunctionReferenceExpression;
				if (appliedFunction.ReferencedFunction != null && !context.IsBad(appliedFunction.ReferencedFunction))
				{
					if (appliedFunction.ReferencedFunction.Parameters.Count<IEdmFunctionParameter>() != expression.Arguments.Count<IEdmExpression>())
					{
						context.AddError(new EdmError(expression.Location(), EdmErrorCode.IncorrectNumberOfArguments, Strings.EdmModel_Validator_Semantic_IncorrectNumberOfArguments(expression.Arguments.Count<IEdmExpression>(), appliedFunction.ReferencedFunction.FullName(), appliedFunction.ReferencedFunction.Parameters.Count<IEdmFunctionParameter>())));
					}
					IEnumerator<IEdmExpression> enumerator = expression.Arguments.GetEnumerator();
					foreach (IEdmFunctionParameter edmFunctionParameter in edmErrors)
					{
						enumerator.MoveNext();
						if (enumerator.Current.TryAssertType(edmFunctionParameter.Type, out edmErrors))
						{
							continue;
						}
						IEnumerator<EdmError> enumerator1 = edmErrors.GetEnumerator();
						using (enumerator1)
						{
							while (enumerator1.MoveNext())
							{
								EdmError edmError = enumerator1.Current;
								context.AddError(edmError);
							}
						}
					}
				}
			}
			);
			ValidationRules.VocabularyAnnotatableNoDuplicateAnnotations = new ValidationRule<IEdmVocabularyAnnotatable>((ValidationContext context, IEdmVocabularyAnnotatable annotatable) => {
				HashSetInternal<string> strs = new HashSetInternal<string>();
				foreach (IEdmVocabularyAnnotation edmVocabularyAnnotation in annotatable.VocabularyAnnotations(context.Model))
				{
					if (strs.Add(string.Concat(edmVocabularyAnnotation.Term.FullName(), ":", edmVocabularyAnnotation.Qualifier)))
					{
						continue;
					}
					context.AddError(new EdmError(edmVocabularyAnnotation.Location(), EdmErrorCode.DuplicateAnnotation, Strings.EdmModel_Validator_Semantic_DuplicateAnnotation(EdmUtil.FullyQualifiedName(annotatable), edmVocabularyAnnotation.Term.FullName(), edmVocabularyAnnotation.Qualifier)));
				}
			}
			);
			ValidationRules.PrimitiveValueValidForType = new ValidationRule<IEdmPrimitiveValue>((ValidationContext context, IEdmPrimitiveValue value) => {
				IEnumerable<EdmError> edmErrors = null;
				if (value.Type != null && !context.IsBad(value) && !context.IsBad(value.Type))
				{
					value.TryAssertPrimitiveAsType(value.Type, out edmErrors);
					foreach (EdmError edmError in edmErrors)
					{
						context.AddError(edmError);
					}
				}
			}
			);
		}

		private static void CheckForNameError(ValidationContext context, string name, EdmLocation location)
		{
			if (EdmUtil.IsNullOrWhiteSpaceInternal(name) || name.Length == 0)
			{
				context.AddError(location, EdmErrorCode.InvalidName, Strings.EdmModel_Validator_Syntactic_MissingName);
				return;
			}
			else
			{
				if (name.Length <= 0x1e0)
				{
					if (!EdmUtil.IsValidUndottedName(name))
					{
						context.AddError(location, EdmErrorCode.InvalidName, Strings.EdmModel_Validator_Syntactic_EdmModel_NameIsNotAllowed(name));
					}
					return;
				}
				else
				{
					context.AddError(location, EdmErrorCode.NameTooLong, Strings.EdmModel_Validator_Syntactic_EdmModel_NameIsTooLong(name));
					return;
				}
			}
		}

		private static void CheckForUnreacheableTypeError(ValidationContext context, IEdmSchemaType type, EdmLocation location)
		{
			IEdmType edmType = context.Model.FindType(type.FullName());
			if (edmType as AmbiguousTypeBinding == null)
			{
				if (!edmType.IsEquivalentTo(type))
				{
					context.AddError(location, EdmErrorCode.BadUnresolvedType, Strings.EdmModel_Validator_Semantic_InaccessibleType(type.FullName()));
				}
				return;
			}
			else
			{
				context.AddError(location, EdmErrorCode.BadAmbiguousElementBinding, Strings.EdmModel_Validator_Semantic_AmbiguousType(type.FullName()));
				return;
			}
		}
	}
}