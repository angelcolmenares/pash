namespace System.Spatial
{
    using System;
    using System.Collections.Generic;

    internal abstract class GeoJsonObjectFormatter
    {
        protected GeoJsonObjectFormatter()
        {
        }

        public static GeoJsonObjectFormatter Create()
        {
            return SpatialImplementation.CurrentImplementation.CreateGeoJsonObjectFormatter();
        }

        public abstract T Read<T>(IDictionary<string, object> source) where T: class, ISpatial;
        public abstract IDictionary<string, object> Write(ISpatial value);
    }
}

