namespace System.Spatial
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class SpatialTypeExtensions
    {
        public static void SendTo(this ISpatial shape, SpatialPipeline destination)
        {
            if (shape != null)
            {
                if (shape.GetType().IsSubclassOf(typeof(Geometry)))
                {
                    ((Geometry) shape).SendTo((GeometryPipeline) destination);
                }
                else
                {
                    ((Geography) shape).SendTo((GeographyPipeline) destination);
                }
            }
        }
    }
}

