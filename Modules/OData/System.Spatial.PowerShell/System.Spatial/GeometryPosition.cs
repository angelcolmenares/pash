namespace System.Spatial
{
    using System;
    using System.Globalization;

    internal class GeometryPosition : IEquatable<GeometryPosition>
    {
        private readonly double? m;
        private readonly double x;
        private readonly double y;
        private readonly double? z;

        public GeometryPosition(double x, double y) : this(x, y, null, null)
        {
        }

        public GeometryPosition(double x, double y, double? z, double? m)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.m = m;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj))
            {
                return false;
            }
            if (obj.GetType() != typeof(GeometryPosition))
            {
                return false;
            }
            return this.Equals((GeometryPosition) obj);
        }

        public bool Equals(GeometryPosition other)
        {
            return ((((other != null) && other.x.Equals(this.x)) && (other.y.Equals(this.y) && other.z.Equals(this.z))) && other.m.Equals(this.m));
        }

        public override int GetHashCode()
        {
            int num = (this.x.GetHashCode() * 0x18d) ^ this.y.GetHashCode();
            num = (num * 0x18d) ^ (this.z.HasValue ? this.z.Value.GetHashCode() : 0);
            return ((num * 0x18d) ^ (this.m.HasValue ? this.m.Value.GetHashCode() : 0));
        }

        public static bool operator ==(GeometryPosition left, GeometryPosition right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            if (object.ReferenceEquals(right, null))
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(GeometryPosition left, GeometryPosition right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "GeometryPosition({0}, {1}, {2}, {3})", new object[] { this.x, this.y, this.z.HasValue ? this.z.ToString() : "null", this.m.HasValue ? this.m.ToString() : "null" });
        }

        public double? M
        {
            get
            {
                return this.m;
            }
        }

        public double X
        {
            get
            {
                return this.x;
            }
        }

        public double Y
        {
            get
            {
                return this.y;
            }
        }

        public double? Z
        {
            get
            {
                return this.z;
            }
        }
    }
}

