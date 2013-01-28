namespace System.Data.Services.Client.Materialization
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Client;

    internal static class ODataEntityMaterializerInvoker
    {
        internal static object DirectMaterializePlan(object materializer, object entry, Type expectedEntryType)
        {
            return ODataEntityMaterializer.DirectMaterializePlan((ODataEntityMaterializer) materializer, MaterializerEntry.GetEntry((ODataEntry) entry), expectedEntryType);
        }

        internal static IEnumerable<T> EnumerateAsElementType<T>(IEnumerable source)
        {
            return ODataEntityMaterializer.EnumerateAsElementType<T>(source);
        }

        internal static List<TTarget> ListAsElementType<T, TTarget>(object materializer, IEnumerable<T> source) where T: TTarget
        {
            return ODataEntityMaterializer.ListAsElementType<T, TTarget>((ODataEntityMaterializer) materializer, source);
        }

        internal static bool ProjectionCheckValueForPathIsNull(object entry, Type expectedType, object path)
        {
            return ODataEntityMaterializer.ProjectionCheckValueForPathIsNull(MaterializerEntry.GetEntry((ODataEntry) entry), expectedType, (ProjectionPath) path);
        }

        internal static object ProjectionGetEntry(object entry, string name)
        {
            return ODataEntityMaterializer.ProjectionGetEntry(MaterializerEntry.GetEntry((ODataEntry) entry), name);
        }

        internal static object ProjectionInitializeEntity(object materializer, object entry, Type expectedType, Type resultType, string[] properties, Func<object, object, Type, object>[] propertyValues)
        {
            return ODataEntityMaterializer.ProjectionInitializeEntity((ODataEntityMaterializer) materializer, MaterializerEntry.GetEntry((ODataEntry) entry), expectedType, resultType, properties, propertyValues);
        }

        internal static IEnumerable ProjectionSelect(object materializer, object entry, Type expectedType, Type resultType, object path, Func<object, object, Type, object> selector)
        {
            return ODataEntityMaterializer.ProjectionSelect((ODataEntityMaterializer) materializer, MaterializerEntry.GetEntry((ODataEntry) entry), expectedType, resultType, (ProjectionPath) path, selector);
        }

        internal static object ProjectionValueForPath(object materializer, object entry, Type expectedType, object path)
        {
            return ODataEntityMaterializer.ProjectionValueForPath((ODataEntityMaterializer) materializer, MaterializerEntry.GetEntry((ODataEntry) entry), expectedType, (ProjectionPath) path);
        }

        internal static object ShallowMaterializePlan(object materializer, object entry, Type expectedEntryType)
        {
            return ODataEntityMaterializer.ShallowMaterializePlan((ODataEntityMaterializer) materializer, MaterializerEntry.GetEntry((ODataEntry) entry), expectedEntryType);
        }
    }
}

