namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Spatial;

    internal class GeometryCollectionImplementation : GeometryCollection
    {
        private Geometry[] geometryArray;

        internal GeometryCollectionImplementation(SpatialImplementation creator, params Geometry[] geometry) : this(CoordinateSystem.DefaultGeometry, creator, geometry)
        {
        }

        internal GeometryCollectionImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, params Geometry[] geometry) : base(coordinateSystem, creator)
        {
            this.geometryArray = geometry ?? new Geometry[0];
        }

        public override void SendTo(GeometryPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeometry(SpatialType.Collection);
            for (int i = 0; i < this.geometryArray.Length; i++)
            {
                this.geometryArray[i].SendTo(pipeline);
            }
            pipeline.EndGeometry();
        }

        public override ReadOnlyCollection<Geometry> Geometries
        {
            get
            {
                return this.geometryArray.AsReadOnly<Geometry>();
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return (this.geometryArray.Length == 0);
            }
        }
    }
}

