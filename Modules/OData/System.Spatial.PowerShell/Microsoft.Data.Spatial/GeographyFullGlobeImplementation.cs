namespace Microsoft.Data.Spatial
{
    using System;
    using System.Spatial;

    internal class GeographyFullGlobeImplementation : GeographyFullGlobe
    {
        internal GeographyFullGlobeImplementation(SpatialImplementation creator) : this(CoordinateSystem.DefaultGeography, creator)
        {
        }

        internal GeographyFullGlobeImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }

        public override void SendTo(GeographyPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeography(SpatialType.FullGlobe);
            pipeline.EndGeography();
        }

        public override bool IsEmpty
        {
            get
            {
                return false;
            }
        }
    }
}

