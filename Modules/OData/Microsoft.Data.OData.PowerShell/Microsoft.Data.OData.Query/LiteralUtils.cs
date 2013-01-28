namespace Microsoft.Data.OData.Query
{
    using System;
    using System.IO;
    using System.Spatial;

    internal static class LiteralUtils
    {
        internal static Geography ParseGeography(string text)
        {
            using (StringReader reader = new StringReader(text))
            {
                return Formatter.Read<Geography>(reader);
            }
        }

        internal static Geometry ParseGeometry(string text)
        {
            using (StringReader reader = new StringReader(text))
            {
                return Formatter.Read<Geometry>(reader);
            }
        }

        internal static string ToWellKnownText(Geography instance)
        {
            return Formatter.Write(instance);
        }

        internal static string ToWellKnownText(Geometry instance)
        {
            return Formatter.Write(instance);
        }

        private static WellKnownTextSqlFormatter Formatter
        {
            get
            {
                return SpatialImplementation.CurrentImplementation.CreateWellKnownTextSqlFormatter();
            }
        }
    }
}

