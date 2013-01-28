namespace System.Spatial
{
    using System;

    internal class CompositeKey<T1, T2> : IEquatable<CompositeKey<T1, T2>>
    {
        private readonly T1 first;
        private readonly T2 second;

        public CompositeKey(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CompositeKey<T1, T2>);
        }

        public bool Equals(CompositeKey<T1, T2> other)
        {
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }
            return (object.ReferenceEquals(this, other) || (object.Equals(other.first, this.first) && object.Equals(other.second, this.second)));
        }

        public override int GetHashCode()
        {
            return ((this.first.GetHashCode() * 0x18d) ^ this.second.GetHashCode());
        }

        public static bool operator ==(CompositeKey<T1, T2> left, CompositeKey<T1, T2> right)
        {
            return object.Equals(left, right);
        }

        public static bool operator !=(CompositeKey<T1, T2> left, CompositeKey<T1, T2> right)
        {
            return !object.Equals(left, right);
        }
    }
}

