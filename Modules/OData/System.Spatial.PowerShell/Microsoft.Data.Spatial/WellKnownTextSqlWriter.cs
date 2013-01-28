namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Spatial;

    internal sealed class WellKnownTextSqlWriter : DrawBoth
    {
        private bool allowOnlyTwoDimensions;
        private bool coordinateSystemWritten;
        private bool figureWritten;
        private Stack<SpatialType> parentStack;
        private bool shapeWritten;
        private TextWriter writer;

        public WellKnownTextSqlWriter(TextWriter writer) : this(writer, false)
        {
        }

        public WellKnownTextSqlWriter(TextWriter writer, bool allowOnlyTwoDimensions)
        {
            this.allowOnlyTwoDimensions = allowOnlyTwoDimensions;
            this.writer = writer;
            this.parentStack = new Stack<SpatialType>();
            this.Reset();
        }

        private void AddLineTo(double x, double y, double? z, double? m)
        {
            this.writer.Write(", ");
            this.WritePoint(x, y, z, m);
        }

        private void BeginGeo(SpatialType type)
        {
            SpatialType type2 = (this.parentStack.Count == 0) ? SpatialType.Unknown : this.parentStack.Peek();
            switch (type2)
            {
                case SpatialType.MultiPoint:
                case SpatialType.MultiLineString:
                case SpatialType.MultiPolygon:
                case SpatialType.Collection:
                    this.writer.Write(this.shapeWritten ? ", " : "(");
                    break;
            }
            if ((type2 == SpatialType.Unknown) || (type2 == SpatialType.Collection))
            {
                this.WriteTaggedText(type);
            }
            this.figureWritten = false;
            this.parentStack.Push(type);
        }

        private void EndFigure()
        {
            this.writer.Write(")");
        }

        private void EndGeo()
        {
            switch (this.parentStack.Pop())
            {
                case SpatialType.Point:
                case SpatialType.LineString:
                    if (!this.figureWritten)
                    {
                        this.writer.Write("EMPTY");
                    }
                    break;

                case SpatialType.Polygon:
                    this.writer.Write(this.figureWritten ? ")" : "EMPTY");
                    break;

                case SpatialType.MultiPoint:
                case SpatialType.MultiLineString:
                case SpatialType.MultiPolygon:
                case SpatialType.Collection:
                    this.writer.Write(this.shapeWritten ? ")" : "EMPTY");
                    break;

                case SpatialType.FullGlobe:
                    this.writer.Write(")");
                    break;
            }
            this.shapeWritten = true;
            this.writer.Flush();
        }

        protected override GeographyPosition OnBeginFigure(GeographyPosition position)
        {
            this.WriteFigureScope(position.Longitude, position.Latitude, position.Z, position.M);
            return position;
        }

        protected override GeometryPosition OnBeginFigure(GeometryPosition position)
        {
            this.WriteFigureScope(position.X, position.Y, position.Z, position.M);
            return position;
        }

        protected override SpatialType OnBeginGeography(SpatialType type)
        {
            this.BeginGeo(type);
            return type;
        }

        protected override SpatialType OnBeginGeometry(SpatialType type)
        {
            this.BeginGeo(type);
            return type;
        }

        protected override void OnEndFigure()
        {
            this.EndFigure();
        }

        protected override void OnEndGeography()
        {
            this.EndGeo();
        }

        protected override void OnEndGeometry()
        {
            this.EndGeo();
        }

        protected override GeographyPosition OnLineTo(GeographyPosition position)
        {
            this.AddLineTo(position.Longitude, position.Latitude, position.Z, position.M);
            return position;
        }

        protected override GeometryPosition OnLineTo(GeometryPosition position)
        {
            this.AddLineTo(position.X, position.Y, position.Z, position.M);
            return position;
        }

        protected override void OnReset()
        {
            this.Reset();
        }

        protected override CoordinateSystem OnSetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            this.WriteCoordinateSystem(coordinateSystem);
            return coordinateSystem;
        }

        private void Reset()
        {
            this.figureWritten = false;
            this.parentStack.Clear();
            this.shapeWritten = false;
            this.coordinateSystemWritten = false;
        }

        private void WriteCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            if (!this.coordinateSystemWritten)
            {
                this.writer.Write("SRID");
                this.writer.Write("=");
                this.writer.Write(coordinateSystem.Id);
                this.writer.Write(";");
                this.coordinateSystemWritten = true;
            }
        }

        private void WriteFigureScope(double coordinate1, double coordinate2, double? coordinate3, double? coordinate4)
        {
            if (this.figureWritten)
            {
                this.writer.Write(", ");
            }
            else if (((SpatialType) this.parentStack.Peek()) == SpatialType.Polygon)
            {
                this.writer.Write("(");
            }
            this.writer.Write("(");
            this.figureWritten = true;
            this.WritePoint(coordinate1, coordinate2, coordinate3, coordinate4);
        }

        private void WritePoint(double x, double y, double? z, double? m)
        {
            this.writer.WriteRoundtrippable(x);
            this.writer.Write(" ");
            this.writer.WriteRoundtrippable(y);
            if (!this.allowOnlyTwoDimensions && z.HasValue)
            {
                this.writer.Write(" ");
                this.writer.WriteRoundtrippable(z.Value);
                if (!this.allowOnlyTwoDimensions && m.HasValue)
                {
                    this.writer.Write(" ");
                    this.writer.WriteRoundtrippable(m.Value);
                }
            }
            else if (!this.allowOnlyTwoDimensions && m.HasValue)
            {
                this.writer.Write(" ");
                this.writer.Write("NULL");
                this.writer.Write(" ");
                this.writer.Write(m.Value);
            }
        }

        private void WriteTaggedText(SpatialType type)
        {
            switch (type)
            {
                case SpatialType.Point:
                    this.writer.Write("POINT");
                    break;

                case SpatialType.LineString:
                    this.writer.Write("LINESTRING");
                    break;

                case SpatialType.Polygon:
                    this.writer.Write("POLYGON");
                    break;

                case SpatialType.MultiPoint:
                    this.shapeWritten = false;
                    this.writer.Write("MULTIPOINT");
                    break;

                case SpatialType.MultiLineString:
                    this.shapeWritten = false;
                    this.writer.Write("MULTILINESTRING");
                    break;

                case SpatialType.MultiPolygon:
                    this.shapeWritten = false;
                    this.writer.Write("MULTIPOLYGON");
                    break;

                case SpatialType.Collection:
                    this.shapeWritten = false;
                    this.writer.Write("GEOMETRYCOLLECTION");
                    break;

                case SpatialType.FullGlobe:
                    this.writer.Write("FULLGLOBE");
                    break;
            }
            if (type != SpatialType.FullGlobe)
            {
                this.writer.Write(" ");
            }
        }
    }
}

