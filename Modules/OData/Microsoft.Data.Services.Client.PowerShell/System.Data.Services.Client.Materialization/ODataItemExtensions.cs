namespace System.Data.Services.Client.Materialization
{
    using Microsoft.Data.OData;
    using System;
    using System.Runtime.CompilerServices;

    internal static class ODataItemExtensions
    {
        public static object GetMaterializedValue(this ODataComplexValue complexValue)
        {
            return GetMaterializedValueCore(complexValue);
        }

        public static object GetMaterializedValue(this ODataProperty property)
        {
            ODataAnnotatable annotatableObject = (property.Value as ODataAnnotatable) ?? property;
            return GetMaterializedValueCore(annotatableObject);
        }

        private static object GetMaterializedValueCore(ODataAnnotatable annotatableObject)
        {
            return annotatableObject.GetAnnotation<MaterializerPropertyValue>().Value;
        }

        public static bool HasMaterializedValue(this ODataComplexValue complexValue)
        {
            return HasMaterializedValueCore(complexValue);
        }

        public static bool HasMaterializedValue(this ODataProperty property)
        {
            ODataAnnotatable annotatableObject = (property.Value as ODataAnnotatable) ?? property;
            return HasMaterializedValueCore(annotatableObject);
        }

        private static bool HasMaterializedValueCore(ODataAnnotatable annotatableObject)
        {
            return (annotatableObject.GetAnnotation<MaterializerPropertyValue>() != null);
        }

        public static void SetMaterializedValue(this ODataComplexValue complexValue, object materializedValue)
        {
            SetMaterializedValueCore(complexValue, materializedValue);
        }

        public static void SetMaterializedValue(this ODataProperty property, object materializedValue)
        {
            ODataAnnotatable annotatableObject = (property.Value as ODataAnnotatable) ?? property;
            SetMaterializedValueCore(annotatableObject, materializedValue);
        }

        private static void SetMaterializedValueCore(ODataAnnotatable annotatableObject, object materializedValue)
        {
            MaterializerPropertyValue annotation = new MaterializerPropertyValue {
                Value = materializedValue
            };
            annotatableObject.SetAnnotation<MaterializerPropertyValue>(annotation);
        }

        private class MaterializerPropertyValue
        {
            public object Value { get; set; }
        }
    }
}

