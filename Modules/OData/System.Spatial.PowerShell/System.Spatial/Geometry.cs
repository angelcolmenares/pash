namespace System.Spatial
{
    using System;

    internal abstract class Geometry : ISpatial
    {
        private readonly System.Spatial.CoordinateSystem coordinateSystem;
        private readonly SpatialImplementation creator;

        protected Geometry(System.Spatial.CoordinateSystem coordinateSystem, SpatialImplementation creator)
        {
            Util.CheckArgumentNull(coordinateSystem, "coordinateSystem");
            Util.CheckArgumentNull(creator, "creator");
            this.coordinateSystem = coordinateSystem;
            this.creator = creator;
        }

        internal bool? BaseEquals(Geometry other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (!this.coordinateSystem.Equals(other.coordinateSystem))
            {
                return false;
            }
            if (this.IsEmpty || other.IsEmpty)
            {
                return new bool?(this.IsEmpty && other.IsEmpty);
            }
            return null;
        }

        public virtual void SendTo(GeometryPipeline chain)
        {
            Util.CheckArgumentNull(chain, "chain");
            chain.SetCoordinateSystem(this.coordinateSystem);
        }

        public System.Spatial.CoordinateSystem CoordinateSystem
        {
            get
            {
                return this.coordinateSystem;
            }
        }

        internal SpatialImplementation Creator
        {
            get
            {
                return this.creator;
            }
        }

        public abstract bool IsEmpty { get; }
    }
}

