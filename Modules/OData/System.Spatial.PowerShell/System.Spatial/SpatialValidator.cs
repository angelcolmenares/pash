namespace System.Spatial
{
    using System;

    internal static class SpatialValidator
    {
        public static SpatialPipeline Create()
        {
            return SpatialImplementation.CurrentImplementation.CreateValidator();
        }
    }
}

