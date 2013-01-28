namespace Microsoft.Data.OData.Metadata
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Annotations;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.Edm.Library.Annotations;
    using Microsoft.Data.Edm.Library.Values;
    using Microsoft.Data.Edm.Values;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class EpmExtensionMethods
    {
        private static readonly string[] EpmAnnotationBaseNames = new string[] { "FC_TargetPath", "FC_SourcePath", "FC_KeepInContent", "FC_ContentKind", "FC_NsUri", "FC_NsPrefix" };
        private static readonly Dictionary<string, SyndicationItemProperty> TargetPathToSyndicationItemMap;

        static EpmExtensionMethods()
        {
            Dictionary<string, SyndicationItemProperty> dictionary = new Dictionary<string, SyndicationItemProperty>(StringComparer.OrdinalIgnoreCase);
            dictionary.Add("SyndicationAuthorEmail", SyndicationItemProperty.AuthorEmail);
            dictionary.Add("SyndicationAuthorName", SyndicationItemProperty.AuthorName);
            dictionary.Add("SyndicationAuthorUri", SyndicationItemProperty.AuthorUri);
            dictionary.Add("SyndicationContributorEmail", SyndicationItemProperty.ContributorEmail);
            dictionary.Add("SyndicationContributorName", SyndicationItemProperty.ContributorName);
            dictionary.Add("SyndicationContributorUri", SyndicationItemProperty.ContributorUri);
            dictionary.Add("SyndicationUpdated", SyndicationItemProperty.Updated);
            dictionary.Add("SyndicationPublished", SyndicationItemProperty.Published);
            dictionary.Add("SyndicationRights", SyndicationItemProperty.Rights);
            dictionary.Add("SyndicationSummary", SyndicationItemProperty.Summary);
            dictionary.Add("SyndicationTitle", SyndicationItemProperty.Title);
            TargetPathToSyndicationItemMap = dictionary;
        }

        internal static void ClearInMemoryEpmAnnotations(this IEdmModel model, IEdmElement annotatable)
        {
            IEdmDirectValueAnnotationBinding[] annotations = new IEdmDirectValueAnnotationBinding[] { new EdmTypedDirectValueAnnotationBinding<ODataEntityPropertyMappingCollection>(annotatable, null), new EdmTypedDirectValueAnnotationBinding<ODataEntityPropertyMappingCache>(annotatable, null) };
            model.SetAnnotationValues(annotations);
        }

        private static string ConvertEdmAnnotationValue(IEdmDirectValueAnnotation annotation)
        {
            object obj2 = annotation.Value;
            if (obj2 == null)
            {
                return null;
            }
            IEdmStringValue value2 = obj2 as IEdmStringValue;
            if (value2 == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.EpmExtensionMethods_CannotConvertEdmAnnotationValue(annotation.NamespaceUri, annotation.Name, annotation.GetType().FullName));
            }
            return value2.Value;
        }

        internal static ODataEntityPropertyMappingCache EnsureEpmCache(this IEdmModel model, IEdmEntityType entityType, int maxMappingCount)
        {
            bool flag;
            return EnsureEpmCacheInternal(model, entityType, maxMappingCount, out flag);
        }

        private static ODataEntityPropertyMappingCache EnsureEpmCacheInternal(IEdmModel model, IEdmEntityType entityType, int maxMappingCount, out bool cacheModified)
        {
            cacheModified = false;
            if (entityType == null)
            {
                return null;
            }
            IEdmEntityType type = entityType.BaseEntityType();
            ODataEntityPropertyMappingCache baseCache = null;
            if (type != null)
            {
                baseCache = EnsureEpmCacheInternal(model, type, maxMappingCount, out cacheModified);
            }
            ODataEntityPropertyMappingCache epmCache = model.GetEpmCache(entityType);
            if (model.HasOwnOrInheritedEpm(entityType))
            {
                ODataEntityPropertyMappingCollection entityPropertyMappings = model.GetEntityPropertyMappings(entityType);
                if (!(((epmCache == null) || cacheModified) || epmCache.IsDirty(entityPropertyMappings)))
                {
                    return epmCache;
                }
                cacheModified = true;
                int totalMappingCount = ValidationUtils.ValidateTotalEntityPropertyMappingCount(baseCache, entityPropertyMappings, maxMappingCount);
                epmCache = new ODataEntityPropertyMappingCache(entityPropertyMappings, model, totalMappingCount);
                try
                {
                    epmCache.BuildEpmForType(entityType, entityType);
                    epmCache.EpmSourceTree.Validate(entityType);
                    model.SetAnnotationValue<ODataEntityPropertyMappingCache>(entityType, epmCache);
                    return epmCache;
                }
                catch
                {
                    model.RemoveEpmCache(entityType);
                    throw;
                }
            }
            if (epmCache != null)
            {
                cacheModified = true;
                model.RemoveEpmCache(entityType);
            }
            return epmCache;
        }

        internal static CachedPrimitiveKeepInContentAnnotation EpmCachedKeepPrimitiveInContent(this IEdmModel model, IEdmComplexType complexType)
        {
            return model.GetAnnotationValue<CachedPrimitiveKeepInContentAnnotation>(complexType);
        }

        internal static Dictionary<string, IEdmDirectValueAnnotationBinding> GetAnnotationBindingsToRemoveSerializableEpmAnnotations(this IEdmModel model, IEdmElement annotatable)
        {
            Dictionary<string, IEdmDirectValueAnnotationBinding> dictionary = new Dictionary<string, IEdmDirectValueAnnotationBinding>(StringComparer.Ordinal);
            IEnumerable<IEdmDirectValueAnnotation> oDataAnnotations = model.GetODataAnnotations(annotatable);
            if (oDataAnnotations != null)
            {
                foreach (IEdmDirectValueAnnotation annotation in oDataAnnotations)
                {
                    if (annotation.IsEpmAnnotation())
                    {
                        dictionary.Add(annotation.Name, new EdmDirectValueAnnotationBinding(annotatable, annotation.NamespaceUri, annotation.Name, null));
                    }
                }
            }
            return dictionary;
        }

        internal static ODataEntityPropertyMappingCollection GetEntityPropertyMappings(this IEdmModel model, IEdmEntityType entityType)
        {
            return model.GetAnnotationValue<ODataEntityPropertyMappingCollection>(entityType);
        }

        internal static ODataEntityPropertyMappingCache GetEpmCache(this IEdmModel model, IEdmEntityType entityType)
        {
            return model.GetAnnotationValue<ODataEntityPropertyMappingCache>(entityType);
        }

        private static IEdmDirectValueAnnotationBinding GetODataAnnotationBinding(IEdmElement annotatable, string localName, string value)
        {
            IEdmStringValue value2 = null;
            if (value != null)
            {
                value2 = new EdmStringConstant(EdmCoreModel.Instance.GetString(true), value);
            }
            return new EdmDirectValueAnnotationBinding(annotatable, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", localName, value2);
        }

        internal static bool HasEntityPropertyMappings(this IEdmModel model, IEdmEntityType entityType)
        {
            for (IEdmEntityType type = entityType; type != null; type = type.BaseEntityType())
            {
                if (model.GetEntityPropertyMappings(type) != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasOwnOrInheritedEpm(this IEdmModel model, IEdmEntityType entityType)
        {
            if (entityType == null)
            {
                return false;
            }
            if (model.GetAnnotationValue<ODataEntityPropertyMappingCollection>(entityType) != null)
            {
                return true;
            }
            LoadEpmAnnotations(model, entityType);
            return ((model.GetAnnotationValue<ODataEntityPropertyMappingCollection>(entityType) != null) || model.HasOwnOrInheritedEpm(entityType.BaseEntityType()));
        }

        private static bool IsEpmAnnotation(this IEdmDirectValueAnnotation annotation)
        {
            string str;
            string str2;
            return annotation.IsEpmAnnotation(out str, out str2);
        }

        private static bool IsEpmAnnotation(this IEdmDirectValueAnnotation annotation, out string baseName, out string suffix)
        {
            string name = annotation.Name;
            for (int i = 0; i < EpmAnnotationBaseNames.Length; i++)
            {
                string str2 = EpmAnnotationBaseNames[i];
                if (name.StartsWith(str2, StringComparison.Ordinal))
                {
                    baseName = str2;
                    suffix = name.Substring(str2.Length);
                    return true;
                }
            }
            baseName = null;
            suffix = null;
            return false;
        }

        private static void LoadEpmAnnotations(IEdmModel model, IEdmEntityType entityType)
        {
            string typeName = entityType.ODataFullName();
            ODataEntityPropertyMappingCollection mappings = new ODataEntityPropertyMappingCollection();
            model.LoadEpmAnnotations(entityType, mappings, typeName, null);
            IEnumerable<IEdmProperty> declaredProperties = entityType.DeclaredProperties;
            if (declaredProperties != null)
            {
                foreach (IEdmProperty property in declaredProperties)
                {
                    model.LoadEpmAnnotations(property, mappings, typeName, property);
                }
            }
            model.SetAnnotationValue<ODataEntityPropertyMappingCollection>(entityType, mappings);
        }

        private static void LoadEpmAnnotations(this IEdmModel model, IEdmElement annotatable, ODataEntityPropertyMappingCollection mappings, string typeName, IEdmProperty property)
        {
            IEnumerable<EpmAnnotationValues> enumerable = model.ParseSerializableEpmAnnotations(annotatable, typeName, property);
            if (enumerable != null)
            {
                foreach (EpmAnnotationValues values in enumerable)
                {
                    EntityPropertyMappingAttribute mapping = ValidateAnnotationValues(values, typeName, property);
                    mappings.Add(mapping);
                }
            }
        }

        private static SyndicationTextContentKind MapContentKindToSyndicationTextContentKind(string contentKind, string attributeSuffix, string typeName, string propertyName)
        {
            switch (contentKind)
            {
                case "text":
                    return SyndicationTextContentKind.Plaintext;

                case "html":
                    return SyndicationTextContentKind.Html;

                case "xhtml":
                    return SyndicationTextContentKind.Xhtml;
            }
            string message = (propertyName == null) ? Microsoft.Data.OData.Strings.EpmExtensionMethods_InvalidTargetTextContentKindOnType("FC_ContentKind" + attributeSuffix, typeName) : Microsoft.Data.OData.Strings.EpmExtensionMethods_InvalidTargetTextContentKindOnProperty("FC_ContentKind" + attributeSuffix, propertyName, typeName);
            throw new ODataException(message);
        }

        private static SyndicationItemProperty MapTargetPathToSyndicationProperty(string targetPath)
        {
            SyndicationItemProperty property;
            if (!TargetPathToSyndicationItemMap.TryGetValue(targetPath, out property))
            {
                return SyndicationItemProperty.CustomProperty;
            }
            return property;
        }

        private static bool NamesMatchByReference(string first, string second)
        {
            return object.ReferenceEquals(first, second);
        }

        private static IEnumerable<EpmAnnotationValues> ParseSerializableEpmAnnotations(this IEdmModel model, IEdmElement annotatable, string typeName, IEdmProperty property)
        {
            Dictionary<string, EpmAnnotationValues> dictionary = null;
            IEnumerable<IEdmDirectValueAnnotation> oDataAnnotations = model.GetODataAnnotations(annotatable);
            if (oDataAnnotations != null)
            {
                foreach (IEdmDirectValueAnnotation annotation in oDataAnnotations)
                {
                    string str;
                    string str2;
                    if (annotation.IsEpmAnnotation(out str2, out str))
                    {
                        EpmAnnotationValues values;
                        string str3 = ConvertEdmAnnotationValue(annotation);
                        if (dictionary == null)
                        {
                            dictionary = new Dictionary<string, EpmAnnotationValues>(StringComparer.Ordinal);
                        }
                        if (!dictionary.TryGetValue(str, out values))
                        {
                            values = new EpmAnnotationValues {
                                AttributeSuffix = str
                            };
                            dictionary[str] = values;
                        }
                        if (!NamesMatchByReference("FC_TargetPath", str2))
                        {
                            if (!NamesMatchByReference("FC_SourcePath", str2))
                            {
                                if (!NamesMatchByReference("FC_KeepInContent", str2))
                                {
                                    if (!NamesMatchByReference("FC_ContentKind", str2))
                                    {
                                        if (!NamesMatchByReference("FC_NsUri", str2))
                                        {
                                            if (!NamesMatchByReference("FC_NsPrefix", str2))
                                            {
                                                throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataUtils_ParseSerializableEpmAnnotations_UnreachableCodePath));
                                            }
                                            values.NamespacePrefix = str3;
                                        }
                                        else
                                        {
                                            values.NamespaceUri = str3;
                                        }
                                    }
                                    else
                                    {
                                        values.ContentKind = str3;
                                    }
                                }
                                else
                                {
                                    values.KeepInContent = str3;
                                }
                            }
                            else
                            {
                                values.SourcePath = str3;
                            }
                        }
                        else
                        {
                            values.TargetPath = str3;
                        }
                    }
                }
                if (dictionary != null)
                {
                    foreach (EpmAnnotationValues values3 in dictionary.Values)
                    {
                        string sourcePath = values3.SourcePath;
                        if (sourcePath == null)
                        {
                            if (property == null)
                            {
                                throw new ODataException(Microsoft.Data.OData.Strings.EpmExtensionMethods_MissingAttributeOnType("FC_SourcePath" + values3.AttributeSuffix, typeName));
                            }
                            values3.SourcePath = property.Name;
                        }
                        else if ((property != null) && !property.Type.IsODataPrimitiveTypeKind())
                        {
                            values3.SourcePath = property.Name + "/" + sourcePath;
                        }
                    }
                }
            }
            if (dictionary != null)
            {
                return dictionary.Values;
            }
            return null;
        }

        private static void RemoveEpmCache(this IEdmModel model, IEdmEntityType entityType)
        {
            model.SetAnnotationValue<ODataEntityPropertyMappingCache>(entityType, null);
        }

        internal static void SaveEpmAnnotations(this IEdmModel model, IEdmElement annotatable, IEnumerable<EntityPropertyMappingAttribute> mappings, bool skipSourcePath, bool removePrefix)
        {
            EpmAttributeNameBuilder builder = new EpmAttributeNameBuilder();
            Dictionary<string, IEdmDirectValueAnnotationBinding> annotationBindingsToRemoveSerializableEpmAnnotations = model.GetAnnotationBindingsToRemoveSerializableEpmAnnotations(annotatable);
            foreach (EntityPropertyMappingAttribute attribute in mappings)
            {
                string epmKeepInContent;
                if (attribute.TargetSyndicationItem == SyndicationItemProperty.CustomProperty)
                {
                    epmKeepInContent = builder.EpmTargetPath;
                    annotationBindingsToRemoveSerializableEpmAnnotations[epmKeepInContent] = GetODataAnnotationBinding(annotatable, epmKeepInContent, attribute.TargetPath);
                    epmKeepInContent = builder.EpmNsUri;
                    annotationBindingsToRemoveSerializableEpmAnnotations[epmKeepInContent] = GetODataAnnotationBinding(annotatable, epmKeepInContent, attribute.TargetNamespaceUri);
                    string targetNamespacePrefix = attribute.TargetNamespacePrefix;
                    if (!string.IsNullOrEmpty(targetNamespacePrefix))
                    {
                        epmKeepInContent = builder.EpmNsPrefix;
                        annotationBindingsToRemoveSerializableEpmAnnotations[epmKeepInContent] = GetODataAnnotationBinding(annotatable, epmKeepInContent, targetNamespacePrefix);
                    }
                }
                else
                {
                    epmKeepInContent = builder.EpmTargetPath;
                    annotationBindingsToRemoveSerializableEpmAnnotations[epmKeepInContent] = GetODataAnnotationBinding(annotatable, epmKeepInContent, attribute.TargetSyndicationItem.ToAttributeValue());
                    epmKeepInContent = builder.EpmContentKind;
                    annotationBindingsToRemoveSerializableEpmAnnotations[epmKeepInContent] = GetODataAnnotationBinding(annotatable, epmKeepInContent, attribute.TargetTextContentKind.ToAttributeValue());
                }
                if (!skipSourcePath)
                {
                    string sourcePath = attribute.SourcePath;
                    if (removePrefix)
                    {
                        sourcePath = sourcePath.Substring(sourcePath.IndexOf('/') + 1);
                    }
                    epmKeepInContent = builder.EpmSourcePath;
                    annotationBindingsToRemoveSerializableEpmAnnotations[epmKeepInContent] = GetODataAnnotationBinding(annotatable, epmKeepInContent, sourcePath);
                }
                string str4 = attribute.KeepInContent ? "true" : "false";
                epmKeepInContent = builder.EpmKeepInContent;
                annotationBindingsToRemoveSerializableEpmAnnotations[epmKeepInContent] = GetODataAnnotationBinding(annotatable, epmKeepInContent, str4);
                builder.MoveNext();
            }
            model.SetAnnotationValues(annotationBindingsToRemoveSerializableEpmAnnotations.Values);
        }

        internal static void SaveEpmAnnotationsForProperty(this IEdmModel model, IEdmProperty property, ODataEntityPropertyMappingCache epmCache)
        {
            bool flag;
            bool flag2;
            Func<EntityPropertyMappingAttribute, bool> predicate = null;
            string propertyName = property.Name;
            IEnumerable<EntityPropertyMappingAttribute> source = epmCache.MappingsForDeclaredProperties.Where<EntityPropertyMappingAttribute>(delegate (EntityPropertyMappingAttribute m) {
                if (!m.SourcePath.StartsWith(propertyName, StringComparison.Ordinal))
                {
                    return false;
                }
                if (m.SourcePath.Length != propertyName.Length)
                {
                    return m.SourcePath[propertyName.Length] == '/';
                }
                return true;
            });
            if (property.Type.IsODataPrimitiveTypeKind())
            {
                flag = true;
                flag2 = false;
            }
            else
            {
                flag2 = true;
                if (predicate == null)
                {
                    predicate = m => m.SourcePath == propertyName;
                }
                flag = source.Any<EntityPropertyMappingAttribute>(predicate);
            }
            model.SaveEpmAnnotations(property, source, flag, flag2);
        }

        private static string ToAttributeValue(this SyndicationItemProperty syndicationItemProperty)
        {
            switch (syndicationItemProperty)
            {
                case SyndicationItemProperty.AuthorEmail:
                    return "SyndicationAuthorEmail";

                case SyndicationItemProperty.AuthorName:
                    return "SyndicationAuthorName";

                case SyndicationItemProperty.AuthorUri:
                    return "SyndicationAuthorUri";

                case SyndicationItemProperty.ContributorEmail:
                    return "SyndicationContributorEmail";

                case SyndicationItemProperty.ContributorName:
                    return "SyndicationContributorName";

                case SyndicationItemProperty.ContributorUri:
                    return "SyndicationContributorUri";

                case SyndicationItemProperty.Updated:
                    return "SyndicationUpdated";

                case SyndicationItemProperty.Published:
                    return "SyndicationPublished";

                case SyndicationItemProperty.Rights:
                    return "SyndicationRights";

                case SyndicationItemProperty.Summary:
                    return "SyndicationSummary";

                case SyndicationItemProperty.Title:
                    return "SyndicationTitle";
            }
            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.EpmExtensionMethods_ToAttributeValue_SyndicationItemProperty));
        }

        private static string ToAttributeValue(this SyndicationTextContentKind contentKind)
        {
            switch (contentKind)
            {
                case SyndicationTextContentKind.Html:
                    return "html";

                case SyndicationTextContentKind.Xhtml:
                    return "xhtml";
            }
            return "text";
        }

        internal static string ToTargetPath(this SyndicationItemProperty targetSyndicationItem)
        {
            switch (targetSyndicationItem)
            {
                case SyndicationItemProperty.AuthorEmail:
                    return "author/email";

                case SyndicationItemProperty.AuthorName:
                    return "author/name";

                case SyndicationItemProperty.AuthorUri:
                    return "author/uri";

                case SyndicationItemProperty.ContributorEmail:
                    return "contributor/email";

                case SyndicationItemProperty.ContributorName:
                    return "contributor/name";

                case SyndicationItemProperty.ContributorUri:
                    return "contributor/uri";

                case SyndicationItemProperty.Updated:
                    return "updated";

                case SyndicationItemProperty.Published:
                    return "published";

                case SyndicationItemProperty.Rights:
                    return "rights";

                case SyndicationItemProperty.Summary:
                    return "summary";

                case SyndicationItemProperty.Title:
                    return "title";
            }
            throw new ArgumentException(Microsoft.Data.OData.Strings.EntityPropertyMapping_EpmAttribute("targetSyndicationItem"));
        }

        private static EntityPropertyMappingAttribute ValidateAnnotationValues(EpmAnnotationValues annotationValues, string typeName, IEdmProperty property)
        {
            if (annotationValues.TargetPath == null)
            {
                string str = "FC_TargetPath" + annotationValues.AttributeSuffix;
                string message = (property == null) ? Microsoft.Data.OData.Strings.EpmExtensionMethods_MissingAttributeOnType(str, typeName) : Microsoft.Data.OData.Strings.EpmExtensionMethods_MissingAttributeOnProperty(str, property.Name, typeName);
                throw new ODataException(message);
            }
            bool result = true;
            if ((annotationValues.KeepInContent != null) && !bool.TryParse(annotationValues.KeepInContent, out result))
            {
                string str3 = "FC_KeepInContent" + annotationValues.AttributeSuffix;
                throw new InvalidOperationException((property == null) ? Microsoft.Data.OData.Strings.EpmExtensionMethods_InvalidKeepInContentOnType(str3, typeName) : Microsoft.Data.OData.Strings.EpmExtensionMethods_InvalidKeepInContentOnProperty(str3, property.Name, typeName));
            }
            SyndicationItemProperty targetSyndicationItem = MapTargetPathToSyndicationProperty(annotationValues.TargetPath);
            if (targetSyndicationItem == SyndicationItemProperty.CustomProperty)
            {
                if (annotationValues.ContentKind != null)
                {
                    string str4 = "FC_ContentKind" + annotationValues.AttributeSuffix;
                    string str5 = (property == null) ? Microsoft.Data.OData.Strings.EpmExtensionMethods_AttributeNotAllowedForCustomMappingOnType(str4, typeName) : Microsoft.Data.OData.Strings.EpmExtensionMethods_AttributeNotAllowedForCustomMappingOnProperty(str4, property.Name, typeName);
                    throw new ODataException(str5);
                }
                return new EntityPropertyMappingAttribute(annotationValues.SourcePath, annotationValues.TargetPath, annotationValues.NamespacePrefix, annotationValues.NamespaceUri, result);
            }
            if (annotationValues.NamespaceUri != null)
            {
                string str6 = "FC_NsUri" + annotationValues.AttributeSuffix;
                string str7 = (property == null) ? Microsoft.Data.OData.Strings.EpmExtensionMethods_AttributeNotAllowedForAtomPubMappingOnType(str6, typeName) : Microsoft.Data.OData.Strings.EpmExtensionMethods_AttributeNotAllowedForAtomPubMappingOnProperty(str6, property.Name, typeName);
                throw new ODataException(str7);
            }
            if (annotationValues.NamespacePrefix != null)
            {
                string str8 = "FC_NsPrefix" + annotationValues.AttributeSuffix;
                string str9 = (property == null) ? Microsoft.Data.OData.Strings.EpmExtensionMethods_AttributeNotAllowedForAtomPubMappingOnType(str8, typeName) : Microsoft.Data.OData.Strings.EpmExtensionMethods_AttributeNotAllowedForAtomPubMappingOnProperty(str8, property.Name, typeName);
                throw new ODataException(str9);
            }
            SyndicationTextContentKind plaintext = SyndicationTextContentKind.Plaintext;
            if (annotationValues.ContentKind != null)
            {
                plaintext = MapContentKindToSyndicationTextContentKind(annotationValues.ContentKind, annotationValues.AttributeSuffix, typeName, (property == null) ? null : property.Name);
            }
            return new EntityPropertyMappingAttribute(annotationValues.SourcePath, targetSyndicationItem, plaintext, result);
        }

        private sealed class EpmAnnotationValues
        {
            internal string AttributeSuffix { get; set; }

            internal string ContentKind { get; set; }

            internal string KeepInContent { get; set; }

            internal string NamespacePrefix { get; set; }

            internal string NamespaceUri { get; set; }

            internal string SourcePath { get; set; }

            internal string TargetPath { get; set; }
        }
    }
}

