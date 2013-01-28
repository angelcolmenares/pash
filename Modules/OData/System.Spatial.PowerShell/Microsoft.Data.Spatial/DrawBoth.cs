namespace Microsoft.Data.Spatial
{
    using System;
    using System.Spatial;

    internal abstract class DrawBoth
    {
        protected DrawBoth()
        {
        }

        protected virtual GeographyPosition OnBeginFigure(GeographyPosition position)
        {
            return position;
        }

        protected virtual GeometryPosition OnBeginFigure(GeometryPosition position)
        {
            return position;
        }

        protected virtual SpatialType OnBeginGeography(SpatialType type)
        {
            return type;
        }

        protected virtual SpatialType OnBeginGeometry(SpatialType type)
        {
            return type;
        }

        protected virtual void OnEndFigure()
        {
        }

        protected virtual void OnEndGeography()
        {
        }

        protected virtual void OnEndGeometry()
        {
        }

        protected virtual GeographyPosition OnLineTo(GeographyPosition position)
        {
            return position;
        }

        protected virtual GeometryPosition OnLineTo(GeometryPosition position)
        {
            return position;
        }

        protected virtual void OnReset()
        {
        }

        protected virtual CoordinateSystem OnSetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            return coordinateSystem;
        }

        public static implicit operator SpatialPipeline(DrawBoth both)
        {
            if (both != null)
            {
                return new SpatialPipeline(both.GeographyPipeline, both.GeometryPipeline);
            }
            return null;
        }

        public virtual System.Spatial.GeographyPipeline GeographyPipeline
        {
            get
            {
                return new DrawGeographyInput(this);
            }
        }

        public virtual System.Spatial.GeometryPipeline GeometryPipeline
        {
            get
            {
                return new DrawGeometryInput(this);
            }
        }

        private class DrawGeographyInput : GeographyPipeline
        {
            private readonly DrawBoth both;

            public DrawGeographyInput(DrawBoth both)
            {
                this.both = both;
            }

            public override void BeginFigure(GeographyPosition position)
            {
                this.both.OnBeginFigure(position);
            }

            public override void BeginGeography(SpatialType type)
            {
                this.both.OnBeginGeography(type);
            }

            public override void EndFigure()
            {
                this.both.OnEndFigure();
            }

            public override void EndGeography()
            {
                this.both.OnEndGeography();
            }

            public override void LineTo(GeographyPosition position)
            {
                this.both.OnLineTo(position);
            }

            public override void Reset()
            {
                this.both.OnReset();
            }

            public override void SetCoordinateSystem(CoordinateSystem coordinateSystem)
            {
                this.both.OnSetCoordinateSystem(coordinateSystem);
            }
        }

        private class DrawGeometryInput : GeometryPipeline
        {
            private readonly DrawBoth both;

            public DrawGeometryInput(DrawBoth both)
            {
                this.both = both;
            }

            public override void BeginFigure(GeometryPosition position)
            {
                this.both.OnBeginFigure(position);
            }

            public override void BeginGeometry(SpatialType type)
            {
                this.both.OnBeginGeometry(type);
            }

            public override void EndFigure()
            {
                this.both.OnEndFigure();
            }

            public override void EndGeometry()
            {
                this.both.OnEndGeometry();
            }

            public override void LineTo(GeometryPosition position)
            {
                this.both.OnLineTo(position);
            }

            public override void Reset()
            {
                this.both.OnReset();
            }

            public override void SetCoordinateSystem(CoordinateSystem coordinateSystem)
            {
                this.both.OnSetCoordinateSystem(coordinateSystem);
            }
        }
    }
}

