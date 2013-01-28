namespace Microsoft.Data.Spatial
{
    using System;
    using System.Spatial;

    internal class ForwardingSegment : SpatialPipeline
    {
        private readonly SpatialPipeline current;
        private GeographyForwarder geographyForwarder;
        private GeometryForwarder geometryForwarder;
        private SpatialPipeline next;
        internal static readonly SpatialPipeline SpatialPipelineNoOp = new SpatialPipeline(new NoOpGeographyPipeline(), new NoOpGeometryPipeline());

        public ForwardingSegment(SpatialPipeline current)
        {
            this.next = SpatialPipelineNoOp;
            this.current = current;
        }

        public ForwardingSegment(System.Spatial.GeographyPipeline currentGeography, System.Spatial.GeometryPipeline currentGeometry) : this(new SpatialPipeline(currentGeography, currentGeometry))
        {
        }

        public override SpatialPipeline ChainTo(SpatialPipeline destination)
        {
            Util.CheckArgumentNull(destination, "destination");
            this.next = destination;
            destination.StartingLink = base.StartingLink;
            return destination;
        }

        private static void DoAction(Action handler, Action handlerReset, Action delegation, Action delegationReset)
        {
            try
            {
                handler();
            }
            catch (Exception exception)
            {
                if (Util.IsCatchableExceptionType(exception))
                {
                    handlerReset();
                    delegationReset();
                }
                throw;
            }
            try
            {
                delegation();
            }
            catch (Exception exception2)
            {
                if (Util.IsCatchableExceptionType(exception2))
                {
                    handlerReset();
                }
                throw;
            }
        }

        private static void DoAction<T>(Action<T> handler, Action handlerReset, Action<T> delegation, Action delegationReset, T argument)
        {
            try
            {
                handler(argument);
            }
            catch (Exception exception)
            {
                if (Util.IsCatchableExceptionType(exception))
                {
                    handlerReset();
                    delegationReset();
                }
                throw;
            }
            try
            {
                delegation(argument);
            }
            catch (Exception exception2)
            {
                if (Util.IsCatchableExceptionType(exception2))
                {
                    handlerReset();
                }
                throw;
            }
        }

        public override System.Spatial.GeographyPipeline GeographyPipeline
        {
            get
            {
                return (this.geographyForwarder ?? (this.geographyForwarder = new GeographyForwarder(this)));
            }
        }

        public override System.Spatial.GeometryPipeline GeometryPipeline
        {
            get
            {
                return (this.geometryForwarder ?? (this.geometryForwarder = new GeometryForwarder(this)));
            }
        }

        public System.Spatial.GeographyPipeline NextDrawGeography
        {
            get
            {
                return (System.Spatial.GeographyPipeline) this.next;
            }
        }

        public System.Spatial.GeometryPipeline NextDrawGeometry
        {
            get
            {
                return (System.Spatial.GeometryPipeline) this.next;
            }
        }

        internal class GeographyForwarder : GeographyPipeline
        {
            private readonly ForwardingSegment segment;

            public GeographyForwarder(ForwardingSegment segment)
            {
                this.segment = segment;
            }

            public override void BeginFigure(GeographyPosition position)
            {
                Util.CheckArgumentNull(position, "position");
                this.DoAction<GeographyPosition>(val => this.Current.BeginFigure(val), val => this.Next.BeginFigure(val), position);
            }

            public override void BeginGeography(SpatialType type)
            {
                this.DoAction<SpatialType>(val => this.Current.BeginGeography(val), val => this.Next.BeginGeography(val), type);
            }

            private void DoAction(Action handler, Action delegation)
            {
                GeographyPipeline current = this.Current;
                GeographyPipeline next = this.Next;
                ForwardingSegment.DoAction(handler, new Action(current.Reset), delegation, new Action(next.Reset));
            }

            private void DoAction<T>(Action<T> handler, Action<T> delegation, T argument)
            {
                GeographyPipeline current = this.Current;
                GeographyPipeline next = this.Next;
                ForwardingSegment.DoAction<T>(handler, new Action(current.Reset), delegation, new Action(next.Reset), argument);
            }

            public override void EndFigure()
            {
                GeographyPipeline current = this.Current;
                GeographyPipeline next = this.Next;
                this.DoAction(new Action(current.EndFigure), new Action(next.EndFigure));
            }

            public override void EndGeography()
            {
                GeographyPipeline current = this.Current;
                GeographyPipeline next = this.Next;
                this.DoAction(new Action(current.EndGeography), new Action(next.EndGeography));
            }

            public override void LineTo(GeographyPosition position)
            {
                Util.CheckArgumentNull(position, "position");
                this.DoAction<GeographyPosition>(val => this.Current.LineTo(val), val => this.Next.LineTo(val), position);
            }

            public override void Reset()
            {
                GeographyPipeline current = this.Current;
                GeographyPipeline next = this.Next;
                this.DoAction(new Action(current.Reset), new Action(next.Reset));
            }

            public override void SetCoordinateSystem(CoordinateSystem coordinateSystem)
            {
                this.DoAction<CoordinateSystem>(val => this.Current.SetCoordinateSystem(val), val => this.Next.SetCoordinateSystem(val), coordinateSystem);
            }

            private GeographyPipeline Current
            {
                get
                {
                    return (GeographyPipeline) this.segment.current;
                }
            }

            private GeographyPipeline Next
            {
                get
                {
                    return (GeographyPipeline) this.segment.next;
                }
            }
        }

        internal class GeometryForwarder : GeometryPipeline
        {
            private readonly ForwardingSegment segment;

            public GeometryForwarder(ForwardingSegment segment)
            {
                this.segment = segment;
            }

            public override void BeginFigure(GeometryPosition position)
            {
                Util.CheckArgumentNull(position, "position");
                this.DoAction<GeometryPosition>(val => this.Current.BeginFigure(val), val => this.Next.BeginFigure(val), position);
            }

            public override void BeginGeometry(SpatialType type)
            {
                this.DoAction<SpatialType>(val => this.Current.BeginGeometry(val), val => this.Next.BeginGeometry(val), type);
            }

            private void DoAction(Action handler, Action delegation)
            {
                GeometryPipeline current = this.Current;
                GeometryPipeline next = this.Next;
                ForwardingSegment.DoAction(handler, new Action(current.Reset), delegation, new Action(next.Reset));
            }

            private void DoAction<T>(Action<T> handler, Action<T> delegation, T argument)
            {
                GeometryPipeline current = this.Current;
                GeometryPipeline next = this.Next;
                ForwardingSegment.DoAction<T>(handler, new Action(current.Reset), delegation, new Action(next.Reset), argument);
            }

            public override void EndFigure()
            {
                GeometryPipeline current = this.Current;
                GeometryPipeline next = this.Next;
                this.DoAction(new Action(current.EndFigure), new Action(next.EndFigure));
            }

            public override void EndGeometry()
            {
                GeometryPipeline current = this.Current;
                GeometryPipeline next = this.Next;
                this.DoAction(new Action(current.EndGeometry), new Action(next.EndGeometry));
            }

            public override void LineTo(GeometryPosition position)
            {
                Util.CheckArgumentNull(position, "position");
                this.DoAction<GeometryPosition>(val => this.Current.LineTo(val), val => this.Next.LineTo(val), position);
            }

            public override void Reset()
            {
                GeometryPipeline current = this.Current;
                GeometryPipeline next = this.Next;
                this.DoAction(new Action(current.Reset), new Action(next.Reset));
            }

            public override void SetCoordinateSystem(CoordinateSystem coordinateSystem)
            {
                this.DoAction<CoordinateSystem>(val => this.Current.SetCoordinateSystem(val), val => this.Next.SetCoordinateSystem(val), coordinateSystem);
            }

            private GeometryPipeline Current
            {
                get
                {
                    return (GeometryPipeline) this.segment.current;
                }
            }

            private GeometryPipeline Next
            {
                get
                {
                    return (GeometryPipeline) this.segment.next;
                }
            }
        }

        private class NoOpGeographyPipeline : GeographyPipeline
        {
            public override void BeginFigure(GeographyPosition position)
            {
            }

            public override void BeginGeography(SpatialType type)
            {
            }

            public override void EndFigure()
            {
            }

            public override void EndGeography()
            {
            }

            public override void LineTo(GeographyPosition position)
            {
            }

            public override void Reset()
            {
            }

            public override void SetCoordinateSystem(CoordinateSystem coordinateSystem)
            {
            }
        }

        private class NoOpGeometryPipeline : GeometryPipeline
        {
            public override void BeginFigure(GeometryPosition position)
            {
            }

            public override void BeginGeometry(SpatialType type)
            {
            }

            public override void EndFigure()
            {
            }

            public override void EndGeometry()
            {
            }

            public override void LineTo(GeometryPosition position)
            {
            }

            public override void Reset()
            {
            }

            public override void SetCoordinateSystem(CoordinateSystem coordinateSystem)
            {
            }
        }
    }
}

