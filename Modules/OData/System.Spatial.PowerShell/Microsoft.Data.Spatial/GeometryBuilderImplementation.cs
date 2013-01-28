namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Spatial;

    internal class GeometryBuilderImplementation : GeometryPipeline, IGeometryProvider
    {
        private readonly SpatialTreeBuilder<Geometry> builder;

        public event Action<Geometry> ProduceGeometry
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

        public GeometryBuilderImplementation(SpatialImplementation creator)
        {
            this.builder = new GeometryTreeBuilder(creator);
        }

        public override void BeginFigure(GeometryPosition position)
        {
            this.builder.BeginFigure(position.X, position.Y, position.Z, position.M);
        }

        public override void BeginGeometry(SpatialType type)
        {
            this.builder.BeginGeo(type);
        }

        public override void EndFigure()
        {
            this.builder.EndFigure();
        }

        public override void EndGeometry()
        {
            this.builder.EndGeo();
        }

        public override void LineTo(GeometryPosition position)
        {
            this.builder.LineTo(position.X, position.Y, position.Z, position.M);
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

        public Geometry ConstructedGeometry
        {
            get
            {
                return this.builder.ConstructedInstance;
            }
        }

        private class GeometryTreeBuilder : SpatialTreeBuilder<Geometry>
        {
            private CoordinateSystem buildCoordinateSystem;
            private readonly SpatialImplementation creator;

            public GeometryTreeBuilder(SpatialImplementation creator)
            {
                Util.CheckArgumentNull(creator, "creator");
                this.creator = creator;
            }

            protected override Geometry CreatePoint(bool isEmpty, double x, double y, double? z, double? m)
            {
                if (!isEmpty)
                {
                    return new GeometryPointImplementation(this.buildCoordinateSystem, this.creator, x, y, z, m);
                }
                return new GeometryPointImplementation(this.buildCoordinateSystem, this.creator);
            }

            protected override Geometry CreateShapeInstance(SpatialType type, IEnumerable<Geometry> spatialData)
            {
                switch (type)
                {
                    case SpatialType.LineString:
                        return new GeometryLineStringImplementation(this.buildCoordinateSystem, this.creator, spatialData.Cast<GeometryPoint>().ToArray<GeometryPoint>());

                    case SpatialType.Polygon:
                        return new GeometryPolygonImplementation(this.buildCoordinateSystem, this.creator, spatialData.Cast<GeometryLineString>().ToArray<GeometryLineString>());

                    case SpatialType.MultiPoint:
                        return new GeometryMultiPointImplementation(this.buildCoordinateSystem, this.creator, spatialData.Cast<GeometryPoint>().ToArray<GeometryPoint>());

                    case SpatialType.MultiLineString:
                        return new GeometryMultiLineStringImplementation(this.buildCoordinateSystem, this.creator, spatialData.Cast<GeometryLineString>().ToArray<GeometryLineString>());

                    case SpatialType.MultiPolygon:
                        return new GeometryMultiPolygonImplementation(this.buildCoordinateSystem, this.creator, spatialData.Cast<GeometryPolygon>().ToArray<GeometryPolygon>());

                    case SpatialType.Collection:
                        return new GeometryCollectionImplementation(this.buildCoordinateSystem, this.creator, spatialData.ToArray<Geometry>());
                }
                return null;
            }

            internal override void SetCoordinateSystem(int? epsgId)
            {
                this.buildCoordinateSystem = CoordinateSystem.Geometry(epsgId);
            }
        }
    }
}

