namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Spatial;

    internal abstract class GeoJsonWriterBase : DrawBoth
    {
        private CoordinateSystem currentCoordinateSystem;
        private bool figureDrawn;
        private readonly Stack<SpatialType> stack = new Stack<SpatialType>();

        protected abstract void AddPropertyName(string name);
        protected abstract void AddValue(double value);
        protected abstract void AddValue(string value);
        private void BeginFigure()
        {
            if (this.FigureHasArrayScope)
            {
                this.StartArrayScope();
            }
            this.figureDrawn = true;
        }

        private void BeginShape(SpatialType type, CoordinateSystem defaultCoordinateSystem)
        {
            if (this.currentCoordinateSystem == null)
            {
                this.currentCoordinateSystem = defaultCoordinateSystem;
            }
            if (this.ShapeHasObjectScope)
            {
                this.WriteShapeHeader(type);
            }
            if (TypeHasArrayScope(type))
            {
                this.StartArrayScope();
            }
            this.stack.Push(type);
            this.figureDrawn = false;
        }

        protected abstract void EndArrayScope();
        private void EndFigure()
        {
            if (this.FigureHasArrayScope)
            {
                this.EndArrayScope();
            }
        }

        protected abstract void EndObjectScope();
        private void EndShape()
        {
            if (TypeHasArrayScope(this.stack.Pop()))
            {
                this.EndArrayScope();
            }
            else if (!this.figureDrawn)
            {
                this.StartArrayScope();
                this.EndArrayScope();
            }
            if (this.IsTopLevel)
            {
                this.WriteCrs();
            }
            if (this.ShapeHasObjectScope)
            {
                this.EndObjectScope();
            }
        }

        private static string GetDataName(SpatialType type)
        {
            switch (type)
            {
                case SpatialType.Point:
                case SpatialType.LineString:
                case SpatialType.Polygon:
                case SpatialType.MultiPoint:
                case SpatialType.MultiLineString:
                case SpatialType.MultiPolygon:
                    return "coordinates";

                case SpatialType.Collection:
                    return "geometries";
            }
            throw new NotImplementedException();
        }

        private static string GetSpatialTypeName(SpatialType type)
        {
            switch (type)
            {
                case SpatialType.Point:
                    return "Point";

                case SpatialType.LineString:
                    return "LineString";

                case SpatialType.Polygon:
                    return "Polygon";

                case SpatialType.MultiPoint:
                    return "MultiPoint";

                case SpatialType.MultiLineString:
                    return "MultiLineString";

                case SpatialType.MultiPolygon:
                    return "MultiPolygon";

                case SpatialType.Collection:
                    return "GeometryCollection";
            }
            throw new NotImplementedException();
        }

        protected override GeographyPosition OnBeginFigure(GeographyPosition position)
        {
            this.BeginFigure();
            this.WriteControlPoint(position.Longitude, position.Latitude, position.Z, position.M);
            return position;
        }

        protected override GeometryPosition OnBeginFigure(GeometryPosition position)
        {
            this.BeginFigure();
            this.WriteControlPoint(position.X, position.Y, position.Z, position.M);
            return position;
        }

        protected override SpatialType OnBeginGeography(SpatialType type)
        {
            this.BeginShape(type, CoordinateSystem.DefaultGeography);
            return type;
        }

        protected override SpatialType OnBeginGeometry(SpatialType type)
        {
            this.BeginShape(type, CoordinateSystem.DefaultGeometry);
            return type;
        }

        protected override void OnEndFigure()
        {
            this.EndFigure();
        }

        protected override void OnEndGeography()
        {
            this.EndShape();
        }

        protected override void OnEndGeometry()
        {
            this.EndShape();
        }

        protected override GeographyPosition OnLineTo(GeographyPosition position)
        {
            this.WriteControlPoint(position.Longitude, position.Latitude, position.Z, position.M);
            return position;
        }

        protected override GeometryPosition OnLineTo(GeometryPosition position)
        {
            this.WriteControlPoint(position.X, position.Y, position.Z, position.M);
            return position;
        }

        protected override void OnReset()
        {
            this.Reset();
        }

        protected override CoordinateSystem OnSetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            this.SetCoordinateSystem(coordinateSystem);
            return coordinateSystem;
        }

        protected virtual void Reset()
        {
            this.stack.Clear();
            this.currentCoordinateSystem = null;
        }

        private void SetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            this.currentCoordinateSystem = coordinateSystem;
        }

        protected abstract void StartArrayScope();
        protected abstract void StartObjectScope();
        private static bool TypeHasArrayScope(SpatialType type)
        {
            return ((type != SpatialType.Point) && (type != SpatialType.LineString));
        }

        private void WriteControlPoint(double first, double second, double? z, double? m)
        {
            this.StartArrayScope();
            this.AddValue(first);
            this.AddValue(second);
            if (z.HasValue)
            {
                this.AddValue(z.Value);
                if (m.HasValue)
                {
                    this.AddValue(m.Value);
                }
            }
            else if (m.HasValue)
            {
                this.AddValue((string) null);
                this.AddValue(m.Value);
            }
            this.EndArrayScope();
        }

        private void WriteCrs()
        {
            this.AddPropertyName("crs");
            this.StartObjectScope();
            this.AddPropertyName("type");
            this.AddValue("name");
            this.AddPropertyName("properties");
            this.StartObjectScope();
            this.AddPropertyName("name");
            this.AddValue("EPSG" + ':' + this.currentCoordinateSystem.Id);
            this.EndObjectScope();
            this.EndObjectScope();
        }

        private void WriteShapeHeader(SpatialType type)
        {
            this.StartObjectScope();
            this.AddPropertyName("type");
            this.AddValue(GetSpatialTypeName(type));
            this.AddPropertyName(GetDataName(type));
        }

        private bool FigureHasArrayScope
        {
            get
            {
                return (((SpatialType) this.stack.Peek()) != SpatialType.Point);
            }
        }

        private bool IsTopLevel
        {
            get
            {
                return (this.stack.Count == 0);
            }
        }

        private bool ShapeHasObjectScope
        {
            get
            {
                if (!this.IsTopLevel)
                {
                    return (((SpatialType) this.stack.Peek()) == SpatialType.Collection);
                }
                return true;
            }
        }
    }
}

