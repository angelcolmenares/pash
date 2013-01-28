using System.Xml;

namespace System.Spatial
{
    using System;

    internal abstract class GmlFormatter : SpatialFormatter<XmlReader, XmlWriter>
    {
        protected GmlFormatter(SpatialImplementation creator) : base(creator)
        {
        }

        public static GmlFormatter Create()
        {
            return SpatialImplementation.CurrentImplementation.CreateGmlFormatter();
        }
    }
}

