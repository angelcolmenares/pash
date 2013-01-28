namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Spatial;

    internal class GeographyMultiLineStringImplementation : GeographyMultiLineString
    {
        private GeographyLineString[] lineStrings;

        internal GeographyMultiLineStringImplementation(SpatialImplementation creator, params GeographyLineString[] lineStrings) : this(CoordinateSystem.DefaultGeography, creator, lineStrings)
        {
        }

        internal GeographyMultiLineStringImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, params GeographyLineString[] lineStrings) : base(coordinateSystem, creator)
        {
            this.lineStrings = lineStrings ?? new GeographyLineString[0];
        }

        public override void SendTo(GeographyPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeography(SpatialType.MultiLineString);
            for (int i = 0; i < this.lineStrings.Length; i++)
            {
                this.lineStrings[i].SendTo(pipeline);
            }
            pipeline.EndGeography();
        }

        public override ReadOnlyCollection<Geography> Geographies
        {
            get
            {
                return this.lineStrings.AsReadOnly<Geography>();
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return (this.lineStrings.Length == 0);
            }
        }

        public override ReadOnlyCollection<GeographyLineString> LineStrings
        {
            get
            {
                return this.lineStrings.AsReadOnly<GeographyLineString>();
            }
        }
    }
}

