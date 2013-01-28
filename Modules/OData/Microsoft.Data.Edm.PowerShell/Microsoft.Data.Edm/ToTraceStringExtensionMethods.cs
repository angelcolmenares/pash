using System;
using System.Linq;
using System.Text;

namespace Microsoft.Data.Edm
{
	internal static class ToTraceStringExtensionMethods
	{
		private static void AppendBinaryFacets(this StringBuilder sb, IEdmBinaryTypeReference type)
		{
			string str;
			bool? isFixedLength = type.IsFixedLength;
			sb.AppendKeyValue("FixedLength", isFixedLength.ToString());
			if (!type.IsUnbounded)
			{
				int? maxLength = type.MaxLength;
				if (!maxLength.HasValue)
				{
					return;
				}
			}
			StringBuilder stringBuilder = sb;
			string str1 = "MaxLength";
			if (type.IsUnbounded)
			{
				str = "Max";
			}
			else
			{
				int? nullable = type.MaxLength;
				str = nullable.ToString();
			}
			stringBuilder.AppendKeyValue(str1, str);
		}

		private static void AppendDecimalFacets(this StringBuilder sb, IEdmDecimalTypeReference type)
		{
			int? precision = type.Precision;
			if (precision.HasValue)
			{
				int? nullable = type.Precision;
				sb.AppendKeyValue("Precision", nullable.ToString());
			}
			int? scale = type.Scale;
			if (scale.HasValue)
			{
				int? scale1 = type.Scale;
				sb.AppendKeyValue("Scale", scale1.ToString());
			}
		}

		private static void AppendFacets(this StringBuilder sb, IEdmPrimitiveTypeReference type)
		{
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = type.PrimitiveKind();
			switch (edmPrimitiveTypeKind)
			{
				case EdmPrimitiveTypeKind.Binary:
				{
					sb.AppendBinaryFacets(type.AsBinary());
					return;
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
					return;
				}
				case EdmPrimitiveTypeKind.DateTime:
				case EdmPrimitiveTypeKind.DateTimeOffset:
				case EdmPrimitiveTypeKind.Time:
				{
					sb.AppendTemporalFacets(type.AsTemporal());
					return;
				}
				case EdmPrimitiveTypeKind.Decimal:
				{
					sb.AppendDecimalFacets(type.AsDecimal());
					return;
				}
				case EdmPrimitiveTypeKind.String:
				{
					sb.AppendStringFacets(type.AsString());
					return;
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
					sb.AppendSpatialFacets(type.AsSpatial());
					return;
				}
				default:
				{
					return;
				}
			}
		}

		private static void AppendKeyValue(this StringBuilder sb, string key, string value)
		{
			sb.Append(' ');
			sb.Append(key);
			sb.Append('=');
			sb.Append(value);
		}

		private static void AppendSpatialFacets(this StringBuilder sb, IEdmSpatialTypeReference type)
		{
			string str;
			StringBuilder stringBuilder = sb;
			string str1 = "SRID";
			int? spatialReferenceIdentifier = type.SpatialReferenceIdentifier;
			if (spatialReferenceIdentifier.HasValue)
			{
				int? nullable = type.SpatialReferenceIdentifier;
				str = nullable.ToString();
			}
			else
			{
				str = "Variable";
			}
			stringBuilder.AppendKeyValue(str1, str);
		}

		private static void AppendStringFacets(this StringBuilder sb, IEdmStringTypeReference type)
		{
			bool? isUnicode;
			bool? nullable;
			string str;
			bool? isFixedLength = type.IsFixedLength;
			if (isFixedLength.HasValue)
			{
				bool? isFixedLength1 = type.IsFixedLength;
				sb.AppendKeyValue("FixedLength", isFixedLength1.ToString());
			}
			if (!type.IsUnbounded)
			{
				int? maxLength = type.MaxLength;
				if (!maxLength.HasValue)
				{
					isUnicode = type.IsUnicode;
					if (isUnicode.HasValue)
					{
						nullable = type.IsUnicode;
						sb.AppendKeyValue("Unicode", nullable.ToString());
					}
					if (type.Collation != null)
					{
						sb.AppendKeyValue("Collation", type.Collation.ToString());
					}
					return;
				}
			}
			StringBuilder stringBuilder = sb;
			string str1 = "MaxLength";
			if (type.IsUnbounded)
			{
				str = "Max";
			}
			else
			{
				int? maxLength1 = type.MaxLength;
				str = maxLength1.ToString();
			}
			stringBuilder.AppendKeyValue(str1, str);
			isUnicode = type.IsUnicode;
			if (isUnicode.HasValue)
			{
				nullable = type.IsUnicode;
				sb.AppendKeyValue("Unicode", nullable.ToString());
			}
			if (type.Collation != null)
			{
				sb.AppendKeyValue("Collation", type.Collation.ToString());
			}
		}

		private static void AppendTemporalFacets(this StringBuilder sb, IEdmTemporalTypeReference type)
		{
			int? precision = type.Precision;
			if (precision.HasValue)
			{
				int? nullable = type.Precision;
				sb.AppendKeyValue("Precision", nullable.ToString());
			}
		}

		public static string ToTraceString(this IEdmSchemaType schemaType)
		{
			return schemaType.ToTraceString();
		}

		public static string ToTraceString(this IEdmSchemaElement schemaElement)
		{
			return schemaElement.FullName();
		}

		public static string ToTraceString(this IEdmType type)
		{
			EdmUtil.CheckArgumentNull<IEdmType>(type, "type");
			EdmTypeKind typeKind = type.TypeKind;
			switch (typeKind)
			{
				case EdmTypeKind.Row:
				{
					return ((IEdmRowType)type).ToTraceString();
				}
				case EdmTypeKind.Collection:
				{
					return ((IEdmCollectionType)type).ToTraceString();
				}
				case EdmTypeKind.EntityReference:
				{
					return ((IEdmEntityReferenceType)type).ToTraceString();
				}
				default:
				{
					IEdmSchemaType edmSchemaType = type as IEdmSchemaType;
					if (edmSchemaType == null)
					{
						break;
					}
					return edmSchemaType.ToTraceString();
				}
			}
			return "UnknownType";
		}

		public static string ToTraceString(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append('[');
			if (type.Definition != null)
			{
				stringBuilder.Append(type.Definition.ToTraceString());
				bool isNullable = type.IsNullable;
				stringBuilder.AppendKeyValue("Nullable", isNullable.ToString());
				if (type.IsPrimitive())
				{
					stringBuilder.AppendFacets(type.AsPrimitive());
				}
			}
			stringBuilder.Append(']');
			return stringBuilder.ToString();
		}

		public static string ToTraceString(this IEdmProperty property)
		{
			string name;
			string traceString;
			EdmUtil.CheckArgumentNull<IEdmProperty>(property, "property");
			if (property.Name != null)
			{
				name = property.Name;
			}
			else
			{
				name = "";
			}
			string str = ":";
			if (property.Type != null)
			{
				traceString = property.Type.ToTraceString();
			}
			else
			{
				traceString = "";
			}
			return string.Concat(name, str, traceString);
		}

		private static string ToTraceString(this IEdmEntityReferenceType type)
		{
			object traceString;
			object[] str = new object[4];
			str[0] = EdmTypeKind.EntityReference.ToString();
			str[1] = (char)40;
			object[] objArray = str;
			int num = 2;
			if (type.EntityType != null)
			{
				traceString = type.EntityType.ToTraceString();
			}
			else
			{
				traceString = "";
			}
			objArray[num] = traceString;
			str[3] = (char)41;
			return string.Concat(str);
		}

		private static string ToTraceString(this IEdmCollectionType type)
		{
			object traceString;
			object[] str = new object[4];
			str[0] = EdmTypeKind.Collection.ToString();
			str[1] = (char)40;
			object[] objArray = str;
			int num = 2;
			if (type.ElementType != null)
			{
				traceString = type.ElementType.ToTraceString();
			}
			else
			{
				traceString = "";
			}
			objArray[num] = traceString;
			str[3] = (char)41;
			return string.Concat(str);
		}

		private static string ToTraceString(this IEdmRowType type)
		{
			StringBuilder stringBuilder = new StringBuilder(EdmTypeKind.Row.ToString());
			stringBuilder.Append('(');
			if (type.Properties().Any<IEdmProperty>())
			{
				IEdmProperty edmProperty = type.Properties().Last<IEdmProperty>();
				foreach (IEdmProperty edmProperty1 in type.Properties())
				{
					if (edmProperty1 == null)
					{
						continue;
					}
					stringBuilder.Append(edmProperty1.ToTraceString());
					if (edmProperty1.Equals(edmProperty))
					{
						continue;
					}
					stringBuilder.Append(", ");
				}
			}
			stringBuilder.Append(')');
			return stringBuilder.ToString();
		}
	}
}