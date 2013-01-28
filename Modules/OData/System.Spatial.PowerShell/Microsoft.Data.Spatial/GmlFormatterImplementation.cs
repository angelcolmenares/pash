namespace Microsoft.Data.Spatial
{
    using System;
    using System.Spatial;
    using System.Xml;

    internal class GmlFormatterImplementation : GmlFormatter
    {
        internal GmlFormatterImplementation(SpatialImplementation creator) : base(creator)
        {
        }

        public override SpatialPipeline CreateWriter(XmlWriter target)
        {
            return new ForwardingSegment((SpatialPipeline) new GmlWriter(target));
        }

        protected override void ReadGeography(XmlReader readerStream, SpatialPipeline pipeline)
        {
            new GmlReader(pipeline).ReadGeography(readerStream);
        }

        protected override void ReadGeometry(XmlReader readerStream, SpatialPipeline pipeline)
        {
            new GmlReader(pipeline).ReadGeometry(readerStream);
        }
    }
}

