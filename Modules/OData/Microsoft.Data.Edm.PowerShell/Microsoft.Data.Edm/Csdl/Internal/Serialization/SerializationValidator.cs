using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Validation.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.Serialization
{
	internal static class SerializationValidator
	{
		private readonly static ValidationRule<IEdmTypeReference> TypeReferenceTargetMustHaveValidName;

		private readonly static ValidationRule<IEdmEntityReferenceType> EntityReferenceTargetMustHaveValidName;

		private readonly static ValidationRule<IEdmEntitySet> EntitySetTypeMustHaveValidName;

		private readonly static ValidationRule<IEdmStructuredType> StructuredTypeBaseTypeMustHaveValidName;

		private readonly static ValidationRule<IEdmNavigationProperty> NavigationPropertyVerifyAssociationName;

		private readonly static ValidationRule<IEdmVocabularyAnnotation> VocabularyAnnotationOutOfLineMustHaveValidTargetName;

		private readonly static ValidationRule<IEdmVocabularyAnnotation> VocabularyAnnotationMustHaveValidTermName;

		private static ValidationRuleSet serializationRuleSet;

		static SerializationValidator()
		{
			SerializationValidator.TypeReferenceTargetMustHaveValidName = new ValidationRule<IEdmTypeReference>((ValidationContext context, IEdmTypeReference typeReference) => {
				IEdmSchemaType definition = typeReference.Definition as IEdmSchemaType;
				if (definition != null && !EdmUtil.IsQualifiedName(definition.FullName()))
				{
					context.AddError(typeReference.Location(), EdmErrorCode.ReferencedTypeMustHaveValidName, Strings.Serializer_ReferencedTypeMustHaveValidName(definition.FullName()));
				}
			}
			);
			SerializationValidator.EntityReferenceTargetMustHaveValidName = new ValidationRule<IEdmEntityReferenceType>((ValidationContext context, IEdmEntityReferenceType entityReference) => {
				if (!EdmUtil.IsQualifiedName(entityReference.EntityType.FullName()))
				{
					context.AddError(entityReference.Location(), EdmErrorCode.ReferencedTypeMustHaveValidName, Strings.Serializer_ReferencedTypeMustHaveValidName(entityReference.EntityType.FullName()));
				}
			}
			);
			SerializationValidator.EntitySetTypeMustHaveValidName = new ValidationRule<IEdmEntitySet>((ValidationContext context, IEdmEntitySet set) => {
				if (!EdmUtil.IsQualifiedName(set.ElementType.FullName()))
				{
					context.AddError(set.Location(), EdmErrorCode.ReferencedTypeMustHaveValidName, Strings.Serializer_ReferencedTypeMustHaveValidName(set.ElementType.FullName()));
				}
			}
			);
			SerializationValidator.StructuredTypeBaseTypeMustHaveValidName = new ValidationRule<IEdmStructuredType>((ValidationContext context, IEdmStructuredType type) => {
				IEdmSchemaType baseType = type.BaseType as IEdmSchemaType;
				if (baseType != null && !EdmUtil.IsQualifiedName(baseType.FullName()))
				{
					context.AddError(type.Location(), EdmErrorCode.ReferencedTypeMustHaveValidName, Strings.Serializer_ReferencedTypeMustHaveValidName(baseType.FullName()));
				}
			}
			);
			SerializationValidator.NavigationPropertyVerifyAssociationName = new ValidationRule<IEdmNavigationProperty>((ValidationContext context, IEdmNavigationProperty property) => {
				if (!EdmUtil.IsQualifiedName(context.Model.GetAssociationFullName(property)))
				{
					context.AddError(property.Location(), EdmErrorCode.ReferencedTypeMustHaveValidName, Strings.Serializer_ReferencedTypeMustHaveValidName(context.Model.GetAssociationFullName(property)));
				}
			}
			);
			SerializationValidator.VocabularyAnnotationOutOfLineMustHaveValidTargetName = new ValidationRule<IEdmVocabularyAnnotation>((ValidationContext context, IEdmVocabularyAnnotation annotation) => {
				bool hasValue;
				EdmVocabularyAnnotationSerializationLocation? serializationLocation = annotation.GetSerializationLocation(context.Model);
				if (serializationLocation.GetValueOrDefault() != EdmVocabularyAnnotationSerializationLocation.OutOfLine)
				{
					hasValue = false;
				}
				else
				{
					hasValue = serializationLocation.HasValue;
				}
				if (hasValue && !EdmUtil.IsQualifiedName(annotation.TargetString()))
				{
					context.AddError(annotation.Location(), EdmErrorCode.InvalidName, Strings.Serializer_OutOfLineAnnotationTargetMustHaveValidName(EdmUtil.FullyQualifiedName(annotation.Target)));
				}
			}
			);
			SerializationValidator.VocabularyAnnotationMustHaveValidTermName = new ValidationRule<IEdmVocabularyAnnotation>((ValidationContext context, IEdmVocabularyAnnotation annotation) => {
				if (!EdmUtil.IsQualifiedName(annotation.Term.FullName()))
				{
					context.AddError(annotation.Location(), EdmErrorCode.InvalidName, Strings.Serializer_OutOfLineAnnotationTargetMustHaveValidName(annotation.Term.FullName()));
				}
			}
			);
			ValidationRule[] typeReferenceTargetMustHaveValidName = new ValidationRule[18];
			typeReferenceTargetMustHaveValidName[0] = SerializationValidator.TypeReferenceTargetMustHaveValidName;
			typeReferenceTargetMustHaveValidName[1] = SerializationValidator.EntityReferenceTargetMustHaveValidName;
			typeReferenceTargetMustHaveValidName[2] = SerializationValidator.EntitySetTypeMustHaveValidName;
			typeReferenceTargetMustHaveValidName[3] = SerializationValidator.StructuredTypeBaseTypeMustHaveValidName;
			typeReferenceTargetMustHaveValidName[4] = SerializationValidator.VocabularyAnnotationOutOfLineMustHaveValidTargetName;
			typeReferenceTargetMustHaveValidName[5] = SerializationValidator.VocabularyAnnotationMustHaveValidTermName;
			typeReferenceTargetMustHaveValidName[6] = SerializationValidator.NavigationPropertyVerifyAssociationName;
			typeReferenceTargetMustHaveValidName[7] = ValidationRules.FunctionImportEntitySetExpressionIsInvalid;
			typeReferenceTargetMustHaveValidName[8] = ValidationRules.FunctionImportParametersCannotHaveModeOfNone;
			typeReferenceTargetMustHaveValidName[9] = ValidationRules.FunctionOnlyInputParametersAllowedInFunctions;
			typeReferenceTargetMustHaveValidName[10] = ValidationRules.TypeMustNotHaveKindOfNone;
			typeReferenceTargetMustHaveValidName[11] = ValidationRules.PrimitiveTypeMustNotHaveKindOfNone;
			typeReferenceTargetMustHaveValidName[12] = ValidationRules.PropertyMustNotHaveKindOfNone;
			typeReferenceTargetMustHaveValidName[13] = ValidationRules.TermMustNotHaveKindOfNone;
			typeReferenceTargetMustHaveValidName[14] = ValidationRules.SchemaElementMustNotHaveKindOfNone;
			typeReferenceTargetMustHaveValidName[15] = ValidationRules.EntityContainerElementMustNotHaveKindOfNone;
			typeReferenceTargetMustHaveValidName[16] = ValidationRules.EnumMustHaveIntegerUnderlyingType;
			typeReferenceTargetMustHaveValidName[17] = ValidationRules.EnumMemberValueMustHaveSameTypeAsUnderlyingType;
			SerializationValidator.serializationRuleSet = new ValidationRuleSet(typeReferenceTargetMustHaveValidName);
		}

		public static IEnumerable<EdmError> GetSerializationErrors(this IEdmModel root)
		{
			IEnumerable<EdmError> edmErrors = null;
			root.Validate(SerializationValidator.serializationRuleSet, out edmErrors);
			edmErrors = edmErrors.Where<EdmError>(new Func<EdmError, bool>(SerializationValidator.SignificantToSerialization));
			return edmErrors;
		}

		internal static bool SignificantToSerialization(EdmError error)
		{
			if (!ValidationHelper.IsInterfaceCritical(error))
			{
				EdmErrorCode errorCode = error.ErrorCode;
				if (errorCode > EdmErrorCode.RowTypeMustNotHaveBaseType)
				{
					if (errorCode > EdmErrorCode.EnumMemberTypeMustMatchEnumUnderlyingType)
					{
						if (errorCode != EdmErrorCode.ReferencedTypeMustHaveValidName)
						{
							if (errorCode == EdmErrorCode.InvalidFunctionImportParameterMode || errorCode == EdmErrorCode.TypeMustNotHaveKindOfNone || errorCode == EdmErrorCode.PrimitiveTypeMustNotHaveKindOfNone || errorCode == EdmErrorCode.PropertyMustNotHaveKindOfNone || errorCode == EdmErrorCode.TermMustNotHaveKindOfNone || errorCode == EdmErrorCode.SchemaElementMustNotHaveKindOfNone || errorCode == EdmErrorCode.EntityContainerElementMustNotHaveKindOfNone || errorCode == EdmErrorCode.BinaryValueCannotHaveEmptyValue)
							{
								return true;
							}
							if (errorCode != EdmErrorCode.EnumMustHaveIntegerUnderlyingType)
							{
								return false;
							}
						}
					}
					else
					{
						if (errorCode == EdmErrorCode.OnlyInputParametersAllowedInFunctions || errorCode == EdmErrorCode.FunctionImportParameterIncorrectType)
						{
							return true;
						}
						else if (errorCode == EdmErrorCode.ComplexTypeMustHaveProperties)
						{
							return false;
						}
						if (errorCode == EdmErrorCode.EnumMemberTypeMustMatchEnumUnderlyingType)
						{
							return true;
						}
						return false;
					}
				}
				else
				{
					if (errorCode > EdmErrorCode.NameTooLong)
					{
						if (errorCode != EdmErrorCode.FunctionImportEntitySetExpressionIsInvalid)
						{
							if (errorCode == EdmErrorCode.SystemNamespaceEncountered || errorCode == EdmErrorCode.InvalidNamespaceName)
							{
								return true;
							}
							else if ((int)errorCode == 162)
							{
								return false;
							}
							if (errorCode == EdmErrorCode.RowTypeMustNotHaveBaseType)
							{
								return true;
							}
							return false;
						}
					}
					else
					{
						if (errorCode == EdmErrorCode.InvalidName || errorCode == EdmErrorCode.NameTooLong)
						{
							return true;
						}
						return false;
					}
				}
				return true;
			}
			else
			{
				return true;
			}
		}
	}
}