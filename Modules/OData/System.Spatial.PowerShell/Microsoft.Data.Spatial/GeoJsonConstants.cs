namespace Microsoft.Data.Spatial
{
    using System;

    internal static class GeoJsonConstants
    {
        internal const string CoordinatesMemberName = "coordinates";
        internal const string CrsMemberName = "crs";
        internal const string CrsNameMemberName = "name";
        internal const string CrsPropertiesMemberName = "properties";
        internal const string CrsTypeMemberValue = "name";
        internal const string CrsValuePrefix = "EPSG";
        internal const string GeometriesMemberName = "geometries";
        internal const string TypeMemberName = "type";
        internal const string TypeMemberValueGeometryCollection = "GeometryCollection";
        internal const string TypeMemberValueLineString = "LineString";
        internal const string TypeMemberValueMultiLineString = "MultiLineString";
        internal const string TypeMemberValueMultiPoint = "MultiPoint";
        internal const string TypeMemberValueMultiPolygon = "MultiPolygon";
        internal const string TypeMemberValuePoint = "Point";
        internal const string TypeMemberValuePolygon = "Polygon";
    }
}

