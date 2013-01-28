namespace Microsoft.Data.Spatial
{
    using System;
    using System.IO;
    using System.Spatial;

    internal class WellKnownTextSqlFormatterImplementation : WellKnownTextSqlFormatter
    {
        private readonly bool allowOnlyTwoDimensions;

        internal WellKnownTextSqlFormatterImplementation(SpatialImplementation creator) : base(creator)
        {
        }

        internal WellKnownTextSqlFormatterImplementation(SpatialImplementation creator, bool allowOnlyTwoDimensions) : base(creator)
        {
            this.allowOnlyTwoDimensions = allowOnlyTwoDimensions;
        }

        public override SpatialPipeline CreateWriter(TextWriter target)
        {
            return new ForwardingSegment((SpatialPipeline) new WellKnownTextSqlWriter(target, this.allowOnlyTwoDimensions));
        }

        protected override void ReadGeography(TextReader readerStream, SpatialPipeline pipeline)
        {
            new WellKnownTextSqlReader(pipeline, this.allowOnlyTwoDimensions).ReadGeography(readerStream);
        }

        protected override void ReadGeometry(TextReader readerStream, SpatialPipeline pipeline)
        {
            new WellKnownTextSqlReader(pipeline, this.allowOnlyTwoDimensions).ReadGeometry(readerStream);
        }
    }
}

