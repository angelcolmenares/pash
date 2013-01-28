using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Data.Edm.Validation
{
	internal static class ExpressionTypeChecker
	{
		private readonly static bool[,] promotionMap;

		static ExpressionTypeChecker()
		{
			ExpressionTypeChecker.promotionMap = ExpressionTypeChecker.InitializePromotionMap();
		}

		private static bool[,] InitializePromotionMap()
		{
			FieldInfo[] fields = typeof(EdmPrimitiveTypeKind).GetFields();
			int num = fields.Where<FieldInfo>((FieldInfo f) => f.IsLiteral).Count<FieldInfo>();
			bool[,] flagArray = new bool[num, num];
			flagArray[3, 9] = true;
			flagArray[3, 10] = true;
			flagArray[3, 11] = true;
			flagArray[12, 9] = true;
			flagArray[12, 10] = true;
			flagArray[12, 11] = true;
			flagArray[9, 10] = true;
			flagArray[9, 11] = true;
			flagArray[10, 11] = true;
			flagArray[13, 7] = true;
			flagArray[21, 17] = true;
			flagArray[19, 17] = true;
			flagArray[23, 17] = true;
			flagArray[24, 17] = true;
			flagArray[22, 17] = true;
			flagArray[18, 17] = true;
			flagArray[20, 17] = true;
			flagArray[29, 25] = true;
			flagArray[27, 25] = true;
			flagArray[31, 25] = true;
			flagArray[32, 25] = true;
			flagArray[30, 25] = true;
			flagArray[26, 25] = true;
			flagArray[28, 25] = true;
			return flagArray;
		}

		private static bool PromotesTo(this EdmPrimitiveTypeKind startingKind, EdmPrimitiveTypeKind target)
		{
			if (startingKind == target)
			{
				return true;
			}
			else
			{
				return ExpressionTypeChecker.promotionMap[(int)startingKind, (int)target];
			}
		}

		private static bool TestNullabilityMatch(this IEdmTypeReference expressionType, IEdmTypeReference assertedType, EdmLocation location, out IEnumerable<EdmError> discoveredErrors)
		{
			if (assertedType.IsNullable || !expressionType.IsNullable)
			{
				discoveredErrors = Enumerable.Empty<EdmError>();
				return true;
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(location, EdmErrorCode.CannotAssertNullableTypeAsNonNullableType, Strings.EdmModel_Validator_Semantic_CannotAssertNullableTypeAsNonNullableType(expressionType.FullName()));
				discoveredErrors = edmError;
				return false;
			}
		}

		private static bool TestTypeMatch(this IEdmTypeReference expressionType, IEdmTypeReference assertedType, EdmLocation location, out IEnumerable<EdmError> discoveredErrors)
		{
			if (expressionType.TestNullabilityMatch(assertedType, location, out discoveredErrors))
			{
				if (expressionType.TypeKind() == EdmTypeKind.None || expressionType.IsBad())
				{
					discoveredErrors = Enumerable.Empty<EdmError>();
					return true;
				}
				else
				{
					if (!expressionType.IsPrimitive() || !assertedType.IsPrimitive())
					{
						if (!expressionType.Definition.IsEquivalentTo(assertedType.Definition))
						{
							EdmError[] edmError = new EdmError[1];
							edmError[0] = new EdmError(location, EdmErrorCode.ExpressionNotValidForTheAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionNotValidForTheAssertedType);
							discoveredErrors = edmError;
							return false;
						}
					}
					else
					{
						if (!expressionType.PrimitiveKind().PromotesTo(assertedType.AsPrimitive().PrimitiveKind()))
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = new EdmError(location, EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindCannotPromoteToAssertedType(expressionType.FullName(), assertedType.FullName()));
							discoveredErrors = edmErrorArray;
							return false;
						}
					}
					discoveredErrors = Enumerable.Empty<EdmError>();
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		private static bool TryAssertBinaryConstantAsType(IEdmBinaryConstantExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			if (type.IsBinary())
			{
				IEdmBinaryTypeReference edmBinaryTypeReference = type.AsBinary();
				int? maxLength = edmBinaryTypeReference.MaxLength;
				if (maxLength.HasValue)
				{
					int? nullable = edmBinaryTypeReference.MaxLength;
					if ((int)expression.Value.Length > nullable.Value)
					{
						EdmError[] edmError = new EdmError[1];
						int? maxLength1 = edmBinaryTypeReference.MaxLength;
						edmError[0] = new EdmError(expression.Location(), EdmErrorCode.BinaryConstantLengthOutOfRange, Strings.EdmModel_Validator_Semantic_BinaryConstantLengthOutOfRange((int)expression.Value.Length, maxLength1.Value));
						discoveredErrors = edmError;
						return false;
					}
				}
				discoveredErrors = Enumerable.Empty<EdmError>();
				return true;
			}
			else
			{
				EdmError[] edmErrorArray = new EdmError[1];
				edmErrorArray[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
				discoveredErrors = edmErrorArray;
				return false;
			}
		}

		private static bool TryAssertBooleanConstantAsType(IEdmBooleanConstantExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			if (type.IsBoolean())
			{
				discoveredErrors = Enumerable.Empty<EdmError>();
				return true;
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
				discoveredErrors = edmError;
				return false;
			}
		}

		internal static bool TryAssertCollectionAsType(this IEdmCollectionExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			IEnumerable<EdmError> edmErrors = null;
			bool flag;
			if (type.IsCollection())
			{
				IEdmTypeReference edmTypeReference = type.AsCollection().ElementType();
				bool flag1 = true;
				List<EdmError> edmErrors1 = new List<EdmError>();
				foreach (IEdmExpression element in expression.Elements)
				{
					if (!element.TryAssertType(edmTypeReference, out edmErrors))
					{
						flag = false;
					}
					else
					{
						flag = flag1;
					}
					flag1 = flag;
					edmErrors1.AddRange(edmErrors);
				}
				discoveredErrors = edmErrors1;
				return flag1;
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(expression.Location(), EdmErrorCode.CollectionExpressionNotValidForNonCollectionType, Strings.EdmModel_Validator_Semantic_CollectionExpressionNotValidForNonCollectionType);
				discoveredErrors = edmError;
				return false;
			}
		}

		private static bool TryAssertDateTimeConstantAsType(IEdmDateTimeConstantExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			if (type.IsDateTime())
			{
				discoveredErrors = Enumerable.Empty<EdmError>();
				return true;
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
				discoveredErrors = edmError;
				return false;
			}
		}

		private static bool TryAssertDateTimeOffsetConstantAsType(IEdmDateTimeOffsetConstantExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			if (type.IsDateTimeOffset())
			{
				discoveredErrors = Enumerable.Empty<EdmError>();
				return true;
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
				discoveredErrors = edmError;
				return false;
			}
		}

		private static bool TryAssertDecimalConstantAsType(IEdmDecimalConstantExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			if (type.IsDecimal())
			{
				discoveredErrors = Enumerable.Empty<EdmError>();
				return true;
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
				discoveredErrors = edmError;
				return false;
			}
		}

		private static bool TryAssertFloatingConstantAsType(IEdmFloatingConstantExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			if (type.IsFloating())
			{
				discoveredErrors = Enumerable.Empty<EdmError>();
				return true;
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
				discoveredErrors = edmError;
				return false;
			}
		}

		private static bool TryAssertGuidConstantAsType(IEdmGuidConstantExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			if (type.IsGuid())
			{
				discoveredErrors = Enumerable.Empty<EdmError>();
				return true;
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
				discoveredErrors = edmError;
				return false;
			}
		}

		internal static bool TryAssertIfAsType(this IEdmIfExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			IEnumerable<EdmError> edmErrors = null;
			IEnumerable<EdmError> edmErrors1 = null;
			bool flag;
			bool flag1 = expression.TrueExpression.TryAssertType(type, out edmErrors);
			if (!expression.FalseExpression.TryAssertType(type, out edmErrors1))
			{
				flag = false;
			}
			else
			{
				flag = flag1;
			}
			flag1 = flag;
			if (flag1)
			{
				discoveredErrors = Enumerable.Empty<EdmError>();
			}
			else
			{
				List<EdmError> edmErrors2 = new List<EdmError>(edmErrors);
				edmErrors2.AddRange(edmErrors1);
				discoveredErrors = edmErrors2;
			}
			return flag1;
		}

		private static bool TryAssertIntegerConstantAsType(IEdmIntegerConstantExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			if (type.IsIntegral())
			{
				EdmPrimitiveTypeKind edmPrimitiveTypeKind = type.PrimitiveKind();
				if (edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Byte)
				{
					return ExpressionTypeChecker.TryAssertIntegerConstantInRange(expression, (long)0, (long)0xff, out discoveredErrors);
				}
				else
				{
					switch (edmPrimitiveTypeKind)
					{
						case EdmPrimitiveTypeKind.Int16:
						{
							return ExpressionTypeChecker.TryAssertIntegerConstantInRange(expression, (long)-32768, (long)0x7fff, out discoveredErrors);
						}
						case EdmPrimitiveTypeKind.Int32:
						{
							return ExpressionTypeChecker.TryAssertIntegerConstantInRange(expression, (long)-2147483648, (long)0x7fffffff, out discoveredErrors);
						}
						case EdmPrimitiveTypeKind.Int64:
						{
							return ExpressionTypeChecker.TryAssertIntegerConstantInRange(expression, -9223372036854775808L, 0x7fffffffffffffffL, out discoveredErrors);
						}
						case EdmPrimitiveTypeKind.SByte:
						{
							return ExpressionTypeChecker.TryAssertIntegerConstantInRange(expression, (long)-128, (long)127, out discoveredErrors);
						}
					}
					EdmError[] edmError = new EdmError[1];
					edmError[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
					discoveredErrors = edmError;
					return false;
				}
			}
			else
			{
				EdmError[] edmErrorArray = new EdmError[1];
				edmErrorArray[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
				discoveredErrors = edmErrorArray;
				return false;
			}
		}

		private static bool TryAssertIntegerConstantInRange(IEdmIntegerConstantExpression expression, long min, long max, out IEnumerable<EdmError> discoveredErrors)
		{
			if (expression.Value < min || expression.Value > max)
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(expression.Location(), EdmErrorCode.IntegerConstantValueOutOfRange, Strings.EdmModel_Validator_Semantic_IntegerConstantValueOutOfRange);
				discoveredErrors = edmError;
				return false;
			}
			else
			{
				discoveredErrors = Enumerable.Empty<EdmError>();
				return true;
			}
		}

		internal static bool TryAssertNullAsType(this IEdmNullExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			if (type.IsNullable)
			{
				discoveredErrors = Enumerable.Empty<EdmError>();
				return true;
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(expression.Location(), EdmErrorCode.NullCannotBeAssertedToBeANonNullableType, Strings.EdmModel_Validator_Semantic_NullCannotBeAssertedToBeANonNullableType);
				discoveredErrors = edmError;
				return false;
			}
		}

		internal static bool TryAssertPathAsType(this IEdmPathExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			discoveredErrors = Enumerable.Empty<EdmError>();
			return true;
		}

		internal static bool TryAssertPrimitiveAsType(this IEdmPrimitiveValue expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			EdmError[] edmError;
			if (type.IsPrimitive())
			{
				EdmValueKind valueKind = expression.ValueKind;
				switch (valueKind)
				{
					case EdmValueKind.Binary:
					{
						return ExpressionTypeChecker.TryAssertBinaryConstantAsType((IEdmBinaryConstantExpression)expression, type, out discoveredErrors);
					}
					case EdmValueKind.Boolean:
					{
						return ExpressionTypeChecker.TryAssertBooleanConstantAsType((IEdmBooleanConstantExpression)expression, type, out discoveredErrors);
					}
					case EdmValueKind.Collection:
					case EdmValueKind.Enum:
					case EdmValueKind.Null:
					case EdmValueKind.Structured:
					{
						edmError = new EdmError[1];
						edmError[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
						discoveredErrors = edmError;
						return false;
					}
					case EdmValueKind.DateTimeOffset:
					{
						return ExpressionTypeChecker.TryAssertDateTimeOffsetConstantAsType((IEdmDateTimeOffsetConstantExpression)expression, type, out discoveredErrors);
					}
					case EdmValueKind.DateTime:
					{
						return ExpressionTypeChecker.TryAssertDateTimeConstantAsType((IEdmDateTimeConstantExpression)expression, type, out discoveredErrors);
					}
					case EdmValueKind.Decimal:
					{
						return ExpressionTypeChecker.TryAssertDecimalConstantAsType((IEdmDecimalConstantExpression)expression, type, out discoveredErrors);
					}
					case EdmValueKind.Floating:
					{
						return ExpressionTypeChecker.TryAssertFloatingConstantAsType((IEdmFloatingConstantExpression)expression, type, out discoveredErrors);
					}
					case EdmValueKind.Guid:
					{
						return ExpressionTypeChecker.TryAssertGuidConstantAsType((IEdmGuidConstantExpression)expression, type, out discoveredErrors);
					}
					case EdmValueKind.Integer:
					{
						return ExpressionTypeChecker.TryAssertIntegerConstantAsType((IEdmIntegerConstantExpression)expression, type, out discoveredErrors);
					}
					case EdmValueKind.String:
					{
						return ExpressionTypeChecker.TryAssertStringConstantAsType((IEdmStringConstantExpression)expression, type, out discoveredErrors);
					}
					case EdmValueKind.Time:
					{
						return ExpressionTypeChecker.TryAssertTimeConstantAsType((IEdmTimeConstantExpression)expression, type, out discoveredErrors);
					}
					default:
					{
						edmError = new EdmError[1];
						edmError[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
						discoveredErrors = edmError;
						return false;
					}
				}
			}
			else
			{
				EdmError[] edmErrorArray = new EdmError[1];
				edmErrorArray[0] = new EdmError(expression.Location(), EdmErrorCode.PrimitiveConstantExpressionNotValidForNonPrimitiveType, Strings.EdmModel_Validator_Semantic_PrimitiveConstantExpressionNotValidForNonPrimitiveType);
				discoveredErrors = edmErrorArray;
				return false;
			}
		}

		internal static bool TryAssertRecordAsType(this IEdmRecordExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			IEnumerable<EdmError> edmErrors = null;
			EdmUtil.CheckArgumentNull<IEdmRecordExpression>(expression, "expression");
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			if (type.IsStructured())
			{
				HashSetInternal<string> strs = new HashSetInternal<string>();
				List<EdmError> edmErrors1 = new List<EdmError>();
				IEdmStructuredTypeReference edmStructuredTypeReference = type.AsStructured();
				IEnumerator<IEdmProperty> enumerator = edmStructuredTypeReference.StructuredDefinition().Properties().GetEnumerator();
				using (enumerator)
				{
					Func<IEdmPropertyConstructor, bool> func = null;
					while (enumerator.MoveNext())
					{
						IEdmProperty current = enumerator.Current;
						IEnumerable<IEdmPropertyConstructor> properties = expression.Properties;
						if (func == null)
						{
							func = (IEdmPropertyConstructor p) => p.Name == current.Name;
						}
						IEdmPropertyConstructor edmPropertyConstructor = properties.FirstOrDefault<IEdmPropertyConstructor>(func);
						if (edmPropertyConstructor != null)
						{
							if (!edmPropertyConstructor.Value.TryAssertType(current.Type, out edmErrors))
							{
								IEnumerator<EdmError> enumerator1 = edmErrors.GetEnumerator();
								using (enumerator1)
								{
									while (enumerator1.MoveNext())
									{
										EdmError edmError = enumerator1.Current;
										edmErrors1.Add(edmError);
									}
								}
							}
							strs.Add(current.Name);
						}
						else
						{
							edmErrors1.Add(new EdmError(expression.Location(), EdmErrorCode.RecordExpressionMissingRequiredProperty, Strings.EdmModel_Validator_Semantic_RecordExpressionMissingProperty(current.Name)));
						}
					}
				}
				if (!edmStructuredTypeReference.IsOpen())
				{
					foreach (IEdmPropertyConstructor property in expression.Properties)
					{
						if (strs.Contains(property.Name))
						{
							continue;
						}
						edmErrors1.Add(new EdmError(expression.Location(), EdmErrorCode.RecordExpressionHasExtraProperties, Strings.EdmModel_Validator_Semantic_RecordExpressionHasExtraProperties(property.Name)));
					}
				}
				if (edmErrors1.FirstOrDefault<EdmError>() == null)
				{
					discoveredErrors = Enumerable.Empty<EdmError>();
					return true;
				}
				else
				{
					discoveredErrors = edmErrors1;
					return false;
				}
			}
			else
			{
				EdmError[] edmErrorArray = new EdmError[1];
				edmErrorArray[0] = new EdmError(expression.Location(), EdmErrorCode.RecordExpressionNotValidForNonStructuredType, Strings.EdmModel_Validator_Semantic_RecordExpressionNotValidForNonStructuredType);
				discoveredErrors = edmErrorArray;
				return false;
			}
		}

		private static bool TryAssertStringConstantAsType(IEdmStringConstantExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			if (type.IsString())
			{
				IEdmStringTypeReference edmStringTypeReference = type.AsString();
				int? maxLength = edmStringTypeReference.MaxLength;
				if (maxLength.HasValue)
				{
					int? nullable = edmStringTypeReference.MaxLength;
					if (expression.Value.Length > nullable.Value)
					{
						EdmError[] edmError = new EdmError[1];
						int? maxLength1 = edmStringTypeReference.MaxLength;
						edmError[0] = new EdmError(expression.Location(), EdmErrorCode.StringConstantLengthOutOfRange, Strings.EdmModel_Validator_Semantic_StringConstantLengthOutOfRange(expression.Value.Length, maxLength1.Value));
						discoveredErrors = edmError;
						return false;
					}
				}
				discoveredErrors = Enumerable.Empty<EdmError>();
				return true;
			}
			else
			{
				EdmError[] edmErrorArray = new EdmError[1];
				edmErrorArray[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
				discoveredErrors = edmErrorArray;
				return false;
			}
		}

		private static bool TryAssertTimeConstantAsType(IEdmTimeConstantExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			if (type.IsTime())
			{
				discoveredErrors = Enumerable.Empty<EdmError>();
				return true;
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionPrimitiveKindNotValidForAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
				discoveredErrors = edmError;
				return false;
			}
		}

		public static bool TryAssertType(this IEdmExpression expression, IEdmTypeReference type, out IEnumerable<EdmError> discoveredErrors)
		{
			EdmError[] edmError;
			EdmUtil.CheckArgumentNull<IEdmExpression>(expression, "expression");
			if (type == null || type.TypeKind() == EdmTypeKind.None)
			{
				discoveredErrors = Enumerable.Empty<EdmError>();
				return true;
			}
			else
			{
				EdmExpressionKind expressionKind = expression.ExpressionKind;
				switch (expressionKind)
				{
					case EdmExpressionKind.BinaryConstant:
					case EdmExpressionKind.BooleanConstant:
					case EdmExpressionKind.DateTimeConstant:
					case EdmExpressionKind.DateTimeOffsetConstant:
					case EdmExpressionKind.DecimalConstant:
					case EdmExpressionKind.FloatingConstant:
					case EdmExpressionKind.GuidConstant:
					case EdmExpressionKind.IntegerConstant:
					case EdmExpressionKind.StringConstant:
					case EdmExpressionKind.TimeConstant:
					{
						IEdmPrimitiveValue edmPrimitiveValue = (IEdmPrimitiveValue)expression;
						if (edmPrimitiveValue.Type == null)
						{
							return edmPrimitiveValue.TryAssertPrimitiveAsType(type, out discoveredErrors);
						}
						else
						{
							return edmPrimitiveValue.Type.TestTypeMatch(type, expression.Location(), out discoveredErrors);
						}
					}
					case EdmExpressionKind.Null:
					{
						return ((IEdmNullExpression)expression).TryAssertNullAsType(type, out discoveredErrors);
					}
					case EdmExpressionKind.Record:
					{
						IEdmRecordExpression edmRecordExpression = (IEdmRecordExpression)expression;
						if (edmRecordExpression.DeclaredType == null)
						{
							return edmRecordExpression.TryAssertRecordAsType(type, out discoveredErrors);
						}
						else
						{
							return edmRecordExpression.DeclaredType.TestTypeMatch(type, expression.Location(), out discoveredErrors);
						}
					}
					case EdmExpressionKind.Collection:
					{
						IEdmCollectionExpression edmCollectionExpression = (IEdmCollectionExpression)expression;
						if (edmCollectionExpression.DeclaredType == null)
						{
							return edmCollectionExpression.TryAssertCollectionAsType(type, out discoveredErrors);
						}
						else
						{
							return edmCollectionExpression.DeclaredType.TestTypeMatch(type, expression.Location(), out discoveredErrors);
						}
					}
					case EdmExpressionKind.Path:
					{
						return ((IEdmPathExpression)expression).TryAssertPathAsType(type, out discoveredErrors);
					}
					case EdmExpressionKind.ParameterReference:
					case EdmExpressionKind.FunctionReference:
					case EdmExpressionKind.PropertyReference:
					case EdmExpressionKind.ValueTermReference:
					case EdmExpressionKind.EntitySetReference:
					case EdmExpressionKind.EnumMemberReference:
					{
						edmError = new EdmError[1];
						edmError[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionNotValidForTheAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionNotValidForTheAssertedType);
						discoveredErrors = edmError;
						return false;
					}
					case EdmExpressionKind.If:
					{
						return ((IEdmIfExpression)expression).TryAssertIfAsType(type, out discoveredErrors);
					}
					case EdmExpressionKind.AssertType:
					{
						return ((IEdmAssertTypeExpression)expression).Type.TestTypeMatch(type, expression.Location(), out discoveredErrors);
					}
					case EdmExpressionKind.IsType:
					{
						return EdmCoreModel.Instance.GetBoolean(false).TestTypeMatch(type, expression.Location(), out discoveredErrors);
					}
					case EdmExpressionKind.FunctionApplication:
					{
						IEdmApplyExpression edmApplyExpression = (IEdmApplyExpression)expression;
						if (edmApplyExpression.AppliedFunction != null)
						{
							IEdmFunctionBase appliedFunction = edmApplyExpression.AppliedFunction as IEdmFunctionBase;
							if (appliedFunction != null)
							{
								return appliedFunction.ReturnType.TestTypeMatch(type, expression.Location(), out discoveredErrors);
							}
						}
						discoveredErrors = Enumerable.Empty<EdmError>();
						return true;
					}
					case EdmExpressionKind.LabeledExpressionReference:
					{
						return ((IEdmLabeledExpressionReferenceExpression)expression).ReferencedLabeledExpression.TryAssertType(type, out discoveredErrors);
					}
					case EdmExpressionKind.Labeled:
					{
						return ((IEdmLabeledExpression)expression).Expression.TryAssertType(type, out discoveredErrors);
					}
					default:
					{
						edmError = new EdmError[1];
						edmError[0] = new EdmError(expression.Location(), EdmErrorCode.ExpressionNotValidForTheAssertedType, Strings.EdmModel_Validator_Semantic_ExpressionNotValidForTheAssertedType);
						discoveredErrors = edmError;
						return false;
					}
				}
			}
		}
	}
}