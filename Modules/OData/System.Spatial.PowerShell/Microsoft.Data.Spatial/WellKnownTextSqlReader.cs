namespace Microsoft.Data.Spatial
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Spatial;
    using System.Text;
    using System.Xml;

    internal class WellKnownTextSqlReader : SpatialReader<TextReader>
    {
        private bool allowOnlyTwoDimensions;

        public WellKnownTextSqlReader(SpatialPipeline destination) : this(destination, false)
        {
        }

        public WellKnownTextSqlReader(SpatialPipeline destination, bool allowOnlyTwoDimensions) : base(destination)
        {
            this.allowOnlyTwoDimensions = allowOnlyTwoDimensions;
        }

        protected override void ReadGeographyImplementation(TextReader input)
        {
            new Parser(input, new TypeWashedToGeographyLongLatPipeline(base.Destination), this.allowOnlyTwoDimensions).Read();
        }

        protected override void ReadGeometryImplementation(TextReader input)
        {
            new Parser(input, new TypeWashedToGeometryPipeline(base.Destination), this.allowOnlyTwoDimensions).Read();
        }

        private class Parser
        {
            private readonly bool allowOnlyTwoDimensions;
            private readonly TextLexerBase lexer;
            private readonly TypeWashedPipeline pipeline;

            public Parser(TextReader reader, TypeWashedPipeline pipeline, bool allowOnlyTwoDimensions)
            {
                this.lexer = new WellKnownTextLexer(reader);
                this.pipeline = pipeline;
                this.allowOnlyTwoDimensions = allowOnlyTwoDimensions;
            }

            private bool IsTokenMatch(WellKnownTextTokenType type, string text)
            {
                return this.lexer.CurrentToken.MatchToken((int) type, text, StringComparison.OrdinalIgnoreCase);
            }

            private bool NextToken()
            {
                while (this.lexer.Next())
                {
                    if (!this.lexer.CurrentToken.MatchToken(8, string.Empty, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
                return false;
            }

            private void ParseCollectionText()
            {
                if (!this.ReadEmptySet())
                {
                    this.ReadToken(WellKnownTextTokenType.LeftParen, null);
                    this.ParseTaggedText();
                    while (this.ReadOptionalToken(WellKnownTextTokenType.Comma, null))
                    {
                        this.ParseTaggedText();
                    }
                    this.ReadToken(WellKnownTextTokenType.RightParen, null);
                }
            }

            private void ParseLineStringText()
            {
                if (!this.ReadEmptySet())
                {
                    this.ReadToken(WellKnownTextTokenType.LeftParen, null);
                    this.ParsePoint(true);
                    while (this.ReadOptionalToken(WellKnownTextTokenType.Comma, null))
                    {
                        this.ParsePoint(false);
                    }
                    this.ReadToken(WellKnownTextTokenType.RightParen, null);
                    this.pipeline.EndFigure();
                }
            }

            private void ParseMultiGeoText(SpatialType innerType, Action innerReader)
            {
                if (!this.ReadEmptySet())
                {
                    this.ReadToken(WellKnownTextTokenType.LeftParen, null);
                    this.pipeline.BeginGeo(innerType);
                    innerReader();
                    this.pipeline.EndGeo();
                    while (this.ReadOptionalToken(WellKnownTextTokenType.Comma, null))
                    {
                        this.pipeline.BeginGeo(innerType);
                        innerReader();
                        this.pipeline.EndGeo();
                    }
                    this.ReadToken(WellKnownTextTokenType.RightParen, null);
                }
            }

            private void ParsePoint(bool firstFigure)
            {
                double? nullable;
                double? nullable2;
                double num = this.ReadDouble();
                double num2 = this.ReadDouble();
                if (this.TryReadOptionalNullableDouble(out nullable) && this.allowOnlyTwoDimensions)
                {
                    throw new FormatException(Strings.WellKnownText_TooManyDimensions);
                }
                if (this.TryReadOptionalNullableDouble(out nullable2) && this.allowOnlyTwoDimensions)
                {
                    throw new FormatException(Strings.WellKnownText_TooManyDimensions);
                }
                if (firstFigure)
                {
                    this.pipeline.BeginFigure(num, num2, nullable, nullable2);
                }
                else
                {
                    this.pipeline.LineTo(num, num2, nullable, nullable2);
                }
            }

            private void ParsePointText()
            {
                if (!this.ReadEmptySet())
                {
                    this.ReadToken(WellKnownTextTokenType.LeftParen, null);
                    this.ParsePoint(true);
                    this.ReadToken(WellKnownTextTokenType.RightParen, null);
                    this.pipeline.EndFigure();
                }
            }

            private void ParsePolygonText()
            {
                if (!this.ReadEmptySet())
                {
                    this.ReadToken(WellKnownTextTokenType.LeftParen, null);
                    this.ParseLineStringText();
                    while (this.ReadOptionalToken(WellKnownTextTokenType.Comma, null))
                    {
                        this.ParseLineStringText();
                    }
                    this.ReadToken(WellKnownTextTokenType.RightParen, null);
                }
            }

            private void ParseSRID()
            {
                if (this.ReadOptionalToken(WellKnownTextTokenType.Text, "SRID"))
                {
                    this.ReadToken(WellKnownTextTokenType.Equals, null);
                    this.pipeline.SetCoordinateSystem(new int?(this.ReadInteger()));
                    this.ReadToken(WellKnownTextTokenType.Semicolon, null);
                }
                else
                {
                    this.pipeline.SetCoordinateSystem(null);
                }
            }

            private void ParseTaggedText()
            {
                if (!this.NextToken())
                {
                    throw new FormatException(Strings.WellKnownText_UnknownTaggedText(string.Empty));
                }
                switch (this.lexer.CurrentToken.Text.ToUpperInvariant())
                {
                    case "POINT":
                        this.pipeline.BeginGeo(SpatialType.Point);
                        this.ParsePointText();
                        this.pipeline.EndGeo();
                        return;

                    case "LINESTRING":
                        this.pipeline.BeginGeo(SpatialType.LineString);
                        this.ParseLineStringText();
                        this.pipeline.EndGeo();
                        return;

                    case "POLYGON":
                        this.pipeline.BeginGeo(SpatialType.Polygon);
                        this.ParsePolygonText();
                        this.pipeline.EndGeo();
                        return;

                    case "MULTIPOINT":
                        this.pipeline.BeginGeo(SpatialType.MultiPoint);
                        this.ParseMultiGeoText(SpatialType.Point, new Action(this.ParsePointText));
                        this.pipeline.EndGeo();
                        return;

                    case "MULTILINESTRING":
                        this.pipeline.BeginGeo(SpatialType.MultiLineString);
                        this.ParseMultiGeoText(SpatialType.LineString, new Action(this.ParseLineStringText));
                        this.pipeline.EndGeo();
                        return;

                    case "MULTIPOLYGON":
                        this.pipeline.BeginGeo(SpatialType.MultiPolygon);
                        this.ParseMultiGeoText(SpatialType.Polygon, new Action(this.ParsePolygonText));
                        this.pipeline.EndGeo();
                        return;

                    case "GEOMETRYCOLLECTION":
                        this.pipeline.BeginGeo(SpatialType.Collection);
                        this.ParseCollectionText();
                        this.pipeline.EndGeo();
                        return;

                    case "FULLGLOBE":
                        this.pipeline.BeginGeo(SpatialType.FullGlobe);
                        this.pipeline.EndGeo();
                        return;
                }
                throw new FormatException(Strings.WellKnownText_UnknownTaggedText(this.lexer.CurrentToken.Text));
            }

            public void Read()
            {
                this.ParseSRID();
                this.ParseTaggedText();
            }

            private double ReadDouble()
            {
                StringBuilder builder = new StringBuilder();
                this.ReadToken(WellKnownTextTokenType.Number, null);
                builder.Append(this.lexer.CurrentToken.Text);
                if (this.ReadOptionalToken(WellKnownTextTokenType.Period, null))
                {
                    builder.Append(".");
                    this.ReadToken(WellKnownTextTokenType.Number, null);
                    builder.Append(this.lexer.CurrentToken.Text);
                }
                return double.Parse(builder.ToString(), CultureInfo.InvariantCulture);
            }

            private bool ReadEmptySet()
            {
                return this.ReadOptionalToken(WellKnownTextTokenType.Text, "EMPTY");
            }

            private int ReadInteger()
            {
                this.ReadToken(WellKnownTextTokenType.Number, null);
                return XmlConvert.ToInt32(this.lexer.CurrentToken.Text);
            }

            private bool ReadOptionalToken(WellKnownTextTokenType expectedTokenType, string expectedTokenText)
            {
                LexerToken token;
                while (this.lexer.Peek(out token))
                {
                    if (token.MatchToken(8, null, StringComparison.OrdinalIgnoreCase))
                    {
                        this.lexer.Next();
                    }
                    else
                    {
                        if (token.MatchToken((int) expectedTokenType, expectedTokenText, StringComparison.OrdinalIgnoreCase))
                        {
                            this.lexer.Next();
                            return true;
                        }
                        return false;
                    }
                }
                return false;
            }

            private void ReadToken(WellKnownTextTokenType type, string text)
            {
                if (!this.NextToken() || !this.IsTokenMatch(type, text))
                {
                    throw new FormatException(Strings.WellKnownText_UnexpectedToken(type, text, this.lexer.CurrentToken));
                }
            }

            private bool TryReadOptionalNullableDouble(out double? value)
            {
                StringBuilder builder = new StringBuilder();
                if (this.ReadOptionalToken(WellKnownTextTokenType.Number, null))
                {
                    builder.Append(this.lexer.CurrentToken.Text);
                    if (this.ReadOptionalToken(WellKnownTextTokenType.Period, null))
                    {
                        builder.Append(".");
                        this.ReadToken(WellKnownTextTokenType.Number, null);
                        builder.Append(this.lexer.CurrentToken.Text);
                    }
                    value = new double?(double.Parse(builder.ToString(), CultureInfo.InvariantCulture));
                    return true;
                }
                value = 0;
                return this.ReadOptionalToken(WellKnownTextTokenType.Text, "NULL");
            }
        }
    }
}

