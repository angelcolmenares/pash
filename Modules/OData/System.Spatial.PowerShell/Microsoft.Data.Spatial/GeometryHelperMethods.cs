namespace Microsoft.Data.Spatial
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Spatial;

    internal static class GeometryHelperMethods
    {
        internal static void SendFigure(this GeometryLineString GeometryLineString, GeometryPipeline pipeline)
        {
            Util.CheckArgumentNull(GeometryLineString, "GeometryLineString");
            for (int i = 0; i < GeometryLineString.Points.Count; i++)
            {
                GeometryPoint point = GeometryLineString.Points[i];
                GeometryPosition position = new GeometryPosition(point.X, point.Y, point.Z, point.M);
                if (i == 0)
                {
                    pipeline.BeginFigure(position);
                }
                else
                {
                    pipeline.LineTo(position);
                }
            }
            if (GeometryLineString.Points.Count > 0)
            {
                pipeline.EndFigure();
            }
        }
    }
}

