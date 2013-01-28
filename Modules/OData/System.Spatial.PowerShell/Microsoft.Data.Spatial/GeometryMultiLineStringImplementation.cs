namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Spatial;

    internal class GeometryMultiLineStringImplementation : GeometryMultiLineString
    {
        private GeometryLineString[] lineStrings;

        internal GeometryMultiLineStringImplementation(SpatialImplementation creator, params GeometryLineString[] lineStrings) : this(CoordinateSystem.DefaultGeometry, creator, lineStrings)
        {
        }

        internal GeometryMultiLineStringImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, params GeometryLineString[] lineStrings) : base(coordinateSystem, creator)
        {
            this.lineStrings = lineStrings ?? new GeometryLineString[0];
        }

        public override void SendTo(GeometryPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeometry(SpatialType.MultiLineString);
            for (int i = 0; i < this.lineStrings.Length; i++)
            {
                this.lineStrings[i].SendTo(pipeline);
            }
            pipeline.EndGeometry();
        }

        public override ReadOnlyCollection<Geometry> Geometries
        {
            get
            {
                return this.lineStrings.AsReadOnly<Geometry>();
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return (this.lineStrings.Length == 0);
            }
        }

        public override ReadOnlyCollection<GeometryLineString> LineStrings
        {
            get
            {
                return this.lineStrings.AsReadOnly<GeometryLineString>();
            }
        }
    }
}

