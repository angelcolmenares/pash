namespace System.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal abstract class Geography : ISpatial
    {
        private readonly System.Spatial.CoordinateSystem coordinateSystem;
        private readonly SpatialImplementation creator;

        protected Geography(System.Spatial.CoordinateSystem coordinateSystem, SpatialImplementation creator)
        {
            Util.CheckArgumentNull(coordinateSystem, "coordinateSystem");
            Util.CheckArgumentNull(creator, "creator");
            this.coordinateSystem = coordinateSystem;
            this.creator = creator;
        }

        internal bool? BaseEquals(Geography other)
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

        internal static int ComputeHashCodeFor<T>(System.Spatial.CoordinateSystem coords, IEnumerable<T> fields)
        {
            Func<int, T, int> func = null;
            if (func == null)
            {
                func = (current, field) => (current * 0x18d) ^ field.GetHashCode();
            }
            return fields.Aggregate<T, int>(coords.GetHashCode(), func);
        }

        public virtual void SendTo(GeographyPipeline chain)
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

