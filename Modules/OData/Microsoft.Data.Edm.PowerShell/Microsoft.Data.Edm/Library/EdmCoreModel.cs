using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Library.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmCoreModel : EdmElement, IEdmModel, IEdmElement, IEdmValidCoreModelElement
	{
		private const string EdmNamespace = "Edm";

		public readonly static EdmCoreModel Instance;

		private readonly EdmCoreModel.EdmValidCoreModelPrimitiveType[] primitiveTypes;

		private readonly Dictionary<string, EdmPrimitiveTypeKind> primitiveTypeKinds;

		private readonly Dictionary<EdmPrimitiveTypeKind, EdmCoreModel.EdmValidCoreModelPrimitiveType> primitiveTypesByKind;

		private readonly Dictionary<string, EdmCoreModel.EdmValidCoreModelPrimitiveType> primitiveTypeByName;

		private readonly IEdmDirectValueAnnotationsManager annotationsManager;

		public IEdmDirectValueAnnotationsManager DirectValueAnnotationsManager
		{
			get
			{
				return this.annotationsManager;
			}
		}

		public static string Namespace
		{
			get
			{
				return "Edm";
			}
		}

		public IEnumerable<IEdmModel> ReferencedModels
		{
			get
			{
				return Enumerable.Empty<IEdmModel>();
			}
		}

		public IEnumerable<IEdmSchemaElement> SchemaElements
		{
			get
			{
				return this.primitiveTypes;
			}
		}

		public IEnumerable<IEdmVocabularyAnnotation> VocabularyAnnotations
		{
			get
			{
				return Enumerable.Empty<IEdmVocabularyAnnotation>();
			}
		}

		static EdmCoreModel()
		{
			EdmCoreModel.Instance = new EdmCoreModel();
		}

		private EdmCoreModel()
		{
			this.primitiveTypeKinds = new Dictionary<string, EdmPrimitiveTypeKind>();
			this.primitiveTypesByKind = new Dictionary<EdmPrimitiveTypeKind, EdmCoreModel.EdmValidCoreModelPrimitiveType>();
			this.primitiveTypeByName = new Dictionary<string, EdmCoreModel.EdmValidCoreModelPrimitiveType>();
			this.annotationsManager = new EdmDirectValueAnnotationsManager();
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Double", EdmPrimitiveTypeKind.Double);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType1 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Single", EdmPrimitiveTypeKind.Single);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType2 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Int64", EdmPrimitiveTypeKind.Int64);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType3 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Int32", EdmPrimitiveTypeKind.Int32);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType4 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Int16", EdmPrimitiveTypeKind.Int16);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType5 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "SByte", EdmPrimitiveTypeKind.SByte);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType6 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Byte", EdmPrimitiveTypeKind.Byte);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType7 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Boolean", EdmPrimitiveTypeKind.Boolean);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType8 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Guid", EdmPrimitiveTypeKind.Guid);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType9 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Time", EdmPrimitiveTypeKind.Time);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType10 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "DateTime", EdmPrimitiveTypeKind.DateTime);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType11 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "DateTimeOffset", EdmPrimitiveTypeKind.DateTimeOffset);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType12 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Decimal", EdmPrimitiveTypeKind.Decimal);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType13 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Binary", EdmPrimitiveTypeKind.Binary);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType14 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "String", EdmPrimitiveTypeKind.String);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType15 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Stream", EdmPrimitiveTypeKind.Stream);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType16 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Geography", EdmPrimitiveTypeKind.Geography);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType17 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeographyPoint", EdmPrimitiveTypeKind.GeographyPoint);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType18 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeographyLineString", EdmPrimitiveTypeKind.GeographyLineString);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType19 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeographyPolygon", EdmPrimitiveTypeKind.GeographyPolygon);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType20 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeographyCollection", EdmPrimitiveTypeKind.GeographyCollection);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType21 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeographyMultiPolygon", EdmPrimitiveTypeKind.GeographyMultiPolygon);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType22 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeographyMultiLineString", EdmPrimitiveTypeKind.GeographyMultiLineString);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType23 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeographyMultiPoint", EdmPrimitiveTypeKind.GeographyMultiPoint);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType24 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "Geometry", EdmPrimitiveTypeKind.Geometry);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType25 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeometryPoint", EdmPrimitiveTypeKind.GeometryPoint);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType26 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeometryLineString", EdmPrimitiveTypeKind.GeometryLineString);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType27 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeometryPolygon", EdmPrimitiveTypeKind.GeometryPolygon);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType28 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeometryCollection", EdmPrimitiveTypeKind.GeometryCollection);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType29 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeometryMultiPolygon", EdmPrimitiveTypeKind.GeometryMultiPolygon);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType30 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeometryMultiLineString", EdmPrimitiveTypeKind.GeometryMultiLineString);
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType31 = new EdmCoreModel.EdmValidCoreModelPrimitiveType("Edm", "GeometryMultiPoint", EdmPrimitiveTypeKind.GeometryMultiPoint);
			EdmCoreModel.EdmValidCoreModelPrimitiveType[] edmValidCoreModelPrimitiveTypeArray = new EdmCoreModel.EdmValidCoreModelPrimitiveType[32];
			edmValidCoreModelPrimitiveTypeArray[0] = edmValidCoreModelPrimitiveType13;
			edmValidCoreModelPrimitiveTypeArray[1] = edmValidCoreModelPrimitiveType7;
			edmValidCoreModelPrimitiveTypeArray[2] = edmValidCoreModelPrimitiveType6;
			edmValidCoreModelPrimitiveTypeArray[3] = edmValidCoreModelPrimitiveType10;
			edmValidCoreModelPrimitiveTypeArray[4] = edmValidCoreModelPrimitiveType11;
			edmValidCoreModelPrimitiveTypeArray[5] = edmValidCoreModelPrimitiveType12;
			edmValidCoreModelPrimitiveTypeArray[6] = edmValidCoreModelPrimitiveType;
			edmValidCoreModelPrimitiveTypeArray[7] = edmValidCoreModelPrimitiveType8;
			edmValidCoreModelPrimitiveTypeArray[8] = edmValidCoreModelPrimitiveType4;
			edmValidCoreModelPrimitiveTypeArray[9] = edmValidCoreModelPrimitiveType3;
			edmValidCoreModelPrimitiveTypeArray[10] = edmValidCoreModelPrimitiveType2;
			edmValidCoreModelPrimitiveTypeArray[11] = edmValidCoreModelPrimitiveType5;
			edmValidCoreModelPrimitiveTypeArray[12] = edmValidCoreModelPrimitiveType1;
			edmValidCoreModelPrimitiveTypeArray[13] = edmValidCoreModelPrimitiveType15;
			edmValidCoreModelPrimitiveTypeArray[14] = edmValidCoreModelPrimitiveType14;
			edmValidCoreModelPrimitiveTypeArray[15] = edmValidCoreModelPrimitiveType9;
			edmValidCoreModelPrimitiveTypeArray[16] = edmValidCoreModelPrimitiveType16;
			edmValidCoreModelPrimitiveTypeArray[17] = edmValidCoreModelPrimitiveType17;
			edmValidCoreModelPrimitiveTypeArray[18] = edmValidCoreModelPrimitiveType18;
			edmValidCoreModelPrimitiveTypeArray[19] = edmValidCoreModelPrimitiveType19;
			edmValidCoreModelPrimitiveTypeArray[20] = edmValidCoreModelPrimitiveType20;
			edmValidCoreModelPrimitiveTypeArray[21] = edmValidCoreModelPrimitiveType21;
			edmValidCoreModelPrimitiveTypeArray[22] = edmValidCoreModelPrimitiveType22;
			edmValidCoreModelPrimitiveTypeArray[23] = edmValidCoreModelPrimitiveType23;
			edmValidCoreModelPrimitiveTypeArray[24] = edmValidCoreModelPrimitiveType24;
			edmValidCoreModelPrimitiveTypeArray[25] = edmValidCoreModelPrimitiveType25;
			edmValidCoreModelPrimitiveTypeArray[26] = edmValidCoreModelPrimitiveType26;
			edmValidCoreModelPrimitiveTypeArray[27] = edmValidCoreModelPrimitiveType27;
			edmValidCoreModelPrimitiveTypeArray[28] = edmValidCoreModelPrimitiveType28;
			edmValidCoreModelPrimitiveTypeArray[29] = edmValidCoreModelPrimitiveType29;
			edmValidCoreModelPrimitiveTypeArray[30] = edmValidCoreModelPrimitiveType30;
			edmValidCoreModelPrimitiveTypeArray[31] = edmValidCoreModelPrimitiveType31;
			this.primitiveTypes = edmValidCoreModelPrimitiveTypeArray;
			EdmCoreModel.EdmValidCoreModelPrimitiveType[] edmValidCoreModelPrimitiveTypeArray1 = this.primitiveTypes;
			for (int i = 0; i < (int)edmValidCoreModelPrimitiveTypeArray1.Length; i++)
			{
				EdmCoreModel.EdmValidCoreModelPrimitiveType primitiveKind = edmValidCoreModelPrimitiveTypeArray1[i];
				this.primitiveTypeKinds[primitiveKind.Name] = primitiveKind.PrimitiveKind;
				this.primitiveTypeKinds[string.Concat(primitiveKind.Namespace, (object)((char)46), primitiveKind.Name)] = primitiveKind.PrimitiveKind;
				this.primitiveTypesByKind[primitiveKind.PrimitiveKind] = primitiveKind;
				this.primitiveTypeByName[string.Concat(primitiveKind.Namespace, (object)((char)46), primitiveKind.Name)] = primitiveKind;
			}
		}

		public IEdmEntityContainer FindDeclaredEntityContainer(string name)
		{
			return null;
		}

		public IEnumerable<IEdmFunction> FindDeclaredFunctions(string qualifiedName)
		{
			return Enumerable.Empty<IEdmFunction>();
		}

		public IEdmSchemaType FindDeclaredType(string qualifiedName)
		{
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType = null;
			if (this.primitiveTypeByName.TryGetValue(qualifiedName, out edmValidCoreModelPrimitiveType))
			{
				return edmValidCoreModelPrimitiveType;
			}
			else
			{
				return null;
			}
		}

		public IEdmValueTerm FindDeclaredValueTerm(string qualifiedName)
		{
			return null;
		}

		public IEnumerable<IEdmVocabularyAnnotation> FindDeclaredVocabularyAnnotations(IEdmVocabularyAnnotatable element)
		{
			return Enumerable.Empty<IEdmVocabularyAnnotation>();
		}

		public IEnumerable<IEdmStructuredType> FindDirectlyDerivedTypes(IEdmStructuredType baseType)
		{
			return Enumerable.Empty<IEdmStructuredType>();
		}

		public IEdmBinaryTypeReference GetBinary(bool isUnbounded, int? maxLength, bool? isFixedLength, bool isNullable)
		{
			return new EdmBinaryTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Binary), isNullable, isUnbounded, maxLength, isFixedLength);
		}

		public IEdmBinaryTypeReference GetBinary(bool isNullable)
		{
			return new EdmBinaryTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Binary), isNullable);
		}

		public IEdmPrimitiveTypeReference GetBoolean(bool isNullable)
		{
			return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Boolean), isNullable);
		}

		public IEdmPrimitiveTypeReference GetByte(bool isNullable)
		{
			return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Byte), isNullable);
		}

		public static IEdmCollectionTypeReference GetCollection(IEdmTypeReference elementType)
		{
			return new EdmCollectionTypeReference(new EdmCollectionType(elementType), false);
		}

		private EdmCoreModel.EdmValidCoreModelPrimitiveType GetCoreModelPrimitiveType(EdmPrimitiveTypeKind kind)
		{
			EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType = null;
			if (this.primitiveTypesByKind.TryGetValue(kind, out edmValidCoreModelPrimitiveType))
			{
				return edmValidCoreModelPrimitiveType;
			}
			else
			{
				return null;
			}
		}

		public IEdmTemporalTypeReference GetDateTime(bool isNullable)
		{
			return new EdmTemporalTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.DateTime), isNullable);
		}

		public IEdmTemporalTypeReference GetDateTimeOffset(bool isNullable)
		{
			return new EdmTemporalTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset), isNullable);
		}

		public IEdmDecimalTypeReference GetDecimal(int? precision, int? scale, bool isNullable)
		{
			if (precision.HasValue || scale.HasValue)
			{
				return new EdmDecimalTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Decimal), isNullable, precision, scale);
			}
			else
			{
				return new EdmDecimalTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Decimal), isNullable);
			}
		}

		public IEdmDecimalTypeReference GetDecimal(bool isNullable)
		{
			return new EdmDecimalTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Decimal), isNullable);
		}

		public IEdmPrimitiveTypeReference GetDouble(bool isNullable)
		{
			return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Double), isNullable);
		}

		public IEdmPrimitiveTypeReference GetGuid(bool isNullable)
		{
			return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Guid), isNullable);
		}

		public IEdmPrimitiveTypeReference GetInt16(bool isNullable)
		{
			return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Int16), isNullable);
		}

		public IEdmPrimitiveTypeReference GetInt32(bool isNullable)
		{
			return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Int32), isNullable);
		}

		public IEdmPrimitiveTypeReference GetInt64(bool isNullable)
		{
			return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Int64), isNullable);
		}

		public IEdmPrimitiveTypeReference GetPrimitive(EdmPrimitiveTypeKind kind, bool isNullable)
		{
			IEdmPrimitiveType coreModelPrimitiveType = this.GetCoreModelPrimitiveType(kind);
			if (coreModelPrimitiveType == null)
			{
				throw new InvalidOperationException(Strings.EdmPrimitive_UnexpectedKind);
			}
			else
			{
				return coreModelPrimitiveType.GetPrimitiveTypeReference(isNullable);
			}
		}

		public IEdmPrimitiveType GetPrimitiveType(EdmPrimitiveTypeKind kind)
		{
			return this.GetCoreModelPrimitiveType(kind);
		}

		public EdmPrimitiveTypeKind GetPrimitiveTypeKind(string typeName)
		{
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = EdmPrimitiveTypeKind.None;
			if (this.primitiveTypeKinds.TryGetValue(typeName, out edmPrimitiveTypeKind))
			{
				return edmPrimitiveTypeKind;
			}
			else
			{
				return EdmPrimitiveTypeKind.None;
			}
		}

		public IEdmPrimitiveTypeReference GetSByte(bool isNullable)
		{
			return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.SByte), isNullable);
		}

		public IEdmPrimitiveTypeReference GetSingle(bool isNullable)
		{
			return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Single), isNullable);
		}

		public IEdmSpatialTypeReference GetSpatial(EdmPrimitiveTypeKind kind, int? spatialReferenceIdentifier, bool isNullable)
		{
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = kind;
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
					return new EdmSpatialTypeReference(this.GetCoreModelPrimitiveType(kind), isNullable, spatialReferenceIdentifier);
				}
			}
			throw new InvalidOperationException(Strings.EdmPrimitive_UnexpectedKind);
		}

		public IEdmSpatialTypeReference GetSpatial(EdmPrimitiveTypeKind kind, bool isNullable)
		{
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = kind;
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
					return new EdmSpatialTypeReference(this.GetCoreModelPrimitiveType(kind), isNullable);
				}
			}
			throw new InvalidOperationException(Strings.EdmPrimitive_UnexpectedKind);
		}

		public IEdmPrimitiveTypeReference GetStream(bool isNullable)
		{
			return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Stream), isNullable);
		}

		public IEdmStringTypeReference GetString(bool isUnbounded, int? maxLength, bool? isFixedLength, bool? isUnicode, string collation, bool isNullable)
		{
			return new EdmStringTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.String), isNullable, isUnbounded, maxLength, isFixedLength, isUnicode, collation);
		}

		public IEdmStringTypeReference GetString(bool isNullable)
		{
			return new EdmStringTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.String), isNullable);
		}

		public IEdmTemporalTypeReference GetTemporal(EdmPrimitiveTypeKind kind, int? precision, bool isNullable)
		{
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = kind;
			switch (edmPrimitiveTypeKind)
			{
				case EdmPrimitiveTypeKind.DateTime:
				case EdmPrimitiveTypeKind.DateTimeOffset:
				{
					return new EdmTemporalTypeReference(this.GetCoreModelPrimitiveType(kind), isNullable, precision);
				}
				default:
				{
					if (edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Time)
					{
						return new EdmTemporalTypeReference(this.GetCoreModelPrimitiveType(kind), isNullable, precision);
					}
					throw new InvalidOperationException(Strings.EdmPrimitive_UnexpectedKind);
				}
			}
		}

		public IEdmTemporalTypeReference GetTemporal(EdmPrimitiveTypeKind kind, bool isNullable)
		{
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = kind;
			switch (edmPrimitiveTypeKind)
			{
				case EdmPrimitiveTypeKind.DateTime:
				case EdmPrimitiveTypeKind.DateTimeOffset:
				{
					return new EdmTemporalTypeReference(this.GetCoreModelPrimitiveType(kind), isNullable);
				}
				default:
				{
					if (edmPrimitiveTypeKind == EdmPrimitiveTypeKind.Time)
					{
						return new EdmTemporalTypeReference(this.GetCoreModelPrimitiveType(kind), isNullable);
					}
					throw new InvalidOperationException(Strings.EdmPrimitive_UnexpectedKind);
				}
			}
		}

		public IEdmTemporalTypeReference GetTime(bool isNullable)
		{
			return new EdmTemporalTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Time), isNullable);
		}

		private sealed class EdmValidCoreModelPrimitiveType : EdmType, IEdmPrimitiveType, IEdmSchemaType, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmType, IEdmElement, IEdmValidCoreModelElement
		{
			private string namespaceName;

			private string name;

			private EdmPrimitiveTypeKind primitiveKind;

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

			public EdmPrimitiveTypeKind PrimitiveKind
			{
				get
				{
					return this.primitiveKind;
				}
			}

			public EdmSchemaElementKind SchemaElementKind
			{
				get
				{
					return EdmSchemaElementKind.TypeDefinition;
				}
			}

			public override EdmTypeKind TypeKind
			{
				get
				{
					return EdmTypeKind.Primitive;
				}
			}

			public EdmValidCoreModelPrimitiveType(string namespaceName, string name, EdmPrimitiveTypeKind primitiveKind)
			{
				EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType = this;
				string str = namespaceName;
				string empty = str;
				if (str == null)
				{
					empty = string.Empty;
				}
				edmValidCoreModelPrimitiveType.namespaceName = empty;
				EdmCoreModel.EdmValidCoreModelPrimitiveType edmValidCoreModelPrimitiveType1 = this;
				string str1 = name;
				string empty1 = str1;
				if (str1 == null)
				{
					empty1 = string.Empty;
				}
				edmValidCoreModelPrimitiveType1.name = empty1;
				this.primitiveKind = primitiveKind;
			}
		}
	}
}