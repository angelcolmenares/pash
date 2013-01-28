namespace System.Data.Services.Providers
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Annotations;
    using Microsoft.Data.Edm.Csdl;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.Edm.Library.Annotations;
    using Microsoft.Data.Edm.Library.Values;
    using Microsoft.Data.Edm.Validation;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Data.Services;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;

    internal static class MetadataProviderUtils
    {
        internal static readonly Dictionary<Version, Version> DataServiceEdmVersionMap;
        private static readonly char[] InvalidCharactersInPropertyNames;
        internal static readonly ValidationRule<IEdmProperty> PropertyNameIncludesReservedODataCharacters;
        private static readonly Version Version1Dot0;
        private static readonly Version Version1Dot1;
        private static readonly Version Version1Dot2;
        private static readonly Version Version2Dot0;
        private static readonly Version Version3Dot0;
        private static readonly XmlWriterSettings xmlWriterSettingsForElementAnnotations;

        static MetadataProviderUtils()
        {
            Dictionary<Version, Version> dictionary = new Dictionary<Version, Version>(EqualityComparer<Version>.Default);
            dictionary.Add(DataServiceProtocolVersion.V1.ToVersion(), new Version(1, 2));
            dictionary.Add(DataServiceProtocolVersion.V2.ToVersion(), new Version(2, 0));
            dictionary.Add(DataServiceProtocolVersion.V3.ToVersion(), new Version(3, 0));
            DataServiceEdmVersionMap = dictionary;
            PropertyNameIncludesReservedODataCharacters = new ValidationRule<IEdmProperty>(delegate (ValidationContext context, IEdmProperty item) {
                string name = item.Name;
                if ((name != null) && (name.IndexOfAny(InvalidCharactersInPropertyNames) >= 0))
                {
                    string str2 = string.Join(", ", (from c in InvalidCharactersInPropertyNames select string.Format(CultureInfo.InvariantCulture, "'{0}'", new object[] { c })).ToArray<string>());
                    context.AddError(item.Location(), EdmErrorCode.InvalidName, System.Data.Services.Strings.MetadataProviderUtils_PropertiesMustNotContainReservedChars(name, str2));
                }
            });
            Version1Dot0 = new Version(1, 0);
            Version1Dot1 = new Version(1, 1);
            Version1Dot2 = new Version(1, 2);
            Version2Dot0 = new Version(2, 0);
            Version3Dot0 = new Version(3, 0);
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true,
                OmitXmlDeclaration = true
            };
            xmlWriterSettingsForElementAnnotations = settings;
            InvalidCharactersInPropertyNames = new char[] { ':', '.', '@' };
        }

        private static IEdmPrimitiveTypeReference ApplyFacetAnnotations(this IEdmPrimitiveTypeReference primitiveTypeReference, List<KeyValuePair<string, object>> annotations)
        {
            object obj2;
            if ((annotations == null) || (annotations.Count == 0))
            {
                return primitiveTypeReference;
            }
            IEdmPrimitiveTypeReference reference = primitiveTypeReference;
            bool isNullable = primitiveTypeReference.IsNullable;
            if (TryFindAndRemoveAnnotation(annotations, "Nullable", out obj2))
            {
                isNullable = ConvertAnnotationValue<bool>(obj2, "Nullable");
            }
            EdmPrimitiveTypeKind kind = primitiveTypeReference.PrimitiveKind();
            switch (kind)
            {
                case EdmPrimitiveTypeKind.Binary:
                    return CreateBinaryTypeReference(primitiveTypeReference, isNullable, annotations);

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
                    if (primitiveTypeReference.IsNullable != isNullable)
                    {
                        reference = new EdmPrimitiveTypeReference(primitiveTypeReference.PrimitiveDefinition(), isNullable);
                    }
                    return reference;

                case EdmPrimitiveTypeKind.DateTime:
                case EdmPrimitiveTypeKind.DateTimeOffset:
                case EdmPrimitiveTypeKind.Time:
                    return CreateTemporalTypeReference(primitiveTypeReference, isNullable, annotations);

                case EdmPrimitiveTypeKind.Decimal:
                    return CreateDecimalTypeReference(primitiveTypeReference, isNullable, annotations);

                case EdmPrimitiveTypeKind.String:
                    return CreateStringTypeReference(primitiveTypeReference, isNullable, annotations);

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
                    return CreateSpatialTypeReference(primitiveTypeReference, isNullable, annotations);
            }
            throw new InvalidOperationException(System.Data.Services.Strings.MetadataProviderUtils_UnsupportedPrimitiveTypeKind(kind.ToString()));
        }

        private static T ConvertAnnotationValue<T>(object annotationValue, string facetName)
        {
            T local;
            if (!TryConvertAnnotationValue<T>(annotationValue, out local))
            {
                string str = (annotationValue == null) ? "null" : annotationValue.GetType().FullName;
                throw new FormatException(System.Data.Services.Strings.MetadataProviderUtils_ConversionError(facetName, str, typeof(T).FullName));
            }
            return local;
        }

        private static string ConvertAttributeAnnotationValue(object annotationValue)
        {
            if (annotationValue == null)
            {
                return string.Empty;
            }
            Type type = annotationValue.GetType();
            if (type == typeof(bool))
            {
                if (!((bool) annotationValue))
                {
                    return "false";
                }
                return "true";
            }
            if (type == typeof(int))
            {
                return XmlConvert.ToString((int) annotationValue);
            }
            if (type.IsEnum)
            {
                return annotationValue.ToString();
            }
            if (type == typeof(byte))
            {
                return XmlConvert.ToString((byte) annotationValue);
            }
            if (type == typeof(DateTime))
            {
                return XmlConvert.ToString((DateTime) annotationValue, "yyyy-MM-dd HH:mm:ss.fffZ");
            }
            if (!(type == typeof(byte[])))
            {
                return annotationValue.ToString();
            }
            string str = string.Concat((IEnumerable<string>) (from b in (byte[]) annotationValue select b.ToString("X2", CultureInfo.InvariantCulture)));
            return ("0x" + str);
        }

        internal static IEnumerable<IEdmDirectValueAnnotation> ConvertCustomAnnotations(MetadataProviderEdmModel model, IEnumerable<KeyValuePair<string, object>> customAnnotations)
        {
            if (customAnnotations != null)
            {
                foreach (KeyValuePair<string, object> iteratorVariable0 in customAnnotations)
                {
                    object iteratorVariable1 = iteratorVariable0.Value;
                    Type iteratorVariable2 = (iteratorVariable1 == null) ? null : iteratorVariable1.GetType();
                    bool iteratorVariable3 = iteratorVariable2 == typeof(XElement);
                    int length = iteratorVariable0.Key.LastIndexOf(":", StringComparison.Ordinal);
                    if (length == -1)
                    {
                        if (!iteratorVariable3)
                        {
                            string iteratorVariable5 = ConvertAttributeAnnotationValue(iteratorVariable0.Value);
                            yield return new EdmDirectValueAnnotation(string.Empty, iteratorVariable0.Key, new EdmStringConstant(EdmCoreModel.Instance.GetString(true), iteratorVariable5));
                        }
                    }
                    else
                    {
                        string namespaceUri = iteratorVariable0.Key.Substring(0, length);
                        string name = iteratorVariable0.Key.Substring(length + 1);
                        if ((iteratorVariable1 == null) || !iteratorVariable3)
                        {
                            string iteratorVariable8 = ConvertAttributeAnnotationValue(iteratorVariable0.Value);
                            yield return new EdmDirectValueAnnotation(namespaceUri, name, new EdmStringConstant(EdmCoreModel.Instance.GetString(true), iteratorVariable8));
                        }
                        else if ((iteratorVariable1 != null) && (iteratorVariable2 == typeof(XElement)))
                        {
                            XElement xmlElement = (XElement) iteratorVariable1;
                            string iteratorVariable10 = CreateElementAnnotationStringRepresentation(xmlElement);
                            EdmStringConstant iteratorVariable11 = new EdmStringConstant(EdmCoreModel.Instance.GetString(false), iteratorVariable10);
                            iteratorVariable11.SetIsSerializedAsElement(model, true);
                            yield return new EdmDirectValueAnnotation(namespaceUri, name, iteratorVariable11);
                        }
                    }
                }
            }
        }

        internal static void ConvertCustomAnnotations(MetadataProviderEdmModel model, IEnumerable<KeyValuePair<string, object>> customAnnotations, IEdmElement target)
        {
            foreach (IEdmDirectValueAnnotation annotation in ConvertCustomAnnotations(model, customAnnotations))
            {
                model.SetAnnotationValue(target, annotation.NamespaceUri, annotation.Name, annotation.Value);
            }
        }

        private static string ConvertDefaultValue(object annotationValue)
        {
            if (annotationValue == null)
            {
                return null;
            }
            Type type = annotationValue.GetType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int32:
                    return XmlConvert.ToString((int) annotationValue);

                case TypeCode.DateTime:
                    return XmlConvert.ToString((DateTime) annotationValue, "yyyy-MM-dd HH:mm:ss.fffZ");

                case TypeCode.Boolean:
                    if (!((bool) annotationValue))
                    {
                        return "false";
                    }
                    return "true";

                case TypeCode.Byte:
                    return XmlConvert.ToString((byte) annotationValue);
            }
            if (type.IsEnum)
            {
                return annotationValue.ToString();
            }
            if (!(type == typeof(byte[])))
            {
                return annotationValue.ToString();
            }
            string str = string.Concat((IEnumerable<string>) (from b in (byte[]) annotationValue select b.ToString("X2", CultureInfo.InvariantCulture)));
            return ("0x" + str);
        }

        internal static void ConvertEntityPropertyMappings(MetadataProviderEdmModel model, ResourceType resourceType, EdmEntityType entityType)
        {
            IEnumerable<EntityPropertyMappingAttribute> allEpmInfo = resourceType.AllEpmInfo;
            if (allEpmInfo != null)
            {
                ODataEntityPropertyMappingCollection mappings = new ODataEntityPropertyMappingCollection();
                foreach (EntityPropertyMappingAttribute attribute in allEpmInfo)
                {
                    mappings.Add(attribute);
                }
                model.SetAnnotationValue<ODataEntityPropertyMappingCollection>(entityType, mappings);
            }
        }

        internal static EdmMultiplicity ConvertMultiplicity(string multiplicity)
        {
            switch (multiplicity)
            {
                case "*":
                    return EdmMultiplicity.Many;

                case "1":
                    return EdmMultiplicity.One;

                case "0..1":
                    return EdmMultiplicity.ZeroOrOne;
            }
            return EdmMultiplicity.Unknown;
        }

        private static IEdmPrimitiveTypeReference CreateBinaryTypeReference(IEdmPrimitiveTypeReference primitiveTypeReference, bool nullableFacet, List<KeyValuePair<string, object>> annotations)
        {
            bool flag;
            if (annotations.Count == 0)
            {
                if (primitiveTypeReference.IsNullable != nullableFacet)
                {
                    return new EdmBinaryTypeReference(primitiveTypeReference.PrimitiveDefinition(), nullableFacet);
                }
                return primitiveTypeReference;
            }
            int? maxLengthAnnotation = GetMaxLengthAnnotation(annotations, out flag);
            return new EdmBinaryTypeReference(primitiveTypeReference.PrimitiveDefinition(), nullableFacet, flag, maxLengthAnnotation, GetFixedLengthAnnotation(annotations));
        }

        private static IEdmPrimitiveTypeReference CreateDecimalTypeReference(IEdmPrimitiveTypeReference primitiveTypeReference, bool nullableFacet, List<KeyValuePair<string, object>> annotations)
        {
            if (annotations.Count == 0)
            {
                if (primitiveTypeReference.IsNullable != nullableFacet)
                {
                    return new EdmDecimalTypeReference(primitiveTypeReference.PrimitiveDefinition(), nullableFacet);
                }
                return primitiveTypeReference;
            }
            int? precisionAnnotation = GetPrecisionAnnotation(annotations);
            return new EdmDecimalTypeReference(primitiveTypeReference.PrimitiveDefinition(), nullableFacet, precisionAnnotation, GetScaleAnnotation(annotations));
        }

        private static string CreateElementAnnotationStringRepresentation(XElement xmlElement)
        {
            StringBuilder output = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(output, xmlWriterSettingsForElementAnnotations))
            {
                xmlElement.WriteTo(writer);
            }
            return output.ToString();
        }

        internal static IEdmPrimitiveTypeReference CreatePrimitiveTypeReference(ResourceType resourceType)
        {
            return CreatePrimitiveTypeReference(resourceType, null);
        }

        internal static IEdmPrimitiveTypeReference CreatePrimitiveTypeReference(ResourceType resourceType, List<KeyValuePair<string, object>> annotations)
        {
            Type instanceType = resourceType.InstanceType;
            if (instanceType == typeof(Binary))
            {
                instanceType = typeof(byte[]);
            }
            else if (instanceType == typeof(XElement))
            {
                instanceType = typeof(string);
            }
            return GetPrimitiveTypeReferenceFromTypeAndFacets(instanceType, annotations);
        }

        private static IEdmPrimitiveTypeReference CreateSpatialTypeReference(IEdmPrimitiveTypeReference primitiveTypeReference, bool nullableFacet, List<KeyValuePair<string, object>> annotations)
        {
            object obj2;
            if ((annotations.Count == 0) || !TryFindAndRemoveAnnotation(annotations, "SRID", out obj2))
            {
                if (primitiveTypeReference.IsNullable != nullableFacet)
                {
                    return new EdmSpatialTypeReference(primitiveTypeReference.PrimitiveDefinition(), nullableFacet, null);
                }
                return primitiveTypeReference;
            }
            int num = ConvertAnnotationValue<int>(obj2, "SRID");
            return new EdmSpatialTypeReference(primitiveTypeReference.PrimitiveDefinition(), nullableFacet, new int?(num));
        }

        private static IEdmPrimitiveTypeReference CreateStringTypeReference(IEdmPrimitiveTypeReference primitiveTypeReference, bool nullableFacet, List<KeyValuePair<string, object>> annotations)
        {
            bool flag;
            if (annotations.Count == 0)
            {
                if (primitiveTypeReference.IsNullable != nullableFacet)
                {
                    return new EdmStringTypeReference(primitiveTypeReference.PrimitiveDefinition(), nullableFacet);
                }
                return primitiveTypeReference;
            }
            int? maxLengthAnnotation = GetMaxLengthAnnotation(annotations, out flag);
            bool? fixedLengthAnnotation = GetFixedLengthAnnotation(annotations);
            bool? isUnicodeAnnotation = GetIsUnicodeAnnotation(annotations);
            return new EdmStringTypeReference(primitiveTypeReference.PrimitiveDefinition(), nullableFacet, flag, maxLengthAnnotation, fixedLengthAnnotation, isUnicodeAnnotation, GetCollationAnnotation(annotations));
        }

        private static IEdmPrimitiveTypeReference CreateTemporalTypeReference(IEdmPrimitiveTypeReference primitiveTypeReference, bool nullableFacet, List<KeyValuePair<string, object>> annotations)
        {
            if (annotations.Count == 0)
            {
                if (primitiveTypeReference.IsNullable != nullableFacet)
                {
                    return new EdmTemporalTypeReference(primitiveTypeReference.PrimitiveDefinition(), nullableFacet);
                }
                return primitiveTypeReference;
            }
            return new EdmTemporalTypeReference(primitiveTypeReference.PrimitiveDefinition(), nullableFacet, GetPrecisionAnnotation(annotations));
        }

        internal static void FixUpNavigationPropertyWithAssociationSetData(IEdmNavigationProperty navigationProperty, IEdmNavigationProperty partner, bool isPrinciple, List<IEdmStructuralProperty> dependentProperties, EdmOnDeleteAction deleteAction, EdmMultiplicity opposingMultiplicity)
        {
            MetadataProviderEdmNavigationProperty property = navigationProperty as MetadataProviderEdmNavigationProperty;
            if (property != null)
            {
                property.FixUpNavigationProperty(partner, isPrinciple, deleteAction);
                if (opposingMultiplicity == EdmMultiplicity.One)
                {
                    property.Type = property.Type.Definition.ToTypeReference(false);
                }
                if ((dependentProperties != null) && !isPrinciple)
                {
                    property.SetDependentProperties(dependentProperties);
                }
            }
            else if ((dependentProperties != null) && !isPrinciple)
            {
                ((MetadataProviderEdmSilentNavigationProperty) navigationProperty).SetDependentProperties(dependentProperties);
            }
        }

        internal static string GetAndRemoveDefaultValue(List<KeyValuePair<string, object>> annotations)
        {
            object obj2;
            if (((annotations != null) && (annotations.Count != 0)) && TryFindAndRemoveAnnotation(annotations, "DefaultValue", out obj2))
            {
                return ConvertDefaultValue(obj2);
            }
            return null;
        }

        internal static bool? GetAndRemoveNullableFacet(List<KeyValuePair<string, object>> annotations)
        {
            object obj2;
            if (((annotations != null) && (annotations.Count != 0)) && TryFindAndRemoveAnnotation(annotations, "Nullable", out obj2))
            {
                return new bool?(ConvertAnnotationValue<bool>(obj2, "Nullable"));
            }
            return null;
        }

        internal static string GetAssociationEndName(ResourceType resourceType, ResourceProperty resourceProperty)
        {
            string name = resourceType.Name;
            if (resourceProperty != null)
            {
                name = name + '_' + resourceProperty.Name;
            }
            return name;
        }

        internal static string GetAssociationName(ResourceAssociationSet associationSet)
        {
            ResourceAssociationSetEnd end = (associationSet.End1.ResourceProperty != null) ? associationSet.End1 : associationSet.End2;
            ResourceAssociationSetEnd end2 = (end == associationSet.End1) ? ((associationSet.End2.ResourceProperty != null) ? associationSet.End2 : null) : null;
            string str = end.ResourceType.Name + '_' + end.ResourceProperty.Name;
            if (end2 != null)
            {
                str = string.Concat(new object[] { str, '_', end2.ResourceType.Name, '_', end2.ResourceProperty.Name });
            }
            return str;
        }

        private static string GetCollationAnnotation(List<KeyValuePair<string, object>> annotations)
        {
            object obj2;
            if (TryFindAndRemoveAnnotation(annotations, "Collation", out obj2))
            {
                return ConvertAnnotationValue<string>(obj2, "Collation");
            }
            return null;
        }

        internal static string GetEntitySetName(ResourceSet resourceSet)
        {
            string entityContainerName = resourceSet.EntityContainerName;
            if (((entityContainerName != null) && resourceSet.Name.StartsWith(entityContainerName, StringComparison.Ordinal)) && (resourceSet.Name[entityContainerName.Length] == '.'))
            {
                return resourceSet.Name.Substring(resourceSet.EntityContainerName.Length + 1);
            }
            return resourceSet.Name;
        }

        private static bool? GetFixedLengthAnnotation(List<KeyValuePair<string, object>> annotations)
        {
            object obj2;
            if (TryFindAndRemoveAnnotation(annotations, "FixedLength", out obj2))
            {
                return new bool?(ConvertAnnotationValue<bool>(obj2, "FixedLength"));
            }
            return null;
        }

        private static bool? GetIsUnicodeAnnotation(List<KeyValuePair<string, object>> annotations)
        {
            object obj2;
            if (TryFindAndRemoveAnnotation(annotations, "Unicode", out obj2))
            {
                return new bool?(ConvertAnnotationValue<bool>(obj2, "Unicode"));
            }
            return null;
        }

        private static int? GetMaxLengthAnnotation(List<KeyValuePair<string, object>> annotations, out bool isMaxMaxLength)
        {
            object obj2;
            if (TryFindAndRemoveAnnotation(annotations, "MaxLength", out obj2))
            {
                int num;
                string str;
                if (TryConvertAnnotationValue<int>(obj2, out num))
                {
                    isMaxMaxLength = false;
                    return new int?(num);
                }
                if (!TryConvertAnnotationValue<string>(obj2, out str))
                {
                    str = (obj2 == null) ? null : obj2.ToString();
                }
                if (string.CompareOrdinal("Max", str) == 0)
                {
                    isMaxMaxLength = true;
                    return null;
                }
                string str2 = (obj2 == null) ? "null" : obj2.GetType().FullName;
                throw new FormatException(System.Data.Services.Strings.MetadataProviderUtils_ConversionError("MaxLength", str2, typeof(string).FullName));
            }
            isMaxMaxLength = false;
            return null;
        }

        internal static EdmMultiplicity GetMultiplicity(ResourceProperty property)
        {
            if ((property != null) && (property.Kind == ResourcePropertyKind.ResourceReference))
            {
                return EdmMultiplicity.ZeroOrOne;
            }
            return EdmMultiplicity.Many;
        }

        private static int? GetPrecisionAnnotation(List<KeyValuePair<string, object>> annotations)
        {
            object obj2;
            if (TryFindAndRemoveAnnotation(annotations, "Precision", out obj2))
            {
                return new int?(ConvertAnnotationValue<int>(obj2, "Precision"));
            }
            return null;
        }

        private static IEdmPrimitiveTypeReference GetPrimitiveTypeReferenceFromTypeAndFacets(Type clrType, List<KeyValuePair<string, object>> annotations)
        {
            IEdmPrimitiveTypeReference primitiveTypeReference = EdmLibraryExtensions.GetPrimitiveTypeReference(clrType);
            if (primitiveTypeReference.IsSpatial())
            {
                primitiveTypeReference = new EdmSpatialTypeReference(primitiveTypeReference.PrimitiveDefinition(), primitiveTypeReference.IsNullable, null);
            }
            return primitiveTypeReference.ApplyFacetAnnotations(annotations);
        }

        private static int? GetScaleAnnotation(List<KeyValuePair<string, object>> annotations)
        {
            object obj2;
            if (TryFindAndRemoveAnnotation(annotations, "Scale", out obj2))
            {
                return new int?(ConvertAnnotationValue<int>(obj2, "Scale"));
            }
            return null;
        }

        internal static bool ShouldDisablePrimitivePropertyNullValidation(ResourceProperty resourceProperty, IEdmPrimitiveTypeReference primitiveTypeReference)
        {
            return (WebUtil.TypeAllowsNull(resourceProperty.ResourceType.InstanceType) && !primitiveTypeReference.IsNullable);
        }

        internal static Version ToVersion(this MetadataEdmSchemaVersion schemaVersion)
        {
            switch (schemaVersion)
            {
                case MetadataEdmSchemaVersion.Version1Dot0:
                    return Version1Dot0;

                case MetadataEdmSchemaVersion.Version1Dot1:
                    return Version1Dot1;

                case MetadataEdmSchemaVersion.Version1Dot2:
                    return Version1Dot2;

                case MetadataEdmSchemaVersion.Version2Dot0:
                    return Version2Dot0;

                case MetadataEdmSchemaVersion.Version3Dot0:
                    return Version3Dot0;
            }
            return Version3Dot0;
        }

        private static bool TryConvertAnnotationValue<T>(object annotationValue, out T convertedValue)
        {
            Type type = typeof(T);
            bool flag = WebUtil.TypeAllowsNull(type);
            if (annotationValue == null)
            {
                if (flag)
                {
                    convertedValue = (T) annotationValue;
                    return true;
                }
                convertedValue = default(T);
                return false;
            }
            IConvertible convertible = annotationValue as IConvertible;
            if (convertible != null)
            {
                try
                {
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.Boolean:
                        case TypeCode.Int32:
                        case TypeCode.String:
                            convertedValue = (T) Convert.ChangeType(convertible, type, CultureInfo.CurrentCulture);
                            return true;
                    }
                }
                catch (Exception exception)
                {
                    if (!CommonUtil.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                }
            }
            convertedValue = default(T);
            return false;
        }

        private static bool TryFindAndRemoveAnnotation(List<KeyValuePair<string, object>> annotations, string key, out object value)
        {
            for (int i = 0; i < annotations.Count; i++)
            {
                KeyValuePair<string, object> pair = annotations[i];
                if (string.CompareOrdinal(pair.Key, key) == 0)
                {
                    KeyValuePair<string, object> pair2 = annotations[i];
                    value = pair2.Value;
                    annotations.RemoveAt(i);
                    return true;
                }
            }
            value = null;
            return false;
        }

        
    }
}

