namespace System.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal class CoordinateSystem
    {
        public static readonly CoordinateSystem DefaultGeography = new CoordinateSystem(0x10e6, "WGS84", Topology.Geography);
        public static readonly CoordinateSystem DefaultGeometry = new CoordinateSystem(0, "Unitless Plane", Topology.Geometry);
        private static readonly Dictionary<CompositeKey<int, Topology>, CoordinateSystem> References = new Dictionary<CompositeKey<int, Topology>, CoordinateSystem>(EqualityComparer<CompositeKey<int, Topology>>.Default);
        private static readonly object referencesLock = new object();
        private readonly Topology topology;

        static CoordinateSystem()
        {
            AddRef(DefaultGeometry);
            AddRef(DefaultGeography);
        }

        internal CoordinateSystem(int epsgId, string name, Topology topology)
        {
            this.topology = topology;
            this.EpsgId = new int?(epsgId);
            this.Name = name;
        }

        private static void AddRef(CoordinateSystem coords)
        {
            References.Add(KeyFor(coords.EpsgId.Value, coords.topology), coords);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CoordinateSystem);
        }

        public bool Equals(CoordinateSystem other)
        {
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }
            return (object.ReferenceEquals(this, other) || (object.Equals(other.topology, this.topology) && other.EpsgId.Equals(this.EpsgId)));
        }

        public static CoordinateSystem Geography(int? epsgId)
        {
            if (!epsgId.HasValue)
            {
                return DefaultGeography;
            }
            return GetOrCreate(epsgId.Value, Topology.Geography);
        }

        public static CoordinateSystem Geometry(int? epsgId)
        {
            if (!epsgId.HasValue)
            {
                return DefaultGeometry;
            }
            return GetOrCreate(epsgId.Value, Topology.Geometry);
        }

        public override int GetHashCode()
        {
            return ((this.topology.GetHashCode() * 0x18d) ^ (this.EpsgId.HasValue ? this.EpsgId.Value : 0));
        }

        private static CoordinateSystem GetOrCreate(int epsgId, Topology topology)
        {
            CoordinateSystem system;
            lock (referencesLock)
            {
                if (References.TryGetValue(KeyFor(epsgId, topology), out system))
                {
                    return system;
                }
                system = new CoordinateSystem(epsgId, "ID " + epsgId, topology);
                AddRef(system);
            }
            return system;
        }

        private static CompositeKey<int, Topology> KeyFor(int epsgId, Topology topology)
        {
            return new CompositeKey<int, Topology>(epsgId, topology);
        }

        internal bool TopologyIs(Topology expected)
        {
            return (this.topology == expected);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}CoordinateSystem(EpsgId={1})", new object[] { this.topology, this.EpsgId });
        }

        public string ToWktId()
        {
            return ("SRID=" + this.EpsgId + ";");
        }

        public int? EpsgId { get; private set; }

        public string Id
        {
            get
            {
                return this.EpsgId.Value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public string Name { get; private set; }

        internal enum Topology
        {
            Geography,
            Geometry
        }
    }
}

