namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Spatial;

    internal class GeoJsonObjectReader : SpatialReader<IDictionary<string, object>>
    {
        internal GeoJsonObjectReader(SpatialPipeline destination) : base(destination)
        {
        }

        protected override void ReadGeographyImplementation(IDictionary<string, object> input)
        {
            TypeWashedPipeline pipeline = new TypeWashedToGeographyLongLatPipeline(base.Destination);
            new SendToTypeWashedPipeline(pipeline).SendToPipeline(input, true);
        }

        protected override void ReadGeometryImplementation(IDictionary<string, object> input)
        {
            TypeWashedPipeline pipeline = new TypeWashedToGeometryPipeline(base.Destination);
            new SendToTypeWashedPipeline(pipeline).SendToPipeline(input, true);
        }

        private class SendToTypeWashedPipeline
        {
            private TypeWashedPipeline pipeline;

            internal SendToTypeWashedPipeline(TypeWashedPipeline pipeline)
            {
                this.pipeline = pipeline;
            }

            private static bool EnumerableAny(IEnumerable enumerable)
            {
                return enumerable.GetEnumerator().MoveNext();
            }

            private static int GetCoordinateSystemIdFromCrs(IDictionary<string, object> crsJsonObject)
            {
                object obj2;
                object obj3;
                object obj4;
                int num2;
                if (!crsJsonObject.TryGetValue("type", out obj2))
                {
                    throw new FormatException(Strings.GeoJsonReader_MissingRequiredMember("type"));
                }
                string a = ValueAsString("type", obj2);
                if (!string.Equals(a, "name", StringComparison.Ordinal))
                {
                    throw new FormatException(Strings.GeoJsonReader_InvalidCrsType(a));
                }
                if (!crsJsonObject.TryGetValue("properties", out obj3))
                {
                    throw new FormatException(Strings.GeoJsonReader_MissingRequiredMember("properties"));
                }
                if (!ValueAsJsonObject(obj3).TryGetValue("name", out obj4))
                {
                    throw new FormatException(Strings.GeoJsonReader_MissingRequiredMember("name"));
                }
                string str2 = ValueAsString("name", obj4);
                int length = "EPSG".Length;
                if (((str2 == null) || !str2.StartsWith("EPSG", StringComparison.Ordinal)) || (((str2.Length == length) || (str2[length] != ':')) || !int.TryParse(str2.Substring(length + 1), out num2)))
                {
                    throw new FormatException(Strings.GeoJsonReader_InvalidCrsName(str2));
                }
                return num2;
            }

            private static IEnumerable GetMemberValueAsJsonArray(IDictionary<string, object> geoJsonObject, string memberName)
            {
                object obj2;
                if (!geoJsonObject.TryGetValue(memberName, out obj2))
                {
                    throw new FormatException(Strings.GeoJsonReader_MissingRequiredMember(memberName));
                }
                return ValueAsJsonArray(obj2);
            }

            private static SpatialType GetSpatialType(IDictionary<string, object> geoJsonObject)
            {
                object obj2;
                if (!geoJsonObject.TryGetValue("type", out obj2))
                {
                    throw new FormatException(Strings.GeoJsonReader_MissingRequiredMember("type"));
                }
                return ReadTypeName(ValueAsString("type", obj2));
            }

            private static SpatialType ReadTypeName(string typeName)
            {
                switch (typeName)
                {
                    case "Point":
                        return SpatialType.Point;

                    case "LineString":
                        return SpatialType.LineString;

                    case "Polygon":
                        return SpatialType.Polygon;

                    case "MultiPoint":
                        return SpatialType.MultiPoint;

                    case "MultiLineString":
                        return SpatialType.MultiLineString;

                    case "MultiPolygon":
                        return SpatialType.MultiPolygon;

                    case "GeometryCollection":
                        return SpatialType.Collection;
                }
                throw new FormatException(Strings.GeoJsonReader_InvalidTypeName(typeName));
            }

            private static void SendArrayOfArray(IEnumerable array, Action<IEnumerable> send)
            {
                foreach (object obj2 in array)
                {
                    IEnumerable enumerable = ValueAsJsonArray(obj2);
                    send(enumerable);
                }
            }

            private void SendCoordinates(SpatialType spatialType, IEnumerable contentMembers)
            {
                if (EnumerableAny(contentMembers))
                {
                    switch (spatialType)
                    {
                        case SpatialType.Point:
                            this.SendPoint(contentMembers);
                            return;

                        case SpatialType.LineString:
                            this.SendLineString(contentMembers);
                            return;

                        case SpatialType.Polygon:
                            this.SendPolygon(contentMembers);
                            return;

                        case SpatialType.MultiPoint:
                            this.SendMultiShape(SpatialType.Point, contentMembers);
                            return;

                        case SpatialType.MultiLineString:
                            this.SendMultiShape(SpatialType.LineString, contentMembers);
                            return;

                        case SpatialType.MultiPolygon:
                            this.SendMultiShape(SpatialType.Polygon, contentMembers);
                            return;

                        case SpatialType.Collection:
                            foreach (IDictionary<string, object> dictionary in contentMembers)
                            {
                                this.SendToPipeline(dictionary, false);
                            }
                            break;

                        default:
                            return;
                    }
                }
            }

            private void SendLineString(IEnumerable coordinates)
            {
                this.SendPositionArray(coordinates);
            }

            private void SendMultiShape(SpatialType containedSpatialType, IEnumerable coordinates)
            {
                SendArrayOfArray(coordinates, containedShapeCoordinates => this.SendShape(containedSpatialType, containedShapeCoordinates));
            }

            private void SendPoint(IEnumerable coordinates)
            {
                this.SendPosition(coordinates, true);
                this.pipeline.EndFigure();
            }

            private void SendPolygon(IEnumerable coordinates)
            {
                SendArrayOfArray(coordinates, positionArray => this.SendPositionArray(positionArray));
            }

            private void SendPosition(IEnumerable positionElements, bool first)
            {
                int num = 0;
                double num2 = 0.0;
                double num3 = 0.0;
                double? nullable = null;
                double? nullable2 = null;
                foreach (object obj2 in positionElements)
                {
                    num++;
                    switch (num)
                    {
                        case 1:
                            num2 = ValueAsDouble(obj2);
                            break;

                        case 2:
                            num3 = ValueAsDouble(obj2);
                            break;

                        case 3:
                            nullable = ValueAsNullableDouble(obj2);
                            break;

                        case 4:
                            nullable2 = ValueAsNullableDouble(obj2);
                            break;
                    }
                }
                if ((num < 2) || (num > 4))
                {
                    throw new FormatException(Strings.GeoJsonReader_InvalidPosition);
                }
                if (first)
                {
                    this.pipeline.BeginFigure(num2, num3, nullable, nullable2);
                }
                else
                {
                    this.pipeline.LineTo(num2, num3, nullable, nullable2);
                }
            }

            private void SendPositionArray(IEnumerable positionArray)
            {
                bool first = true;
                SendArrayOfArray(positionArray, delegate (IEnumerable array) {
                    this.SendPosition(array, first);
                    if (first)
                    {
                        first = false;
                    }
                });
                this.pipeline.EndFigure();
            }

            private void SendShape(SpatialType spatialType, IEnumerable contentMembers)
            {
                this.pipeline.BeginGeo(spatialType);
                this.SendCoordinates(spatialType, contentMembers);
                this.pipeline.EndGeo();
            }

            internal void SendToPipeline(IDictionary<string, object> members, bool requireSetCoordinates)
            {
                int? nullable;
                string str;
                SpatialType spatialType = GetSpatialType(members);
                if (!TryGetCoordinateSystemId(members, out nullable))
                {
                    nullable = null;
                }
                if (requireSetCoordinates || nullable.HasValue)
                {
                    this.pipeline.SetCoordinateSystem(nullable);
                }
                if (spatialType == SpatialType.Collection)
                {
                    str = "geometries";
                }
                else
                {
                    str = "coordinates";
                }
                IEnumerable memberValueAsJsonArray = GetMemberValueAsJsonArray(members, str);
                this.SendShape(spatialType, memberValueAsJsonArray);
            }

            private static bool TryGetCoordinateSystemId(IDictionary<string, object> geoJsonObject, out int? epsgId)
            {
                object obj2;
                if (!geoJsonObject.TryGetValue("crs", out obj2))
                {
                    epsgId = 0;
                    return false;
                }
                IDictionary<string, object> crsJsonObject = ValueAsJsonObject(obj2);
                epsgId = new int?(GetCoordinateSystemIdFromCrs(crsJsonObject));
                return true;
            }

            private static double ValueAsDouble(object value)
            {
                if (value == null)
                {
                    throw new FormatException(Strings.GeoJsonReader_InvalidNullElement);
                }
                if (((value is string) || (value is IDictionary<string, object>)) || ((value is IEnumerable) || (value is bool)))
                {
                    throw new FormatException(Strings.GeoJsonReader_ExpectedNumeric);
                }
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }

            private static IEnumerable ValueAsJsonArray(object value)
            {
                if (value == null)
                {
                    return null;
                }
                if (value is string)
                {
                    throw new FormatException(Strings.GeoJsonReader_ExpectedArray);
                }
                if ((value is IDictionary) || (value is IDictionary<string, object>))
                {
                    throw new FormatException(Strings.GeoJsonReader_ExpectedArray);
                }
                IEnumerable enumerable = value as IEnumerable;
                if (enumerable == null)
                {
                    throw new FormatException(Strings.GeoJsonReader_ExpectedArray);
                }
                return enumerable;
            }

            private static IDictionary<string, object> ValueAsJsonObject(object value)
            {
                if (value == null)
                {
                    return null;
                }
                IDictionary<string, object> dictionary = value as IDictionary<string, object>;
                if (dictionary == null)
                {
                    throw new FormatException(Strings.JsonReaderExtensions_CannotReadValueAsJsonObject(value));
                }
                return dictionary;
            }

            private static double? ValueAsNullableDouble(object value)
            {
                if (value != null)
                {
                    return new double?(ValueAsDouble(value));
                }
                return null;
            }

            private static string ValueAsString(string propertyName, object value)
            {
                if (value == null)
                {
                    return null;
                }
                string str = value as string;
                if (str == null)
                {
                    throw new FormatException(Strings.JsonReaderExtensions_CannotReadPropertyValueAsString(value, propertyName));
                }
                return str;
            }
        }
    }
}

