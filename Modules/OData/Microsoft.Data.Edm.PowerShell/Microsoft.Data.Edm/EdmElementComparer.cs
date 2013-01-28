using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm
{
	internal static class EdmElementComparer
	{
		public static bool IsEquivalentTo(this IEdmType thisType, IEdmType otherType)
		{
			if (thisType != otherType)
			{
				if (thisType == null || otherType == null)
				{
					return false;
				}
				else
				{
					if (thisType.TypeKind == otherType.TypeKind)
					{
						EdmTypeKind typeKind = thisType.TypeKind;
						switch (typeKind)
						{
							case EdmTypeKind.None:
							{
								return otherType.TypeKind == EdmTypeKind.None;
							}
							case EdmTypeKind.Primitive:
							{
								return thisType.IsEquivalentTo((IEdmPrimitiveType)otherType);
							}
							case EdmTypeKind.Entity:
							case EdmTypeKind.Complex:
							case EdmTypeKind.Enum:
							{
								return thisType.IsEquivalentTo((IEdmSchemaType)otherType);
							}
							case EdmTypeKind.Row:
							{
								return thisType.IsEquivalentTo((IEdmRowType)otherType);
							}
							case EdmTypeKind.Collection:
							{
								return thisType.IsEquivalentTo((IEdmCollectionType)otherType);
							}
							case EdmTypeKind.EntityReference:
							{
								return thisType.IsEquivalentTo((IEdmEntityReferenceType)otherType);
							}
						}
						throw new InvalidOperationException(Strings.UnknownEnumVal_TypeKind(thisType.TypeKind));
					}
					else
					{
						return false;
					}
				}
			}
			else
			{
				return true;
			}
		}

		public static bool IsEquivalentTo(this IEdmTypeReference thisType, IEdmTypeReference otherType)
		{
			if (thisType != otherType)
			{
				if (thisType == null || otherType == null)
				{
					return false;
				}
				else
				{
					EdmTypeKind edmTypeKind = thisType.TypeKind();
					if (edmTypeKind == otherType.TypeKind())
					{
						if (edmTypeKind != EdmTypeKind.Primitive)
						{
							if (thisType.IsNullable != otherType.IsNullable)
							{
								return false;
							}
							else
							{
								return thisType.Definition.IsEquivalentTo(otherType.Definition);
							}
						}
						else
						{
							return thisType.IsEquivalentTo((IEdmPrimitiveTypeReference)otherType);
						}
					}
					else
					{
						return false;
					}
				}
			}
			else
			{
				return true;
			}
		}

		private static bool IsEquivalentTo(this IEdmFunctionParameter thisParameter, IEdmFunctionParameter otherParameter)
		{
			if (thisParameter != otherParameter)
			{
				if (thisParameter == null || otherParameter == null)
				{
					return false;
				}
				else
				{
					if (!(thisParameter.Name == otherParameter.Name) || thisParameter.Mode != otherParameter.Mode)
					{
						return false;
					}
					else
					{
						return thisParameter.Type.IsEquivalentTo(otherParameter.Type);
					}
				}
			}
			else
			{
				return true;
			}
		}

		private static bool IsEquivalentTo(this IEdmPrimitiveType thisType, IEdmPrimitiveType otherType)
		{
			if (thisType.PrimitiveKind != otherType.PrimitiveKind)
			{
				return false;
			}
			else
			{
				return thisType.FullName() == otherType.FullName();
			}
		}

		private static bool IsEquivalentTo(this IEdmSchemaType thisType, IEdmSchemaType otherType)
		{
			return object.ReferenceEquals(thisType, otherType);
		}

		private static bool IsEquivalentTo(this IEdmCollectionType thisType, IEdmCollectionType otherType)
		{
			return thisType.ElementType.IsEquivalentTo(otherType.ElementType);
		}

		private static bool IsEquivalentTo(this IEdmEntityReferenceType thisType, IEdmEntityReferenceType otherType)
		{
			return thisType.EntityType.IsEquivalentTo(otherType.EntityType);
		}

		private static bool IsEquivalentTo(this IEdmRowType thisType, IEdmRowType otherType)
		{
			bool flag;
			if (thisType.DeclaredProperties.Count<IEdmProperty>() == otherType.DeclaredProperties.Count<IEdmProperty>())
			{
				IEnumerator<IEdmProperty> enumerator = thisType.DeclaredProperties.GetEnumerator();
				IEnumerator<IEdmProperty> enumerator1 = otherType.DeclaredProperties.GetEnumerator();
				using (enumerator1)
				{
					while (enumerator1.MoveNext())
					{
						IEdmProperty current = enumerator1.Current;
						enumerator.MoveNext();
						if (enumerator.Current.DeclaringType.IsEquivalentTo(current.DeclaringType))
						{
							continue;
						}
						flag = false;
						return flag;
					}
					return true;
				}
				return flag;
			}
			else
			{
				return false;
			}
		}

		private static bool IsEquivalentTo(this IEdmStructuralProperty thisProp, IEdmStructuralProperty otherProp)
		{
			if (thisProp != otherProp)
			{
				if (thisProp == null || otherProp == null)
				{
					return false;
				}
				else
				{
					if (thisProp.Name != otherProp.Name)
					{
						return false;
					}
					else
					{
						return thisProp.Type.IsEquivalentTo(otherProp.Type);
					}
				}
			}
			else
			{
				return true;
			}
		}

		private static bool IsEquivalentTo(this IEdmPrimitiveTypeReference thisType, IEdmPrimitiveTypeReference otherType)
		{
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = thisType.PrimitiveKind();
			if (edmPrimitiveTypeKind == otherType.PrimitiveKind())
			{
				EdmPrimitiveTypeKind edmPrimitiveTypeKind1 = edmPrimitiveTypeKind;
				switch (edmPrimitiveTypeKind1)
				{
					case EdmPrimitiveTypeKind.Binary:
					{
						return thisType.IsEquivalentTo((IEdmBinaryTypeReference)otherType);
					}
					case EdmPrimitiveTypeKind.Boolean:
					case EdmPrimitiveTypeKind.Byte:
					{
						if (edmPrimitiveTypeKind.IsSpatial())
						{
							break;
						}
						if (thisType.IsNullable != otherType.IsNullable)
						{
							return false;
						}
						else
						{
							return thisType.Definition.IsEquivalentTo(otherType.Definition);
						}
					}
					case EdmPrimitiveTypeKind.DateTime:
					case EdmPrimitiveTypeKind.DateTimeOffset:
					{
						return thisType.IsEquivalentTo((IEdmTemporalTypeReference)otherType);
					}
					case EdmPrimitiveTypeKind.Decimal:
					{
						return thisType.IsEquivalentTo((IEdmDecimalTypeReference)otherType);
					}
					default:
					{
						if (edmPrimitiveTypeKind1 == EdmPrimitiveTypeKind.String)
						{
							return thisType.IsEquivalentTo((IEdmStringTypeReference)otherType);
						}
						else if (edmPrimitiveTypeKind1 == EdmPrimitiveTypeKind.Stream)
						{
							if (edmPrimitiveTypeKind.IsSpatial())
							{
								break;
							}
							if (thisType.IsNullable != otherType.IsNullable)
							{
								return false;
							}
							else
							{
								return thisType.Definition.IsEquivalentTo(otherType.Definition);
							}
						}
						else if (edmPrimitiveTypeKind1 == EdmPrimitiveTypeKind.Time)
						{
							return thisType.IsEquivalentTo((IEdmTemporalTypeReference)otherType);
						}
						if (edmPrimitiveTypeKind.IsSpatial())
						{
							break;
						}
						if (thisType.IsNullable != otherType.IsNullable)
						{
							return false;
						}
						else
						{
							return thisType.Definition.IsEquivalentTo(otherType.Definition);
						}
					}
				}
				return thisType.IsEquivalentTo((IEdmSpatialTypeReference)otherType);
			}
			else
			{
				return false;
			}
		}

		private static bool IsEquivalentTo(this IEdmBinaryTypeReference thisType, IEdmBinaryTypeReference otherType)
		{
			bool hasValue;
			if (thisType.IsNullable == otherType.IsNullable)
			{
				bool? isFixedLength = thisType.IsFixedLength;
				bool? nullable = otherType.IsFixedLength;
				if (isFixedLength.GetValueOrDefault() != nullable.GetValueOrDefault())
				{
					hasValue = false;
				}
				else
				{
					hasValue = isFixedLength.HasValue == nullable.HasValue;
				}
				if (hasValue && thisType.IsUnbounded == otherType.IsUnbounded)
				{
					int? maxLength = thisType.MaxLength;
					int? maxLength1 = otherType.MaxLength;
					if (maxLength.GetValueOrDefault() != maxLength1.GetValueOrDefault())
					{
						return false;
					}
					else
					{
						return maxLength.HasValue == maxLength1.HasValue;
					}
				}
			}
			return false;
		}

		private static bool IsEquivalentTo(this IEdmDecimalTypeReference thisType, IEdmDecimalTypeReference otherType)
		{
			bool hasValue;
			if (thisType.IsNullable == otherType.IsNullable)
			{
				int? precision = thisType.Precision;
				int? nullable = otherType.Precision;
				if (precision.GetValueOrDefault() != nullable.GetValueOrDefault())
				{
					hasValue = false;
				}
				else
				{
					hasValue = precision.HasValue == nullable.HasValue;
				}
				if (hasValue)
				{
					int? scale = thisType.Scale;
					int? scale1 = otherType.Scale;
					if (scale.GetValueOrDefault() != scale1.GetValueOrDefault())
					{
						return false;
					}
					else
					{
						return scale.HasValue == scale1.HasValue;
					}
				}
			}
			return false;
		}

		private static bool IsEquivalentTo(this IEdmTemporalTypeReference thisType, IEdmTemporalTypeReference otherType)
		{
			if (thisType.TypeKind() != otherType.TypeKind() || thisType.IsNullable != otherType.IsNullable)
			{
				return false;
			}
			else
			{
				int? precision = thisType.Precision;
				int? nullable = otherType.Precision;
				if (precision.GetValueOrDefault() != nullable.GetValueOrDefault())
				{
					return false;
				}
				else
				{
					return precision.HasValue == nullable.HasValue;
				}
			}
		}

		private static bool IsEquivalentTo(this IEdmStringTypeReference thisType, IEdmStringTypeReference otherType)
		{
			bool hasValue;
			bool flag;
			bool hasValue1;
			if (thisType.IsNullable == otherType.IsNullable)
			{
				bool? isFixedLength = thisType.IsFixedLength;
				bool? nullable = otherType.IsFixedLength;
				if (isFixedLength.GetValueOrDefault() != nullable.GetValueOrDefault())
				{
					hasValue = false;
				}
				else
				{
					hasValue = isFixedLength.HasValue == nullable.HasValue;
				}
				if (hasValue && thisType.IsUnbounded == otherType.IsUnbounded)
				{
					int? maxLength = thisType.MaxLength;
					int? maxLength1 = otherType.MaxLength;
					if (maxLength.GetValueOrDefault() != maxLength1.GetValueOrDefault())
					{
						flag = false;
					}
					else
					{
						flag = maxLength.HasValue == maxLength1.HasValue;
					}
					if (flag)
					{
						bool? isUnicode = thisType.IsUnicode;
						bool? isUnicode1 = otherType.IsUnicode;
						if (isUnicode.GetValueOrDefault() != isUnicode1.GetValueOrDefault())
						{
							hasValue1 = false;
						}
						else
						{
							hasValue1 = isUnicode.HasValue == isUnicode1.HasValue;
						}
						if (hasValue1)
						{
							return thisType.Collation == otherType.Collation;
						}
					}
				}
			}
			return false;
		}

		private static bool IsEquivalentTo(this IEdmSpatialTypeReference thisType, IEdmSpatialTypeReference otherType)
		{
			if (thisType.IsNullable != otherType.IsNullable)
			{
				return false;
			}
			else
			{
				int? spatialReferenceIdentifier = thisType.SpatialReferenceIdentifier;
				int? nullable = otherType.SpatialReferenceIdentifier;
				if (spatialReferenceIdentifier.GetValueOrDefault() != nullable.GetValueOrDefault())
				{
					return false;
				}
				else
				{
					return spatialReferenceIdentifier.HasValue == nullable.HasValue;
				}
			}
		}

		internal static bool IsFunctionSignatureEquivalentTo(this IEdmFunctionBase thisFunction, IEdmFunctionBase otherFunction)
		{
			bool flag;
			if (thisFunction != otherFunction)
			{
				if (thisFunction.Name == otherFunction.Name)
				{
					if (thisFunction.ReturnType.IsEquivalentTo(otherFunction.ReturnType))
					{
						IEnumerator<IEdmFunctionParameter> enumerator = otherFunction.Parameters.GetEnumerator();
						IEnumerator<IEdmFunctionParameter> enumerator1 = thisFunction.Parameters.GetEnumerator();
						using (enumerator1)
						{
							while (enumerator1.MoveNext())
							{
								IEdmFunctionParameter current = enumerator1.Current;
								enumerator.MoveNext();
								if (current.IsEquivalentTo(enumerator.Current))
								{
									continue;
								}
								flag = false;
								return flag;
							}
							return true;
						}
						return flag;
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				return true;
			}
		}
	}
}