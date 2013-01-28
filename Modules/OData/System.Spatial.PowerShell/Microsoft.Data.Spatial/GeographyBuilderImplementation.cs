namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Spatial;

    internal class GeographyBuilderImplementation : GeographyPipeline, IGeographyProvider
    {
        private readonly SpatialTreeBuilder<Geography> builder;

        public event Action<Geography> ProduceGeography
        {
            add
            {
                this.builder.ProduceInstance += value;
            }
            remove
            {
                this.builder.ProduceInstance -= value;
            }
        }

        public GeographyBuilderImplementation(SpatialImplementation creator)
        {
            this.builder = new GeographyTreeBuilder(creator);
        }

        public override void BeginFigure(GeographyPosition position)
        {
            this.builder.BeginFigure(position.Latitude, position.Longitude, position.Z, position.M);
        }

        public override void BeginGeography(SpatialType type)
        {
            this.builder.BeginGeo(type);
        }

        public override void EndFigure()
        {
            this.builder.EndFigure();
        }

        public override void EndGeography()
        {
            this.builder.EndGeo();
        }

        public override void LineTo(GeographyPosition position)
        {
            this.builder.LineTo(position.Latitude, position.Longitude, position.Z, position.M);
        }

        public override void Reset()
        {
            this.builder.Reset();
        }

        public override void SetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            Util.CheckArgumentNull(coordinateSystem, "coordinateSystem");
            this.builder.SetCoordinateSystem(coordinateSystem.EpsgId);
        }

        public Geography ConstructedGeography
        {
            get
            {
                return this.builder.ConstructedInstance;
            }
        }

        private class GeographyTreeBuilder : SpatialTreeBuilder<Geography>
        {
            private readonly SpatialImplementation creator;
            private CoordinateSystem currentCoordinateSystem;

            public GeographyTreeBuilder(SpatialImplementation creator)
            {
                Util.CheckArgumentNull(creator, "creator");
                this.creator = creator;
            }

            protected override Geography CreatePoint(bool isEmpty, double x, double y, double? z, double? m)
            {
                if (!isEmpty)
                {
                    return new GeographyPointImplementation(this.currentCoordinateSystem, this.creator, x, y, z, m);
                }
                return new GeographyPointImplementation(this.currentCoordinateSystem, this.creator);
            }

            protected override Geography CreateShapeInstance(SpatialType type, IEnumerable<Geography> spatialData)
            {
                switch (type)
                {
                    case SpatialType.LineString:
                        return new GeographyLineStringImplementation(this.currentCoordinateSystem, this.creator, spatialData.Cast<GeographyPoint>().ToArray<GeographyPoint>());

                    case SpatialType.Polygon:
                        return new GeographyPolygonImplementation(this.currentCoordinateSystem, this.creator, spatialData.Cast<GeographyLineString>().ToArray<GeographyLineString>());

                    case SpatialType.MultiPoint:
                        return new GeographyMultiPointImplementation(this.currentCoordinateSystem, this.creator, spatialData.Cast<GeographyPoint>().ToArray<GeographyPoint>());

                    case SpatialType.MultiLineString:
                        return new GeographyMultiLineStringImplementation(this.currentCoordinateSystem, this.creator, spatialData.Cast<GeographyLineString>().ToArray<GeographyLineString>());

                    case SpatialType.MultiPolygon:
                        return new GeographyMultiPolygonImplementation(this.currentCoordinateSystem, this.creator, spatialData.Cast<GeographyPolygon>().ToArray<GeographyPolygon>());

                    case SpatialType.Collection:
                        return new GeographyCollectionImplementation(this.currentCoordinateSystem, this.creator, spatialData.ToArray<Geography>());

                    case SpatialType.FullGlobe:
                        return new GeographyFullGlobeImplementation(this.currentCoordinateSystem, this.creator);
                }
                return null;
            }

            internal override void SetCoordinateSystem(int? epsgId)
            {
                this.currentCoordinateSystem = CoordinateSystem.Geography(epsgId);
            }
        }
    }
}

