namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Linq;
    using System.Runtime.InteropServices;

    internal static class EpmWriterUtils
    {
        internal static void CacheEpmProperties(EntryPropertiesValueCache propertyValueCache, EpmSourceTree sourceTree)
        {
            EpmSourcePathSegment root = sourceTree.Root;
            CacheEpmSourcePathSegments(propertyValueCache, root.SubProperties, propertyValueCache.EntryProperties);
        }

        private static void CacheEpmSourcePathSegments(EpmValueCache valueCache, List<EpmSourcePathSegment> segments, IEnumerable<ODataProperty> properties)
        {
            if (properties != null)
            {
                foreach (EpmSourcePathSegment segment in segments)
                {
                    ODataComplexValue value2;
                    if ((segment.EpmInfo == null) && TryGetPropertyValue<ODataComplexValue>(properties, segment.PropertyName, out value2))
                    {
                        IEnumerable<ODataProperty> enumerable = valueCache.CacheComplexValueProperties(value2);
                        CacheEpmSourcePathSegments(valueCache, segment.SubProperties, enumerable);
                    }
                }
            }
        }

        internal static EntityPropertyMappingAttribute GetEntityPropertyMapping(EpmSourcePathSegment epmSourcePathSegment)
        {
            if (epmSourcePathSegment == null)
            {
                return null;
            }
            EntityPropertyMappingInfo epmInfo = epmSourcePathSegment.EpmInfo;
            if (epmInfo == null)
            {
                return null;
            }
            return epmInfo.Attribute;
        }

        internal static EntityPropertyMappingAttribute GetEntityPropertyMapping(EpmSourcePathSegment epmParentSourcePathSegment, string propertyName)
        {
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(propertyName, "propertyName");
            return GetEntityPropertyMapping(GetPropertySourcePathSegment(epmParentSourcePathSegment, propertyName));
        }

        internal static EpmSourcePathSegment GetPropertySourcePathSegment(EpmSourcePathSegment epmParentSourcePathSegment, string propertyName)
        {
            Func<EpmSourcePathSegment, bool> predicate = null;
            EpmSourcePathSegment segment = null;
            if (epmParentSourcePathSegment == null)
            {
                return segment;
            }
            if (predicate == null)
            {
                predicate = subProperty => subProperty.PropertyName == propertyName;
            }
            return epmParentSourcePathSegment.SubProperties.FirstOrDefault<EpmSourcePathSegment>(predicate);
        }

        internal static string GetPropertyValueAsText(object propertyValue)
        {
            string str;
            if (propertyValue == null)
            {
                return null;
            }
            if (!AtomValueUtils.TryConvertPrimitiveToString(propertyValue, out str))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.AtomValueUtils_CannotConvertValueToAtomPrimitive(propertyValue.GetType().FullName));
            }
            return str;
        }

        private static bool TryGetPropertyValue<T>(IEnumerable<ODataProperty> properties, string propertyName, out T propertyValue) where T: class
        {
            propertyValue = default(T);
            ODataProperty property = (from p in properties
                where string.CompareOrdinal(p.Name, propertyName) == 0
                select p).FirstOrDefault<ODataProperty>();
            if (property == null)
            {
                return false;
            }
            propertyValue = property.Value as T;
            if (((T) propertyValue) == null)
            {
                return (property.Value == null);
            }
            return true;
        }
    }
}

