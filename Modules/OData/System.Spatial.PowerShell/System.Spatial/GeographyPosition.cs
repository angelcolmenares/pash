namespace System.Spatial
{
    using System;
    using System.Globalization;

    internal class GeographyPosition : IEquatable<GeographyPosition>
    {
        private readonly double latitude;
        private readonly double longitude;
        private readonly double? m;
        private readonly double? z;

        public GeographyPosition(double latitude, double longitude) : this(latitude, longitude, null, null)
        {
        }

        public GeographyPosition(double latitude, double longitude, double? z, double? m)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.z = z;
            this.m = m;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj))
            {
                return false;
            }
            if (obj.GetType() != typeof(GeographyPosition))
            {
                return false;
            }
            return this.Equals((GeographyPosition) obj);
        }

        public bool Equals(GeographyPosition other)
        {
            return ((((other != null) && other.latitude.Equals(this.latitude)) && (other.longitude.Equals(this.longitude) && other.z.Equals(this.z))) && other.m.Equals(this.m));
        }

        public override int GetHashCode()
        {
            int num = (this.latitude.GetHashCode() * 0x18d) ^ this.longitude.GetHashCode();
            num = (num * 0x18d) ^ (this.z.HasValue ? this.z.Value.GetHashCode() : 0);
            return ((num * 0x18d) ^ (this.m.HasValue ? this.m.Value.GetHashCode() : 0));
        }

        public static bool operator ==(GeographyPosition left, GeographyPosition right)
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

        public static bool operator !=(GeographyPosition left, GeographyPosition right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "GeographyPosition(latitude:{0}, longitude:{1}, z:{2}, m:{3})", new object[] { this.latitude, this.longitude, this.z.HasValue ? this.z.ToString() : "null", this.m.HasValue ? this.m.ToString() : "null" });
        }

        public double Latitude
        {
            get
            {
                return this.latitude;
            }
        }

        public double Longitude
        {
            get
            {
                return this.longitude;
            }
        }

        public double? M
        {
            get
            {
                return this.m;
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

