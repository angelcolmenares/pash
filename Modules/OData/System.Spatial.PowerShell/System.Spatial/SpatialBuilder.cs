namespace System.Spatial
{
    using System;

    internal class SpatialBuilder : SpatialPipeline, IShapeProvider, IGeographyProvider, IGeometryProvider
    {
        private readonly IGeographyProvider geographyOutput;
        private readonly IGeometryProvider geometryOutput;

        public event Action<Geography> ProduceGeography
        {
            add
            {
                this.geographyOutput.ProduceGeography += value;
            }
            remove
            {
                this.geographyOutput.ProduceGeography -= value;
            }
        }

        public event Action<Geometry> ProduceGeometry
        {
            add
            {
                this.geometryOutput.ProduceGeometry += value;
            }
            remove
            {
                this.geometryOutput.ProduceGeometry -= value;
            }
        }

        public SpatialBuilder(GeographyPipeline geographyInput, GeometryPipeline geometryInput, IGeographyProvider geographyOutput, IGeometryProvider geometryOutput) : base(geographyInput, geometryInput)
        {
            this.geographyOutput = geographyOutput;
            this.geometryOutput = geometryOutput;
        }

        public static SpatialBuilder Create()
        {
            return SpatialImplementation.CurrentImplementation.CreateBuilder();
        }

        public Geography ConstructedGeography
        {
            get
            {
                return this.geographyOutput.ConstructedGeography;
            }
        }

        public Geometry ConstructedGeometry
        {
            get
            {
                return this.geometryOutput.ConstructedGeometry;
            }
        }
    }
}

