namespace Microsoft.Data.Spatial
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Spatial;

    internal class DataServicesSpatialImplementation : SpatialImplementation
    {
        public override SpatialBuilder CreateBuilder()
        {
            GeographyBuilderImplementation currentGeography = new GeographyBuilderImplementation(this);
            GeometryBuilderImplementation currentGeometry = new GeometryBuilderImplementation(this);
            ForwardingSegment segment = new ForwardingSegment(currentGeography, currentGeometry);
            return new SpatialBuilder((GeographyPipeline) segment, (GeometryPipeline) segment, currentGeography, currentGeometry);
        }

        public override GeoJsonObjectFormatter CreateGeoJsonObjectFormatter()
        {
            return new GeoJsonObjectFormatterImplementation(this);
        }

        public override GmlFormatter CreateGmlFormatter()
        {
            return new GmlFormatterImplementation(this);
        }

        public override SpatialPipeline CreateValidator()
        {
            return new ForwardingSegment(new SpatialValidatorImplementation());
        }

        public override WellKnownTextSqlFormatter CreateWellKnownTextSqlFormatter()
        {
            return new WellKnownTextSqlFormatterImplementation(this);
        }

        public override WellKnownTextSqlFormatter CreateWellKnownTextSqlFormatter(bool allowOnlyTwoDimensions)
        {
            return new WellKnownTextSqlFormatterImplementation(this, allowOnlyTwoDimensions);
        }

        public override SpatialOperations Operations { get; set; }
    }
}

