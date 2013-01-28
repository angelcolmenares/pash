namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Spatial;
    using System.Xml;

    internal sealed class GmlWriter : DrawBoth
    {
        private bool coordinateSystemWritten;
        private CoordinateSystem currentCoordinateSystem;
        private bool figureWritten;
        private Stack<SpatialType> parentStack;
        private bool shouldWriteContainerWrapper;
        private XmlWriter writer;

        public GmlWriter(XmlWriter writer)
        {
            this.writer = writer;
            this.OnReset();
        }

        private void BeginFigure(double x, double y, double? z, double? m)
        {
            if (((SpatialType) this.parentStack.Peek()) == SpatialType.Polygon)
            {
                this.WriteStartElement(this.figureWritten ? "interior" : "exterior");
                this.WriteStartElement("LinearRing");
            }
            this.figureWritten = true;
            this.WritePoint(x, y, z, m);
        }

        private void BeginGeo(SpatialType type)
        {
            if (this.shouldWriteContainerWrapper)
            {
                switch (this.parentStack.Peek())
                {
                    case SpatialType.MultiPoint:
                        this.WriteStartElement("pointMembers");
                        break;

                    case SpatialType.MultiLineString:
                        this.WriteStartElement("curveMembers");
                        break;

                    case SpatialType.MultiPolygon:
                        this.WriteStartElement("surfaceMembers");
                        break;

                    case SpatialType.Collection:
                        this.WriteStartElement("geometryMembers");
                        break;
                }
                this.shouldWriteContainerWrapper = false;
            }
            this.figureWritten = false;
            this.parentStack.Push(type);
            switch (type)
            {
                case SpatialType.Point:
                    this.WriteStartElement("Point");
                    break;

                case SpatialType.LineString:
                    this.WriteStartElement("LineString");
                    break;

                case SpatialType.Polygon:
                    this.WriteStartElement("Polygon");
                    break;

                case SpatialType.MultiPoint:
                    this.shouldWriteContainerWrapper = true;
                    this.WriteStartElement("MultiPoint");
                    break;

                case SpatialType.MultiLineString:
                    this.shouldWriteContainerWrapper = true;
                    this.WriteStartElement("MultiCurve");
                    break;

                case SpatialType.MultiPolygon:
                    this.shouldWriteContainerWrapper = true;
                    this.WriteStartElement("MultiSurface");
                    break;

                case SpatialType.Collection:
                    this.shouldWriteContainerWrapper = true;
                    this.WriteStartElement("MultiGeometry");
                    break;

                case SpatialType.FullGlobe:
                    this.writer.WriteStartElement("FullGlobe", "http://schemas.microsoft.com/sqlserver/2011/geography");
                    break;

                default:
                    throw new NotSupportedException("Unknown type " + type);
            }
            this.WriteCoordinateSystem();
        }

        private void EndGeo()
        {
            switch (this.parentStack.Pop())
            {
                case SpatialType.Point:
                    if (!this.figureWritten)
                    {
                        this.WriteStartElement("pos");
                        this.writer.WriteEndElement();
                    }
                    this.writer.WriteEndElement();
                    return;

                case SpatialType.LineString:
                    if (!this.figureWritten)
                    {
                        this.WriteStartElement("posList");
                        this.writer.WriteEndElement();
                    }
                    this.writer.WriteEndElement();
                    return;

                case SpatialType.Polygon:
                case SpatialType.FullGlobe:
                    this.writer.WriteEndElement();
                    break;

                case SpatialType.MultiPoint:
                case SpatialType.MultiLineString:
                case SpatialType.MultiPolygon:
                case SpatialType.Collection:
                    if (!this.shouldWriteContainerWrapper)
                    {
                        this.writer.WriteEndElement();
                    }
                    this.writer.WriteEndElement();
                    this.shouldWriteContainerWrapper = false;
                    return;

                case ((SpatialType) 8):
                case ((SpatialType) 9):
                case ((SpatialType) 10):
                    break;

                default:
                    return;
            }
        }

        protected override GeographyPosition OnBeginFigure(GeographyPosition position)
        {
            this.BeginFigure(position.Latitude, position.Longitude, position.Z, position.M);
            return position;
        }

        protected override GeometryPosition OnBeginFigure(GeometryPosition position)
        {
            this.BeginFigure(position.X, position.Y, position.Z, position.M);
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
            if (((SpatialType) this.parentStack.Peek()) == SpatialType.Polygon)
            {
                this.writer.WriteEndElement();
                this.writer.WriteEndElement();
            }
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
            this.WritePoint(position.Latitude, position.Longitude, position.Z, position.M);
            return position;
        }

        protected override GeometryPosition OnLineTo(GeometryPosition position)
        {
            this.WritePoint(position.X, position.Y, position.Z, position.M);
            return position;
        }

        protected override void OnReset()
        {
            this.parentStack = new Stack<SpatialType>();
            this.coordinateSystemWritten = false;
            this.currentCoordinateSystem = null;
            this.figureWritten = false;
            this.shouldWriteContainerWrapper = false;
        }

        protected override CoordinateSystem OnSetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            this.currentCoordinateSystem = coordinateSystem;
            return coordinateSystem;
        }

        private void WriteCoordinateSystem()
        {
            if (!this.coordinateSystemWritten && (this.currentCoordinateSystem != null))
            {
                this.coordinateSystemWritten = true;
                string str = "http://www.opengis.net/def/crs/EPSG/0/" + this.currentCoordinateSystem.Id;
                this.writer.WriteAttributeString("gml", "srsName", "http://www.opengis.net/gml", str);
            }
        }

        private void WritePoint(double x, double y, double? z, double? m)
        {
            this.WriteStartElement("pos");
            this.writer.WriteValue(x);
            this.writer.WriteValue(" ");
            this.writer.WriteValue(y);
            if (z.HasValue)
            {
                this.writer.WriteValue(" ");
                this.writer.WriteValue(z.Value);
                if (m.HasValue)
                {
                    this.writer.WriteValue(" ");
                    this.writer.WriteValue(m.Value);
                }
            }
            else if (m.HasValue)
            {
                this.writer.WriteValue(" ");
                this.writer.WriteValue(double.NaN);
                this.writer.WriteValue(" ");
                this.writer.WriteValue(m.Value);
            }
            this.writer.WriteEndElement();
        }

        private void WriteStartElement(string elementName)
        {
            this.writer.WriteStartElement("gml", elementName, "http://www.opengis.net/gml");
        }
    }
}

