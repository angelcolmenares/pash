namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Spatial;
    using System.Xml;

    internal class GmlReader : SpatialReader<XmlReader>
    {
        public GmlReader(SpatialPipeline destination) : base(destination)
        {
        }

        protected override void ReadGeographyImplementation(XmlReader input)
        {
            new Parser(input, new TypeWashedToGeographyLatLongPipeline(base.Destination)).Read();
        }

        protected override void ReadGeometryImplementation(XmlReader input)
        {
            new Parser(input, new TypeWashedToGeometryPipeline(base.Destination)).Read();
        }

        private class Parser
        {
            private static readonly char[] coordinateDelimiter = new char[] { ' ', '\t', '\r', '\n' };
            private readonly string fullGlobeNamespace;
            private readonly string gmlNamespace;
            private readonly TypeWashedPipeline pipeline;
            private int points;
            private readonly XmlReader reader;
            private static readonly Dictionary<string, string> skippableElements;

            static Parser()
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
                dictionary.Add("name", "name");
                dictionary.Add("description", "description");
                dictionary.Add("metaDataProperty", "metaDataProperty");
                dictionary.Add("descriptionReference", "descriptionReference");
                dictionary.Add("identifier", "identifier");
                skippableElements = dictionary;
            }

            internal Parser(XmlReader reader, TypeWashedPipeline pipeline)
            {
                this.reader = reader;
                this.pipeline = pipeline;
                XmlNameTable nameTable = this.reader.NameTable;
                this.gmlNamespace = nameTable.Add("http://www.opengis.net/gml");
                this.fullGlobeNamespace = nameTable.Add("http://schemas.microsoft.com/sqlserver/2011/geography");
            }

            private void AddPoint(double x, double y, double? z, double? m)
            {
                if (z.HasValue && double.IsNaN(z.Value))
                {
                    z = null;
                }
                if (m.HasValue && double.IsNaN(m.Value))
                {
                    m = null;
                }
                if (this.points == 0)
                {
                    this.pipeline.BeginFigure(x, y, z, m);
                }
                else
                {
                    this.pipeline.LineTo(x, y, z, m);
                }
                this.points++;
            }

            private void EndFigure()
            {
                if (this.points > 0)
                {
                    this.pipeline.EndFigure();
                }
            }

            private bool IsEndElement(string element)
            {
                this.reader.MoveToContent();
                return ((this.reader.NodeType == XmlNodeType.EndElement) && this.reader.LocalName.Equals(element, StringComparison.Ordinal));
            }

            private bool IsPosListStart()
            {
                if (!this.IsStartElement("pos"))
                {
                    return this.IsStartElement("pointProperty");
                }
                return true;
            }

            private bool IsStartElement(string element)
            {
                return this.reader.IsStartElement(element, this.gmlNamespace);
            }

            private void ParseGmlFullGlobeElement()
            {
                this.pipeline.BeginGeo(SpatialType.FullGlobe);
                if (this.ReadStartOrEmptyElement("FullGlobe") && this.IsEndElement("FullGlobe"))
                {
                    this.ReadEndElement();
                }
                this.pipeline.EndGeo();
            }

            private void ParseGmlGeometry(bool readCoordinateSystem)
            {
                if (!this.reader.IsStartElement())
                {
                    throw new FormatException(Strings.GmlReader_ExpectReaderAtElement);
                }
                if (object.ReferenceEquals(this.reader.NamespaceURI, this.gmlNamespace))
                {
                    this.ReadAttributes(readCoordinateSystem);
                    switch (this.reader.LocalName)
                    {
                        case "Point":
                            this.ParseGmlPointShape();
                            return;

                        case "LineString":
                            this.ParseGmlLineStringShape();
                            return;

                        case "Polygon":
                            this.ParseGmlPolygonShape();
                            return;

                        case "MultiPoint":
                            this.ParseGmlMultiPointShape();
                            return;

                        case "MultiCurve":
                            this.ParseGmlMultiCurveShape();
                            return;

                        case "MultiSurface":
                            this.ParseGmlMultiSurfaceShape();
                            return;

                        case "MultiGeometry":
                            this.ParseGmlMultiGeometryShape();
                            return;
                    }
                    throw new FormatException(Strings.GmlReader_InvalidSpatialType(this.reader.LocalName));
                }
                if (!object.ReferenceEquals(this.reader.NamespaceURI, this.fullGlobeNamespace) || !this.reader.LocalName.Equals("FullGlobe"))
                {
                    throw new FormatException(Strings.GmlReader_ExpectReaderAtElement);
                }
                this.ReadAttributes(readCoordinateSystem);
                this.ParseGmlFullGlobeElement();
            }

            private void ParseGmlLinearRingElement()
            {
                if (this.ReadStartOrEmptyElement("LinearRing"))
                {
                    if (this.IsEndElement("LinearRing"))
                    {
                        throw new FormatException(Strings.GmlReader_EmptyRingsNotAllowed);
                    }
                    if (this.IsPosListStart())
                    {
                        this.ParsePosList(false);
                    }
                    else
                    {
                        this.ParseGmlPosListElement(false);
                    }
                    this.ReadEndElement();
                }
            }

            private void ParseGmlLineString()
            {
                if (this.ReadStartOrEmptyElement("LineString"))
                {
                    this.ReadSkippableElements();
                    if (this.IsPosListStart())
                    {
                        this.ParsePosList(false);
                    }
                    else
                    {
                        this.ParseGmlPosListElement(true);
                    }
                    this.ReadSkippableElements();
                    this.ReadEndElement();
                }
            }

            private void ParseGmlLineStringShape()
            {
                this.pipeline.BeginGeo(SpatialType.LineString);
                this.PrepareFigure();
                this.ParseGmlLineString();
                this.EndFigure();
                this.pipeline.EndGeo();
            }

            private void ParseGmlMultiCurveShape()
            {
                this.pipeline.BeginGeo(SpatialType.MultiLineString);
                this.ParseMultiItemElement("MultiCurve", "curveMember", "curveMembers", new Action(this.ParseGmlLineStringShape));
                this.pipeline.EndGeo();
            }

            private void ParseGmlMultiGeometryShape()
            {
                this.pipeline.BeginGeo(SpatialType.Collection);
                this.ParseMultiItemElement("MultiGeometry", "geometryMember", "geometryMembers", () => this.ParseGmlGeometry(false));
                this.pipeline.EndGeo();
            }

            private void ParseGmlMultiPointShape()
            {
                this.pipeline.BeginGeo(SpatialType.MultiPoint);
                this.ParseMultiItemElement("MultiPoint", "pointMember", "pointMembers", new Action(this.ParseGmlPointShape));
                this.pipeline.EndGeo();
            }

            private void ParseGmlMultiSurfaceShape()
            {
                this.pipeline.BeginGeo(SpatialType.MultiPolygon);
                this.ParseMultiItemElement("MultiSurface", "surfaceMember", "surfaceMembers", new Action(this.ParseGmlPolygonShape));
                this.pipeline.EndGeo();
            }

            private void ParseGmlPointElement(bool allowEmpty)
            {
                if (this.ReadStartOrEmptyElement("Point"))
                {
                    this.ReadSkippableElements();
                    this.ParseGmlPosElement(allowEmpty);
                    this.ReadSkippableElements();
                    this.ReadEndElement();
                }
            }

            private void ParseGmlPointPropertyElement(bool allowEmpty)
            {
                if (this.ReadStartOrEmptyElement("pointProperty"))
                {
                    this.ParseGmlPointElement(allowEmpty);
                    this.ReadEndElement();
                }
            }

            private void ParseGmlPointShape()
            {
                this.pipeline.BeginGeo(SpatialType.Point);
                this.PrepareFigure();
                this.ParseGmlPointElement(true);
                this.EndFigure();
                this.pipeline.EndGeo();
            }

            private void ParseGmlPolygonShape()
            {
                this.pipeline.BeginGeo(SpatialType.Polygon);
                if (this.ReadStartOrEmptyElement("Polygon"))
                {
                    this.ReadSkippableElements();
                    if (!this.IsEndElement("Polygon"))
                    {
                        this.PrepareFigure();
                        this.ParseGmlRingElement("exterior");
                        this.EndFigure();
                        this.ReadSkippableElements();
                        while (this.IsStartElement("interior"))
                        {
                            this.PrepareFigure();
                            this.ParseGmlRingElement("interior");
                            this.EndFigure();
                            this.ReadSkippableElements();
                        }
                    }
                    this.ReadSkippableElements();
                    this.ReadEndElement();
                }
                this.pipeline.EndGeo();
            }

            private void ParseGmlPosElement(bool allowEmpty)
            {
                this.ReadAttributes(false);
                if (this.ReadStartOrEmptyElement("pos"))
                {
                    double[] numArray = this.ReadContentAsDoubleArray();
                    if (numArray.Length != 0)
                    {
                        if (numArray.Length < 2)
                        {
                            throw new FormatException(Strings.GmlReader_PosNeedTwoNumbers);
                        }
                        this.AddPoint(numArray[0], numArray[1], (numArray.Length > 2) ? new double?(numArray[2]) : null, (numArray.Length > 3) ? new double?(numArray[3]) : null);
                    }
                    else if (!allowEmpty)
                    {
                        throw new FormatException(Strings.GmlReader_PosNeedTwoNumbers);
                    }
                    this.ReadEndElement();
                }
                else if (!allowEmpty)
                {
                    throw new FormatException(Strings.GmlReader_PosNeedTwoNumbers);
                }
            }

            private void ParseGmlPosListElement(bool allowEmpty)
            {
                if (this.ReadStartOrEmptyElement("posList"))
                {
                    if (!this.IsEndElement("posList"))
                    {
                        double[] numArray = this.ReadContentAsDoubleArray();
                        if (numArray.Length == 0)
                        {
                            throw new FormatException(Strings.GmlReader_PosListNeedsEvenCount);
                        }
                        if ((numArray.Length % 2) != 0)
                        {
                            throw new FormatException(Strings.GmlReader_PosListNeedsEvenCount);
                        }
                        for (int i = 0; i < numArray.Length; i += 2)
                        {
                            double? z = null;
                            double? m = null;
                            this.AddPoint(numArray[i], numArray[i + 1], z, m);
                        }
                    }
                    else if (!allowEmpty)
                    {
                        throw new FormatException(Strings.GmlReader_PosListNeedsEvenCount);
                    }
                    this.ReadEndElement();
                }
                else if (!allowEmpty)
                {
                    throw new FormatException(Strings.GmlReader_PosListNeedsEvenCount);
                }
            }

            private void ParseGmlRingElement(string ringTag)
            {
                if (this.ReadStartOrEmptyElement(ringTag))
                {
                    if (!this.IsEndElement(ringTag))
                    {
                        this.ParseGmlLinearRingElement();
                    }
                    this.ReadEndElement();
                }
            }

            private void ParseMultiItemElement(string header, string member, string members, Action parseItem)
            {
                if (this.ReadStartOrEmptyElement(header))
                {
                    this.ReadSkippableElements();
                    if (!this.IsEndElement(header))
                    {
                        while (this.IsStartElement(member))
                        {
                            if (this.ReadStartOrEmptyElement(member) && !this.IsEndElement(member))
                            {
                                parseItem();
                                this.ReadEndElement();
                                this.ReadSkippableElements();
                            }
                        }
                        if (this.IsStartElement(members) && this.ReadStartOrEmptyElement(members))
                        {
                            while (this.reader.IsStartElement())
                            {
                                parseItem();
                            }
                            this.ReadEndElement();
                        }
                    }
                    this.ReadSkippableElements();
                    this.ReadEndElement();
                }
            }

            private void ParsePosList(bool allowEmpty)
            {
                do
                {
                    if (this.IsStartElement("pos"))
                    {
                        this.ParseGmlPosElement(allowEmpty);
                    }
                    else
                    {
                        this.ParseGmlPointPropertyElement(allowEmpty);
                    }
                }
                while (this.IsPosListStart());
            }

            private void PrepareFigure()
            {
                this.points = 0;
            }

            public void Read()
            {
                this.ParseGmlGeometry(true);
            }

            private void ReadAttributes(bool expectSrsName)
            {
                bool flag = false;
                this.reader.MoveToContent();
                if (!this.reader.MoveToFirstAttribute())
                {
                    goto Label_015A;
                }
            Label_001E:
                if (!this.reader.NamespaceURI.Equals("http://www.w3.org/2000/xmlns/", StringComparison.Ordinal))
                {
                    string localName = this.reader.LocalName;
                    switch (localName)
                    {
                        case "axisLabels":
                        case "uomLabels":
                        case "count":
                        case "id":
                            goto Label_013E;

                        case "srsName":
                        {
                            if (!expectSrsName)
                            {
                                this.reader.MoveToElement();
                                throw new FormatException(Strings.GmlReader_InvalidAttribute(localName, this.reader.Name));
                            }
                            string str2 = this.reader.Value;
                            if (!str2.StartsWith("http://www.opengis.net/def/crs/EPSG/0/", StringComparison.Ordinal))
                            {
                                throw new FormatException(Strings.GmlReader_InvalidSrsName("http://www.opengis.net/def/crs/EPSG/0/"));
                            }
                            int num = XmlConvert.ToInt32(str2.Substring("http://www.opengis.net/def/crs/EPSG/0/".Length));
                            this.pipeline.SetCoordinateSystem(new int?(num));
                            flag = true;
                            goto Label_013E;
                        }
                    }
                    this.reader.MoveToElement();
                    throw new FormatException(Strings.GmlReader_InvalidAttribute(localName, this.reader.Name));
                }
            Label_013E:
                if (this.reader.MoveToNextAttribute())
                {
                    goto Label_001E;
                }
                this.reader.MoveToElement();
            Label_015A:
                if (expectSrsName && !flag)
                {
                    this.pipeline.SetCoordinateSystem(null);
                }
            }

            private double[] ReadContentAsDoubleArray()
            {
                string[] strArray = this.reader.ReadContentAsString().Split(coordinateDelimiter, StringSplitOptions.RemoveEmptyEntries);
                double[] numArray = new double[strArray.Length];
                for (int i = 0; i < strArray.Length; i++)
                {
                    numArray[i] = XmlConvert.ToDouble(strArray[i]);
                }
                return numArray;
            }

            private void ReadEndElement()
            {
                this.reader.MoveToContent();
                if (this.reader.NodeType != XmlNodeType.EndElement)
                {
                    throw new FormatException(Strings.GmlReader_UnexpectedElement(this.reader.Name));
                }
                this.reader.ReadEndElement();
            }

            private void ReadSkippableElements()
            {
                bool flag = true;
                while (flag)
                {
                    this.reader.MoveToContent();
                    if ((this.reader.NodeType == XmlNodeType.Element) && object.ReferenceEquals(this.reader.NamespaceURI, this.gmlNamespace))
                    {
                        string localName = this.reader.LocalName;
                        flag = skippableElements.ContainsKey(localName);
                    }
                    else
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        this.reader.Skip();
                    }
                }
            }

            private bool ReadStartOrEmptyElement(string element)
            {
                bool isEmptyElement = this.reader.IsEmptyElement;
                if (element != "FullGlobe")
                {
                    this.reader.ReadStartElement(element, this.gmlNamespace);
                }
                else
                {
                    this.reader.ReadStartElement(element, "http://schemas.microsoft.com/sqlserver/2011/geography");
                }
                return !isEmptyElement;
            }
        }
    }
}

