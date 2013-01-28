namespace System.Spatial
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Xml;

    internal static class FormatterExtensions
    {
        public static string Write(this SpatialFormatter<TextReader, TextWriter> formatter, ISpatial spatial)
        {
            Util.CheckArgumentNull(formatter, "formatter");
            StringBuilder sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                formatter.Write(spatial, writer);
            }
            return sb.ToString();
        }

        public static string Write(this SpatialFormatter<XmlReader, XmlWriter> formatter, ISpatial spatial)
        {
            Util.CheckArgumentNull(formatter, "formatter");
            StringBuilder output = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings {
                OmitXmlDeclaration = true
            };
            using (XmlWriter writer = XmlWriter.Create(output, settings))
            {
                formatter.Write(spatial, writer);
            }
            return output.ToString();
        }
    }
}

