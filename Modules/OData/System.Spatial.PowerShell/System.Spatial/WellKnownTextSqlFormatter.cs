using System.IO;

namespace System.Spatial
{
    using System;

    internal abstract class WellKnownTextSqlFormatter : SpatialFormatter<TextReader, TextWriter>
    {
        protected WellKnownTextSqlFormatter(SpatialImplementation creator) : base(creator)
        {
        }

        public static WellKnownTextSqlFormatter Create()
        {
            return SpatialImplementation.CurrentImplementation.CreateWellKnownTextSqlFormatter();
        }

        public static WellKnownTextSqlFormatter Create(bool allowOnlyTwoDimensions)
        {
            return SpatialImplementation.CurrentImplementation.CreateWellKnownTextSqlFormatter(allowOnlyTwoDimensions);
        }
    }
}

