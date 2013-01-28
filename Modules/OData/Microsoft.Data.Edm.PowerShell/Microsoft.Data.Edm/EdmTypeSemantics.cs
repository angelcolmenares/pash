using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm
{
	internal static class EdmTypeSemantics
	{
		public static IEdmRowTypeReference ApplyType(this IEdmRowType rowType, bool isNullable)
		{
			EdmUtil.CheckArgumentNull<IEdmRowType>(rowType, "type");
			return new EdmRowTypeReference(rowType, isNullable);
		}

		public static IEdmBinaryTypeReference AsBinary(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmBinaryTypeReference edmBinaryTypeReference = type as IEdmBinaryTypeReference;
			if (edmBinaryTypeReference == null)
			{
				string str = type.FullName();
				List<EdmError> edmErrors = new List<EdmError>(type.Errors());
				if (edmErrors.Count == 0)
				{
					edmErrors.AddRange(EdmTypeSemantics.ConversionError(type.Location(), str, "Binary"));
				}
				return new BadBinaryTypeReference(str, type.IsNullable, edmErrors);
			}
			else
			{
				return edmBinaryTypeReference;
			}
		}

		public static IEdmCollectionTypeReference AsCollection(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmCollectionTypeReference edmCollectionTypeReference = type as IEdmCollectionTypeReference;
			if (edmCollectionTypeReference == null)
			{
				IEdmType definition = type.Definition;
				if (definition.TypeKind != EdmTypeKind.Collection)
				{
					List<EdmError> edmErrors = new List<EdmError>(type.Errors());
					if (edmErrors.Count == 0)
					{
						edmErrors.AddRange(EdmTypeSemantics.ConversionError(type.Location(), type.FullName(), "Collection"));
					}
					return new EdmCollectionTypeReference(new BadCollectionType(edmErrors), type.IsNullable);
				}
				else
				{
					return new EdmCollectionTypeReference((IEdmCollectionType)definition, type.IsNullable);
				}
			}
			else
			{
				return edmCollectionTypeReference;
			}
		}

		public static IEdmComplexTypeReference AsComplex(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmComplexTypeReference edmComplexTypeReference = type as IEdmComplexTypeReference;
			if (edmComplexTypeReference == null)
			{
				IEdmType definition = type.Definition;
				if (definition.TypeKind != EdmTypeKind.Complex)
				{
					string str = type.FullName();
					List<EdmError> edmErrors = new List<EdmError>(type.Errors());
					if (edmErrors.Count == 0)
					{
						edmErrors.AddRange(EdmTypeSemantics.ConversionError(type.Location(), str, "Complex"));
					}
					return new BadComplexTypeReference(str, type.IsNullable, edmErrors);
				}
				else
				{
					return new EdmComplexTypeReference((IEdmComplexType)definition, type.IsNullable);
				}
			}
			else
			{
				return edmComplexTypeReference;
			}
		}

		public static IEdmDecimalTypeReference AsDecimal(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmDecimalTypeReference edmDecimalTypeReference = type as IEdmDecimalTypeReference;
			if (edmDecimalTypeReference == null)
			{
				string str = type.FullName();
				List<EdmError> edmErrors = new List<EdmError>(type.Errors());
				if (edmErrors.Count == 0)
				{
					edmErrors.AddRange(EdmTypeSemantics.ConversionError(type.Location(), str, "Decimal"));
				}
				return new BadDecimalTypeReference(str, type.IsNullable, edmErrors);
			}
			else
			{
				return edmDecimalTypeReference;
			}
		}

		public static IEdmEntityTypeReference AsEntity(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmEntityTypeReference edmEntityTypeReference = type as IEdmEntityTypeReference;
			if (edmEntityTypeReference == null)
			{
				IEdmType definition = type.Definition;
				if (definition.TypeKind != EdmTypeKind.Entity)
				{
					string str = type.FullName();
					List<EdmError> edmErrors = new List<EdmError>(type.Errors());
					if (edmErrors.Count == 0)
					{
						edmErrors.AddRange(EdmTypeSemantics.ConversionError(type.Location(), str, "Entity"));
					}
					return new BadEntityTypeReference(str, type.IsNullable, edmErrors);
				}
				else
				{
					return new EdmEntityTypeReference((IEdmEntityType)definition, type.IsNullable);
				}
			}
			else
			{
				return edmEntityTypeReference;
			}
		}

		public static IEdmEntityReferenceTypeReference AsEntityReference(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmEntityReferenceTypeReference edmEntityReferenceTypeReference = type as IEdmEntityReferenceTypeReference;
			if (edmEntityReferenceTypeReference == null)
			{
				IEdmType definition = type.Definition;
				if (definition.TypeKind != EdmTypeKind.EntityReference)
				{
					List<EdmError> edmErrors = new List<EdmError>(type.Errors());
					if (edmErrors.Count == 0)
					{
						edmErrors.AddRange(EdmTypeSemantics.ConversionError(type.Location(), type.FullName(), "EntityReference"));
					}
					return new EdmEntityReferenceTypeReference(new BadEntityReferenceType(edmErrors), type.IsNullable);
				}
				else
				{
					return new EdmEntityReferenceTypeReference((IEdmEntityReferenceType)definition, type.IsNullable);
				}
			}
			else
			{
				return edmEntityReferenceTypeReference;
			}
		}

		public static IEdmEnumTypeReference AsEnum(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmEnumTypeReference edmEnumTypeReference = type as IEdmEnumTypeReference;
			if (edmEnumTypeReference == null)
			{
				IEdmType definition = type.Definition;
				if (definition.TypeKind != EdmTypeKind.Enum)
				{
					string str = type.FullName();
					return new EdmEnumTypeReference(new BadEnumType(str, EdmTypeSemantics.ConversionError(type.Location(), str, "Enum")), type.IsNullable);
				}
				else
				{
					return new EdmEnumTypeReference((IEdmEnumType)definition, type.IsNullable);
				}
			}
			else
			{
				return edmEnumTypeReference;
			}
		}

		public static IEdmPrimitiveTypeReference AsPrimitive(this IEdmTypeReference type)
		{
			string str;
			List<EdmError> edmErrors;
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmPrimitiveTypeReference edmPrimitiveTypeReference = type as IEdmPrimitiveTypeReference;
			if (edmPrimitiveTypeReference == null)
			{
				IEdmType definition = type.Definition;
				if (definition.TypeKind == EdmTypeKind.Primitive)
				{
					IEdmPrimitiveType edmPrimitiveType = definition as IEdmPrimitiveType;
					if (edmPrimitiveType != null)
					{
						EdmPrimitiveTypeKind primitiveKind = edmPrimitiveType.PrimitiveKind;
						if (primitiveKind == EdmPrimitiveTypeKind.None)
						{
							str = type.FullName();
							edmErrors = new List<EdmError>(type.Errors());
							if (edmErrors.Count == 0)
							{
								edmErrors.AddRange(EdmTypeSemantics.ConversionError(type.Location(), str, "Primitive"));
							}
							return new BadPrimitiveTypeReference(str, type.IsNullable, edmErrors);
						}
						else if (primitiveKind == EdmPrimitiveTypeKind.Binary)
						{
							return type.AsBinary();
						}
						else if (primitiveKind == EdmPrimitiveTypeKind.Boolean || primitiveKind == EdmPrimitiveTypeKind.Byte || primitiveKind == EdmPrimitiveTypeKind.Double || primitiveKind == EdmPrimitiveTypeKind.Guid || primitiveKind == EdmPrimitiveTypeKind.Int16 || primitiveKind == EdmPrimitiveTypeKind.Int32 || primitiveKind == EdmPrimitiveTypeKind.Int64 || primitiveKind == EdmPrimitiveTypeKind.SByte || primitiveKind == EdmPrimitiveTypeKind.Single || primitiveKind == EdmPrimitiveTypeKind.Stream)
						{
							return new EdmPrimitiveTypeReference(edmPrimitiveType, type.IsNullable);
						}
						else if (primitiveKind == EdmPrimitiveTypeKind.DateTime || primitiveKind == EdmPrimitiveTypeKind.DateTimeOffset || primitiveKind == EdmPrimitiveTypeKind.Time)
						{
							return type.AsTemporal();
						}
						else if (primitiveKind == EdmPrimitiveTypeKind.Decimal)
						{
							return type.AsDecimal();
						}
						else if (primitiveKind == EdmPrimitiveTypeKind.String)
						{
							return type.AsString();
						}
						else if (primitiveKind == EdmPrimitiveTypeKind.Geography || primitiveKind == EdmPrimitiveTypeKind.GeographyPoint || primitiveKind == EdmPrimitiveTypeKind.GeographyLineString || primitiveKind == EdmPrimitiveTypeKind.GeographyPolygon || primitiveKind == EdmPrimitiveTypeKind.GeographyCollection || primitiveKind == EdmPrimitiveTypeKind.GeographyMultiPolygon || primitiveKind == EdmPrimitiveTypeKind.GeographyMultiLineString || primitiveKind == EdmPrimitiveTypeKind.GeographyMultiPoint || primitiveKind == EdmPrimitiveTypeKind.Geometry || primitiveKind == EdmPrimitiveTypeKind.GeometryPoint || primitiveKind == EdmPrimitiveTypeKind.GeometryLineString || primitiveKind == EdmPrimitiveTypeKind.GeometryPolygon || primitiveKind == EdmPrimitiveTypeKind.GeometryCollection || primitiveKind == EdmPrimitiveTypeKind.GeometryMultiPolygon || primitiveKind == EdmPrimitiveTypeKind.GeometryMultiLineString || primitiveKind == EdmPrimitiveTypeKind.GeometryMultiPoint)
						{
							return type.AsSpatial();
						}
					}
				}
				str = type.FullName();
				edmErrors = new List<EdmError>(type.Errors());
				if (edmErrors.Count == 0)
				{
					edmErrors.AddRange(EdmTypeSemantics.ConversionError(type.Location(), str, "Primitive"));
				}
				return new BadPrimitiveTypeReference(str, type.IsNullable, edmErrors);
			}
			else
			{
				return edmPrimitiveTypeReference;
			}
		}

		public static IEdmRowTypeReference AsRow(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmRowTypeReference edmRowTypeReference = type as IEdmRowTypeReference;
			if (edmRowTypeReference == null)
			{
				IEdmType definition = type.Definition;
				if (definition.TypeKind != EdmTypeKind.Row)
				{
					List<EdmError> edmErrors = new List<EdmError>(type.Errors());
					if (edmErrors.Count == 0)
					{
						edmErrors.AddRange(EdmTypeSemantics.ConversionError(type.Location(), type.FullName(), "Row"));
					}
					return new EdmRowTypeReference(new BadRowType(edmErrors), type.IsNullable);
				}
				else
				{
					return new EdmRowTypeReference((IEdmRowType)definition, type.IsNullable);
				}
			}
			else
			{
				return edmRowTypeReference;
			}
		}

		public static IEdmSpatialTypeReference AsSpatial(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmSpatialTypeReference edmSpatialTypeReference = type as IEdmSpatialTypeReference;
			if (edmSpatialTypeReference == null)
			{
				string str = type.FullName();
				List<EdmError> edmErrors = new List<EdmError>(type.Errors());
				if (edmErrors.Count == 0)
				{
					edmErrors.AddRange(EdmTypeSemantics.ConversionError(type.Location(), str, "Spatial"));
				}
				return new BadSpatialTypeReference(str, type.IsNullable, edmErrors);
			}
			else
			{
				return edmSpatialTypeReference;
			}
		}

		public static IEdmStringTypeReference AsString(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmStringTypeReference edmStringTypeReference = type as IEdmStringTypeReference;
			if (edmStringTypeReference == null)
			{
				string str = type.FullName();
				List<EdmError> edmErrors = new List<EdmError>(type.Errors());
				if (edmErrors.Count == 0)
				{
					edmErrors.AddRange(EdmTypeSemantics.ConversionError(type.Location(), str, "String"));
				}
				return new BadStringTypeReference(str, type.IsNullable, edmErrors);
			}
			else
			{
				return edmStringTypeReference;
			}
		}

		public static IEdmStructuredTypeReference AsStructured(this IEdmTypeReference type)
		{
			string str;
			List<EdmError> edmErrors;
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmStructuredTypeReference edmStructuredTypeReference = type as IEdmStructuredTypeReference;
			if (edmStructuredTypeReference == null)
			{
				EdmTypeKind edmTypeKind = type.TypeKind();
				switch (edmTypeKind)
				{
					case EdmTypeKind.Entity:
					{
						return type.AsEntity();
					}
					case EdmTypeKind.Complex:
					{
						return type.AsComplex();
					}
					case EdmTypeKind.Row:
					{
						return type.AsRow();
					}
					default:
					{
						str = type.FullName();
						edmErrors = new List<EdmError>(type.TypeErrors());
						if (edmErrors.Count != 0)
						{
							break;
						}
						edmErrors.AddRange(EdmTypeSemantics.ConversionError(type.Location(), str, "Structured"));
						break;
					}
				}
				return new BadEntityTypeReference(str, type.IsNullable, edmErrors);
			}
			else
			{
				return edmStructuredTypeReference;
			}
		}

		public static IEdmTemporalTypeReference AsTemporal(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmTemporalTypeReference edmTemporalTypeReference = type as IEdmTemporalTypeReference;
			if (edmTemporalTypeReference == null)
			{
				string str = type.FullName();
				List<EdmError> edmErrors = new List<EdmError>(type.Errors());
				if (edmErrors.Count == 0)
				{
					edmErrors.AddRange(EdmTypeSemantics.ConversionError(type.Location(), str, "Temporal"));
				}
				return new BadTemporalTypeReference(str, type.IsNullable, edmErrors);
			}
			else
			{
				return edmTemporalTypeReference;
			}
		}

		private static IEnumerable<EdmError> ConversionError(EdmLocation location, string typeName, string typeKindName)
		{
			EdmError[] edmErrorArray = new EdmError[1];
			EdmError[] edmError = edmErrorArray;
			int num = 0;
			EdmLocation edmLocation = location;
			int num1 = 230;
			string str = typeName;
			object obj = str;
			if (str == null)
			{
				obj = "UnnamedType";
			}
			edmError[num] = new EdmError(edmLocation, (EdmErrorCode)num1, Strings.TypeSemantics_CouldNotConvertTypeReference(obj, typeKindName));
			return edmErrorArray;
		}

		internal static IEdmPrimitiveTypeReference GetPrimitiveTypeReference(this IEdmPrimitiveType type, bool isNullable)
		{
			EdmPrimitiveTypeKind primitiveKind = type.PrimitiveKind;
			switch (primitiveKind)
			{
				case EdmPrimitiveTypeKind.Binary:
				{
					return new EdmBinaryTypeReference(type, isNullable);
				}
				case EdmPrimitiveTypeKind.Boolean:
				case EdmPrimitiveTypeKind.Byte:
				case EdmPrimitiveTypeKind.Double:
				case EdmPrimitiveTypeKind.Guid:
				case EdmPrimitiveTypeKind.Int16:
				case EdmPrimitiveTypeKind.Int32:
				case EdmPrimitiveTypeKind.Int64:
				case EdmPrimitiveTypeKind.SByte:
				case EdmPrimitiveTypeKind.Single:
				case EdmPrimitiveTypeKind.Stream:
				{
					return new EdmPrimitiveTypeReference(type, isNullable);
				}
				case EdmPrimitiveTypeKind.DateTime:
				case EdmPrimitiveTypeKind.DateTimeOffset:
				case EdmPrimitiveTypeKind.Time:
				{
					return new EdmTemporalTypeReference(type, isNullable);
				}
				case EdmPrimitiveTypeKind.Decimal:
				{
					return new EdmDecimalTypeReference(type, isNullable);
				}
				case EdmPrimitiveTypeKind.String:
				{
					return new EdmStringTypeReference(type, isNullable);
				}
				case EdmPrimitiveTypeKind.Geography:
				case EdmPrimitiveTypeKind.GeographyPoint:
				case EdmPrimitiveTypeKind.GeographyLineString:
				case EdmPrimitiveTypeKind.GeographyPolygon:
				case EdmPrimitiveTypeKind.GeographyCollection:
				case EdmPrimitiveTypeKind.GeographyMultiPolygon:
				case EdmPrimitiveTypeKind.GeographyMultiLineString:
				case EdmPrimitiveTypeKind.GeographyMultiPoint:
				case EdmPrimitiveTypeKind.Geometry:
				case EdmPrimitiveTypeKind.GeometryPoint:
				case EdmPrimitiveTypeKind.GeometryLineString:
				case EdmPrimitiveTypeKind.GeometryPolygon:
				case EdmPrimitiveTypeKind.GeometryCollection:
				case EdmPrimitiveTypeKind.GeometryMultiPolygon:
				case EdmPrimitiveTypeKind.GeometryMultiLineString:
				case EdmPrimitiveTypeKind.GeometryMultiPoint:
				{
					return new EdmSpatialTypeReference(type, isNullable);
				}
			}
			throw new InvalidOperationException(Strings.EdmPrimitive_UnexpectedKind);
		}

		public static bool InheritsFrom(this IEdmStructuredType type, IEdmStructuredType potentialBaseType)
		{
			do
			{
				type = type.BaseType;
				if (!type.IsEquivalentTo(potentialBaseType))
				{
					continue;
				}
				return true;
			}
			while (type != null);
			return false;
		}

		public static bool IsBinary(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.Binary;
		}

		public static bool IsBoolean(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.Boolean;
		}

		public static bool IsByte(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.Byte;
		}

		public static bool IsCollection(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.TypeKind() == EdmTypeKind.Collection;
		}

		public static bool IsComplex(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.TypeKind() == EdmTypeKind.Complex;
		}

		public static bool IsDateTime(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.DateTime;
		}

		public static bool IsDateTimeOffset(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.DateTimeOffset;
		}

		public static bool IsDecimal(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.Decimal;
		}

		public static bool IsDouble(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.Double;
		}

		public static bool IsEntity(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.TypeKind() == EdmTypeKind.Entity;
		}

		public static bool IsEntityReference(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.TypeKind() == EdmTypeKind.EntityReference;
		}

		public static bool IsEnum(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.TypeKind() == EdmTypeKind.Enum;
		}

		public static bool IsFloating(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = type.PrimitiveKind();
			if (edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Double || edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Single)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool IsGuid(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.Guid;
		}

		public static bool IsInt16(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.Int16;
		}

		public static bool IsInt32(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.Int32;
		}

		public static bool IsInt64(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.Int64;
		}

		public static bool IsIntegral(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = type.PrimitiveKind();
			if (edmPrimitiveTypeKind != EdmPrimitiveTypeKind.Byte)
			{
				if (edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Int16 || edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Int32 || edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Int64 || edmPrimitiveTypeKind == EdmPrimitiveTypeKind.SByte)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public static bool IsIntegral(this EdmPrimitiveTypeKind primitiveTypeKind)
		{
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = primitiveTypeKind;
			if (edmPrimitiveTypeKind != EdmPrimitiveTypeKind.Byte)
			{
				if (edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Int16 || edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Int32 || edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Int64 || edmPrimitiveTypeKind == EdmPrimitiveTypeKind.SByte)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public static bool IsOrInheritsFrom(this IEdmType thisType, IEdmType otherType)
		{
			if (thisType == null || otherType == null)
			{
				return false;
			}
			else
			{
				if (!thisType.IsEquivalentTo(otherType))
				{
					EdmTypeKind typeKind = thisType.TypeKind;
					if (typeKind != otherType.TypeKind || typeKind != EdmTypeKind.Entity && typeKind != EdmTypeKind.Complex && typeKind != EdmTypeKind.Row)
					{
						return false;
					}
					else
					{
						return thisType.IsOrInheritsFrom(otherType);
					}
				}
				else
				{
					return true;
				}
			}
		}

		public static bool IsPrimitive(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.TypeKind() == EdmTypeKind.Primitive;
		}

		public static bool IsRow(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.TypeKind() == EdmTypeKind.Row;
		}

		public static bool IsSByte(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.SByte;
		}

		public static bool IsSignedIntegral(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = type.PrimitiveKind();
			switch (edmPrimitiveTypeKind)
			{
				case EdmPrimitiveTypeKind.Int16:
				case EdmPrimitiveTypeKind.Int32:
				case EdmPrimitiveTypeKind.Int64:
				case EdmPrimitiveTypeKind.SByte:
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsSingle(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.Single;
		}

		public static bool IsSpatial(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.Definition.IsSpatial();
		}

		public static bool IsSpatial(this IEdmType type)
		{
			EdmUtil.CheckArgumentNull<IEdmType>(type, "type");
			IEdmPrimitiveType edmPrimitiveType = type as IEdmPrimitiveType;
			if (edmPrimitiveType != null)
			{
				return edmPrimitiveType.PrimitiveKind.IsSpatial();
			}
			else
			{
				return false;
			}
		}

		public static bool IsSpatial(this EdmPrimitiveTypeKind typeKind)
		{
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = typeKind;
			switch (edmPrimitiveTypeKind)
			{
				case EdmPrimitiveTypeKind.Geography:
				case EdmPrimitiveTypeKind.GeographyPoint:
				case EdmPrimitiveTypeKind.GeographyLineString:
				case EdmPrimitiveTypeKind.GeographyPolygon:
				case EdmPrimitiveTypeKind.GeographyCollection:
				case EdmPrimitiveTypeKind.GeographyMultiPolygon:
				case EdmPrimitiveTypeKind.GeographyMultiLineString:
				case EdmPrimitiveTypeKind.GeographyMultiPoint:
				case EdmPrimitiveTypeKind.Geometry:
				case EdmPrimitiveTypeKind.GeometryPoint:
				case EdmPrimitiveTypeKind.GeometryLineString:
				case EdmPrimitiveTypeKind.GeometryPolygon:
				case EdmPrimitiveTypeKind.GeometryCollection:
				case EdmPrimitiveTypeKind.GeometryMultiPolygon:
				case EdmPrimitiveTypeKind.GeometryMultiLineString:
				case EdmPrimitiveTypeKind.GeometryMultiPoint:
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsStream(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.Stream;
		}

		public static bool IsString(this IEdmTypeReference type)
		{
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.String;
		}

		public static bool IsStructured(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			EdmTypeKind edmTypeKind = type.TypeKind();
			switch (edmTypeKind)
			{
				case EdmTypeKind.Entity:
				case EdmTypeKind.Complex:
				case EdmTypeKind.Row:
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsStructured(this EdmTypeKind typeKind)
		{
			EdmTypeKind edmTypeKind = typeKind;
			switch (edmTypeKind)
			{
				case EdmTypeKind.Entity:
				case EdmTypeKind.Complex:
				case EdmTypeKind.Row:
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsTemporal(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind().IsTemporal();
		}

		public static bool IsTemporal(this EdmPrimitiveTypeKind typeKind)
		{
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = typeKind;
			switch (edmPrimitiveTypeKind)
			{
				case EdmPrimitiveTypeKind.DateTime:
				case EdmPrimitiveTypeKind.DateTimeOffset:
				{
					return true;
				}
				default:
				{
					if (edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Time)
					{
						return true;
					}
					return false;
				}
			}
		}

		public static bool IsTime(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return type.PrimitiveKind() == EdmPrimitiveTypeKind.Time;
		}

		public static EdmPrimitiveTypeKind PrimitiveKind(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmType definition = type.Definition;
			if (definition.TypeKind == EdmTypeKind.Primitive)
			{
				return ((IEdmPrimitiveType)definition).PrimitiveKind;
			}
			else
			{
				return EdmPrimitiveTypeKind.None;
			}
		}
	}
}