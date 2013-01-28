namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Spatial;

    internal class GeoJsonObjectFormatterImplementation : GeoJsonObjectFormatter
    {
        private SpatialBuilder builder;
        private readonly SpatialImplementation creator;
        private SpatialPipeline parsePipeline;

        public GeoJsonObjectFormatterImplementation(SpatialImplementation creator)
        {
            this.creator = creator;
        }

        private void EnsureParsePipeline()
        {
            if (this.parsePipeline == null)
            {
                this.builder = this.creator.CreateBuilder();
                this.parsePipeline = this.creator.CreateValidator().ChainTo(this.builder);
            }
            else
            {
                this.parsePipeline.GeographyPipeline.Reset();
                this.parsePipeline.GeometryPipeline.Reset();
            }
        }

        public override T Read<T>(IDictionary<string, object> source)
        {
            this.EnsureParsePipeline();
            if (typeof(Geometry).IsAssignableFrom(typeof(T)))
            {
                new GeoJsonObjectReader(this.builder).ReadGeometry(source);
                return (this.builder.ConstructedGeometry as T);
            }
            new GeoJsonObjectReader(this.builder).ReadGeography(source);
            return (this.builder.ConstructedGeography as T);
        }

        public override IDictionary<string, object> Write(ISpatial value)
        {
            GeoJsonObjectWriter writer = new GeoJsonObjectWriter();
            value.SendTo(new ForwardingSegment((SpatialPipeline) writer));
            return writer.JsonObject;
        }
    }
}

