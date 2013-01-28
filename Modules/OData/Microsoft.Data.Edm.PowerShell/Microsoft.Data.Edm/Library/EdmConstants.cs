using System;

namespace Microsoft.Data.Edm.Library
{
	internal static class EdmConstants
	{
		internal const string EdmNamespace = "Edm";

		internal const string TransientNamespace = "Transient";

		internal const string XmlPrefix = "xml";

		internal const string XmlNamespacePrefix = "xmlns";

		internal const string DocumentationUri = "http://schemas.microsoft.com/ado/2011/04/edm/documentation";

		internal const string DocumentationAnnotation = "Documentation";

		internal const string InternalUri = "http://schemas.microsoft.com/ado/2011/04/edm/internal";

		internal const string DataServiceVersion = "DataServiceVersion";

		internal const string MaxDataServiceVersion = "MaxDataServiceVersion";

		internal const string EdmVersionAnnotation = "EdmVersion";

		internal const string FacetName_Nullable = "Nullable";

		internal const string FacetName_Precision = "Precision";

		internal const string FacetName_Scale = "Scale";

		internal const string FacetName_MaxLength = "MaxLength";

		internal const string FacetName_FixedLength = "FixedLength";

		internal const string FacetName_Unicode = "Unicode";

		internal const string FacetName_Collation = "Collation";

		internal const string FacetName_Srid = "SRID";

		internal const string Value_UnknownType = "UnknownType";

		internal const string Value_UnnamedType = "UnnamedType";

		internal const string Value_Max = "Max";

		internal const string Value_SridVariable = "Variable";

		internal const string Type_Association = "Association";

		internal const string Type_Collection = "Collection";

		internal const string Type_Complex = "Complex";

		internal const string Type_Entity = "Entity";

		internal const string Type_EntityReference = "EntityReference";

		internal const string Type_Enum = "Enum";

		internal const string Type_Row = "Row";

		internal const string Type_Primitive = "Primitive";

		internal const string Type_Binary = "Binary";

		internal const string Type_Decimal = "Decimal";

		internal const string Type_String = "String";

		internal const string Type_Stream = "Stream";

		internal const string Type_Spatial = "Spatial";

		internal const string Type_Temporal = "Temporal";

		internal const string Type_Structured = "Structured";

		internal const int Max_Precision = 0x7fffffff;

		internal const int Min_Precision = 0;

		public readonly static Version EdmVersion1;

		public readonly static Version EdmVersion1_1;

		public readonly static Version EdmVersion1_2;

		public readonly static Version EdmVersion2;

		public readonly static Version EdmVersion3;

		public readonly static Version EdmVersionLatest;

		static EdmConstants()
		{
			EdmConstants.EdmVersion1 = new Version(1, 0);
			EdmConstants.EdmVersion1_1 = new Version(1, 1);
			EdmConstants.EdmVersion1_2 = new Version(1, 2);
			EdmConstants.EdmVersion2 = new Version(2, 0);
			EdmConstants.EdmVersion3 = new Version(3, 0);
			EdmConstants.EdmVersionLatest = EdmConstants.EdmVersion3;
		}
	}
}