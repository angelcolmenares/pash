namespace System.Data.Services.Client
{
    using Microsoft.Data.OData;
    using System;
    using System.Data.Services.Client.Materialization;
    using System.Runtime.CompilerServices;

    internal class ProjectionPlan
    {
        internal object Run(ODataEntityMaterializer materializer, ODataEntry entry, Type expectedType)
        {
            return this.Plan(materializer, entry, expectedType);
        }

        internal Type LastSegmentType { get; set; }

        internal Func<object, object, Type, object> Plan { get; set; }

        internal Type ProjectedType { get; set; }
    }
}

