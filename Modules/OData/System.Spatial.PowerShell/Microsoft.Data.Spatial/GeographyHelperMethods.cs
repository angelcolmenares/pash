namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Spatial;

    internal static class GeographyHelperMethods
    {
        internal static void SendFigure(this GeographyLineString lineString, GeographyPipeline pipeline)
        {
            ReadOnlyCollection<GeographyPoint> points = lineString.Points;
            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0)
                {
                    pipeline.BeginFigure(new GeographyPosition(points[i].Latitude, points[i].Longitude, points[i].Z, points[i].M));
                }
                else
                {
                    pipeline.LineTo(new GeographyPosition(points[i].Latitude, points[i].Longitude, points[i].Z, points[i].M));
                }
            }
            if (points.Count > 0)
            {
                pipeline.EndFigure();
            }
        }
    }
}

